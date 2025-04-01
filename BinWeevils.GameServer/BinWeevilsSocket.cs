using System.Text;
using ArcticFox.Codec;
using ArcticFox.Net.Sockets;
using ArcticFox.SmartFoxServer;
using BinWeevils.Protocol.XmlMessages;
using StackXML;

namespace BinWeevils.GameServer
{
    public class BinWeevilsSocket : SmartFoxSocketBase, ISpanConsumer<char>
    {
        public BinWeevilsSocket(SocketInterface socket, SmartFoxManager manager) : base(socket, manager)
        {
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
                Close();
            }
        }
        
        private void InputXml(ReadOnlySpan<char> input)
        {
            var preRead = new MsgPreRead();
            XmlReadBuffer.ReadIntoStatic(input, ref preRead);
            if (preRead.m_bodySpan.Length == 0)
            {
                // todo: log
                Close();
                return;
            }
            Console.Out.WriteLine(preRead.m_bodySpan.ToString());
            
            if (preRead.m_messageType is not "sys")
            {
                // todo: log
                Close();
                return;
            }
            
            var preReadBody = new MsgBodyPreRead();
            XmlReadBuffer.ReadIntoStatic(preRead.m_bodySpan, ref preReadBody);
            Console.Out.WriteLine($"{preReadBody.m_action} {preReadBody.m_room}");
            
            if (preReadBody.m_action.Length == 0 || preReadBody.m_action.IsWhiteSpace())
            {
                // todo: log
                Close();
                return;
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
                default:
                {
                    Console.Out.WriteAsync($"unknown action: {preReadBody.m_action}");
                    break;
                }
            }
        }
        
        private void HandleSfsVerCheck(ReadOnlySpan<char> body)
        {
            var verCheck = XmlReadBuffer.ReadStatic<VerCheckBody>(body);
            if (verCheck.m_ver.m_ver != 154)
            {
                m_taskQueue.Enqueue(() => this.Broadcast(BuildSysMessage(new MsgBody
                {
                    m_action = "apiKO",
                    m_room = 0
                })));
            } else
            {
                m_taskQueue.Enqueue(() => this.Broadcast(BuildSysMessage(new MsgBody
                {
                    m_action = "apiOK",
                    m_room = 0
                })));
            }
        }
        
        private void HandleSfsLogin(ReadOnlySpan<char> body)
        {
            var login = XmlReadBuffer.ReadStatic<LoginBody>(body);
                    
            m_taskQueue.Enqueue(async () =>
            {
                await CreateUser(login.m_data.m_zone, login.m_data.m_nickname);
                        
                await this.Broadcast(BuildXtResMessage(new ActionScriptObject
                {
                    m_vars =
                    [
                        Var.String("commandType", "login"),
                        Var.String("success", "true"),
                    ],
                    m_objects =
                    [
                        new SubActionScriptObject
                        {
                            m_name = "user",
                            m_type = "a",
                            m_vars =
                            [
                                Var.String("weevilDef", "101101406100171700"),
                                Var.String("ip", "no"),
                                Var.String("apparel", ""),
                                Var.String("idx", $"{55}"),
                                Var.String("locale", "UK"),
                                Var.String("userID", $"{999}")
                            ]
                        }
                    ]
                }));
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
                    var weevilDesc = (WeevilRoomDescription)room.m_description;
                    roomList.Add(new RoomInfo
                    {
                        m_id = checked((int)room.m_id),
                        m_name = weevilDesc.m_name,
                                
                        m_private = false,
                        m_temp = weevilDesc.m_isTemporary,
                        m_game = weevilDesc.m_isGame,
                        m_limbo = weevilDesc.m_limbo,
                        
                        m_userCount = await room.GetUserCount(),
                        m_maxUsers = weevilDesc.m_maxUsers,
                        
                        m_spectatorCount = 0,
                        m_maxSpectators = 0,
                    });
                }
                        
                await this.Broadcast(BuildSysMessage(new RoomListBody
                {
                    m_action = "rmList",
                    m_room = 0,
                    m_roomList = roomList
                }));
            });
        }
        
        private static Msg BuildXtResMessage(ActionScriptObject obj, int room=-1)
        {
            var body = XmlWriteBuffer.SerializeStatic(obj, CDataMode.Off);

            var resp = new Msg
            {
                m_messageType = "xt",
                m_body = new XtResBody
                {
                    m_action = "xtRes",
                    m_room = room,
                    m_xmlBody = body
                }
            };
            return resp;
        }
        
        private static Msg BuildSysMessage(MsgBody body)
        {
            return new Msg
            {
                m_messageType = "sys",
                m_body = body,
            };
        }
        
        private void InputStr(ReadOnlySpan<char> input)
        {
            // todo:
            Close();
        }

        public void Abort()
        {
            Close();
        }
    }
}