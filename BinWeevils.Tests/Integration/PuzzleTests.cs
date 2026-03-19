using BinWeevils.Protocol.Form;
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
            var saveIncompleteResp = await client.PostSimpleFormAsync<SaveCrosswordProgressRequest, SaveCrosswordProgressResponse>("api/php/saveCrosswordProgress.php", saveRequest);
            // todo: no fields yet
            
            getResp = await client.PostSimpleFormAsync<GetPuzzleProgressRequest, GetCrosswordProgressResponse>("api/php/getCrosswordProgress.php", new GetPuzzleProgressRequest
            {
                m_userID = account.UserName!,
                m_gridID = puzzleID
            });
            Assert.Equal(completedProgress, getResp.m_progress);
            Assert.False(getResp.m_completed);

            saveRequest.m_completed = true;
            saveIncompleteResp = await client.PostSimpleFormAsync<SaveCrosswordProgressRequest, SaveCrosswordProgressResponse>("api/php/saveCrosswordProgress.php", saveRequest);
            // todo: no fields yet
            
            getResp = await client.PostSimpleFormAsync<GetPuzzleProgressRequest, GetCrosswordProgressResponse>("api/php/getCrosswordProgress.php", new GetPuzzleProgressRequest
            {
                m_userID = account.UserName!,
                m_gridID = puzzleID
            });
            Assert.True(getResp.m_completed);
        }
    }
}