using System.Xml.Linq;
using BinWeevils.Database;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;
using StackXML;

namespace BinWeevils.GameServer
{
    public class WeevilInitializer
    {
        private readonly WeevilDBContext m_dbContext;
        
        public WeevilInitializer(WeevilDBContext dbContext)
        {
            m_dbContext = dbContext;
        }
        
        public async Task<WeevilDB> Create(string name) 
        {
            var createParams = new WeevilCreateParams
            {
                m_name = name,
                m_weevilDef = WeevilDef.DEFAULT
            };
            if (name.Contains("fairriver"))
            {
                createParams.m_weevilDef = WeevilDef.DEFAULT;
            } else if (name.Contains("zingy"))
            {
                createParams.m_weevilDef = WeevilDef.ZINGY;
                createParams.m_nestDef = ZINGY_NEST;
                createParams.m_mulch = 209602;
                createParams.m_xp = 146138;
            } else if (name.Contains("scribbles"))
            {
                createParams.m_weevilDef = WeevilDef.DEFINITELY_SCRIBBLES;
            } else if (name.Contains("coolcat"))
            {
                createParams.m_weevilDef = 1061064041061709;
            } else
            {
                var def = new WeevilDef(WeevilDef.DEFAULT)
                {
                    m_headType = (WeevilDef.HeadType)Random.Shared.Next((int)WeevilDef.HeadType.Spheroid, (int)WeevilDef.HeadType.Count),
                    m_headColorIdx = (byte)Random.Shared.Next(0, WeevilDef.LEGACY_COLOR_COUNT),
                    m_bodyType = (WeevilDef.BodyType)Random.Shared.Next((int)WeevilDef.BodyType.Spheroid, (int)WeevilDef.BodyType.Count),
                    m_bodyColorIdx = (byte)Random.Shared.Next(0, WeevilDef.LEGACY_COLOR_COUNT),    
                    m_eyeType = (WeevilDef.EyeType)Random.Shared.Next((int)WeevilDef.EyeType.MiddleTogether, (int)WeevilDef.EyeType.Count),
                    m_eyeColorIdx = (byte)Random.Shared.Next(0, WeevilDef.LEGACY_EYE_COLOR_COUNT),
                    m_lids = Random.Shared.Next(0, 1) == 1,
                    m_antennaType = (WeevilDef.AntennaType)Random.Shared.Next(0, (int)WeevilDef.AntennaType.SuperOriginal + 1),
                    m_antennaColorIdx = (byte)Random.Shared.Next(0, WeevilDef.LEGACY_COLOR_COUNT),
                    m_legColorIdx = (byte)Random.Shared.Next(0, WeevilDef.LEGACY_COLOR_COUNT),
                    m_legType = WeevilDef.LegType.Normal
                };
                if (!def.Validate())
                {
                    throw new InvalidDataException();
                }
                createParams.m_weevilDef = def.AsNumber();
            }
            
            return await Create(createParams);
        }
        
        public async Task<WeevilDB> Create(WeevilCreateParams createParams)
        {
            var now = DateTime.UtcNow;
            
            var dbWeevil = new WeevilDB
            {
                m_name = createParams.m_name,
                m_createdAt = now,
                m_lastLogin = now,
                m_weevilDef = createParams.m_weevilDef,
                m_food = 75,
                m_fitness = 50,
                m_happiness = 75,
                m_lastAcknowledgedLevel = 1,
                m_mulch = createParams.m_mulch ?? 20000,
                m_dosh = 0
            };
            if (createParams.m_xp != null)
            {
                dbWeevil.m_xp = createParams.m_xp.Value;
                dbWeevil.m_lastAcknowledgedLevel = WeevilLevels.DetermineLevel(createParams.m_xp.Value);
            } else
            {
                dbWeevil.m_xp = WeevilLevels.GetXpForLevel(dbWeevil.m_lastAcknowledgedLevel);
            }
            
            NestDB nest;
            if (createParams.m_nestDef != null)
            {
                nest = await ImportNest(createParams.m_nestDef);
            } else
            {
                nest = await CreateStarterNest();
            }
            dbWeevil.m_nest = nest;
            
            await m_dbContext.m_weevilDBs.AddAsync(dbWeevil);
            return dbWeevil;
        }
        
