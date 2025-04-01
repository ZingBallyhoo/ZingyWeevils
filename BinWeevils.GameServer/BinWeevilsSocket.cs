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
                    var verCheck = XmlReadBuffer.ReadStatic<VerCheckBody>(preRead.m_bodySpan);
                    if (verCheck.m_ver.m_ver != 154)
                    {
                        m_taskQueue.Enqueue(() => this.Broadcast(new Msg
                        {
                            m_body = new MsgBody
                            {
                                m_action = "apiKO",
                                m_room = 0
                            }
                        }));
                    } else
                    {
                        m_taskQueue.Enqueue(() => this.Broadcast(new Msg
                        {
                            m_body = new MsgBody
                            {
                                m_action = "apiOK",
                                m_room = 0
                            }
                        }));
                    }
                    break;
                }
                default:
                {
                    Close();
                    break;
                }
            }
        }
        
        private void InputStr(ReadOnlySpan<char> input)
        {
            Close();
        }

        public void Abort()
        {
            Close();
        }
    }
}