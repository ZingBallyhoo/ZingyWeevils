using System;
using System.Buffers.Binary;
using System.Linq;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Net.Event;
using ArcticFox.Net.Sockets;
using ArcticFox.SmartFoxServer;
using Google.Protobuf;
using WeevilWorldProtobuf.Enums;
using WeevilWorldProtobuf.Notifications;
using WeevilWorldProtobuf.Objects;
using WeevilWorldProtobuf.Requests;
using WeevilWorldProtobuf.Responses;

namespace WeevilWorld.Server.Net
{
    public class WeevilWorldSocket : SmartFoxSocketBase, ISpanConsumer<byte>
    {
        public WeevilWorldSocket(SocketInterface socket, SmartFoxManager manager) : base(socket, manager)
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

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = await CreateUser(WeevilWorldSocketHost.ZONE, loginRequest.Username);
                        
                        var createdWeevil = new Weevil
                        {
                            Name = loginRequest.Username,
                            Idx = (long) user.m_id,
                            Tycoon = true,
                            Def = "101101406100171700",
                            NestStatus = NestStatus.Open,
                            RoomPosition = null,
                            MoveAction = null,
                            Clothes =
                            {
                                //738
                            },
                            LastLogin = 0,
                            OptionBuddyRequests = true,
                            OptionGameInvites = WeevilOptionGameInvitesState.Allow,
                            CoolnessCategory = NestCoolnessCategory.Alist,
                            UserLevel = 100,
                            NestCoolness = 1000,
                            Xp = 0,
                        };
                        var weevilData = new WeevilData(user, createdWeevil);
                        user.SetUserData(weevilData);

                        await weevilData.GetNestRoom(1).SetOwned();

                        var response = new LoginResponse
                        {
                            Result = ResultType.Ok,
                            Idx = createdWeevil.Idx,
                            Tycoon = createdWeevil.Tycoon,
                            Def = createdWeevil.Def,
                            Tasks = "",
                            ErrorCode = 0,
                            FriendshipRequests = 0,
                            NeedsPassword = false,
                            NeedsEmail = false,
                            Weevil = createdWeevil,
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
                        response.ActiveClothes.Clear();
                        response.ActiveClothes.AddRange(createdWeevil.Clothes);
                        
                        await this.Broadcast(PacketIDs.LOGIN_RESPONSE, response);
                    });
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
                    var request = WeevilWorldProtobuf.Requests.RoomJoin.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        
                        var existingWeevils = await JoinRoomCore(user, request.Roomtype.ToString());

