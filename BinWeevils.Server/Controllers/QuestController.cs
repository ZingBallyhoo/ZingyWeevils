using System.Net.Mime;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Form;
using BinWeevils.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class QuestController : Controller
    {
        private readonly ILogger<QuestController> m_logger;
        private readonly WeevilDBContext m_dbContext;
        private readonly QuestRepository m_questRepository;
        
        public QuestController(ILogger<QuestController> logger, WeevilDBContext dbContext, QuestRepository repository)
        {
            m_logger = logger;
            m_dbContext = dbContext;
            m_questRepository = repository;
        }
        
        [HttpGet("php/getQuestData.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetQuestDataResponse> GetQuestData()
        {
            using var activity = ApiServerObservability.StartActivity("QuestController.GetQuestData");
            
            var tasks = await m_dbContext.m_completedTasks
                .Where(x => x.m_weevil.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => x.m_taskID)
                .ToListAsync();
            
            // exactly whose idea was it that the server should control this...
            var items = new List<string>();
            if (IsMissionSectionInProgress(tasks, 16, 17))
            {
                // silverKnight
                items.Add("<preRend3D locID='113' path='assets3D/key1.swf' extUIData='path:externalUIs/quests/silverKnight.swf,section:3' boundary='type:rad,r:40' x='189' y='0' z='120' scale='0.14' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>");
            }
            foreach (var blueDiamondItem in m_questRepository.m_blueDiamondItems)
            {
                if (IsMissionSectionInProgress(tasks, 29, blueDiamondItem.m_collectedTaskID))
                {
                    items.Add(blueDiamondItem.m_text);
                }
            }
            
            var response = new GetQuestDataResponse
            {
                m_responseCode = 1,
                m_tasks = tasks,
                m_itemList = string.Join("", items)
            };
            return response;
        }
        
        private bool IsMissionSectionInProgress(List<int> completedTasks, int startTask, int endTask)
        {
            // todo: lookups here aren't efficient...
            
            var sectionStarted = completedTasks.Any(x => x == startTask);
            if (!sectionStarted) return false;
            
            var sectionEnded = completedTasks.Any(x => x == endTask);
            return !sectionEnded;
        }
        
        [StructuredFormPost("php/taskCompleted.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<TaskCompletedResponse> TaskCompleted([FromBody] TaskCompletedRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("QuestController.TaskCompleted");
            activity?.SetTag("userName", request.m_userName);
            activity?.SetTag("questID", request.m_questID);
            activity?.SetTag("taskID", request.m_taskID);
            
            if (request.m_userName != ControllerContext.HttpContext.User.Identity!.Name)
            {
                throw new InvalidDataException("sending TaskCompleted for wrong user");
            }
            
            if (!m_questRepository.TryGetTask(request.m_taskID, out var task))
            {
                m_logger.LogWarning("Request for unknown task: {TaskID}", request.m_taskID);
                return new TaskCompletedResponse
                {
                    m_responseCode = TaskCompletedResponse.RESPONSE_NOT_FOUND
                };
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var weevilIdx = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_userName)
                .Select(x => x.m_idx)
                .SingleAsync();
            
            var response = new TaskCompletedResponse
            {
                m_responseCode = TaskCompletedResponse.RESPONSE_OK,
                m_result = task.m_isMissionComplete ? 
                    TaskCompletedResponse.RES_QUEST_COMPLETE : 
                    TaskCompletedResponse.RES_TASK_COMPLETE,
            };
            
            var rowsInserted = await m_dbContext.m_completedTasks.Upsert(new CompletedTaskDB
            {
                m_weevilID = weevilIdx,
                m_taskID = task.m_scrapedData.m_id
            }).NoUpdate().RunAsync();
            if (rowsInserted == 0)
            {
                var overrideResponse = await TaskCompleted_AlreadyCompleted(weevilIdx, task, response);
                if (overrideResponse != null)
                {
                    return overrideResponse;
                }
                
                ApiServerObservability.s_tasksCompleted.Add(1);
                
                // we can't reward anything so don't try :)
                await AddCommonCompletedData(weevilIdx, response);
                return response;
            }

            await TryReward(weevilIdx, task, response);
            await AddCommonCompletedData(weevilIdx, response);
            
            ApiServerObservability.s_tasksCompleted.Add(1);
            
            await transaction.CommitAsync();
            return response;
        }
        
        private async Task TryReward(uint weevilIdx, QuestRepository.TaskRuntimeData task, TaskCompletedResponse response) 
        {
            var rowsInserted = await m_dbContext.m_rewardedTasks.Upsert(new RewardedTaskDB
            {
                m_weevilID = weevilIdx,
                m_taskID = task.m_scrapedData.m_id
            }).NoUpdate().RunAsync();
            if (rowsInserted == 0)
            {
                m_logger.LogTrace("Already granted rewards for task {TaskID}", task.m_scrapedData.m_id);
                return;
            }
            
            using var activity = ApiServerObservability.StartActivity("QuestController.Reward");
            m_logger.LogTrace("Granting rewards for task {TaskID}", task.m_scrapedData.m_id);
            
            var rowsUpdated = await m_dbContext.m_weevilDBs
                .Where(x => x.m_idx == weevilIdx)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.m_mulch, x => x.m_mulch + task.m_scrapedData.m_rewardMulch)
                    .SetProperty(x => x.m_xp, x => x.m_xp + task.m_scrapedData.m_rewardXp)
                );
            if (rowsUpdated == 0)
            {
                throw new Exception("failed to award mulch & xp");
            }
            
            await RewardItems(weevilIdx, task, response);
            await RewardSpecialMoves(weevilIdx, task, response);
            
            await m_dbContext.SaveChangesAsync();
            
            ApiServerObservability.s_tasksRewarded.Add(1);
            ApiServerObservability.s_tasksMulchRewarded.Add(task.m_scrapedData.m_rewardMulch);
            ApiServerObservability.s_tasksXpRewarded.Add(task.m_scrapedData.m_rewardXp);
            if (task.m_isMissionComplete)
            {
                ApiServerObservability.s_questsCompleted.Add(1, new KeyValuePair<string, object?>("quest", task.m_scrapedData.m_questID));
            }
            ApiServerObservability.s_tasksNestItemsRewarded.Add(response.m_itemName.Count);
            ApiServerObservability.s_tasksGardenItemsRewarded.Add(response.m_gardenItemName.Count);
            ApiServerObservability.s_tasksSpecialMovesRewarded.Add(response.m_move.Count);
        }
        
        private async Task RewardItems(uint weevilIdx, QuestRepository.TaskRuntimeData task, TaskCompletedResponse response)
        {
            if (task.m_scrapedData.m_rewardItems == null &&
                task.m_scrapedData.m_rewardGardenItems == null &&
                task.m_scrapedData.m_rewardGardenSeeds == null)
            {
                // todo: or bundle items?
                return;
            }

            var nestID = await m_dbContext.m_weevilDBs
                .Where(x => x.m_idx == weevilIdx)
                .Select(x => x.m_nest.m_id)
                .SingleAsync();
            
            if (task.m_scrapedData.m_rewardItems != null)
            {
                foreach (var rewardItem in task.m_scrapedData.m_rewardItems)
                {
                    var itemType = await m_dbContext.m_itemTypes
                        .Where(x => x.m_configLocation == rewardItem.m_configName)
                        .Select(x => new 
                        {
                            x.m_itemTypeID,
                            x.m_name
                        })
                        .SingleAsync();
                    
                    m_logger.LogTrace("Granting nest item \"{Name}\" * {Count}", itemType.m_name, rewardItem.m_count);
                    
                    for (var i = 0; i < rewardItem.m_count; i++)
                    {
                        await m_dbContext.m_nestItems.AddAsync(new NestItemDB
                        {
                            m_nestID = nestID,
                            m_itemTypeID = itemType.m_itemTypeID
                        });
                        response.m_itemName.Add(itemType.m_name);
                    }
                }
            }
            
            if (task.m_scrapedData.m_rewardGardenItems != null)
            {
                foreach (var rewardGardenItem in task.m_scrapedData.m_rewardGardenItems)
                {
                    var itemType = await m_dbContext.m_itemTypes
                        .Where(x => x.m_configLocation == rewardGardenItem.m_configName)
                        .Select(x => new 
                        {
                            x.m_itemTypeID,
                            x.m_name
                        })
                        .SingleAsync();
                    
                    m_logger.LogTrace("Granting garden item \"{Name}\" * {Count}", itemType.m_name, rewardGardenItem.m_count);
                    
                    for (var i = 0; i < rewardGardenItem.m_count; i++)
                    {
                        await m_dbContext.m_nestGardenItems.AddAsync(new NestGardenItemDB
                        {
                            m_nestID = nestID,
                            m_itemTypeID = itemType.m_itemTypeID
                        });
                        response.m_gardenItemName.Add(itemType.m_name);
                    }
                }
            }
            
            if (task.m_scrapedData.m_rewardGardenSeeds != null)
            {
                foreach (var rewardGardenSeed in task.m_scrapedData.m_rewardGardenSeeds)
                {
                    var seedType = await m_dbContext.m_seedTypes
                        .Where(x => x.m_fileName == rewardGardenSeed.Key)
                        .Select(x => new 
                        {
                            x.m_id,
                            x.m_name
                        })
                        .SingleAsync();
                    
                    m_logger.LogTrace("Granting garden seed \"{Name}\" * {Count}", seedType.m_name, rewardGardenSeed.Value);
                    
                    for (var i = 0; i < rewardGardenSeed.Value; i++)
                    {
                        await m_dbContext.m_nestGardenSeeds.AddAsync(new NestSeedItemDB
                        {
                            m_nestID = nestID,
                            m_seedTypeID = seedType.m_id
                        });
                    }
                    
                    // has to be done here as we don't store in response...
                    ApiServerObservability.s_tasksSeedsRewarded.Add(rewardGardenSeed.Value);
                }
            }
        }
        
        private async Task RewardSpecialMoves(uint weevilIdx, QuestRepository.TaskRuntimeData task, TaskCompletedResponse response)
        {
            if (task.m_scrapedData.m_rewardMoves == null) return;
            
            foreach (var rewardMove in task.m_scrapedData.m_rewardMoves)
            {
                m_logger.LogTrace("Granting special move {Move}", rewardMove);
            
                await m_dbContext.m_weevilSpecialMoves
                    .Upsert(new WeevilSpecialMoveDB
                    {
                        m_weevilIdx = weevilIdx,
                        m_action = rewardMove
                    })
                    .NoUpdate()
                    .RunAsync();
                response.m_move.Add((int)rewardMove); // todo: does the game even support a list.. and what delimiter
            }
        }
        
        private async Task AddCommonCompletedData(uint weevilIdx, TaskCompletedResponse resp)
        {
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_idx == weevilIdx)
                .Select(x => new
                {
                    x.m_mulch,
                    x.m_xp,
                })
                .SingleAsync();
            
            resp.m_mulch = dto.m_mulch;
            resp.m_xp = dto.m_xp;
        }
        
        private async Task<TaskCompletedResponse?> TaskCompleted_AlreadyCompleted(uint weevilIdx, QuestRepository.TaskRuntimeData task, TaskCompletedResponse response)
        {
            if (task.m_scrapedData.m_questID.HasValue && task.m_scrapedData.m_nonRestartable)
            {
                if (!await IsMissionComplete(weevilIdx, task.m_scrapedData.m_questID.Value))
                {
                    m_logger.LogWarning("Trying to start a mission that is already active");
                    return new TaskCompletedResponse
                    {
                        m_responseCode = TaskCompletedResponse.RESPONSE_MISSION_ALREADY_ACTIVE
                    };
                }
            }
            
            foreach (var deletedTask in task.m_scrapedData.m_deletedTasks)
            {
                if (await TryRestartTask(weevilIdx, deletedTask)) 
                {
                    m_logger.LogTrace("Restarted child task {TaskID}", deletedTask);
                    response.m_deletedTasks.Add(deletedTask);
                }
            }
            if (response.m_deletedTasks.Count > 0 && task.m_scrapedData.m_questID != null)
            {
                ApiServerObservability.s_questsRestarted.Add(1, new KeyValuePair<string, object?>("quest", task.m_scrapedData.m_questID));
            }
            return null;
        }
        
        private async Task<bool> IsMissionComplete(uint weevilIdx, int questID)
        {
            var quest = m_questRepository.GetQuest(questID);
            if (quest.m_completeTask == null) return false;
            
            return await m_dbContext.m_completedTasks.AnyAsync(x => 
                x.m_weevilID == weevilIdx && 
                x.m_taskID == quest.m_completeTask);
        }
        
        private async Task<bool> TryRestartTask(uint weevilIdx, int taskID)
        {
            var rowsDeleted = await m_dbContext.m_completedTasks
                .Where(x => x.m_weevilID == weevilIdx)
                .Where(x => x.m_taskID == taskID)
                .ExecuteDeleteAsync();
            
            return rowsDeleted != 0;
        }
        
        // fp uses GET, ruffle uses POST
        // todo: (the code intends to use POST but fp behaviour falls back to GET when no body)
        // fixed by https://github.com/ruffle-rs/ruffle/pull/20242 but not in local build
        [HttpPost("php/getMissionList.php")]
        [HttpGet("php/getMissionList.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<MissionList> GetMissionList()
        {
            var missions = m_questRepository.GetQuests()
                .Where(x => x.m_showInList)
                .ToArray();
            
            var dto = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_idx,
                })
                .SingleAsync();
            
            // todo: should this check "is completed ever"? (rewarded)
            // either way, restarting quests is unused
            var isComplete = await missions
                .ToAsyncEnumerable()
                .SelectAwait(async x => await IsMissionComplete(dto.m_idx, x.m_id))
                .ToArrayAsync();
            
            return new MissionList
            {
                m_responseCode = 1,
                m_ids = string.Join('|', missions.Select(x => x.m_id)), 
                m_names = string.Join('|', missions.Select(x => x.m_name)),
                m_paths = string.Join('|', missions.Select(x => x.m_swfPath)),
                m_levels = string.Join('|', missions.Select(x => 0)),
                m_completed = string.Join('|', isComplete.Select(x => x ? 1 : 0)),
                m_tycoon = string.Join('|', missions.Select(x => 0)),
            };
        }
    }
}