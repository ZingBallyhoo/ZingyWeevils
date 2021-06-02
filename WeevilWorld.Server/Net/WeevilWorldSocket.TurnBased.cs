using System.IO;
using System.Threading.Tasks;
using ArcticFox.Net.Event;
using ArcticFox.SmartFoxServer;
using WeevilWorldProtobuf.Enums;
using WeevilWorldProtobuf.Notifications;
using WeevilWorldProtobuf.Objects;
using WeevilWorldProtobuf.Requests;
using WeevilWorldProtobuf.Responses;

namespace WeevilWorld.Server.Net
{
    public partial class WeevilWorldSocket
    {
        private async ValueTask HandleGameInviteRequest(TurnbasedInvite invite)
        {
            var user = GetUser();
            var weevilData = user.GetWeevilData();
            await CancelPendingGameInvite(weevilData);

            if (invite.GameID == 6)
            {
                await SendPairsDoesntWorkNotification(user);
                return;
            }

            if (invite.HasInviteIdx)
            {
                await SendGameInviteToUser(weevilData, invite);
                return;
            }

            weevilData.m_currentlyInvitedUser = 0; // room

            var room = await user.GetRoom();
            var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
            await broadcaster.Broadcast(PacketIDs.TURN_BASED_GAME_ADVERT_NOTIFICATION,
                new TurnbasedGameAdvert
                {
                    GameID = invite.GameID,
                    Weevil = weevilData.m_object
                });
        }

        private async ValueTask HandleGameInviteAcceptRequest(
            WeevilWorldProtobuf.Requests.TurnbasedAcceptInvitation request)
        {
            var user = GetUser();

            var invitingUser = await user.m_zone.GetUser((ulong) request.Idx);
            if (invitingUser == null) return;
            var invitingWeevilData = invitingUser.GetWeevilData();

            var invitedUserID = invitingWeevilData.GetInvitedPlayer();
            if (invitedUserID != null && invitedUserID.Value != user.m_id)
            {
                return;
            }

            if (invitedUserID == null && !invitingWeevilData.IsRoomInvited())
            {
                // no invite at all
                return;
            }

            await CancelPendingGameInvite(invitingWeevilData, false);

            var gameRoom = await user.m_zone.CreateRoom(new RoomDescription
            {
                m_isTemporary = true,
                m_name = RoomTypeIDs.GenerateRoomName(WeevilWorldSocketHost.TURN_BASED_GAME_ROOM_TYPE),
                m_type = WeevilWorldSocketHost.TURN_BASED_GAME_ROOM_TYPE,
                m_maxUsers = 2
            });
            var gameRoomData = new GameData(gameRoom);
            gameRoom.SetData(gameRoomData);

            await user.Broadcast(PacketIDs.TURN_BASED_INVITE_ACCEPT_RESPONSE,
                new WeevilWorldProtobuf.Responses.TurnbasedAcceptInvitation
                {
                    Std = new StdResponse
                    {
                        Result = ResultType.Ok
                    },
                    SessionID = gameRoomData.m_sessionID
                });
            await invitingUser.Broadcast(PacketIDs.TURN_BASED_GAME_INVITE_ACCEPTED_NOTIFICATION,
                new TurnbasedGameInvitationAccepted
                {
                    Weevil = user.GetWeevil(),
                    SessionID = gameRoomData.m_sessionID
                });


            await user.MoveTo(gameRoom);
            await invitingUser.MoveTo(gameRoom);

            await gameRoomData.StartFirstTurn();
        }

        private async ValueTask HandleGameUpdateStateRequest(TurnbasedUpdateState request)
        {
            var user = GetUser();
            var gameRoom = await user.GetRoomOrNull(WeevilWorldSocketHost.TURN_BASED_GAME_ROOM_TYPE);
            if (gameRoom == null) return;
            var gameRoomData = gameRoom.GetData<GameData>();

            if (gameRoomData.m_sessionID != request.SessionID) throw new InvalidDataException("wrong session id");

            await gameRoomData.UpdateState(user, request.StateData);
        }

        private async ValueTask HandleGameEndTurnRequest(WeevilWorldProtobuf.Requests.TurnbasedEndTurn request)
        {
            var user = GetUser();
            var gameRoom = await user.GetRoom(WeevilWorldSocketHost.TURN_BASED_GAME_ROOM_TYPE);
            var gameRoomData = gameRoom.GetData<GameData>();

            if (gameRoomData.m_sessionID != request.SessionID) throw new InvalidDataException("wrong session id");

            await gameRoomData.EndTurn(user, request);
        }