                        var roomJoinResponse = new WeevilWorldProtobuf.Responses.RoomJoin
                        {
                            Result = ResultType.Ok
                        };
                        roomJoinResponse.Weevils.AddRange(existingWeevils);
                        await this.Broadcast(PacketIDs.ROOMJOINED_RESPONSE, roomJoinResponse);
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
                case PacketIDs.ROOMMOVE_REQUEST:
                {
                    var roomMove = RoomMoveRequest.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    var moveAction = roomMove.MoveAction;
                    if (moveAction == null) throw new NullReferenceException();

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();

                        var weevil = user.GetWeevil();
                        weevil.MoveAction = moveAction;

                        var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                        await broadcaster.Broadcast(PacketIDs.CHARACTERMOVE_NOTIFICATION, new CharacterMove
                        {
                            Action = moveAction
                        });
                    });
                    
                    break;
                }
                case PacketIDs.MOVE_REQUEST: // old client
                {
                    var move = MoveRequest.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    var roomPos = move.RoomPosition;
                    if (roomPos == null) throw new NullReferenceException();

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var room = await user.GetRoom();

                        var weevil = user.GetWeevil();
                        weevil.RoomPosition = roomPos;

                        var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                        await broadcaster.Broadcast(PacketIDs.ROOMMOVE_NOTIFICATION, new RoomMove
                        {
                            Position = roomPos
                        });
                    });
                    
                    break;
                }
                case PacketIDs.WEEVIL_CHANGE_DEF_REQUEST:
                {
                    var request = WeevilWorldProtobuf.Requests.WeevilChangeDef.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var weevil = user.GetWeevil();

                        weevil.Def = request.Def;
                        
                        await this.Broadcast(PacketIDs.WEEVIL_CHANGE_DEF_RESPONSE, new WeevilWorldProtobuf.Responses.WeevilChangeDef
                        {
                            Std = new StdResponse
                            {
                                Result = ResultType.Ok
                            },
                            Def = weevil.Def
                        });
                        
                        var room = await user.GetRoomOrNull();
                        if (room != null)
                        {
                            var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                            await broadcaster.Broadcast(PacketIDs.WEEVIL_CHANGE_DEF_NOTIFICATION, new WeevilDefChanged
                            {
                                Def = weevil.Def,
                                OwnerIdx = weevil.Idx
                            });
                        }
                    });
                    break;
                }
                case PacketIDs.SAY_REQUEST:
                {
                    var request = SayRequest.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var weevil = user.GetWeevil();

                        var room = await user.GetRoom();
                        var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);
                        await broadcaster.Broadcast(PacketIDs.CHATMESSAGE_NOTIFICATION, new ChatMessage
                        {
                            Msg = request.Text,
                            OwnerIdx = weevil.Idx
                        });
                    });
                    break;
                }
                case PacketIDs.NESTROOMJOIN_REQUEST:
                {
                    var request = WeevilWorldProtobuf.Requests.NestRoomJoin.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var owningUser = await user.m_zone.GetUser((ulong)request.Idx);

                        if (owningUser == null)
                        {
                            // todo: new client doesn't handle errors codes here
                            throw new Exception();
                        }

                        var owningWeevilData = owningUser.GetWeevilData();
                        var owningWeevil = owningWeevilData.m_object;
                        if (owningWeevil.NestStatus == NestStatus.Closed) throw new Exception();
                        // todo: no friendslist, can't handle FriendsOnly

                        var roomData = owningWeevilData.GetNestRoom(request.Slot);

                        var existingWeevils = await JoinRoomCore(user, roomData.Room());

                        var nestRoomJoinResponse = new WeevilWorldProtobuf.Responses.NestRoomJoin
                        {
                            Result = ResultType.Ok,
                            Owner = owningWeevil,
                            LightOn = roomData.m_lightOn
                        };
                        nestRoomJoinResponse.Weevils.AddRange(existingWeevils);
                        await this.Broadcast(PacketIDs.NESTROOMJOIN_RESPONSE, nestRoomJoinResponse);
                    });
                    break;
                }
                case PacketIDs.NESTINFO_REQUEST:
                {
                    var request = StdIdRequest.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var owningUser = await user.m_zone.GetUser((ulong) request.Id);
                        if (owningUser == null) throw new NullReferenceException();
                        var owningWeevilData = owningUser.GetWeevilData();

                        var response = new WeevilWorldProtobuf.Responses.NestInfo
                        {
                            Result = ResultType.Ok,
                            Owner = owningWeevilData.m_object
                        };
                        foreach (var room in owningWeevilData.GetRooms())
                        {
                            if (!room.m_purchased) continue;
                            response.RoomInfo.Add(new RoomInfo
                            {
                                Slot = room.m_slot,
                                LightOn = room.m_lightOn
                            });
                        }
                        await this.Broadcast(PacketIDs.NESTINFO_REPONSE, response);
                    });
                    break;
                    
                }
                case PacketIDs.BUYROOM_REQUEST:
                {
                    var request = RoomSlot.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();

                        var weevilData = user.GetWeevilData();
                        var nestRoomData = weevilData.GetNestRoom(request.Slot);

                        await nestRoomData.SetOwned();

                        await this.Broadcast(PacketIDs.BUYROOM_RESPONSE, new RoomSlotData
                        {
                            Slot = request.Slot,
                            Std = new StdResponse
                            {
                                Result = ResultType.Ok
                            }
                        });
                    });
                    break;
                }
                case PacketIDs.TOGGLELIGHT_REQUEST:
                {
                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var currentRoom = await user.GetRoom();

                        var nestRoomData = currentRoom.GetData<NestRoomData>();

                        await nestRoomData.ToggleLights(user);
                    });
                    break;
                }
            }
        }

        private static async ValueTask<Weevil[]> JoinRoomCore(User user, string roomName)
        {
            var newRoom = await user.m_zone.GetRoom(roomName);
            if (newRoom == null) throw new NullReferenceException($"room {roomName} doesn't exist");
            return await JoinRoomCore(user, newRoom);
        }
        
        private static async ValueTask<Weevil[]> JoinRoomCore(User user, Room newRoom)
        {
            var weevil = user.GetWeevil();
            weevil.MoveAction = null; // don't replay a move from the previous room
            weevil.RoomPosition = null;

            var existingWeevils = await newRoom.GetAllUserData<WeevilData>();

            await user.MoveTo(newRoom);

            return existingWeevils.Select(x => x.m_object).ToArray();
        }

        public void Abort()
        {
            Close();
        }
    }
}