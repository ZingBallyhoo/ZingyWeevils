using BinWeevils.Protocol.Amf;
using BinWeevils.Protocol.Enums;
using BinWeevils.Protocol.Form.Game;

namespace BinWeevils.Tests.Integration
{
    [Collection("Integration")]
    public class GamesTests
    {
        private readonly IntegrationAppFactory m_factory;

        public GamesTests(IntegrationAppFactory factory)
        {
            m_factory = factory;
        }
        
        [Fact]
        public async Task OneTimeAward()
        {
            var account = await m_factory.CreateAccount(nameof(OneTimeAward));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            
            var hasPlayed = await client.PostSimpleFormAsync<HasTheUserPlayedRequest, HasTheUserPlayedResponse>("api/game/has-the-user-played", new HasTheUserPlayedRequest
            {
                m_gameID = EGameType.SkullGame
            });
            Assert.Equal(0, hasPlayed.m_hasPlayed);
            
            var awardResp = await client.PostSimpleFormAsync<SaveGameStatsRequest, SaveGameStatsResponse>("api/game/save-game-stats", new SaveGameStatsRequest
            {
                m_gameID = EGameType.SkullGame,
                m_awardGiven = true
            });
            Assert.Equal(SaveGameStatsResponse.ERROR_SUCCESS, awardResp.m_error);
            
            hasPlayed = await client.PostSimpleFormAsync<HasTheUserPlayedRequest, HasTheUserPlayedResponse>("api/game/has-the-user-played", new HasTheUserPlayedRequest
            {
                m_gameID = EGameType.SkullGame
            });
            Assert.Equal(1, hasPlayed.m_hasPlayed);
            
            var tryingAgainResp = await client.PostFormAsync("api/game/save-game-stats", new SaveGameStatsRequest
            {
                m_gameID = EGameType.SkullGame,
                m_awardGiven = true
            });
            Assert.False(tryingAgainResp.IsSuccessStatusCode);
        }
        
        private async Task<SubmitLapTimesResponse> SubmitTimes(HttpClient client, SubmitLapTimesRequest request)
        {
            return await client.PostAmfAsync<SubmitLapTimesRequest, SubmitLapTimesResponse>(
                m_factory.GetAmfOptions(), 
                "weevilservices.cWeevilKartGameScores.submitSingleUserTimes", 
                request);
        }
        
        [Fact]
        public async Task KartGetAllTrophies()
        {
            var account = await m_factory.CreateAccount(nameof(KartGetAllTrophies));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            
            var tooSlow = await SubmitTimes(client, new SubmitLapTimesRequest
            {
                m_userID = account.UserName!,
                m_lap1 = 50,
                m_lap2 = 50,
                m_lap3 = 50,
                m_trackID = EGameType.WeevilWheelsTrack1
            });
            Assert.Equal(0, tooSlow.m_pbTotal);
            Assert.False(tooSlow.m_medalInfo.m_hasWonMedal);
            
            var response = await SubmitTimes(client, new SubmitLapTimesRequest
            {
                m_userID = account.UserName!,
                m_lap1 = 10,
                m_lap2 = 10,
                m_lap3 = 10,
                m_trackID = EGameType.WeevilWheelsTrack1
            });
            
            Assert.True(response.m_medalInfo.m_hasWonMedal);
            Assert.Equal("gold", response.m_medalInfo.m_medalType);
            Assert.Equal(50, response.m_pbLap1);
            
            var storedItems = await client.GetStoredItems(account.UserName!);
            Assert.Equal(3, storedItems.m_items.Count(x => x.m_configName == "o_wwTrophy1"));
        }
        
        [Fact]
        public async Task KartGetTrophiesInStages()
        {
            var account = await m_factory.CreateAccount(nameof(KartGetTrophiesInStages));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            
            var bronzeTime = new SubmitLapTimesRequest
            {
                m_userID = account.UserName!,
                m_lap1 = 15,
                m_lap2 = 15,
                m_lap3 = 15,
                m_trackID = EGameType.WeevilWheelsTrack1
            };
            var getBronze = await SubmitTimes(client, bronzeTime);
            var getBronzeAgain = await SubmitTimes(client, bronzeTime);
            Assert.True(getBronze.m_medalInfo.m_hasWonMedal);
            Assert.Equal("bronze", getBronze.m_medalInfo.m_medalType);
            Assert.False(getBronzeAgain.m_medalInfo.m_hasWonMedal);
            
            var storedItems = await client.GetStoredItems(account.UserName!);
            Assert.Equal(1, storedItems.m_items.Count(x => x.m_configName == "o_wwTrophy1"));
            
            var goldTime = new SubmitLapTimesRequest
            {
                m_userID = account.UserName!,
                m_lap1 = 10,
                m_lap2 = 10,
                m_lap3 = 10,
                m_trackID = EGameType.WeevilWheelsTrack1
            };
            var getGold = await SubmitTimes(client, goldTime);
            var getGoldAgain = await SubmitTimes(client, goldTime);
            Assert.True(getGold.m_medalInfo.m_hasWonMedal);
            Assert.False(getGoldAgain.m_medalInfo.m_hasWonMedal);
            
            storedItems = await client.GetStoredItems(account.UserName!);
            Assert.Equal(3, storedItems.m_items.Count(x => x.m_configName == "o_wwTrophy1"));
        }
    }
}