        private async Task<NestDB> CreateStarterNest()
        {
            var nest = NestDB.Empty();
            
            nest.m_items.Add(new NestItemDB
            {
                // todo: there are 2 f_shelf1... one with color one without
                
                m_itemType = await m_dbContext.m_itemTypes
                    .Where(x => x.m_paletteID == -1)
                    .SingleAsync(x => x.m_configLocation == "f_shelf1")
            });
            nest.m_items.Add(new NestItemDB
            {
                m_itemTypeID = (await m_dbContext.FindItemByConfigName("o_egg"))!.Value
            });
            nest.m_gardenSeeds.Add(new NestSeedItemDB
            {
                m_seedTypeID = (await m_dbContext.FindSeedByConfigName("speedySeed"))!.Value
            });
            /*nest.m_gardenItems.Add(new NestGardenItemDB
            {
                m_itemTypeID = (await m_dbContext.FindItemByConfigName("wateringCan"))!.Value,
                m_placedItem = new NestPlacedGardenItemDB
                {
                    m_x = -69,
                    m_z = 417,
                    m_room = nest.m_rooms.Single(x => x.m_type == ENestRoom.Garden)
                }
            });*/
            
            return nest;
        }
        
        private async Task<NestDB> ImportNest(string nestDef)
        {
            var nest = new NestDB();
            await m_dbContext.m_nests.AddAsync(nest);

            var locMap = new Dictionary<uint, NestRoomDB>();
            var nestItemMap = new Dictionary<uint, NestItemDB>();
            
            var xdoc = XDocument.Parse(nestDef);
            foreach (var locElement in xdoc.Root!.Elements("loc"))
            {
                var isBusiness = locElement.Attribute("name") != null;
                
                NestConfig.Loc loc;
                if (isBusiness)
                {
                    loc = XmlReadBuffer.ReadStatic<NestConfig.BusinessLoc>(locElement.ToString());
                } else
                {
                    loc = XmlReadBuffer.ReadStatic<NestConfig.Loc>(locElement.ToString());
                }
                
                var newRoom = new NestRoomDB
                {
                    m_type = (ENestRoom)loc.m_id,
                    m_color = loc.m_color,
                };
                if (loc is NestConfig.BusinessLoc businessLoc)
                {
                    newRoom.m_business = new BusinessDB
                    {
                        m_name = businessLoc.m_name,
                        m_open = businessLoc.m_busOpen,
                        m_type = (EBusinessType)businessLoc.m_busType,
                        m_signColor = businessLoc.m_signColor,
                        m_signTextColor = businessLoc.m_signTextColor,
                        m_playList = businessLoc.m_playList
                    };
                }
                
                locMap.Add(loc.m_instanceID, newRoom);
                nest.m_rooms.Add(newRoom);
            }
            
            // create furniture & garden items
            foreach (var itemElement in xdoc.Root!.Elements("item"))
            {
                var isGardenItem = itemElement.Attribute("fName") != null;
                
                if (isGardenItem)
                {
                    var gardenItem = XmlReadBuffer.ReadStatic<GardenItem>(itemElement.ToString());;
                    
                    // todo: item color(?)
                    nest.m_gardenItems.Add(new NestGardenItemDB
                    {
                        m_itemTypeID = (await m_dbContext.FindItemByConfigName(gardenItem.m_fileName)).Value,
                        m_color = gardenItem.m_color,
                        m_placedItem = new NestPlacedGardenItemDB
                        {
                            m_room = locMap[gardenItem.m_locID],
                            m_x = gardenItem.m_x,
                            m_z = gardenItem.m_z
                        }
                    });
                    continue;
                }
                
                var nestItem = XmlReadBuffer.ReadStatic<NestItem>(itemElement.ToString());
                if (nestItem.m_placedOnFurnitureID != 0) continue;
                await CreateNestItem(nestItem);
            }
            
            // populate furniture ids, we will need this for ornament identities
            await m_dbContext.SaveChangesAsync();
            
            // create ornaments
            foreach (var itemElement in xdoc.Root!.Elements("item"))
            {
                var isGardenItem = itemElement.Attribute("fName") != null;
                if (isGardenItem) continue;
                
                var nestItem = XmlReadBuffer.ReadStatic<NestItem>(itemElement.ToString());
                if (nestItem.m_placedOnFurnitureID == 0) continue;
                await CreateNestItem(nestItem);
            }

            return nest;

            async Task CreateNestItem(NestItem nestItem)
            {
                NestItemDB? placedOnFurniture = null;
                if (nestItem.m_placedOnFurnitureID != 0)
                {
                    placedOnFurniture = nestItemMap[nestItem.m_placedOnFurnitureID];
                    if (placedOnFurniture.m_id == 0)
                    {
                        throw new InvalidOperationException("furniture identity not populated");
                    }
                }
                
                var itemTypeID = await m_dbContext.m_itemTypes
                    .Where(x => x.m_configLocation == nestItem.m_configName)
                    .Select(x => x.m_itemTypeID)
                    .FirstOrDefaultAsync();
                if (itemTypeID == 0) return; // ceiling
                
                var dbItem = new NestItemDB
                {
                    m_itemTypeID = itemTypeID,
                    m_color = nestItem.m_color,
                    m_placedItem = new NestPlacedItemDB
                    {
                        m_room = locMap[nestItem.m_locID],
                        
                        m_posAnimationFrame = nestItem.m_currentPos,
                        m_placedOnFurniture = placedOnFurniture?.m_placedItem,
                        m_posIdentity = placedOnFurniture?.m_id ?? itemTypeID,
                        m_spotOnFurniture = nestItem.m_spot,
                    }
                };
                nestItemMap.Add(nestItem.m_databaseID, dbItem);
                nest.m_items.Add(dbItem);
            }
        }
        
