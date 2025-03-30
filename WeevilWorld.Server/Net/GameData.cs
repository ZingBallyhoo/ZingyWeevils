using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArcticFox.Net.Event;
using ArcticFox.Net.Util;
using ArcticFox.SmartFoxServer;
using Google.Protobuf;
using WeevilWorld.Protocol;
using WeevilWorldProtobuf.Enums;
using WeevilWorldProtobuf.Notifications;
using WeevilWorldProtobuf.Objects;
using WeevilWorldProtobuf.Requests;
using WeevilWorldProtobuf.Responses;

namespace WeevilWorld.Server.Net
{
    public record GamePlayer(User m_user, int m_number);
    
    public class GameData : IRoomEventHandler
    {
        private readonly Room m_room;
        private readonly AsyncLockedAccess<Dictionary<int, GamePlayer>> m_gamePlayers;
        private int m_currentTurnPlayer;

        public long m_sessionID => (long)m_room.m_id;

        public GameData(Room room)
        {
            m_room = room;
            m_gamePlayers = new AsyncLockedAccess<Dictionary<int, GamePlayer>>(new Dictionary<int, GamePlayer>());
        }

        private GamePlayer? FindPlayer(User user, AsyncLockedAccess<Dictionary<int, GamePlayer>>.Token players)
        {
            foreach (var player in players.m_value)
            {
                if (player.Value.m_user.m_id != user.m_id) continue;
                return player.Value;
            }
            return null;
        }

        public async ValueTask UserJoinedRoom(Room room, User user)
        {
            using var players = await m_gamePlayers.Get();

            var existingPlayer = FindPlayer(user, players);
            if (existingPlayer != null) throw new ArgumentException($"player {user.m_name} already in game");

            foreach (var otherPlayerPair in players.m_value)
            {
                await user.Broadcast(PacketIDs.TURN_BASED_GAME_PLAYER_JOINED_NOTIFICATION, 
                    CreateJoinedNotification(otherPlayerPair.Value));
            }
            
            var newPlayer = new GamePlayer(user, players.m_value.Count);
            players.m_value.Add(newPlayer.m_number, newPlayer);

            await m_room.Broadcast(PacketIDs.TURN_BASED_GAME_PLAYER_JOINED_NOTIFICATION,
                CreateJoinedNotification(newPlayer));
        }

        public async ValueTask StartFirstTurn()
        {
            using var players = await m_gamePlayers.Get();

            var firstTurn = Random.Shared.Next(0, 2);
            m_currentTurnPlayer = firstTurn;

            await StartTurn(firstTurn, players);
        }

        private ValueTask StartTurn(int playerIdx, AsyncLockedAccess<Dictionary<int, GamePlayer>>.Token players)
        {
            return m_room.Broadcast(PacketIDs.TURN_BASED_GAME_PLAYER_TURN_NOTIFICATION,
                new TurnbasedPlayerTurn
                {
                    SessionID = m_sessionID,
                    PlayerNumber = playerIdx,
                    Weevil = players.m_value[playerIdx].m_user.GetWeevil()
                });
        }

        private TurnbasedPlayerJoined CreateJoinedNotification(GamePlayer player)
        {
            return new TurnbasedPlayerJoined
            {
                SessionID = m_sessionID,
                PlayerNumber = player.m_number,
                Weevil = player.m_user.GetWeevil(),
                TallyOverall = new TurnbasedGameTally(),
                TallyPvP = new TurnbasedGameTally()
            };
        }

        public async ValueTask UserLeftRoom(Room room, User user)
        {
            using var players = await m_gamePlayers.Get();
            
            var playerInGame = FindPlayer(user, players);
            if (playerInGame == null) return;

            players.m_value.Remove(playerInGame.m_number);
        }

        public async ValueTask UpdateState(User user, ByteString stateData)
        {
            using var players = await m_gamePlayers.Get();
            var player = FindPlayer(user, players);
            if (player == null || player.m_number != m_currentTurnPlayer) return; // don't throw, i think client does this

            var broadcaster = new FilterBroadcaster<User>(m_room.m_userExcludeFilter, user);
            await broadcaster.Broadcast(PacketIDs.TURN_BASED_GAME_STATE_UPDATE_NOTIFICATION,
                new TurnbasedStateUpdate
                {
                    StateData = stateData,
                    SessionID = m_sessionID
                });
        }

        public async ValueTask EndTurn(User user, WeevilWorldProtobuf.Requests.TurnbasedEndTurn request)
        {
            using (var players = await m_gamePlayers.Get())
            {
                var player = FindPlayer(user, players);

                var awardedMulch = 0;
                var awardedDosh = 0;
                if (request.Finished)
                {
                    var endCondition = (GameEndCondition)request.Value;
                    if (endCondition == GameEndCondition.Win)
                    {
                        awardedMulch = 100;
                        awardedDosh = 5;
                    } 
                    // todo: i would like to do this but the client doesn't handle this
                    /*else if (endCondition == GameEndCondition.Draw)
                    {
                        awardedMulch = 25;
                        awardedDosh = 2;
                    }*/
                }
                
                await user.Broadcast(PacketIDs.TURN_BASED_GAME_END_TURN_RESPONSE, new WeevilWorldProtobuf.Responses.TurnbasedEndTurn
                {
                    SessionID = m_sessionID,
                    AwardedMulch = awardedMulch,
                    AwardedDosh = awardedDosh,
                    Std = new StdResponse
                    {
                        Result = ResultType.Ok
                    }
                });

                if (player != null && player.m_number == m_currentTurnPlayer && !request.Finished)
                {
                    if (m_currentTurnPlayer == 0) m_currentTurnPlayer = 1;
                    else m_currentTurnPlayer = 0;

                    await StartTurn(m_currentTurnPlayer, players);
                    return;
                }

                if (!request.Finished) return; // non current player also sends this for finish
            }
            
            await m_room.Broadcast(PacketIDs.TURN_BASED_GAME_ENDED_NOTIFICATION,
                new TurnbasedGameEnded
                {
                    SessionID = m_sessionID
                });
            await user.RemoveFromRoom(WeevilWorldSocketHost.TURN_BASED_GAME_ROOM_TYPE); // needs lock released...
        }

        public async Task LeaveGame(User user)
        {
            await user.RemoveFromRoom(WeevilWorldSocketHost.TURN_BASED_GAME_ROOM_TYPE);

            await user.Broadcast(PacketIDs.TURN_BASED_GAME_LEFT_RESPONSE, 
                new TurnbasedLeft // todo: client doesn't even use
                {
                    Std = new StdResponse
                    {
                        Result = ResultType.Ok
                    },
                    Quitter = true,
                    SessionID = m_sessionID
                });
            
            await m_room.Broadcast(PacketIDs.TURN_BASED_GAME_PLAYER_LEFT_NOTIFICATION, new DummyRequest());
        }
        
        private enum GameEndCondition
        {
            Lose = 0,
            Draw = 1,
            Win = 2
        }
    }
}