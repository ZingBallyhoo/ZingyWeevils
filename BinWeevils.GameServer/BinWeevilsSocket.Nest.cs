using ArcticFox.Net.Event;
using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Rooms;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Str;
using Microsoft.Extensions.Logging;
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
                    joinLocRequest.FullyDeserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var us = GetUser();
                        var room = await us.GetRoom();
                        
                        m_services.GetLogger().LogDebug("Nest - JoinLocation: {LocID}", joinLocRequest.m_locID);
                        
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
                    outgoingInvite.FullyDeserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var us = GetUser();
                        if (us.m_name == outgoingInvite.m_userName) throw new InvalidDataException("trying to invite self");
                        
                        m_services.GetLogger().LogDebug("Nest - InviteToNest: {Name}", outgoingInvite.m_userName);
                        
                        var nest = us.GetUserData<WeevilData>().GetNestAddress();
                        m_services.GetActorSystem().Root.Send(nest, outgoingInvite);
                    });
                    break;
                }
                case Modules.NEST_REMOVE_GUESTS: // 5#4
                {
                    var removeGuests = new NestInvite();
                    removeGuests.FullyDeserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var us = GetUser();
                        if (us.m_name == removeGuests.m_userName) throw new InvalidDataException("trying remove invite from self");
                        
                        m_services.GetLogger().LogDebug("Nest - RemoveGuests: {Name}", removeGuests.m_userName);
                        
                        var nest = us.GetUserData<WeevilData>().GetNestAddress();
                        if (removeGuests.m_userName == NestInvite.ALL_GUESTS)
                        {
                            m_services.GetActorSystem().Root.Send(nest, new NestActor.RemoveAllGuests());
                        } else
                        {
                            m_services.GetActorSystem().Root.Send(nest, new NestActor.RemoveGuest(removeGuests.m_userName!));
                        }
                    });
                    break;
                }
                case Modules.NEST_GUEST_JOINED_NEST: // 5#5
                {
                    var guestJoined = new NestGuestJoined();
                    guestJoined.FullyDeserialize(ref reader);
                    
                    var us = GetUser();
                    if (us.m_name == guestJoined.m_name) throw new InvalidDataException("trying to be a guest in own nest");
                    
                    m_services.GetLogger().LogDebug("Nest - GuestJoinedNest: {HostName} {Joined}", guestJoined.m_name, guestJoined.m_joined);
                    
                    // we don't actually need to do anything.
                    // the actor handles enter/leave events
                    break;
                }
                case Modules.NEST_DENY_NEST_INVITE: // 5#6
                {
                    var deniedInvite = new NestInvite();
                    deniedInvite.FullyDeserialize(ref reader);
                    
                    m_taskQueue.Enqueue(async () =>
                    {
                        var us = GetUser();
                        if (us.m_name == deniedInvite.m_userName) throw new InvalidDataException("trying to be a deny invite to own nest");
                        
                        m_services.GetLogger().LogDebug("Nest - DenyNestInvite: {HostName}", deniedInvite.m_userName);
                        
                        var nestOwner = await us.m_zone.GetUser(deniedInvite.m_userName!);
                        if (nestOwner == null) return;
                        
                        var nest = nestOwner.GetUserData<WeevilData>().GetNestAddress();
                        m_services.GetActorSystem().Root.Send(nest, new NestActor.DenyNestInvite(us.m_name));
                    });
                    break;
                }
                default:
                {
                    m_services.GetLogger().LogWarning("Unknown nest command: {Command} - {Args}", message.m_command.ToString(), string.Join(" ", reader.ReadToEnd()));
                    break;
                }
            }
        }
    }
}