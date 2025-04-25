using BinWeevils.Protocol.XmlMessages;
using StackXML;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        private void HandleSfsLoadBuddyList(ReadOnlySpan<char> body)
        {
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().m_userActor, new SocketActor.LoadBuddyListRequest());
        }
        
        private void HandleSfsAddBuddy(ReadOnlySpan<char> body)
        {
            var addBuddy = XmlReadBuffer.ReadStatic<AddBuddyRequest>(body, CDataMode.Off);
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().m_userActor, addBuddy);
        }
        
        private void HandleSfsBuddyPermission(ReadOnlySpan<char> body)
        {
            var buddyPermission = XmlReadBuffer.ReadStatic<BuddyPermissionResponse>(body, CDataMode.Off);
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().m_userActor, buddyPermission);
        }
        
        private void HandleSfsSetBuddyVars(ReadOnlySpan<char> body)
        {
            var setBuddyVars = XmlReadBuffer.ReadStatic<SetBuddyVarsRequest>(body);
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().m_userActor, setBuddyVars);
        }
        
        private void HandleSfsFindBuddy(ReadOnlySpan<char> body)
        {
            var findBuddy = XmlReadBuffer.ReadStatic<FindBuddyRequest>(body);
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().m_userActor, findBuddy);
        }
        
        private void HandleSfsRemoveBuddy(ReadOnlySpan<char> body)
        {
            var removeBuddy = XmlReadBuffer.ReadStatic<RemoveBuddyBody>(body, CDataMode.Off);
            m_services.GetActorSystem().Root.Send(GetUser().GetUserData<WeevilData>().m_userActor, removeBuddy);
        }
    }
}