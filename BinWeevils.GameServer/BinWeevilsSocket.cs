using System.Text;
using ArcticFox.Codec;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using ArcticFox.SmartFoxServer;
using BinWeevils.GameServer.Rooms;
using BinWeevils.GameServer.Sfs;
using BinWeevils.Protocol;
using BinWeevils.Protocol.DataObj;
using BinWeevils.Protocol.Str;
using BinWeevils.Protocol.XmlMessages;
using Proto;
using StackXML;

namespace BinWeevils.GameServer
{
    public partial class BinWeevilsSocket : SmartFoxSocketBase, ISpanConsumer<char>
    {
        private readonly WeevilSocketServices m_services;
        
        public BinWeevilsSocket(SocketInterface socket, SmartFoxManager manager) : base(socket, manager)
        {
            m_services = new WeevilSocketServices(manager.m_provider);
            
            // todo: will differ for bluebox/socket...
            m_netInputCodec = new CodecChain()
                .AddCodec(new ZeroDelimitedDecodeCodec())
                .AddCodec(new TextDecodeCodec(Encoding.ASCII))
                .AddCodec(this);
        }

        public void Input(ReadOnlySpan<char> input, ref object? state)
        {
            if (input[0] == '<')
            {
                InputXml(input);
            } else if (input[0] == '%')
            {
                InputStr(input);
            } else
            {
                throw new InvalidDataException("message wasn't xml or str");
            }
        }
        
        private void InputXml(ReadOnlySpan<char> input)
        {
            if (input is "<policy-file-request/>")
            {
                this.BroadcastZeroTerminatedAscii("<cross-domain-policy><allow-access-from domain='*' to-ports='9339' /></cross-domain-policy>");
                return;
            }
            
            var preRead = new MsgPreRead();
            XmlReadBuffer.ReadIntoStatic(input, ref preRead);
            if (preRead.m_bodySpan.Length == 0)
            {
                throw new InvalidDataException($"couldn't find xml message body");
            }
            Console.Out.WriteLine(preRead.m_bodySpan.ToString());
            
            if (preRead.m_messageType is not "sys")
            {
                throw new InvalidDataException($"client sent an xml message with a type other than \"sys\": {preRead.m_messageType}");
            }
            
            var preReadBody = new MsgBodyPreRead();
            XmlReadBuffer.ReadIntoStatic(preRead.m_bodySpan, ref preReadBody);
            Console.Out.WriteLine($"{preReadBody.m_action} {preReadBody.m_room}");
            
            if (preReadBody.m_action.Length == 0 || preReadBody.m_action.IsWhiteSpace())
            {
                throw new InvalidDataException($"couldn't find xml message action");
            }

            switch (preReadBody.m_action)
            {
                case "verChk":
                {
                    HandleSfsVerCheck(preRead.m_bodySpan);
                    break;
                }
                case "login":
                {
                    HandleSfsLogin(preRead.m_bodySpan);
                    break;
                }
                case "getRmList":
                {
                    HandleSfsGetRoomList(preRead.m_bodySpan);
                    break;
                }
                case "pubMsg":
                {
                    HandleSfsPubMsg(preRead.m_bodySpan);
                    break;
                }
                default:
                {
                    Console.Out.WriteLine($"unknown action: {preReadBody.m_action}");
                    break;
                }
            }
        }
        
        private void HandleSfsVerCheck(ReadOnlySpan<char> body)
        {
            var verCheck = XmlReadBuffer.ReadStatic<VerCheckBody>(body);
            if (verCheck.m_ver.m_ver != 154)
            {
                m_taskQueue.Enqueue(() => this.BroadcastSys(new MsgBody
                {
                    m_action = "apiKO",
                    m_room = 0
                }));
            } else
            {
                m_taskQueue.Enqueue(() => this.BroadcastSys(new MsgBody
                {
                    m_action = "apiOK",
                    m_room = 0
                }));
            }
        }
        