        private const string ZINGY_NEST = """
            <?xml version="1.0" encoding="UTF-8"?>
            <nestConfig id="5857393" idx="6714151" lastUpdate="2020-08-18 04:02:42" score="8300" canSubmit="0" gardenCanSubmit="0" fuel="46807" weevilXp="146138" gardenSize="1">
              <loc id="1" instanceID="251624512" colour="0|0|0"/>
              <loc id="2" instanceID="253877569" colour="-120|-120|-120"/>
              <loc id="3" instanceID="246492737" colour="0|0|0"/>
              <loc id="4" instanceID="19551435" colour="-74|36|-100"/>
              <loc id="5" instanceID="19551436" colour="-120|-120|60"/>
              <loc id="6" instanceID="34518426" colour="-120|-120|-120"/>
              <loc id="7" instanceID="246318423" colour="-120|-120|-120"/>
              <loc id="8" instanceID="22420125" colour="-120|-120|-120"/>
              <loc id="9" instanceID="34518411" colour="60|-30|-120"/>
              <loc id="10" instanceID="22390190" colour="-120|-120|-120"/>
              <loc id="20" instanceID="19551437" colour="-8|-6|2"/>
              <loc id="50" instanceID="22390189" colour="60|-26|-120"/>
              <loc id="51" instanceID="36595934" name="Dance Fushion" busType="2" signClr="2631720" signTxtClr="15597568" playList="4,7" busOpen="0" colour="-120|-120|-120"/>
              <loc id="52" instanceID="249607204" name="Zingy zangy club" busType="2" signClr="16750848" signTxtClr="153" playList="6" busOpen="1" colour="-120|-120|30"/>
              <loc id="53" instanceID="249607205" name="" busType="2" signClr="13369548" signTxtClr="16777145" playList="" busOpen="0" colour="-120|-120|-120"/>
              <loc id="54" instanceID="249607212" name="" busType="2" signClr="16750848" signTxtClr="0" playList="" busOpen="0" colour="-120|-120|-120"/>
              <loc id="55" instanceID="22474294" name="" busType="7" signClr="-1" signTxtClr="-1" playList="" busOpen="1" colour="-120|-120|-120"/>
              <item id="75750048" cat="1" configName="o_egg" locID="251624512" pc="1" crntPos="0" fID="136219751" spot="0"/>
              <item id="75774142" cat="1" configName="o_trophy_SWS" locID="246492737" pc="1" crntPos="0" fID="230181161" spot="1"/>
              <item id="75779055" cat="1" configName="f_quest_trophy" locID="19551435" pc="1" crntPos="7" fID="0" spot=""/>
              <item id="77344126" cat="1" configName="f_chair5" locID="22420125" pc="1" crntPos="5" fID="0" spot=""/>
              <item id="77643644" cat="1" configName="f_wallLight1" locID="246492737" clr="0xF9F19B" pc="1" crntPos="5" fID="0" spot=""/>
              <item id="77644419" cat="1" configName="f_wallLight1" locID="246492737" clr="0xF9F19B" pc="1" crntPos="9" fID="0" spot=""/>
              <item id="85026872" cat="1" configName="f_bookcase_macmillan" locID="251624512" pc="1" crntPos="1" fID="0" spot=""/>
              <item id="86821070" cat="1" configName="f_petBowl_orange" locID="246492737" pc="1" crntPos="20" fID="0" spot=""/>
              <item id="86821071" cat="1" configName="f_petBasket2" locID="246492737" clr="16759552" pc="1" crntPos="10" fID="0" spot=""/>
              <item id="86910844" cat="6" configName="f_VODskin3" locID="22390190" clr="-150,0,112" pc="5" crntPos="1" fID="0" spot=""/>
              <item id="92799845" cat="1" configName="o_awardBENFTA" locID="246492737" pc="1" crntPos="0" fID="230181161" spot="2"/>
              <item id="94896929" cat="1" configName="f_fireExtinguisher" locID="246492737" pc="1" crntPos="10" fID="0" spot=""/>
              <item id="101892445" cat="1" configName="o_bing2" locID="246492737" pc="1" crntPos="0" fID="230181154" spot="2"/>
              <item id="103945884" cat="1" configName="o_gong2" locID="246492737" pc="1" crntPos="0" fID="230181188" spot="0"/>
              <item id="104279778" cat="1" configName="o_clott2" locID="251624512" pc="1" crntPos="0" fID="136220344" spot="0"/>
              <item id="111635266" cat="6" configName="f_VODmenuTab2" locID="22390190" clr="112,0,-150" pc="3" crntPos="1" fID="0" spot=""/>
              <item id="111642684" cat="6" configName="wallpaper_VODtt" locID="22390190" clr="112,-77,-240" pc="0" crntPos="" fID="0" spot=""/>
              <item id="111665654" cat="6" configName="f_VODcushion1" locID="22390190" clr="110,115,115" pc="0" crntPos="13" fID="0" spot=""/>
              <item id="111666077" cat="6" configName="f_VODcushion1" locID="22390190" clr="110,115,115" pc="0" crntPos="1" fID="0" spot=""/>
              <item id="112289839" cat="1" configName="f_dishWasher" locID="34518411" pc="3" crntPos="13" fID="0" spot=""/>
              <item id="112377947" cat="1" configName="f_kitchenWorkTop3" locID="34518411" clr="0xFFFFFF" pc="1" crntPos="1" fID="0" spot=""/>
              <item id="112850409" cat="6" configName="f_VODspeaker3" locID="22390190" clr="150,110,0" pc="6" crntPos="5" fID="0" spot=""/>
              <item id="112850435" cat="6" configName="f_VODspeaker3" locID="22390190" clr="150,110,0" pc="6" crntPos="1" fID="0" spot=""/>
              <item id="113107049" cat="1" configName="f_fridgeTall1" locID="34518411" pc="3" crntPos="11" fID="0" spot=""/>
              <item id="113129673" cat="1" configName="f_washingMachine1" locID="34518411" pc="3" crntPos="12" fID="0" spot=""/>
              <item id="114305469" cat="1" configName="o_wwTrophy1" locID="251624512" clr="12425032" pc="1" crntPos="0" fID="136220344" spot="2"/>
              <item id="114374047" cat="6" configName="f_VODspeaker3" locID="22390190" clr="-125,-75,100" pc="6" crntPos="6" fID="0" spot=""/>
              <item id="114374053" cat="6" configName="f_VODspeaker3" locID="22390190" clr="-125,-75,100" pc="6" crntPos="2" fID="0" spot=""/>
              <item id="115593068" cat="1" configName="wallpaper_tycoonIsland" locID="34518411" pc="1" crntPos="" fID="0" spot=""/>
              <item id="115593069" cat="1" configName="floor_tycoonIsland" locID="34518411" pc="1" crntPos="" fID="0" spot=""/>
              <item id="116172660" cat="1" configName="f_kitchenWorkTop3" locID="34518411" clr="0xFFFFFF" pc="1" crntPos="15" fID="0" spot=""/>
              <item id="116182841" cat="6" configName="carpet_VODroomTiles" locID="22390190" clr="-80,-80,0" pc="0" crntPos="" fID="0" spot=""/>
              <item id="116533209" cat="1" configName="f_kitchenSink" locID="34518411" pc="1" crntPos="9" fID="0" spot=""/>
              <item id="116712884" cat="1" configName="f_unitTallCorner1" locID="34518411" clr="0xFF8484" pc="1" crntPos="2" fID="0" spot=""/>
              <item id="116777071" cat="1" configName="f_cooker_v2" locID="34518411" pc="3" crntPos="5" fID="0" spot=""/>
              <item id="116968836" cat="1" configName="f_unitSinkCladding1" locID="34518411" clr="0x1111AA" pc="1" crntPos="9" fID="0" spot=""/>
              <item id="118142755" cat="1" configName="f_unitSinkCladding1" locID="34518411" clr="0xFFFFFF" pc="1" crntPos="15" fID="0" spot=""/>
              <item id="118142813" cat="1" configName="f_unitSinkCladding1" locID="34518411" clr="0xFFFFFF" pc="1" crntPos="1" fID="0" spot=""/>
              <item id="121527173" cat="1" configName="f_welcomeSign" locID="251624512" pc="1" crntPos="2" fID="0" spot=""/>
              <item id="125805057" cat="1" configName="wallpaper_binPet" locID="22420125" pc="1" crntPos="" fID="0" spot=""/>
              <item id="135957507" cat="1" configName="o_bowlingTrophy_goldenBall" locID="251624512" pc="1" crntPos="0" fID="136219855" spot="0"/>
              <item id="136216241" cat="1" configName="f_FW_clott_bed" locID="251624512" pc="1" crntPos="5" fID="0" spot=""/>
              <item id="136216273" cat="1" configName="wallpaper_FW_clott" locID="251624512" pc="1" crntPos="" fID="0" spot=""/>
              <item id="136216293" cat="1" configName="floor_FW_clott" locID="251624512" pc="1" crntPos="" fID="0" spot=""/>
              <item id="136218404" cat="1" configName="o_bowlingTrophy_tink" locID="251624512" pc="1" crntPos="0" fID="136219827" spot="0"/>
              <item id="136219638" cat="1" configName="f_romancolumn" locID="251624512" clr="0xFBEDC4" pc="1" crntPos="17" fID="0" spot=""/>
              <item id="136219751" cat="1" configName="f_shelf1" locID="251624512" pc="1" crntPos="8" fID="0" spot=""/>
              <item id="136219827" cat="1" configName="f_shelf1" locID="251624512" pc="1" crntPos="2" fID="0" spot=""/>
              <item id="136219855" cat="1" configName="f_shelf1" locID="251624512" pc="1" crntPos="3" fID="0" spot=""/>
              <item id="136220344" cat="1" configName="f_shelf1" locID="251624512" pc="1" crntPos="5" fID="0" spot=""/>
              <item id="136235114" cat="1" configName="rug_FW_clott" locID="251624512" pc="1" crntPos="" fID="0" spot=""/>
              <item id="137493960" cat="1" configName="f_CP_BuntyVIPpass" locID="251624512" pc="1" crntPos="6" fID="0" spot=""/>
              <item id="137718741" cat="1" configName="o_dosh2" locID="246492737" pc="1" crntPos="0" fID="230181154" spot="1"/>
              <item id="139449249" cat="1" configName="f_CP_snappyCamera" locID="34518426" pc="1" crntPos="18" fID="0" spot=""/>
              <item id="139556032" cat="1" configName="f_flingCostume2" locID="251624512" pc="1" crntPos="10" fID="0" spot=""/>
              <item id="144297819" cat="1" configName="o_blueDiamond" locID="246492737" pc="1" crntPos="0" fID="230181161" spot="0"/>
              <item id="144952006" cat="2" configName="f_tycoon_juiceBar" locID="249607204" clr="0xFFFFFF" pc="5" crntPos="1" fID="0" spot=""/>
              <item id="144952021" cat="2" configName="f_tycoon_cornerSeat" locID="249607204" clr="0xFF9900" pc="1" crntPos="2" fID="0" spot=""/>
              <item id="145198489" cat="2" configName="f_tycoon_wallSpeaker" locID="249607204" clr="0xFFFFFF" pc="6" crntPos="8" fID="0" spot=""/>
              <item id="145198615" cat="2" configName="f_tycoon_wallSpeaker" locID="249607204" clr="0xFFFFFF" pc="6" crntPos="1" fID="0" spot=""/>
              <item id="145671152" cat="1" configName="floor_binsy_garden" locID="19551435" pc="1" crntPos="" fID="0" spot=""/>
              <item id="145714008" cat="1" configName="wallpaper_binsy_garden" locID="19551435" pc="1" crntPos="" fID="0" spot=""/>
              <item id="146876722" cat="1" configName="f_HangingChairPod" locID="19551435" pc="1" crntPos="3" fID="0" spot=""/>
              <item id="146876724" cat="1" configName="f_HangingChairPod" locID="19551435" pc="1" crntPos="9" fID="0" spot=""/>
              <item id="147408905" cat="1" configName="f_easter_mushroomStool5" locID="19551435" pc="1" crntPos="6" fID="0" spot=""/>
              <item id="203562752" cat="1" configName="f_easter_mushroomStool5" locID="19551435" pc="1" crntPos="14" fID="0" spot=""/>
              <item id="203562889" cat="1" configName="f_logTable" locID="19551435" pc="1" crntPos="20" fID="0" spot=""/>
              <item id="203619340" cat="1" configName="f_logTable" locID="19551435" pc="1" crntPos="5" fID="0" spot=""/>
              <item id="207395445" cat="1" configName="f_FW_scribbles_teddy" locID="251624512" pc="1" crntPos="16" fID="0" spot=""/>
              <item id="207442018" cat="1" configName="f_LabInvisibilityPotion" locID="34518426" pc="1" crntPos="4" fID="0" spot=""/>
              <item id="212749727" cat="3" configName="facade_nightClub4" locID="22390189" clr="0xFFFFFF" pc="7" crntPos="1" fID="0" spot=""/>
              <item id="214273490" cat="1" configName="o_scribbles2" locID="246492737" pc="1" crntPos="0" fID="230181154" spot="0"/>
              <item id="214423120" cat="1" configName="f_CP_jukeBox" locID="251624512" pc="3" crntPos="2" fID="0" spot=""/>
              <item id="217444134" cat="1" configName="f_jubilee_Throne" locID="19551435" pc="1" crntPos="2" fID="0" spot=""/>
              <item id="220713935" cat="1" configName="ceiling_under_sea_4" locID="251624512" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="220727843" cat="1" configName="ceiling_ClubFling" locID="22420125" clr="0,0,0" pc="4" crntPos="" fID="0" spot=""/>
              <item id="222516736" cat="1" configName="f_binPet_BeanBagOrange" locID="246492737" clr="0,0,0" pc="1" crntPos="3" fID="0" spot=""/>
              <item id="223896918" cat="1" configName="f_binPet_rug3" locID="246492737" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="224524281" cat="1" configName="clock8" locID="246492737" clr="0,0,0" pc="1" crntPos="5" fID="0" spot=""/>
              <item id="230181154" cat="1" configName="f_binPet_Shelf1" locID="246492737" clr="0,0,0" pc="1" crntPos="3" fID="0" spot=""/>
              <item id="230181161" cat="1" configName="f_binPet_Shelf1" locID="246492737" clr="0,0,0" pc="1" crntPos="1" fID="0" spot=""/>
              <item id="230181188" cat="1" configName="f_binPet_Shelf1" locID="246492737" clr="0,0,0" pc="1" crntPos="4" fID="0" spot=""/>
              <item id="230181292" cat="1" configName="f_binPetHouseRed" locID="246492737" clr="0,0,0" pc="1" crntPos="5" fID="0" spot=""/>
              <item id="230181340" cat="1" configName="f_binPet_RibbonSet1Blue" locID="246492737" clr="0,0,0" pc="1" crntPos="4" fID="0" spot=""/>
              <item id="236249125" cat="1" configName="f_Pan_GongSticker" locID="246492737" pc="1" crntPos="13" fID="0" spot=""/>
              <item id="236250739" cat="4" configName="ps_Gong_podium" locID="22474294" pc="1" crntPos="1" fID="0" spot=""/>
              <item id="236250769" cat="4" configName="ps_Gong_torch" locID="22474294" pc="1" crntPos="6" fID="0" spot=""/>
              <item id="236250793" cat="4" configName="ps_Gong_torch" locID="22474294" pc="1" crntPos="3" fID="0" spot=""/>
              <item id="236250831" cat="4" configName="ps_Gong_bg" locID="22474294" pc="1" crntPos="" fID="0" spot=""/>
              <item id="236802191" cat="1" configName="wallpaper_flip_blue" locID="246492737" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="236802253" cat="1" configName="f_flip_gymMatressYellow" locID="246492737" clr="0,0,0" pc="1" crntPos="6" fID="0" spot=""/>
              <item id="236802307" cat="1" configName="floor_flip_UnionJack" locID="246492737" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="236803022" cat="1" configName="o_coffeeMachine2" locID="34518411" clr="0,0,0" pc="2" crntPos="0" fID="116172660" spot="1"/>
              <item id="238028759" cat="2" configName="f_party_stage1" locID="36595934" pc="4" crntPos="2" fID="0" spot=""/>
              <item id="238029530" cat="1" configName="f_flip_chevalArcon" locID="246492737" clr="0,0,0" pc="1" crntPos="14" fID="0" spot=""/>
              <item id="240457732" cat="1" configName="wallpaper_CP_cafe" locID="34518426" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="240457773" cat="1" configName="ceiling_CP_cafe" locID="34518426" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="240768269" cat="1" configName="f_TinkThrone" locID="34518426" pc="1" crntPos="3" fID="0" spot=""/>
              <item id="240768509" cat="1" configName="f_ClottThrone" locID="34518426" pc="1" crntPos="1" fID="0" spot=""/>
              <item id="240952476" cat="1" configName="f_binPet_Poster4" locID="246492737" pc="1" crntPos="1" fID="0" spot=""/>
              <item id="240952593" cat="1" configName="f_CP_doshHatMonocle" locID="34518426" pc="1" crntPos="3" fID="0" spot=""/>
              <item id="241178785" cat="1" configName="floor_CP_cafe" locID="34518426" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="241899446" cat="1" configName="wallpaper_LabsLab_1" locID="246318423" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="241899617" cat="1" configName="ceiling_LabsLab_2" locID="246318423" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="243131773" cat="1" configName="floor_LabsLab_1" locID="246318423" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="243131936" cat="1" configName="f_cctv1" locID="246318423" clr="0,0,0" pc="2" crntPos="1" fID="0" spot=""/>
              <item id="243132056" cat="1" configName="f_cctv1" locID="246318423" clr="0,0,0" pc="2" crntPos="4" fID="0" spot=""/>
              <item id="245581777" cat="1" configName="f_LLD_Trophy2" locID="246318423" pc="1" crntPos="2" fID="0" spot=""/>
              <item id="248936657" cat="1" configName="f_CP_flamPaperPlane" locID="34518426" pc="1" crntPos="18" fID="0" spot=""/>
              <item id="249278609" cat="3" configName="facade_nightClub_BouncyCastle" locID="22390189" pc="7" crntPos="3" fID="0" spot=""/>
              <item id="249278927" cat="2" configName="f_party_wallLight_lines" locID="249607204" clr="0xFFFFFF" pc="4" crntPos="2" fID="0" spot=""/>
              <item id="249278954" cat="2" configName="f_party_wallLight_lines" locID="249607204" clr="0xFFFFFF" pc="4" crntPos="1" fID="0" spot=""/>
              <item id="249281940" cat="1" configName="o_levelTrophy54" locID="251624512" pc="1" crntPos="0" fID="136219638" spot="0"/>
              <item id="249384793" cat="2" configName="floor_disco1" locID="249607204" clr="0xFF84E0" pc="5" crntPos="" fID="0" spot=""/>
              <item id="249501958" cat="1" configName="f_jacuzzi" locID="253877569" clr="0,0,0" pc="3" crntPos="6" fID="0" spot=""/>
              <item id="249567157" cat="4" configName="ps_Gong_floor" locID="22474294" pc="1" crntPos="" fID="0" spot=""/>
              <item id="250363911" cat="1" configName="floor_bathroomTiles" locID="253877569" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="250364013" cat="1" configName="wallpaper_NIbook_bathroomTiles" locID="253877569" clr="0,0,0" pc="1" crntPos="" fID="0" spot=""/>
              <item id="250364319" cat="1" configName="f_NIbook_BlingToilet" locID="253877569" clr="0,0,0" pc="1" crntPos="4" fID="0" spot=""/>
              <item id="252131593" cat="2" configName="f_tycoon_wallSpeaker" locID="249607204" clr="0xFFFFFF" pc="6" crntPos="6" fID="0" spot=""/>
              <item id="252131613" cat="2" configName="f_tycoon_wallSpeaker" locID="249607204" clr="0xFFFFFF" pc="6" crntPos="3" fID="0" spot=""/>
              <item id="255818166" cat="3" configName="facade_nightClub_gingerBread" locID="22390189" pc="7" crntPos="4" fID="0" spot=""/>
              <item id="261140125" cat="1" configName="carpet5" locID="22420125" clr="70,10,-50" pc="1" crntPos="" fID="0" spot=""/>
              <item id="261722227" cat="1" configName="f_wallLight1" locID="34518411" clr="-85,-32,0" pc="1" crntPos="1" fID="0" spot=""/>
              <item id="261722626" cat="1" configName="f_spotLight" locID="253877569" clr="-85,-32,0" pc="1" crntPos="1" fID="0" spot=""/>
              <item id="261722645" cat="1" configName="f_spotLight" locID="253877569" clr="-85,-32,0" pc="1" crntPos="11" fID="0" spot=""/>
              <item id="261722674" cat="1" configName="f_spotLight" locID="253877569" clr="-85,-32,0" pc="1" crntPos="15" fID="0" spot=""/>
              <item id="261722693" cat="1" configName="f_spotLight" locID="253877569" clr="-85,-32,0" pc="1" crntPos="5" fID="0" spot=""/>
              <item id="360412477" cat="1" configName="f_burple_5" locID="34518426" pc="1" crntPos="17" fID="0" spot=""/>
              <!-- getPositionedGardenItems  -->
              <item id="1630256" cat="5" fName="fence3" locID="19551437" pc="0" x="0" z="0" r="0"/>
              <item id="2163338" cat="5" fName="solarPanel" locID="19551437" pc="-250" x="117" z="452" r="22"/>
              <item id="2453158" cat="5" fName="wateringCan" locID="19551437" pc="0" x="148" z="406" r="20"/>
              <item id="3496968" cat="5" fName="gardenTable1_orange" locID="19551437" pc="5" x="-42" z="382" r="24"/>
              <item id="3496970" cat="5" fName="deckChairRed" locID="19551437" pc="0" x="-69" z="417" r="16"/>
              <!-- /getPositionedGardenItems  -->
            </nestConfig>
            """;
    }
}