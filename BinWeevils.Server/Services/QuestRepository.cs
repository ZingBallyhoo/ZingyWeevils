using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using BinWeevils.Protocol.Form;
using BinWeevils.Protocol.Json;
using BinWeevils.Protocol.Xml;
using StackXML;

namespace BinWeevils.Server.Services
{
    public class QuestRepository
    {
        private readonly Dictionary<int, QuestRuntimeData> m_quests = new Dictionary<int, QuestRuntimeData>();
        private readonly Dictionary<int, TaskRuntimeData> m_tasks = new Dictionary<int, TaskRuntimeData>();
        
        public readonly BlueDiamondItem[] m_blueDiamondItems = [
            new BlueDiamondItem(163, "<preRend3D logicID='31' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:31' x='-246' y='18' z='269' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(163, "<preRend3D logicID='32' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:32' x='107' y='18' z='341' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(163, "<preRend3D logicID='33' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:33' x='-245' y='18' z='479' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(163, "<preRend3D logicID='34' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:34' x='-103' y='18' z='689' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(163, "<preRend3D logicID='35' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:35' x='247' y='18' z='761' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(164, "<preRend3D logicID='36' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:36' x='146.3' y='18' z='109.1' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(164, "<preRend3D logicID='37' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:37' x='370.6' y='18' z='297.3' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(164, "<preRend3D logicID='38' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:38' x='421.4' y='18' z='585.5' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(164, "<preRend3D logicID='39' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:39' x='275.1' y='18' z='838.9' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(164, "<preRend3D logicID='40' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:40' x='-275.1' y='18' z='839.1' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(164, "<preRend3D logicID='41' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:41' x='421.5' y='18' z='585.5' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(164, "<preRend3D logicID='42' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:42' x='-421.5' y='18' z='585.5' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(164, "<preRend3D logicID='43' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:43' x='-146.3' y='18' z='108.8' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>"),
            new BlueDiamondItem(165, "<preRend3D logicID='44' path='assets3D/bubble_blueDiamond.swf' extUIData='path:externalUIs/quests/blueDiamond2.swf,section:5,taskID:44' x='212' y='18' z='722' scale='0.07' ry='0' rxMin='6' rxMax='46' ryMin='0' ryMax='1' framesY='1' symAxes='0' rIncr='3'/>")
        ];
        
        public class BlueDiamondItem 
        {
            public readonly string m_text;
            public readonly int m_collectedTaskID;
            
            public BlueDiamondItem(int loc, string text)
            {
                m_text = text;
                
                var preRend3D = XmlReadBuffer.ReadStatic<LocationPreRend3D>(text);
                m_collectedTaskID = preRend3D.m_logicID;
                Debug.Assert(text.Contains($"taskID:{m_collectedTaskID}'"));
                
                const string prefix = "<preRend3D ";
                Debug.Assert(text.StartsWith(prefix));
                m_text = m_text.Insert(prefix.Length, $"locID='{loc}' ");
            }
        } 
        
        public QuestRepository()
        {
            var missions = ParseMissionList();
        
            var tycoonManifest = JsonSerializer.Deserialize<ScrapedManifest>(File.ReadAllText(Path.Combine("Data", "mission_output3_tycoon.json")), new JsonSerializerOptions
            {
                RespectRequiredConstructorParameters = true
            })!;
            
            // validate some assumptions
            ArgumentOutOfRangeException.ThrowIfGreaterThan(tycoonManifest.m_unavailableTasks.Count, 0, "should have been no failing tasks for tycoon data");
            
            foreach (var task in tycoonManifest.m_tasks.Values)
            {
                // represents the quest id that was sent to the scraper
                // i never sent a quest id so...
                if (task.m_questID != null) throw new InvalidDataException("invalid quest id");
                
                if (task.m_nonRestartable)
                {
                    // this only means that the quest can't be restarted while its already in progress
                    
                    // false: ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(task.m_deletedTasks.Count, 0, "quest start task should delete tasks");
                    // truth: not all quests need replayable tasks
                    // this is mostly about all the non-mission quests...
                }
                    
                if (task.m_deletedTasks.Count > 0)
                {
                    // only "mission starts" can delete tasks
                    Debug.Assert(task.m_nonRestartable);
                }
                
                if (task.m_rewardItems != null)
                {
                    // i treated this as a list, but the game will only ever reward one
                    Debug.Assert(task.m_rewardItems.Count == 1);
                }
            }
            
            foreach (var questPair in tycoonManifest.m_quests)
            {
                foreach (var taskID in questPair.Value.m_tasks)
                {
                    var scrapedTask = tycoonManifest.m_tasks[taskID];
                    scrapedTask.m_questID = questPair.Key;
                    
                    m_tasks.Add(taskID, new TaskRuntimeData 
                    {
                        m_scrapedData = scrapedTask
                    });
                }
            }

            foreach (var questPair in tycoonManifest.m_quests)
            {
                if (questPair.Value.m_tasks.Count == 0) continue; // okay idc
                
                var startTask = questPair.Value.m_tasks.First();
                bool replayable;
                if (!tycoonManifest.m_tasks[startTask].m_nonRestartable)
                {
                    // non-replayable
                    replayable = false;
                    
                    // make sure there's none..
                    if (questPair.Key != 16)
                    {
                        // ignore "New User Tasks" as idek what's going on there
                        
                        Debug.Assert(questPair.Value.m_tasks.Count(x => tycoonManifest.m_tasks[x].m_nonRestartable) == 0);
                    }
                } else
                {
                    // replayable
                    replayable = true;

                    // make sure there's only one..
                    Debug.Assert(questPair.Value.m_tasks.Count(x => tycoonManifest.m_tasks[x].m_nonRestartable) == 1);
                }

                int? completeTask = null;
                if (replayable)
                {
                    completeTask = questPair.Key switch
                    {
                        1 => 10,
                        _ => questPair.Value.m_tasks.Last()
                    };
                }
                
                // missions should be replayable
                if (missions[questPair.Key].m_swfPath != "")
                {
                    Debug.Assert(replayable);
                }
                
                var showInMissionList = questPair.Key switch
                {
                    2 => true, // silver knight
                    3 => true, // bd 1
                    4 => true, // bd2
                    5 => true, // aztec
                    6 => true, // binstalk
                    //7 => true, // trouble 1 - 84
                    //8 => true, // trouble 2 - 130
                    _ => false,
                    
                    /*1 => true, // join the sws
                    2 => true, // silver knight
                    3 => true, // bd 1
                    4 => true, // bd2
                    5 => true, // ztec
                    6 => true, // binstalk
                    7 => true, // trouble 1 - 84
                    8 => true, // trouble 2 - 130
                    9 => true, // raiders
                    10 => true, // weevil x
                    12 => true, // danger at dosh
                    13 => true, // tyconn tv showdown
                    14 => true, // lab lockdown
                    17 => true, // {m_name: Case File 1: Good vs WeEvil}
                    18 => true, // {m_name: Case File 2: Micro Mayhem}
                    43 => true, // {m_name: Case File 3: Scribbles the secret hunter}
                    52 => true, // {m_name: The Bin's Big Freeze}
                    */
                };
                
                if (completeTask != null)
                {
                    m_tasks[completeTask.Value].m_isMissionComplete = true;
                }
                
                m_quests.Add(questPair.Key, new QuestRuntimeData
                {
                    m_id = questPair.Key,
                    m_name = missions[questPair.Key].m_name,
                    m_swfPath = missions[questPair.Key].m_swfPath,
                    m_showInList = showInMissionList,
                    m_completeTask = completeTask
                });
            }
        }
    
        private static Dictionary<int, MissionInList> ParseMissionList()
        {
            // where did this come from...? i don't even know
            // missionListClean.txt
            var allMissionsList = new MissionList
            {
                m_responseCode = 1,
                m_ids = "1|2|3|4|5|6|7|8|9|10|11|12|13|14|16|16|17|18|19|20|21|22|23|24|25|26|27|28|29|30|31|32|33|34|35|36|37|37|37|38|39|40|41|43|45|46|47|48|49|50|51|52|53|54|55|56|57|58|59|60|61|62|63|64|65|67|68|69|72|73|74|75|76|77", 
                m_names = "Join the SWS|The Lost Silver Knight|The Blue Diamond (Part 1)|The Blue Diamond (Part 2)|Totem of the Aztecs|Jack and the Binstalk|Trouble at Castle Gam (Part 1)|Trouble at Castle Gam (Part 2)|Raiders of the Lost Bin Pet|The Hunt for Weevil X|Gun Training|Danger at Dosh's Palace|Showdown at Tycoon TV Towers|Laboratory Lockdown|New Users Tasks|New Users Tasks|Case File 1: Good vs WeEvil|Case File 2: Micro Mayhem|WeeklyChallenge1 Part1 - Missing Golden Bin Pet Ball|WeeklyChallenge1 Part2 - Missing Golden Bin Pet Ball|WeeklyChallenge1 Part3 - Missing Golden Bin Pet Ball|WeeklyChallenge2 Part1|WeeklyChallenge2 Part2|WeeklyChallenge2 Part3|WeeklyChallenge3 Part1|WeeklyChallenge3 Part2|WeeklyChallenge3 Part3|WeeklyChallenge4 Part1|WeeklyChallenge4 Part2|WeeklyChallenge4 Part3|WeeklyChallenge5 Part1|WeeklyChallenge5 Part2|WeeklyChallenge5 Part3|WeeklyChallenge6 Part1|WeeklyChallenge6 Part2|WeeklyChallenge6 Part3|Bin Bot Potions|Bin Bot Potions|Bin Bot Potions|WeeklyChallenge7 Part1|WeeklyChallenge7 Part2|WeeklyChallenge7 Part3|Super Antenna Hunt|Case File 3: Scribbles the secret hunter|Halloween 2013 treasure room|Bin Pets VIP gold gift set|Disco Tycoon rewards|Easter Hunt|Random Tasks|Summer Fair Fun House|Halloween 2014|The Bin's Big Freeze|Advent Calendar 2014|Snow Weevil Hunt 2014|SWS vs WEB hunt|Halloween Pumpkin Hunt 2015|Airfix2015|MLP Nest Bundle 1|MLP Nest Bundle 2|Minions Nest Bundle|The Good Dino Nest Bundle|Transformers 2016 Statue pack|Transformers 2016 Poster pack|LEGO Friends Adventure Club Nest Items|Plants Vs Zombies Garden Warfare 2 Nest Items|Zootropolis nest items|Big Bloom|History Hunters|MLPS6 Stamp Card|Transformers2017|Transformers S2 Ultimate Fan|Weevil World Tutorial|Hot Wheels Weevil World Hunt 2018|Weevil World Pumpkin Hunt", 
                m_paths = "|silverKnight|blueDiamond1_04_02_13|blueDiamond2_04_02_13|Aztec_04_02_13|giant_04_02_13|CastleQuest_1_04_02_13|CastleQuest_2_04_02_13|lostBinPet/intro_12_06_12|gemHunt/intro_12_06_12||doshHeritage/intro_12_06_12|tycoonTerrorTowers/intro_12_06_12|laboratoryLockDown/intro||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||", 
                levelList = "0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                completedList = "0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                tycoonList = "1|1|1|1|1|1|1|1|0|1|1|1|1|1|0|0|0|1|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|1|1|0|1|1|0|0|1|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                roomList = "2|0|0|0|0|0|0|0|1|1|2|1|1|1|2|2|3|3|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|3|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|2|0|0|0", 
                priceList = "0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                scoreBronze = "0|0|0|0|0|0|0|0|6|6|0|6|5|7|0|0|40|14|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|12|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                scoreSilver = "0|0|0|0|0|0|0|0|12|12|0|12|10|14|0|0|50|21|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|18|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                scoreGold = "0|0|0|0|0|0|0|0|18|18|0|18|15|24|0|0|60|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                scorePlatinum = "0|0|0|0|0|0|0|0|24|24|0|24|18|34|0|0|70|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0", 
                highScore = "|||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||||"
            };
            
            var ids = allMissionsList.m_ids.Split('|');
            var names = allMissionsList.m_names.Split('|');
            var paths = allMissionsList.m_paths.Split('|');
            Debug.Assert(ids.Length == names.Length);
            Debug.Assert(ids.Length == paths.Length);
            
            var map = new Dictionary<int, MissionInList>();
            for (var i = 0; i < ids.Length; i++)
            {
                var id = int.Parse(ids[i]);
                var name = names[i];
                var swfPath = paths[i];
                
                if (map.TryGetValue(id, out var existingEntry))
                {
                    Debug.Assert(existingEntry.m_name == name);
                    Debug.Assert(existingEntry.m_swfPath == swfPath);
                    // Console.Out.WriteLine($"mission \"{name}\"({id}) exists multiple times for ?? reasons");
                    continue;
                }
                
                map.Add(id, new MissionInList
                {
                    
                    m_id = id,
                    m_name = name,
                    m_swfPath = swfPath,
                });
            }
            
            return map;
        }
        
        private class MissionInList
        {
            public int m_id;
            public string m_name;
            public string m_swfPath;
        }
        
        public class QuestRuntimeData
        {
            public int m_id;
            public string m_name;
            public string m_swfPath;
            public bool m_showInList;
            public int? m_completeTask;
        }
        
        public class TaskRuntimeData
        {
            public ScrapedTaskData m_scrapedData;
            
            // public bool m_isMissionStart;
            public bool m_isMissionComplete;
        }
        
        public QuestRuntimeData GetQuest(int questId)
        {
            return m_quests[questId];
        }
        
        public bool TryGetTask(int id, [MaybeNullWhen(false)] out TaskRuntimeData task)
        {
            return m_tasks.TryGetValue(id, out task);
        }
    }
}