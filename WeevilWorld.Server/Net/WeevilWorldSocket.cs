using System;
using System.Buffers.Binary;
using ArcticFox.Codec;
using ArcticFox.Net;
using ArcticFox.Net.Sockets;
using Google.Protobuf;
using WeevilWorldProtobuf.Enums;
using WeevilWorldProtobuf.Notifications;
using WeevilWorldProtobuf.Objects;
using WeevilWorldProtobuf.Requests;
using WeevilWorldProtobuf.Responses;

namespace WeevilWorld.Server.Net
{
    public class WeevilWorldSocket : HighLevelSocket, ISpanConsumer<byte>
    {
        public WeevilWorldSocket(SocketInterface socket) : base(socket)
        {
            m_netInputCodec = new CodecChain().AddCodec(new WeevilWorldBufferCodec()).AddCodec(this);
        }

        public void Input(ReadOnlySpan<byte> input, ref object? state)
        {
            var type = (PacketIDs)BinaryPrimitives.ReadInt16BigEndian(input);
            input = input.Slice(2);
            
            Console.Out.WriteLine($"{type} {Convert.ToBase64String(input)}");

            switch (type)
            {
                case PacketIDs.CLIENTDESCRIPTION_REQUEST:
                {
                    var clientDescription = WeevilWorldProtobuf.Requests.ClientDescription.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    Console.Out.WriteLine($"desc: {clientDescription.Platform} {clientDescription.Version}");
                    var response = new WeevilWorldProtobuf.Responses.ClientDescription
                    {
                        MustUpdate = false,
                        MinimumVersion = "",
                        Std = new StdResponse
                        {
                            Result = ResultType.Ok
                        }
                    };
                    this.Broadcast(PacketIDs.CLIENTDESCRIPTION_RESPONSE, response);
                    break;
                }
                case PacketIDs.LOGIN_REQUEST:
                {
                    var loginRequest = LoginRequest.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    var response = new LoginResponse
                    {
                        Result = ResultType.Ok,
                        Idx = 1,
                        Tycoon = true,
                        Def = "101101406100171700",
                        Tasks = "",
                        ErrorCode = 0,
                        FriendshipRequests = 0,
                        ActiveClothes =
                        {
                            738
                        },
                        NeedsPassword = false,
                        NeedsEmail = false,
                        Weevil = new Weevil
                        {
                            Name = "Mr Toy",
                            Idx = 1,
                            Tycoon = true,
                            Def = "101101406100171700",
                            NestStatus = NestStatus.Open,
                            RoomPosition = null,
                            MoveAction = null,
                            Clothes =
                            {
                                738
                            },
                            LastLogin = 0,
                            OptionBuddyRequests = true,
                            OptionGameInvites = WeevilOptionGameInvitesState.Allow,
                            CoolnessCategory = NestCoolnessCategory.Alist,
                            UserLevel = 100,
                            NestCoolness = 1000,
                            Xp = 0,
                        },
                        HasGifts = false,
                        FirstLogin = false,
                        HasNewDailyTasks = false,
                        ItemBubbleDelta = 0,
                        Bubbles = { },
                        BubblesCollected = 0,
                        BubbleFireDelta = 0,
                        BubbleAdCounter = 0,
                        BubbleFiredButtons = { },
                        Coins = 1000,
                        Diamonds = 1000,
                        NestRatingCooldown = 0,
                        LevelPendingReward = new LevelPendingReward
                        {
                            Coin = 1,
                            Diamond = 0
                        }
                    };
                    this.Broadcast(PacketIDs.LOGIN_RESPONSE, response);
                    break;
                }
                case PacketIDs.GETKEYVALUE_REQUEST:
                {
                    var getKeyValueReq = GetValuesForKeys.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    Console.Out.WriteLine($"requested keys: {string.Join(", ", getKeyValueReq.Keys)}");

                    this.Broadcast(PacketIDs.GETKEYVALUE_RESPONSE, new ValuesForKeys
                    {
                        Std = new StdResponse
                        {
                            Result = ResultType.Error,
                            Error = "go away pls",
                            ErrorCode = 1
                        }
                    });
                    break;
                }
                case PacketIDs.ITEMLIST_REQUEST:
                {
                    this.Broadcast(PacketIDs.ITEMLIST_RESPONSE, new WeevilWorldProtobuf.Responses.ItemList());
                    break;
                }
                case PacketIDs.CLOTHESITEMLIST_REQUEST:
                {
                    this.Broadcast(PacketIDs.CLOTHESITEMLIST_RESPONSE, new ClothingItemList()
                    {
                        Items = { }
                    });
                    break;
                }
                case PacketIDs.FRIENDLIST_REQUEST:
                {
                    this.Broadcast(PacketIDs.FRIENDLIST_RESPONSE, new FriendList
                    {
                        Result = ResultType.Ok,
                        Friends = { },
                        Error = ""
                    });
                    break;
                }
                case PacketIDs.GETUNIXTIMESTAMP_REQUEST:
                {
                    this.Broadcast(PacketIDs.GETUNIXTIMESTAMP_RESPONSE, new UnixTimestamp
                    {
                        Std = new StdResponse
                        {
                            Result = ResultType.Ok
                        },
                        Value = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    });
                    break;
                }
                case PacketIDs.QUESTTASKLISTCOMPLETE_REQUEST:
                {
                    this.Broadcast(PacketIDs.QUESTTASKLISTCOMPLETE_RESPONSE,
                        new QuestListCompletedTasks
                        {
                            Std = new StdResponse
                            {
                                Result = ResultType.Ok
                            },
                            Info = { }
                        });
                    break;
                }
                case PacketIDs.ADVERTGET_REQUEST:
                {
                    var advertGetRequest = Advertising_Get.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span

                    var response = new AdvertisingGet // todo: please naming. whose idea was this
                    {
                        Location = advertGetRequest.Location,
                        Std = new StdResponse
                        {
                            Result = ResultType.Ok
                        },
                        Ad = new Ad
                        {
                            Path = "angus.png"
                        }
                    };
                    this.Broadcast(PacketIDs.ADVERTGET_RESPONSE, response);
                    break;
                }
                case PacketIDs.ITEMTYPELIST_REQUEST:
                {
                    var request = WeevilWorldProtobuf.Requests.ItemTypeList.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    var itemList = new WeevilWorldProtobuf.Responses.ItemTypeList
                    {
                        Result = ResultType.Ok,
                        CallId = request.CallId
                    };

                    foreach (var id in request.Ids)
                    {
                        itemList.ItemTypes.Add(new ItemType
                        {
                            Id = id,
                            AnimationType = 0,
                            AvailableFrom = 0,
                            AvailableTo = 0,
                            CacheVersion = "",
                            Currency = 0,
                            Description = "",
                            Enabled = true,
                            Giftable = true,
                            Infinite = false,
                            IsBuildable = false,
                            SalePrice = 0,
                            IsMembershipGift = false,
                            SeenTracking = "",
                            ItemCategoryId = 7,
                            SeenPlaywireTracking = "",
                            Qty = 0,
                            Price = 0,
                            IsBundle = false,
                            BundleItems = { },
                            RedemptionMulch = 0,
                            TycoonOnly = false,
                            Xp = 0
                        });
                    }
	            
                    this.Broadcast(PacketIDs.ITEMTYPELIST_RESPONSE, itemList);
                    break;
                }
                case PacketIDs.ROOMJOIN_REQUEST:
                {
                    this.Broadcast(PacketIDs.ROOMJOINED_RESPONSE, new WeevilWorldProtobuf.Responses.RoomJoin
                    {
                        Result = ResultType.Ok,
                    });
                    break;
                }
                case PacketIDs.LOGIN_MESSAGE_LIST_REQUEST:
                {
                    this.Broadcast(PacketIDs.LOGIN_MESSAGE_LIST_RESPONSE, new WeevilWorldProtobuf.Responses.LoginMessageList
                    {
                        Messages =
                        {
                            new LoginMessage
                            {
                                ActionType = LoginMessageActionType.JoinRoom,
                                JoinRoom = RoomType.QuizRoom,
                                IsRead = false,
                                MessageID = 1,
                                AssetUrl = "https://cdn.binw.net/WeevilWorld/Immersives/angus.png",
                                ExternalWebsite = "https://binweevils.net"
                            }
                        }
                    });
                    break;
                }
                case PacketIDs.WOTW_LIST_REQUEST:
                {
                    this.Broadcast(PacketIDs.WOTW_LIST_RESPONSE, new WeevilOfTheWeekList
                    {
                        Std = new StdResponse
                        {
                            Result = ResultType.Ok
                        },
                        Winner = new Weevil
                        {
                            Name = "Mr Toy",
                            Idx = 1,
                            Tycoon = true,
                            Def = "101101406100171700",
                            NestStatus = NestStatus.Open,
                            RoomPosition = null,
                            MoveAction = null,
                            Clothes =
                            {
                                738
                            },
                            LastLogin = 0,
                            OptionBuddyRequests = true,
                            OptionGameInvites = WeevilOptionGameInvitesState.Allow,
                            CoolnessCategory = NestCoolnessCategory.Alist,
                            UserLevel = 100,
                            NestCoolness = 1000,
                            Xp = 0,
                        }
                    });
                    break;
                }
            }
        }

        public void Abort()
        {
            Close();
        }
    }
}