        private async ValueTask HandleGameLeaveRequest(TurnbasedLeave request)
        {
            var user = GetUser();
            var gameRoom = await user.GetRoom(WeevilWorldSocketHost.TURN_BASED_GAME_ROOM_TYPE);
            var gameRoomData = gameRoom.GetData<GameData>();

            if (gameRoomData.m_sessionID != request.SessionID) throw new InvalidDataException("wrong session id");

            await gameRoomData.LeaveGame(user);
        }

        private async ValueTask CancelPendingGameInvite(WeevilData weevilData, bool cancelDirect = true)
        {
            var user = weevilData.m_user;

            var invitedUserID = weevilData.GetInvitedPlayer();
            if (invitedUserID != null && cancelDirect)
            {
                var invitedUser = await user.m_zone.GetUser(invitedUserID.Value);
                if (invitedUser == null) return;

                await invitedUser.Broadcast(PacketIDs.TURN_BASED_GAME_INVITE_CANCELLED_NOTIFICATION,
                    new TurnbasedGameInvitationCancelled
                    {
                        Weevil = weevilData.m_object
                    });
            } else if (weevilData.IsRoomInvited())
            {
                var room = await user.GetRoom();
                var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                await broadcaster.Broadcast(PacketIDs.TURN_BASED_GAME_ADVERT_CANCELLED_NOTIFICATION,
                    new TurnbasedGameAdvertCancelled
                    {
                        Weevil = weevilData.m_object
                    });
            }

            weevilData.RemoveInvite();
        }

        private async ValueTask HandleGameInviteDeclineRequest(
            WeevilWorldProtobuf.Requests.TurnbasedDeclineInvitation request)
        {
            var user = GetUser();

            var inviter = await user.m_zone.GetUser((ulong) request.Idx);
            if (inviter == null) return;

            var inviterWeevilData = inviter.GetWeevilData();
            var invitedUser = inviterWeevilData.GetInvitedPlayer();
            if (invitedUser == null || invitedUser.Value != user.m_id) return;
            inviterWeevilData.RemoveInvite();

            await user.Broadcast(PacketIDs.TURN_BASED_GAME_INVITE_DECLINE_RESPONSE,
                new WeevilWorldProtobuf.Responses.TurnbasedDeclineInvitation
                {
                    Std = new StdResponse
                    {
                        Result = ResultType.Ok
                    },
                    Idx = request.Idx
                });
            await inviter.Broadcast(PacketIDs.TURN_BASED_GAME_INVITE_DECLINED_NOTIFICATION,
                new TurnbasedGameInvitationDeclined
                {
                    Weevil = user.GetWeevil()
                });
        }

        private static async ValueTask SendGameInviteToUser(WeevilData weevilData, TurnbasedInvite invite)
        {
            var us = weevilData.m_user;
            var weevil = weevilData.m_object;

            var idxToInvite = (ulong) invite.InviteIdx;
            var userToInvite = await us.m_zone.GetUser(idxToInvite);
            if (userToInvite == null) return;
            var userToInviteExistingGame =
                await userToInvite.GetRoomOrNull(WeevilWorldSocketHost.TURN_BASED_GAME_ROOM_TYPE);

            if (userToInviteExistingGame != null)
            {
                await us.Broadcast(PacketIDs.TURN_BASED_GAME_INVITED_RESPONSE, new TurnbasedInvited
                {
                    Std = new StdResponse
                    {
                        Result = ResultType.Error,
                        ErrorCode = 4
                    }
                });
                return;
            }

            weevilData.m_currentlyInvitedUser = idxToInvite;

            await userToInvite.Broadcast(PacketIDs.TURN_BASED_GAME_INVITE_NOTIFICATION,
                new TurnbasedGameInvitation
                {
                    GameID = invite.GameID,
                    Weevil = weevil
                });
            await us.Broadcast(PacketIDs.TURN_BASED_GAME_INVITED_RESPONSE, new TurnbasedInvited
            {
                Std = new StdResponse
                {
                    Result = ResultType.Ok
                }
            });
        }

        private static async ValueTask SendPairsDoesntWorkNotification(User user)
        {
            const int senderIdx = 223618381; // "Alfa" character? i don't know for sure
            await user.Broadcast(PacketIDs.NEWCONVERSATION_RESPONSE, new ConversationNew
            {
                Std = new StdResponse
                {
                    Result = ResultType.Ok
                },
                Idx = senderIdx,
                ConversationId = 1
            });
            await user.Broadcast(PacketIDs.BUDDYMESSAGE_NOTIFICATION, new BuddyMessage
            {
                Idx = senderIdx,
                ConversationID = 1,
                Body = "",
                Weevil = new Weevil
                {
                    Name = "Pairs doesn't work, sorry",
                    Idx = senderIdx
                }
            });
            await user.Broadcast(PacketIDs.TURN_BASED_GAME_INVITE_ACCEPTED_NOTIFICATION,
                new TurnbasedGameInvitationAccepted()); // :tf: game "accepted"
        }
    }
}