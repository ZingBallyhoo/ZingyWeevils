using BinWeevils.Protocol.Form.Puzzle;

namespace BinWeevils.Tests.Integration
{
    [Collection("Integration")]
    public class PuzzleTests
    {
        private readonly IntegrationAppFactory m_factory;

        public PuzzleTests(IntegrationAppFactory factory)
        {
            m_factory = factory;
        }
        
        [Theory]
        [InlineData(91, ".....S...........PILOT....S.W.C..R...AUTUMN.H.A.BARK.M....O.E.N..N...P..C.W.E.D..K......O.F.L.L.......SUNGLASSES......A.E.A...S...SEVEN...K........A..D...E........S..W...S........T..I.S.............CROWN...........H.C...............K...............S.......")]
        public async Task CompleteCrossword(byte puzzleID, string completedProgress)
        {
            var account = await m_factory.CreateAccount(nameof(CompleteCrossword));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            
            var getResp = await client.PostSimpleFormAsync<GetPuzzleProgressRequest, GetCrosswordProgressResponse>("api/php/getCrosswordProgress.php", new GetPuzzleProgressRequest
            {
                m_userID = account.UserName!,
                m_gridID = puzzleID
            });
            Assert.Equal("0", getResp.m_progress);
            Assert.False(getResp.m_completed);

            var saveRequest = new SaveCrosswordProgressRequest
            {
                m_userID = account.UserName!,
                m_gridID = puzzleID,
                m_completed = false,
                m_progress = completedProgress
            };
            var saveResponse = await client.PostSimpleFormAsync<SaveCrosswordProgressRequest, SaveCrosswordProgressResponse>("api/php/saveCrosswordProgress.php", saveRequest);
            // todo: no fields yet
            
            getResp = await client.PostSimpleFormAsync<GetPuzzleProgressRequest, GetCrosswordProgressResponse>("api/php/getCrosswordProgress.php", new GetPuzzleProgressRequest
            {
                m_userID = account.UserName!,
                m_gridID = puzzleID
            });
            Assert.Equal(completedProgress, getResp.m_progress);
            Assert.False(getResp.m_completed);

            // "check answers" clicked
            saveRequest.m_completed = true;
            saveResponse = await client.PostSimpleFormAsync<SaveCrosswordProgressRequest, SaveCrosswordProgressResponse>("api/php/saveCrosswordProgress.php", saveRequest);
            Assert.NotEqual(0, saveResponse.m_mulch);
            Assert.NotEqual(0u, saveResponse.m_xp);
            Assert.Equal(SaveCrosswordProgressResponse.RESULT_COMPLETED, saveResponse.m_result);
            
            // second time completing will be ignored
            saveResponse = await client.PostSimpleFormAsync<SaveCrosswordProgressRequest, SaveCrosswordProgressResponse>("api/php/saveCrosswordProgress.php", saveRequest);
            Assert.Equal(0, saveResponse.m_mulch);
            Assert.Equal(0u, saveResponse.m_xp);
            Assert.NotEqual(SaveCrosswordProgressResponse.RESULT_COMPLETED, saveResponse.m_result);
            
            getResp = await client.PostSimpleFormAsync<GetPuzzleProgressRequest, GetCrosswordProgressResponse>("api/php/getCrosswordProgress.php", new GetPuzzleProgressRequest
            {
                m_userID = account.UserName!,
                m_gridID = puzzleID
            });
            Assert.True(getResp.m_completed);
        }

        [Theory]
        [InlineData(91, "hellooo")] // valid puzzle, but invalid data
        [InlineData(254, "hellooo")] // invalid puzzle
        public async Task CantSaveInvalidCrossword(byte puzzleID, string progress)
        {
            var account = await m_factory.CreateAccount(nameof(CantSaveInvalidCrossword));
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();
            
            var saveRequest = new SaveCrosswordProgressRequest
            {
                m_userID = account.UserName!,
                m_gridID = puzzleID,
                m_completed = false,
                m_progress = progress
            };
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
            {
                await client.PostSimpleFormAsync<SaveCrosswordProgressRequest, SaveCrosswordProgressResponse>("api/php/saveCrosswordProgress.php", saveRequest);
            });
        }
    }
}