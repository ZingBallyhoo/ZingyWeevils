using System.Net.Mime;
using BinWeevils.Common.Database;
using BinWeevils.Protocol.Form;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api")]
    public class IgnoreListController : Controller
    {
        private readonly WeevilDBContext m_dbContext;
        
        public IgnoreListController(WeevilDBContext dbContext)
        {
            m_dbContext = dbContext;
        }
        
        [StructuredFormPost("php/getIgnoreList.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task<GetIgnoreListResponse> GetIgnoreList([FromBody] GetIgnoreListRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("IgnoreListController.GetIgnoreList");
            activity?.SetTag("userID", request.m_userID);
            
            if (request.m_userID != ControllerContext.HttpContext.User.Identity!.Name)
            {
                throw new InvalidDataException("trying to get somebody else's ignore list");
            }
            
            var blockedNames = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_userID)
                .SelectMany(x => x.m_ignoredWeevils)
                .Select(x => x.m_ignoredWeevil.m_name)
                .ToListAsync();
            
            return new GetIgnoreListResponse
            {
                m_resultNames = blockedNames
            };
        }
        
        [StructuredFormPost("weevil/add-ignore-list")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task IgnoreUser([FromBody] IgnoreUnignoreUserRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("IgnoreListController.IgnoreUser");
            activity?.SetTag("userName", request.m_userName);

            if (request.m_userName == ControllerContext.HttpContext.User.Identity!.Name)
            {
                throw new InvalidDataException("trying to ignore self");
            }
            
            await using var transaction = await m_dbContext.Database.BeginTransactionAsync();
            
            var self = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Select(x => new
                {
                    x.m_idx
                })
                .SingleAsync();
                
            var userToIgnore = await m_dbContext.m_weevilDBs
                .Where(x => x.m_name == request.m_userName)
                .Select(x => new
                {
                    x.m_idx
                })
                .SingleOrDefaultAsync();
            if (userToIgnore == null)
            {
                throw new InvalidDataException("user to block does not exist");
            }
            
            await m_dbContext.m_ignoreRecords.AddAsync(new IgnoreRecordDB
            {
                m_forWeevilIdx = self.m_idx,
                m_ignoredWeevilIdx = userToIgnore.m_idx,
            });
            await m_dbContext.SaveChangesAsync();
            
            var ignoredCount = await m_dbContext.m_ignoreRecords.CountAsync(x => x.m_forWeevilIdx == self.m_idx);
            if (ignoredCount > 50)
            {
                // todo: config?
                throw new InvalidDataException("too many ignored users");
            }
            
            await transaction.CommitAsync();
        }
        
        [StructuredFormPost("weevil/remove-ignore-list")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public async Task UnignoreUser([FromBody] IgnoreUnignoreUserRequest request)
        {
            using var activity = ApiServerObservability.StartActivity("IgnoreListController.UnignoreUser");
            activity?.SetTag("userName", request.m_userName);
            
            var rowsDeleted = await m_dbContext.m_ignoreRecords
                .Where(x => x.m_forWeevil.m_name == ControllerContext.HttpContext.User.Identity!.Name)
                .Where(x => x.m_ignoredWeevil.m_name == request.m_userName)
                .ExecuteDeleteAsync();
            
            if (rowsDeleted == 0)
            {
                throw new InvalidDataException("user wasn't ignored anyway");
            }
        }
    }
}