        private void HandleSfsLogin(ReadOnlySpan<char> body)
        {
            var login = XmlReadBuffer.ReadStatic<LoginBody>(body);
                    
            m_taskQueue.Enqueue(async () =>
            {
                var weevilIdx = await m_services.CreateTempAccount(login.m_data.m_nickname);
                var loginDto = await m_services.GetLoginData(weevilIdx);
                
                var actorSystem = m_services.GetActorSystem();
                
                var user = await CreateUser(login.m_data.m_zone, login.m_data.m_nickname);
                var userProps = Props.FromProducer(() => new SocketActor
                {
                    m_user = user
                });
                var userActor = actorSystem.Root.SpawnNamed(userProps, $"{user.m_name}");
                var nestActor = await actorSystem.Root.RequestAsync<PID>(userActor, new SocketActor.CreateNest());
                
                var weevilData = new WeevilData(user, userActor, nestActor);
                weevilData.m_idx.SetValue(weevilIdx);
                weevilData.m_weevilDef.SetValue(loginDto.m_weevilDef);
                user.SetUserData(weevilData);
                
                var loginResponse = new LoginResponse
                {
                    m_commandType = "login",
                    m_success = true,
                    m_user = new LoginUser
                    {
                        m_weevilDef = $"{weevilData.m_weevilDef.GetValue()}",
                        m_ip = "no",
                        m_apparel = weevilData.m_apparel,
                        m_idx = $"{weevilData.m_idx.GetValue()}",
                        m_locale = "UK",
                        m_userID = $"{user.m_id}"
                    }
                };
                await this.BroadcastXtRes(loginResponse);
            });
        }
        
        private void HandleSfsGetRoomList(ReadOnlySpan<char> body)
        {
            m_taskQueue.Enqueue(async () =>
            {
                var roomList = new RoomList();
                
                using var rooms = await GetUser().m_zone.GetRooms();
                foreach (var room in rooms.m_value.GetRooms())
                {
                    if (room.IsGame()) continue;
                    
                    var weevilDesc = room.GetWeevilDesc();
                    roomList.Add(new RoomInfo
                    {
                        m_id = checked((int)room.m_id),
                        m_name = weevilDesc.m_name,
                                
                        m_private = false,
                        m_temp = weevilDesc.m_isTemporary,
                        m_game = false,
                        m_limbo = weevilDesc.m_limbo,
                        
                        m_userCount = await room.GetUserCount(),
                        m_maxUsers = weevilDesc.m_maxUsers,
                        
                        m_spectatorCount = 0,
                        m_maxSpectators = 0,
                    });
                }
                        
                await this.BroadcastSys(new RoomListBody
                {
                    m_action = "rmList",
                    m_room = 0,
                    m_roomList = roomList
                });
            });
        }
        
        private void InputStr(ReadOnlySpan<char> input)
        {
            var reader = SmartFoxStrMessage.MakeReader(input);
            var handler = reader.GetString();
            
            if (handler is not "xt")
            {
                throw new InvalidDataException($"client sent a str message with a type other than \"xt\": {handler}");
            }

            var message = new XtClientMessage();
            message.Deserialize(ref reader);
            
            if (message.m_extension is not "login")
            {
                throw new InvalidDataException($"client sent a str message to an extension other than \"login\": {handler}");
            }
            
            var module = message.m_command;
            
            var indexOfSeparator = message.m_command.IndexOf('#');
            if (indexOfSeparator != -1)
            {
                module = module.Slice(0, indexOfSeparator);
            }
            
            switch (module)
            {
                case Modules.CHAT:
                {
                    HandleChatCommand(message, ref reader);
                    return;
                }
                case Modules.INGAME:
                {
                    HandleInGameCommand(message, ref reader);
                    return;
                }
                case Modules.NEST:
                {
                    HandleNestCommand(message, ref reader);
                    return;
                }
                case Modules.DINER:
                {
                    HandleDinerCommand(message, ref reader);
                    return;
                }
                case Modules.TURN_BASED:
                {
                    HandleTurnBasedCommand(message, ref reader);
                    return;
                }
                case Modules.KART:
                {
                    HandleKartCommand(message, ref reader);
                    return;
                }
            }

            Console.Out.WriteLine($"unknown command: {message.m_command} {string.Join(" ", reader.ReadToEnd())}");
        }

        public override ValueTask CleanupAsync()
        {
            var user = GetUser();
            if (user.GetUserDataAs<WeevilData>() is {} weevilData)
            {
                m_services.GetActorSystem().Root.Stop(weevilData.m_userActor);
            }
            return base.CleanupAsync();
        }

        public void Abort()
        {
            Close();
        }
    }
}