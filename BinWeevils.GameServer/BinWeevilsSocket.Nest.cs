using ArcticFox.Net.Event;
using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using StackXML.Str;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket
    {
        private void HandleNestCommand(in XtClientMessage message, ref StrReader reader)
        {
            switch (message.m_command)
            {
                case Modules.NEST_JOIN_LOCATION: // 5#2
                {
                    var joinLocRequest = new NestJoinLocRequest();
                    joinLocRequest.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var us = GetUser();
                        var room = await us.GetRoom();
                        
                        us.GetUserData<WeevilData>().m_locID.SetValue(joinLocRequest.m_locID);
                        
                        var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, us);
                        await broadcaster.BroadcastXtStr(Modules.NEST_JOIN_LOCATION, new NestJoinedLocNotification
                        {
                            m_userID = checked((int)us.m_id),
                            m_body = joinLocRequest
                        });
                    });
                    break;
                }
                case Modules.NEST_INVITE_TO_NEST: // 5#3
                {
                    var outgoingInvite = new NestInvite();
                    outgoingInvite.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var us = GetUser();
                        if (us.m_name == outgoingInvite.m_userName) throw new InvalidDataException("trying to invite self");
                        
                        var otherUser = await us.m_zone.GetUser(outgoingInvite.m_userName!);
                        if (otherUser == null) return;
                        
                        await otherUser.BroadcastXtStr(Modules.NEST_INVITE_TO_NEST, new NestInvite
                        {
                            m_userName = us.m_name
                        });
                    });
                    break;
                }
                case Modules.NEST_GUEST_JOINED_NEST: // 5#5
                {
                    var guestJoined = new NestGuestJoined();
                    guestJoined.Deserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var us = GetUser();
                        if (us.m_name == guestJoined.m_name) throw new InvalidDataException("trying to be a guest in own nest");
                        
                        var nestOwner = await us.m_zone.GetUser(guestJoined.m_name!);
                        if (nestOwner == null) return;
                        
                        await nestOwner.BroadcastXtStr(Modules.NEST_GUEST_JOINED_NEST, guestJoined with
                        {
                            m_name = us.m_name
                        });
                    });
                    break;
                }
                default:
                {
                    Console.Out.WriteLine($"unknown command (nest): {message.m_command}");
                    break;
                }
            }
        }
    }
}