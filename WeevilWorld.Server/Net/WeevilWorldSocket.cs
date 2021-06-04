using System;
using System.Buffers.Binary;
using System.Linq;
using System.Threading.Tasks;
using ArcticFox.Codec;
using ArcticFox.Net.Event;
using ArcticFox.Net.Sockets;
using ArcticFox.SmartFoxServer;
using Google.Protobuf;
using Google.Protobuf.Collections;
using WeevilWorldProtobuf.Enums;
using WeevilWorldProtobuf.Notifications;
using WeevilWorldProtobuf.Objects;
using WeevilWorldProtobuf.Requests;
using WeevilWorldProtobuf.Responses;

namespace WeevilWorld.Server.Net
{
    public partial class WeevilWorldSocket : SmartFoxSocketBase, ISpanConsumer<byte>
    {
        private bool m_is3xxClient;
        
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

                    if (clientDescription.Version.StartsWith("3."))
                    {
                        m_is3xxClient = true;
                    }
                    
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.CLIENTDESCRIPTION_RESPONSE,
                        new WeevilWorldProtobuf.Responses.ClientDescription
                        {
                            MustUpdate = false,
                            MinimumVersion = "",
                            Std = new StdResponse
                            {
                                Result = ResultType.Ok
                            }
                        }));
                    break;
                }
                case PacketIDs.LOGIN_REQUEST:
                {
                    var loginRequest = LoginRequest.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span

                    m_taskQueue.Enqueue(async () =>
                    {
                        User user;
                        try
                        {
                            user = await CreateUser(WeevilWorldSocketHost.ZONE, loginRequest.Username);
                        } catch (UserExistsException)
                        {
                            await this.Broadcast(PacketIDs.LOGIN_RESPONSE, new LoginResponse
                            {
                                Result = ResultType.Error,
                                ErrorCode = 1
                            });
                            return;
                        }

                        var def = WeevilDef.DEFAULT;
                        FixWeevilDef(ref def);
                        
                        var createdWeevil = new Weevil
                        {
                            Name = user.m_name,
                            Idx = (long) user.m_id,
                            Tycoon = true,
                            Def = def,
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
                case PacketIDs.RANDOMNAME_REQUEST:
                {
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.RANDOMNAME_RESPONSE,
                        new GeneratedRandomUsername
                        {
                            Std = new StdResponse
                            {
                                Result = ResultType.Ok
                            },
                            Username = $"fairriver{Random.Shared.Next(1, 100)}"
                        }));
                    break;
                }
                case PacketIDs.CREATEACCOUNT_REQUEST:
                {
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.CREATEACCOUNT_RESPONSE,
                        new WeevilWorldProtobuf.Responses.Registration
                        {
                            Result = ResultType.Ok
                        }));
                    break;
                }
                case PacketIDs.GETKEYVALUE_REQUEST:
                {
                    var getKeyValueReq = GetValuesForKeys.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    Console.Out.WriteLine($"requested keys: {string.Join(", ", getKeyValueReq.Keys)}");

                    if (getKeyValueReq.Keys.Count > 0 && getKeyValueReq.Keys[0].StartsWith("AdUnit"))
                    {
                        // client is dum. will wait for this forever and crash store loader
                        var response = new ValuesForKeys
                        {
                            Std = new StdResponse
                            {
                                Result = ResultType.Ok,
                            },
                        };
                        response.Result.AddRange(getKeyValueReq.Keys.Select(x => new KeyValue
                        {
                            Key = x,
                            Value = string.Empty
                        }).ToArray());
                        m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.GETKEYVALUE_RESPONSE, response));
                        break;
                    }
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.GETKEYVALUE_RESPONSE,
                        new ValuesForKeys
                        {
                            Std = new StdResponse
                            {
                                Result = ResultType.Error,
                                Error = "go away pls",
                                ErrorCode = 1
                            }
                        }));
                    break;
                }
                case PacketIDs.ITEMLIST_REQUEST:
                {
                    m_taskQueue.Enqueue(() =>
                        this.Broadcast(PacketIDs.ITEMLIST_RESPONSE, new WeevilWorldProtobuf.Responses.ItemList()));
                    break;
                }
                case PacketIDs.CLOTHESITEMLIST_REQUEST:
                {
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.CLOTHESITEMLIST_RESPONSE,
                        new ClothingItemList()
                        {
                            Items = { }
                        }));
                    break;
                }
                case PacketIDs.FRIENDLIST_REQUEST:
                {
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.FRIENDLIST_RESPONSE,
                        new FriendList
                        {
                            Result = ResultType.Ok,
                            Friends = { },
                            Error = ""
                        }));
                    break;
                }
                case PacketIDs.GETUNIXTIMESTAMP_REQUEST:
                {
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.GETUNIXTIMESTAMP_RESPONSE,
                        new UnixTimestamp
                        {
                            Std = new StdResponse
                            {
                                Result = ResultType.Ok
                            },
                            Value = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        }));
                    break;
                }
                case PacketIDs.QUESTTASKLISTCOMPLETE_REQUEST:
                {
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.QUESTTASKLISTCOMPLETE_RESPONSE,
                        new QuestListCompletedTasks
                        {
                            Std = new StdResponse
                            {
                                Result = ResultType.Ok
                            },
                            Info = { }
                        }));
                    break;
                }
                case PacketIDs.ADVERTGET_REQUEST:
                {
                    var advertGetRequest = Advertising_Get.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    
                    var adPath = advertGetRequest.Location switch
                    {
                        "101" => "SquareBanner.png", // left square
                        "102" => "SquareBanner.png", // right square
                        
                        "103" => "leftSkyScraper.png", // left scraper
                        "104" => "rightSkyScraper.png", // right scraper
                        "105" => "horizScraper.png", // horiz scraper
                        
                        //"106" => "SquareBanner.png", // left immersive
                        //"107" => "SquareBanner.png", // right immersive
                        _ => null
                    };

                    if (adPath == null)
                    {
                        Console.Out.WriteLine($"missing ad location {advertGetRequest.Location}");
                        m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.ADVERTGET_RESPONSE,
                            new AdvertisingGet
                            {
                                Location = advertGetRequest.Location,
                                Std = new StdResponse
                                {
                                    Result = ResultType.Ko
                                }
                            }));
                        return;
                    }

                    var dir = "Default";
                    if (Random.Shared.Next(0, 2) == 0)
                    {
                        dir = "Zingy";
                    }
                    
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.ADVERTGET_RESPONSE,
                        new AdvertisingGet
                        {
                            Location = advertGetRequest.Location,
                            Std = new StdResponse
                            {
                                Result = ResultType.Ok
                            },
                            Ad = new Ad
                            {
                                Path = $"{dir}/{adPath}"
                            }
                        }));
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
                            CacheVersion = "0",
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
	            
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.ITEMTYPELIST_RESPONSE, itemList));
                    break;
                }
                case PacketIDs.ROOMJOIN_REQUEST:
                {
                    var request = WeevilWorldProtobuf.Requests.RoomJoin.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();

                        if (request.Roomtype == RoomType.Lobby)
                        {
                            await user.RemoveFromRoom(RoomTypeIDs.DEFAULT);
                            return;
                        }
                        
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
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.LOGIN_MESSAGE_LIST_RESPONSE,
                        new WeevilWorldProtobuf.Responses.LoginMessageList
                        {
                            Messages =
                            {
                                // new LoginMessage
                                // {
                                //     ActionType = LoginMessageActionType.JoinRoom,
                                //     JoinRoom = RoomType.CrazyPool,
                                //     IsRead = false,
                                //     MessageID = 1,
                                //     AssetUrl = "https://ww.zingy.dev/WeevilWorld/Immersives/angus.png",
                                //     ExternalWebsite = "https://binweevils.net"
                                // }
                            }
                        }));
                    break;
                }
                case PacketIDs.WOTW_LIST_REQUEST:
                {
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.WOTW_LIST_RESPONSE,
                        new WeevilOfTheWeekList
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
                        }));
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

                        var def = request.Def;
                        FixWeevilDef(ref def);
                        weevil.Def = def;
                        
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
                case PacketIDs.TURN_BASED_GAME_INVITE_REQUEST:
                {
                    var invite = TurnbasedInvite.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    m_taskQueue.Enqueue(() => HandleGameInviteRequest(invite));
                    break;
                }
                case PacketIDs.TURN_BASED_GAME_INVITE_ACCEPT_REQUEST:
                {
                    var request = WeevilWorldProtobuf.Requests.TurnbasedAcceptInvitation.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    m_taskQueue.Enqueue(() => HandleGameInviteAcceptRequest(request));
                    break;
                }
                case PacketIDs.TURN_BASED_GAME_UPDATE_STATE_REQUEST:
                {
                    var request = TurnbasedUpdateState.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    m_taskQueue.Enqueue(() => HandleGameUpdateStateRequest(request));
                    break;
                }
                case PacketIDs.TURN_BASED_GAME_END_TURN_REQUEST:
                {
                    var request = WeevilWorldProtobuf.Requests.TurnbasedEndTurn.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    m_taskQueue.Enqueue(() => HandleGameEndTurnRequest(request));
                    break;
                }
                case PacketIDs.TURN_BASED_GAME_LEAVE_REQUEST:
                {
                    var request = TurnbasedLeave.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    m_taskQueue.Enqueue(() => HandleGameLeaveRequest(request));
                    break;
                }
                case PacketIDs.TURN_BASED_GAME_INVITE_CANCEL_REQUEST:
                {
                    m_taskQueue.Enqueue(async () =>
                    {
                        var weevilData = GetUser().GetWeevilData();
                        await CancelPendingGameInvite(weevilData);
                    });
                    
                    break;
                }
                case PacketIDs.TURN_BASED_GAME_INVITE_DECLINE_REQUEST:
                {
                    var request = WeevilWorldProtobuf.Requests.TurnbasedDeclineInvitation.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    m_taskQueue.Enqueue(() => HandleGameInviteDeclineRequest(request));
                    break;
                }
                case PacketIDs.ITEMBUY_REQUEST:
                {
                    var request = WeevilWorldProtobuf.Requests.ItemBuy.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span
                    if (request.HasGiftToIdx) throw new NotImplementedException("gifting items");

                    // todo: doesn't handle most things. e.g quantity
                    
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.ITEMBUY_RESPONSE, 
                        new WeevilWorldProtobuf.Responses.ItemBuy
                        {
                            AddedXp = 0,
                            AvailableQuantity = 999,
                            Cost = 0,
                            Result = ResultType.Ok,
                            Items =
                            {
                                new Item
                                {
                                    Id = request.ItemType, // ??
                                    TypeId = request.ItemType
                                }
                            }
                        }
                    ));
                    break;
                }
                case PacketIDs.WEARCLOTHES_REQUEST:
                {
                    var request = WeevilWorldProtobuf.Requests.WearClothes.Parser.ParseFrom(ByteString.CopyFrom(input)); // todo: span

                    m_taskQueue.Enqueue(async () =>
                    {
                        var user = GetUser();
                        var weevil = user.GetWeevil();

                        weevil.Clothes.Clear();
                        weevil.Clothes.AddRange(request.ClothingItems);

                        var room = await user.GetRoomOrNull();
                        if (room != null)
                        {
                            var broadcaster = new FilterBroadcaster<User>(room.m_userExcludeFilter, user);

                            var notification = new ClothingChanged
                            {
                                OwnerIdx = weevil.Idx
                            };
                            notification.ActiveClothes.AddRange(weevil.Clothes);
                            await broadcaster.Broadcast(PacketIDs.CLOTHESCHANGED_NOTIFICATION, notification);
                        }

                        await user.Broadcast(PacketIDs.WEARCLOTHES_RESPONSE, 
                            new WeevilWorldProtobuf.Responses.WearClothes
                            {
                                Result = new StdResponse
                                {
                                    Result = ResultType.Ok
                                }
                            });
                    });
                    
                    break;
                }
                case PacketIDs.GETSHOPFILENAME_REQUEST:
                {
                    RepeatedField<ShopFiles.Types.SpecificShopFile> files;
                    if (m_is3xxClient)
                    {
                        files = new RepeatedField<ShopFiles.Types.SpecificShopFile>()
                        {
                            new ShopFiles.Types.SpecificShopFile
                            {
                                Path = "bathroommay2019j1.xml",
                                Shop = "KitchenBath"
                            },
                            new ShopFiles.Types.SpecificShopFile
                            {
                                Path = "bedroommay2019j1.xml",
                                Shop = "Bedroom"
                            },
                            new ShopFiles.Types.SpecificShopFile
                            {
                                Path = "livingmay2019j1.xml",
                                Shop = "Living"
                            }
                        };
                    } else
                    {
                        files = new RepeatedField<ShopFiles.Types.SpecificShopFile>()
                        {
                            new ShopFiles.Types.SpecificShopFile
                            {
                                Path = "WWClothesDecember.xml",
                                Shop = "Clothes"
                            },
                            new ShopFiles.Types.SpecificShopFile
                            {
                                Path = "WWBathroomDecember.xml",
                                Shop = "KitchenBath"
                            },
                            new ShopFiles.Types.SpecificShopFile
                            {
                                Path = "WWBedroomDecember.xml",
                                Shop = "Bedroom"
                            },
                            new ShopFiles.Types.SpecificShopFile
                            {
                                Path = "WWLivingDecember.xml",
                                Shop = "Living"
                            }
                        };
                        ;
                    }
                    var response = new ShopFiles
                    {
                        Std = new StdResponse
                        {
                            Result = ResultType.Ok
                        }
                    };
                    response.ShopFiles_.AddRange(files);
                    m_taskQueue.Enqueue(() => this.Broadcast(PacketIDs.GETSHOPFILENAME_RESPONSE, response));
                    break;
                }
            }
        }

        private void FixWeevilDef(ref string defString)
        {
            // see #2
            
            var parsedDef = new WeevilDef(defString);
            if (!parsedDef.Validate())
            {
                parsedDef = new WeevilDef(WeevilDef.DEFAULT); // become pea
            }

            if (parsedDef.m_bodyType == WeevilDef.BodyType.Cuboid && m_is3xxClient)
            {
                parsedDef.m_bodyType = WeevilDef.BodyType.ConeNarrowInv;
            }
            if (parsedDef.HasSuperAntenna())
            {
                parsedDef.m_antennaType = WeevilDef.AntennaType.TripleLarge;
            }
            
            defString = parsedDef.AsString();
        }

        private static async ValueTask<Weevil[]> JoinRoomCore(User user, string roomName)
        {
            var newRoom = await user.m_zone.GetRoom(roomName);
            if (newRoom == null) throw new NullReferenceException($"room {roomName} doesn't exist");
            return await JoinRoomCore(user, newRoom);
        }
        
        private static async ValueTask<Weevil[]> JoinRoomCore(User user, Room newRoom)
        {
            var weevilData = user.GetWeevilData();
            if (weevilData.IsRoomInvited()) weevilData.RemoveInvite();
            
            var weevil = weevilData.m_object;
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