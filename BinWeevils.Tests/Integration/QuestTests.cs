using BinWeevils.Protocol.Form;

namespace BinWeevils.Tests.Integration
{
    [Collection("Integration")]
    public class QuestTests
    {
        private readonly IntegrationAppFactory m_factory;

        public QuestTests(IntegrationAppFactory factory)
        {
            m_factory = factory;
        }
        
        [Fact]
        public async Task CompleteSilverKnight()
        {
            var account = await m_factory.CreateAccount(nameof(CompleteSilverKnight));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            
            // start mission
            {
                var r = await client.PostSimpleFormAsync<TaskCompletedRequest, TaskCompletedResponse>("api/php/taskCompleted.php", new TaskCompletedRequest
                {
                    m_userName = account.UserName!,
                    m_questID = 2,
                    m_taskID = 16
                });
                Assert.Equal(TaskCompletedResponse.RESPONSE_OK, r.m_responseCode);
                Assert.Equal(TaskCompletedResponse.RES_TASK_COMPLETE, r.m_result);
                
                r = await client.PostSimpleFormAsync<TaskCompletedRequest, TaskCompletedResponse>("api/php/taskCompleted.php", new TaskCompletedRequest
                {
                    m_userName = account.UserName!,
                    m_questID = 2,
                    m_taskID = 16
                });
                Assert.Equal(TaskCompletedResponse.RESPONSE_MISSION_ALREADY_ACTIVE, r.m_responseCode);
            }
            
            // should have added key item
            var questData = await client.GetFormAsync<GetQuestDataResponse>("api/php/getQuestData.php");
            Assert.NotEmpty(questData.m_itemList);

            // find key
            {
                var r = await client.PostSimpleFormAsync<TaskCompletedRequest, TaskCompletedResponse>("api/php/taskCompleted.php", new TaskCompletedRequest
                {
                    m_userName = account.UserName!,
                    m_questID = 2,
                    m_taskID = 17
                });
                Assert.Equal(TaskCompletedResponse.RESPONSE_OK, r.m_responseCode);
                Assert.Equal(TaskCompletedResponse.RES_TASK_COMPLETE, r.m_result);
            }
            
            // we found the key, it should be gone
            questData = await client.GetFormAsync<GetQuestDataResponse>("api/php/getQuestData.php");
            Assert.Empty(questData.m_itemList);
            
            // the intermediate tasks don't matter...
            
            var missionList = await client.GetFormAsync<MissionList>("api/php/getMissionList.php");
            Assert.DoesNotContain(missionList.m_completed.Split('|'), x => x == "1");
            
            // complete mission
            {
                var r = await client.PostSimpleFormAsync<TaskCompletedRequest, TaskCompletedResponse>("api/php/taskCompleted.php", new TaskCompletedRequest
                {
                    m_userName = account.UserName!,
                    m_questID = 2,
                    m_taskID = 21
                });
                Assert.Equal(TaskCompletedResponse.RESPONSE_OK, r.m_responseCode);
                Assert.Equal(TaskCompletedResponse.RES_QUEST_COMPLETE, r.m_result);
                Assert.NotEmpty(r.m_itemName);
            }
            
            missionList = await client.GetFormAsync<MissionList>("api/php/getMissionList.php");
            Assert.Contains(missionList.m_completed.Split('|'), x => x == "1");
            
            // restart mission
            {
                var r = await client.PostSimpleFormAsync<TaskCompletedRequest, TaskCompletedResponse>("api/php/taskCompleted.php", new TaskCompletedRequest
                {
                    m_userName = account.UserName!,
                    m_questID = 2,
                    m_taskID = 16
                });
                Assert.Equal(TaskCompletedResponse.RESPONSE_OK, r.m_responseCode);
                Assert.Equal(TaskCompletedResponse.RES_TASK_COMPLETE, r.m_result);
                Assert.NotEmpty(r.m_deletedTasks);
            }
        }
    }
}