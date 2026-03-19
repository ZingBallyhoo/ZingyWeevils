using BinWeevils.Protocol.Form.Puzzle;
using BinWeevils.Protocol.Str;

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
        [InlineData(1)]
        [InlineData(-1)]
        public async Task CompleteWordSearch(int spanSign)
        {
            var account = await m_factory.CreateAccount($"{nameof(CompleteWordSearch)} {spanSign}");
            m_factory.SetAccount(account.UserName!);
            
            var client = m_factory.CreateClient();

            var progressReq = new GetPuzzleProgressRequest
            {
                m_userID = account.UserName!,
                m_gridID = 254
            };
            var progressResp = await client.PostSimpleFormAsync<GetPuzzleProgressRequest, GetWordSearchProgressResponse>("api/php/getWordSearchProgress.php", progressReq);
            Assert.Equal("0", progressResp.m_result.ToString());

            var saveRequest = new SaveWordSearchProgressRequest
            {
                m_userID = account.UserName!,
                m_gridID = progressReq.m_gridID,
                m_progress = new WordSearchProgress(),
                m_completed = false
            };

            WordSearchSpan[] spans =
            [
                CreateSpan(0, 0, 15, 0, spanSign),
                CreateSpan(0, 0, 0, 15, spanSign),
                CreateSpan(0, 0, 15, 15, spanSign),
                CreateSpan(15, 0, 0, 15, spanSign)
            ];

            foreach (var span in spans)
            {
                saveRequest.m_progress.m_spans.Add(span);
            
                var saveResponse = await client.PostSimpleFormAsync<SaveWordSearchProgressRequest, SaveWordSearchProgressResponse>("api/php/saveWordSearchProgress.php", saveRequest);
                Assert.NotEqual(0, saveResponse.m_mulch);
            
                progressResp = await client.PostSimpleFormAsync<GetPuzzleProgressRequest, GetWordSearchProgressResponse>("api/php/getWordSearchProgress.php", progressReq);
                Assert.Equal(saveRequest.m_progress.m_spans.Count, progressResp.m_result.m_spans.Count);
            }
            
            Assert.Equal(spans.Length, progressResp.m_result.m_spans.Count);
            
            var listResponse = await client.PostSimpleFormAsync<GetPuzzleListRequest, GetPuzzleListResponse>("api/php/getPuzzleList.php", new GetPuzzleListRequest
            {
                m_userID = account.UserName!,
                m_typeID = PuzzleTypeID.WordSearch
            });
            Assert.Single(listResponse.m_completedList.Split('|'), "1");
        }

        private static WordSearchSpan CreateSpan(byte iStart, byte jStart, byte iEnd, byte jEnd, int sign)
        {
            return sign switch
            {
                1 => new WordSearchSpan { m_iStart = iStart, m_jStart = jStart, m_iEnd = iEnd, m_jEnd = jEnd },
                -1 => new WordSearchSpan { m_iStart = iEnd, m_jStart = jEnd, m_iEnd = iStart, m_jEnd = jStart },
                _ => throw new InvalidDataException($"invalid span sign: {sign}")
            };
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
            Assert.NotEqual(SaveCrosswordProgressResponse.RESULT_COMPLETED, saveResponse.m_result);
            Assert.Equal(0, saveResponse.m_mulch);
            Assert.Equal(0u, saveResponse.m_xp);
            
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
            Assert.Equal(SaveCrosswordProgressResponse.RESULT_COMPLETED, saveResponse.m_result);
            Assert.NotEqual(0, saveResponse.m_mulch);
            Assert.NotEqual(0u, saveResponse.m_xp);
            
            // second time completing will be ignored
            saveResponse = await client.PostSimpleFormAsync<SaveCrosswordProgressRequest, SaveCrosswordProgressResponse>("api/php/saveCrosswordProgress.php", saveRequest);
            Assert.NotEqual(SaveCrosswordProgressResponse.RESULT_COMPLETED, saveResponse.m_result);
            Assert.Equal(0, saveResponse.m_mulch);
            Assert.Equal(0u, saveResponse.m_xp);
            
            getResp = await client.PostSimpleFormAsync<GetPuzzleProgressRequest, GetCrosswordProgressResponse>("api/php/getCrosswordProgress.php", new GetPuzzleProgressRequest
            {
                m_userID = account.UserName!,
                m_gridID = puzzleID
            });
            Assert.True(getResp.m_completed);
            
            var listResponse = await client.PostSimpleFormAsync<GetPuzzleListRequest, GetPuzzleListResponse>("api/php/getPuzzleList.php", new GetPuzzleListRequest
            {
                m_userID = account.UserName!,
                m_typeID = PuzzleTypeID.Crossword
            });
            Assert.Single(listResponse.m_completedList.Split('|'), "1");
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