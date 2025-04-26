using BinWeevils.GameServer.Actors;
using BinWeevils.Protocol.XmlMessages;
using Microsoft.Extensions.Logging;
using StackXML;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        private void HandleSfsLoadBuddyList(ReadOnlySpan<char> body)
        {
            m_services.GetLogger().LogDebug("Buddy: Load List");
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().GetUserAddress(), new BuddyListActor.LoadBuddyListRequest());
        }
        
        private void HandleSfsAddBuddy(ReadOnlySpan<char> body)
        {
            var addBuddy = XmlReadBuffer.ReadStatic<AddBuddyRequest>(body, CDataMode.Off);
            m_services.GetLogger().LogDebug("Buddy - Request Add Buddy: {Name}", addBuddy.m_targetName);
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().GetUserAddress(), addBuddy);
        }
        
        private void HandleSfsBuddyPermission(ReadOnlySpan<char> body)
        {
            var buddyPermission = XmlReadBuffer.ReadStatic<BuddyPermissionResponse>(body, CDataMode.Off);
            m_services.GetLogger().LogDebug("Buddy - Permission Response: {Name}", buddyPermission.m_inner.m_name);
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().GetUserAddress(), buddyPermission);
        }
        
        private void HandleSfsSetBuddyVars(ReadOnlySpan<char> body)
        {
            var setBuddyVars = XmlReadBuffer.ReadStatic<SetBuddyVarsRequest>(body);
            m_services.GetLogger().LogDebug("Buddy - Set Vars: {Vars}", setBuddyVars.m_varList.m_buddyVars);
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().GetUserAddress(), setBuddyVars);
        }
        
        private void HandleSfsFindBuddy(ReadOnlySpan<char> body)
        {
            var findBuddy = XmlReadBuffer.ReadStatic<FindBuddyRequest>(body);
            m_services.GetLogger().LogDebug("Buddy - Locate By ID: {ID}", findBuddy.m_record.m_id);
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().GetUserAddress(), findBuddy);
        }
        
        private void HandleSfsRemoveBuddy(ReadOnlySpan<char> body)
        {
            var removeBuddy = XmlReadBuffer.ReadStatic<RemoveBuddyBody>(body, CDataMode.Off);
            m_services.GetLogger().LogDebug("Buddy - Remove: {Name}", removeBuddy.m_buddyName);
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().GetUserAddress(), removeBuddy);
        }
    }
}