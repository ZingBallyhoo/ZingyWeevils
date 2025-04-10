using System.Net.Mime;
using BinWeevils.Protocol;
using BinWeevils.Protocol.Form;
using BinWeevils.Protocol.Xml;
using ByteDev.FormUrlEncoded;
using Microsoft.AspNetCore.Mvc;

namespace BinWeevils.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class Proto : Controller
    {
        private readonly IConfiguration m_configuration;
        
        public Proto(IConfiguration configuration)
        {
            m_configuration = configuration;
        }
        
        [StructuredFormPost("binConfig/{cluster}/checkVersion.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public CheckVersionResponse CheckVersion([FromBody] CheckVersionRequest r)
        {
            return new CheckVersionResponse
            {
                m_ok = 1,
                m_coreVersionNumber = 18,
                m_vodPlayerVersion = 12,
                m_vodContentVersion = 2,
            };
        }
        
        [StructuredFormPost("php/getAdPaths.php")]
        [StructuredFormPost("binConfig/{cluster}/getAdPaths.php")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public AdPathsResponse GetAdPaths([FromBody] AdPathsRequest request)
        {
            //if (request.m_area == AdPathsArea.LOADER)
            //{
            //    return new AdPathsResponse
            //    {
            //        m_paths = ["1", "2"]
            //    };
            //}
            return new AdPathsResponse();
        }
        
        [HttpGet("binConfig/config.xml")]
        [Produces(MediaTypeNames.Application.Xml)]
        public SiteConfig GetConfig()
        {
            const string baseUrl = "http://localhost:80/";
            return new SiteConfig
            {
                m_domain = $"{baseUrl}",
                m_allowMultipleLogins = "true",
                m_servicesLocation = $"{baseUrl}",
                m_restrictFlashPlayers = "true",
                m_basePathSmall = $"{baseUrl}cdn/",
                m_basePathLarge = $"{baseUrl}cdn/",
                m_pathItemConfigs = $"{baseUrl}cdn/users/",
                m_pathAssetsNest = $"{baseUrl}cdn/users/",
                m_pathAssetsTycoon = $"{baseUrl}cdn/users/",
                m_pathAssetsGarden = $"{baseUrl}cdn/assetsGarden/",
                m_pathAssets3D = $"{baseUrl}cdn/assets3D/"
            };
        }
        
        [HttpGet("site/zones")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public ActiveZonesResponse GetActiveZones()
        {
            return new ActiveZonesResponse
            {
                m_names = ["Grime"],
                m_ips = ["127.0.0.1"],
                m_outOf5 = [5],
                m_responseCode = true
            };
        }
        
        [HttpGet("binConfig/getFile/0/locationDefinitions.xml")]
        [Produces(MediaTypeNames.Application.Xml)]
        public IResult GetLocationDefinitions()
        {
            return Results.File(m_configuration["LocationDefinitions"]!);
        }
        
        [HttpGet("binConfig/getFile/0/nestLocDefs.xml")]
        [Produces(MediaTypeNames.Application.Xml)]
        public IResult GetNestLocationDefinitions()
        {
            return Results.File(m_configuration["NestLocationDefinitions"]!);
        }
        
        [HttpGet("nest/get-weevil-stats")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public WeevilStatsResponse GetWeevilStats()
        {
            var stats = new WeevilStatsResponse
            {
                m_level = 99,
                m_mulch = 1000,
                m_xp = 0,
                m_xpLowerThreshold = 0,
                m_xpUpperThreshold = 1000,
                m_food = 100,
                m_fitness = 100,
                m_happiness = 100,
                m_activated = 1,
                m_daysRemaining = 99,
                m_chatState = true,
                m_chatKey = 0,
                m_serverTime = 0
            };
            
            var hashStr = string.Join("", new object[]
            {
                stats.m_level,
                stats.m_mulch,
                stats.m_xp,
                stats.m_xpLowerThreshold,
                stats.m_xpUpperThreshold,
                stats.m_food,
                stats.m_fitness,
                stats.m_happiness,
                
                stats.m_activated,
                stats.m_daysRemaining,
                
                //chatState,
                //chatKey,
                
                stats.m_serverTime
            }.Select(x => x.ToString()));
            
            stats.m_hash = Rssmv.Hash(hashStr);
            return stats;
        }
        
        [HttpGet("weevil/get-progress")]
        public string GetIntroProgress()
        {
            return "res=0";
        }
        
        [HttpGet("php/getTreasureHunt.php")]
        public string GetTreasureHunt()
        {
            return "responseCode=2&failed=0";
        }
        
        [HttpGet("php/getQuestData.php")] // old
        public string GetQuestData()
        {
            return "tasks=&itemList=&b=r&responseCode=1";
        }
        
        [HttpPost("pets/defs")]
        public string GetPetDefs()
        {
            return "result=";
        }
        
        [HttpGet("php/getMyLottoTicketsAndDrawDate.php")]
        public string GetMyLottoTickets()
        {
            return "responseCode=1&nextDraw=2020-08-18+17%3A00%3A00&drawID=1479&gotTicket=0&tickets=0";
        }
        
        [HttpPost("nest/getconfig")]
        public string GetNestConfig()
        {
            var idx = 55; // todo: luckily this doesnt matter
            return
            $"<?xml version=\"1.0\" encoding=\"UTF-8\"?><nestConfig id='5857393' idx='{idx}' lastUpdate='2020-08-18 04:02:42' score='8300' canSubmit='0' gardenCanSubmit='0' fuel='46807' weevilXp='146138' gardenSize='1'><loc id='1' instanceID='251624512' colour='0|0|0'/><loc id='2' instanceID='253877569' colour='-120|-120|-120'/><loc id='3' instanceID='246492737' colour='0|0|0'/><loc id='4' instanceID='19551435' colour='-74|36|-100'/><loc id='5' instanceID='19551436' colour='-120|-120|60'/><loc id='6' instanceID='34518426' colour='-120|-120|-120'/><loc id='7' instanceID='246318423' colour='-120|-120|-120'/><loc id='8' instanceID='22420125' colour='-120|-120|-120'/><loc id='9' instanceID='34518411' colour='60|-30|-120'/><loc id='10' instanceID='22390190' colour='-120|-120|-120'/><loc id='20' instanceID='19551437' colour='-8|-6|2'/><loc id='50' instanceID='22390189' colour='60|-26|-120'/><loc id='51' instanceID='36595934' name='Dance Fushion' busType='2' signClr='2631720' signTxtClr='15597568' playList='4,7' busOpen='0' colour='-120|-120|-120'/><loc id='52' instanceID='249607204' name='Zingy zangy club' busType='2' signClr='16750848' signTxtClr='153' playList='6' busOpen='1' colour='-120|-120|30'/><loc id='53' instanceID='249607205' name='' busType='2' signClr='13369548' signTxtClr='16777145' playList='' busOpen='0' colour='-120|-120|-120'/><loc id='54' instanceID='249607212' name='' busType='2' signClr='16750848' signTxtClr='0' playList='' busOpen='0' colour='-120|-120|-120'/><loc id='55' instanceID='22474294' name='' busType='7' signClr='-1' signTxtClr='-1' playList='' busOpen='1' colour='-120|-120|-120'/><item id='75750048' cat='1' configName='o_egg' locID='251624512' pc='1' crntPos='0' fID='136219751' spot='0'/><item id='75774142' cat='1' configName='o_trophy_SWS' locID='246492737' pc='1' crntPos='0' fID='230181161' spot='1'/><item id='75779055' cat='1' configName='f_quest_trophy' locID='19551435' pc='1' crntPos='7' fID='0' spot=''/><item id='77344126' cat='1' configName='f_chair5' locID='22420125' pc='1' crntPos='5' fID='0' spot=''/><item id='77643644' cat='1' configName='f_wallLight1' locID='246492737' clr='0xF9F19B' pc='1' crntPos='5' fID='0' spot=''/><item id='77644419' cat='1' configName='f_wallLight1' locID='246492737' clr='0xF9F19B' pc='1' crntPos='9' fID='0' spot=''/><item id='85026872' cat='1' configName='f_bookcase_macmillan' locID='251624512' pc='1' crntPos='1' fID='0' spot=''/><item id='86821070' cat='1' configName='f_petBowl_orange' locID='246492737' pc='1' crntPos='20' fID='0' spot=''/><item id='86821071' cat='1' configName='f_petBasket2' locID='246492737' clr='16759552' pc='1' crntPos='10' fID='0' spot=''/><item id='86910844' cat='6' configName='f_VODskin3' locID='22390190' clr='-150,0,112' pc='5' crntPos='1' fID='0' spot=''/><item id='92799845' cat='1' configName='o_awardBENFTA' locID='246492737' pc='1' crntPos='0' fID='230181161' spot='2'/><item id='94896929' cat='1' configName='f_fireExtinguisher' locID='246492737' pc='1' crntPos='10' fID='0' spot=''/><item id='101892445' cat='1' configName='o_bing2' locID='246492737' pc='1' crntPos='0' fID='230181154' spot='2'/><item id='103945884' cat='1' configName='o_gong2' locID='246492737' pc='1' crntPos='0' fID='230181188' spot='0'/><item id='104279778' cat='1' configName='o_clott2' locID='251624512' pc='1' crntPos='0' fID='136220344' spot='0'/><item id='111635266' cat='6' configName='f_VODmenuTab2' locID='22390190' clr='112,0,-150' pc='3' crntPos='1' fID='0' spot=''/><item id='111642684' cat='6' configName='wallpaper_VODtt' locID='22390190' clr='112,-77,-240' pc='0' crntPos='' fID='0' spot=''/><item id='111665654' cat='6' configName='f_VODcushion1' locID='22390190' clr='110,115,115' pc='0' crntPos='13' fID='0' spot=''/><item id='111666077' cat='6' configName='f_VODcushion1' locID='22390190' clr='110,115,115' pc='0' crntPos='1' fID='0' spot=''/><item id='112289839' cat='1' configName='f_dishWasher' locID='34518411' pc='3' crntPos='13' fID='0' spot=''/><item id='112377947' cat='1' configName='f_kitchenWorkTop3' locID='34518411' clr='0xFFFFFF' pc='1' crntPos='1' fID='0' spot=''/><item id='112850409' cat='6' configName='f_VODspeaker3' locID='22390190' clr='150,110,0' pc='6' crntPos='5' fID='0' spot=''/><item id='112850435' cat='6' configName='f_VODspeaker3' locID='22390190' clr='150,110,0' pc='6' crntPos='1' fID='0' spot=''/><item id='113107049' cat='1' configName='f_fridgeTall1' locID='34518411' pc='3' crntPos='11' fID='0' spot=''/><item id='113129673' cat='1' configName='f_washingMachine1' locID='34518411' pc='3' crntPos='12' fID='0' spot=''/><item id='114305469' cat='1' configName='o_wwTrophy1' locID='251624512' clr='12425032' pc='1' crntPos='0' fID='136220344' spot='2'/><item id='114374047' cat='6' configName='f_VODspeaker3' locID='22390190' clr='-125,-75,100' pc='6' crntPos='6' fID='0' spot=''/><item id='114374053' cat='6' configName='f_VODspeaker3' locID='22390190' clr='-125,-75,100' pc='6' crntPos='2' fID='0' spot=''/><item id='115593068' cat='1' configName='wallpaper_tycoonIsland' locID='34518411' pc='1' crntPos='' fID='0' spot=''/><item id='115593069' cat='1' configName='floor_tycoonIsland' locID='34518411' pc='1' crntPos='' fID='0' spot=''/><item id='116172660' cat='1' configName='f_kitchenWorkTop3' locID='34518411' clr='0xFFFFFF' pc='1' crntPos='15' fID='0' spot=''/><item id='116182841' cat='6' configName='carpet_VODroomTiles' locID='22390190' clr='-80,-80,0' pc='0' crntPos='' fID='0' spot=''/><item id='116533209' cat='1' configName='f_kitchenSink' locID='34518411' pc='1' crntPos='9' fID='0' spot=''/><item id='116712884' cat='1' configName='f_unitTallCorner1' locID='34518411' clr='0xFF8484' pc='1' crntPos='2' fID='0' spot=''/><item id='116777071' cat='1' configName='f_cooker_v2' locID='34518411' pc='3' crntPos='5' fID='0' spot=''/><item id='116968836' cat='1' configName='f_unitSinkCladding1' locID='34518411' clr='0x1111AA' pc='1' crntPos='9' fID='0' spot=''/><item id='118142755' cat='1' configName='f_unitSinkCladding1' locID='34518411' clr='0xFFFFFF' pc='1' crntPos='15' fID='0' spot=''/><item id='118142813' cat='1' configName='f_unitSinkCladding1' locID='34518411' clr='0xFFFFFF' pc='1' crntPos='1' fID='0' spot=''/><item id='121527173' cat='1' configName='f_welcomeSign' locID='251624512' pc='1' crntPos='2' fID='0' spot=''/><item id='125805057' cat='1' configName='wallpaper_binPet' locID='22420125' pc='1' crntPos='' fID='0' spot=''/><item id='135957507' cat='1' configName='o_bowlingTrophy_goldenBall' locID='251624512' pc='1' crntPos='0' fID='136219855' spot='0'/><item id='136216241' cat='1' configName='f_FW_clott_bed' locID='251624512' pc='1' crntPos='5' fID='0' spot=''/><item id='136216273' cat='1' configName='wallpaper_FW_clott' locID='251624512' pc='1' crntPos='' fID='0' spot=''/><item id='136216293' cat='1' configName='floor_FW_clott' locID='251624512' pc='1' crntPos='' fID='0' spot=''/><item id='136218404' cat='1' configName='o_bowlingTrophy_tink' locID='251624512' pc='1' crntPos='0' fID='136219827' spot='0'/><item id='136219638' cat='1' configName='f_romancolumn' locID='251624512' clr='0xFBEDC4' pc='1' crntPos='17' fID='0' spot=''/><item id='136219751' cat='1' configName='f_shelf1' locID='251624512' pc='1' crntPos='8' fID='0' spot=''/><item id='136219827' cat='1' configName='f_shelf1' locID='251624512' pc='1' crntPos='2' fID='0' spot=''/><item id='136219855' cat='1' configName='f_shelf1' locID='251624512' pc='1' crntPos='3' fID='0' spot=''/><item id='136220344' cat='1' configName='f_shelf1' locID='251624512' pc='1' crntPos='5' fID='0' spot=''/><item id='136235114' cat='1' configName='rug_FW_clott' locID='251624512' pc='1' crntPos='' fID='0' spot=''/><item id='137493960' cat='1' configName='f_CP_BuntyVIPpass' locID='251624512' pc='1' crntPos='6' fID='0' spot=''/><item id='137718741' cat='1' configName='o_dosh2' locID='246492737' pc='1' crntPos='0' fID='230181154' spot='1'/><item id='139449249' cat='1' configName='f_CP_snappyCamera' locID='34518426' pc='1' crntPos='18' fID='0' spot=''/><item id='139556032' cat='1' configName='f_flingCostume2' locID='251624512' pc='1' crntPos='10' fID='0' spot=''/><item id='144297819' cat='1' configName='o_blueDiamond' locID='246492737' pc='1' crntPos='0' fID='230181161' spot='0'/><item id='144952006' cat='2' configName='f_tycoon_juiceBar' locID='249607204' clr='0xFFFFFF' pc='5' crntPos='1' fID='0' spot=''/><item id='144952021' cat='2' configName='f_tycoon_cornerSeat' locID='249607204' clr='0xFF9900' pc='1' crntPos='2' fID='0' spot=''/><item id='145198489' cat='2' configName='f_tycoon_wallSpeaker' locID='249607204' clr='0xFFFFFF' pc='6' crntPos='8' fID='0' spot=''/><item id='145198615' cat='2' configName='f_tycoon_wallSpeaker' locID='249607204' clr='0xFFFFFF' pc='6' crntPos='1' fID='0' spot=''/><item id='145671152' cat='1' configName='floor_binsy_garden' locID='19551435' pc='1' crntPos='' fID='0' spot=''/><item id='145714008' cat='1' configName='wallpaper_binsy_garden' locID='19551435' pc='1' crntPos='' fID='0' spot=''/><item id='146876722' cat='1' configName='f_HangingChairPod' locID='19551435' pc='1' crntPos='3' fID='0' spot=''/><item id='146876724' cat='1' configName='f_HangingChairPod' locID='19551435' pc='1' crntPos='9' fID='0' spot=''/><item id='147408905' cat='1' configName='f_easter_mushroomStool5' locID='19551435' pc='1' crntPos='6' fID='0' spot=''/><item id='203562752' cat='1' configName='f_easter_mushroomStool5' locID='19551435' pc='1' crntPos='14' fID='0' spot=''/><item id='203562889' cat='1' configName='f_logTable' locID='19551435' pc='1' crntPos='20' fID='0' spot=''/><item id='203619340' cat='1' configName='f_logTable' locID='19551435' pc='1' crntPos='5' fID='0' spot=''/><item id='207395445' cat='1' configName='f_FW_scribbles_teddy' locID='251624512' pc='1' crntPos='16' fID='0' spot=''/><item id='207442018' cat='1' configName='f_LabInvisibilityPotion' locID='34518426' pc='1' crntPos='4' fID='0' spot=''/><item id='212749727' cat='3' configName='facade_nightClub4' locID='22390189' clr='0xFFFFFF' pc='7' crntPos='1' fID='0' spot=''/><item id='214273490' cat='1' configName='o_scribbles2' locID='246492737' pc='1' crntPos='0' fID='230181154' spot='0'/><item id='214423120' cat='1' configName='f_CP_jukeBox' locID='251624512' pc='3' crntPos='2' fID='0' spot=''/><item id='217444134' cat='1' configName='f_jubilee_Throne' locID='19551435' pc='1' crntPos='2' fID='0' spot=''/><!--<item id='220713935' cat='1' configName='ceiling_under_sea_4' locID='251624512' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/><item id='220727843' cat='1' configName='ceiling_ClubFling' locID='22420125' clr='0,0,0' pc='4' crntPos='' fID='0' spot=''/>--><item id='222516736' cat='1' configName='f_binPet_BeanBagOrange' locID='246492737' clr='0,0,0' pc='1' crntPos='3' fID='0' spot=''/><item id='223896918' cat='1' configName='f_binPet_rug3' locID='246492737' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/><item id='224524281' cat='1' configName='clock8' locID='246492737' clr='0,0,0' pc='1' crntPos='5' fID='0' spot=''/><item id='230181154' cat='1' configName='f_binPet_Shelf1' locID='246492737' clr='0,0,0' pc='1' crntPos='3' fID='0' spot=''/><item id='230181161' cat='1' configName='f_binPet_Shelf1' locID='246492737' clr='0,0,0' pc='1' crntPos='1' fID='0' spot=''/><item id='230181188' cat='1' configName='f_binPet_Shelf1' locID='246492737' clr='0,0,0' pc='1' crntPos='4' fID='0' spot=''/><item id='230181292' cat='1' configName='f_binPetHouseRed' locID='246492737' clr='0,0,0' pc='1' crntPos='5' fID='0' spot=''/><item id='230181340' cat='1' configName='f_binPet_RibbonSet1Blue' locID='246492737' clr='0,0,0' pc='1' crntPos='4' fID='0' spot=''/><item id='236249125' cat='1' configName='f_Pan_GongSticker' locID='246492737' pc='1' crntPos='13' fID='0' spot=''/><item id='236250739' cat='4' configName='ps_Gong_podium' locID='22474294' pc='1' crntPos='1' fID='0' spot=''/><item id='236250769' cat='4' configName='ps_Gong_torch' locID='22474294' pc='1' crntPos='6' fID='0' spot=''/><item id='236250793' cat='4' configName='ps_Gong_torch' locID='22474294' pc='1' crntPos='3' fID='0' spot=''/><item id='236250831' cat='4' configName='ps_Gong_bg' locID='22474294' pc='1' crntPos='' fID='0' spot=''/><item id='236802191' cat='1' configName='wallpaper_flip_blue' locID='246492737' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/><item id='236802253' cat='1' configName='f_flip_gymMatressYellow' locID='246492737' clr='0,0,0' pc='1' crntPos='6' fID='0' spot=''/><item id='236802307' cat='1' configName='floor_flip_UnionJack' locID='246492737' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/><item id='236803022' cat='1' configName='o_coffeeMachine2' locID='34518411' clr='0,0,0' pc='2' crntPos='0' fID='116172660' spot='1'/><item id='238028759' cat='2' configName='f_party_stage1' locID='36595934' pc='4' crntPos='2' fID='0' spot=''/><item id='238029530' cat='1' configName='f_flip_chevalArcon' locID='246492737' clr='0,0,0' pc='1' crntPos='14' fID='0' spot=''/><item id='240457732' cat='1' configName='wallpaper_CP_cafe' locID='34518426' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/><!--<item id='240457773' cat='1' configName='ceiling_CP_cafe' locID='34518426' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/>--><item id='240768269' cat='1' configName='f_TinkThrone' locID='34518426' pc='1' crntPos='3' fID='0' spot=''/><item id='240768509' cat='1' configName='f_ClottThrone' locID='34518426' pc='1' crntPos='1' fID='0' spot=''/><item id='240952476' cat='1' configName='f_binPet_Poster4' locID='246492737' pc='1' crntPos='1' fID='0' spot=''/><item id='240952593' cat='1' configName='f_CP_doshHatMonocle' locID='34518426' pc='1' crntPos='3' fID='0' spot=''/><item id='241178785' cat='1' configName='floor_CP_cafe' locID='34518426' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/><item id='241899446' cat='1' configName='wallpaper_LabsLab_1' locID='246318423' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/><!--<item id='241899617' cat='1' configName='ceiling_LabsLab_2' locID='246318423' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/>--><item id='243131773' cat='1' configName='floor_LabsLab_1' locID='246318423' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/><item id='243131936' cat='1' configName='f_cctv1' locID='246318423' clr='0,0,0' pc='2' crntPos='1' fID='0' spot=''/><item id='243132056' cat='1' configName='f_cctv1' locID='246318423' clr='0,0,0' pc='2' crntPos='4' fID='0' spot=''/><item id='245581777' cat='1' configName='f_LLD_Trophy2' locID='246318423' pc='1' crntPos='2' fID='0' spot=''/><item id='248936657' cat='1' configName='f_CP_flamPaperPlane' locID='34518426' pc='1' crntPos='18' fID='0' spot=''/><item id='249278609' cat='3' configName='facade_nightClub_BouncyCastle' locID='22390189' pc='7' crntPos='3' fID='0' spot=''/><item id='249278927' cat='2' configName='f_party_wallLight_lines' locID='249607204' clr='0xFFFFFF' pc='4' crntPos='2' fID='0' spot=''/><item id='249278954' cat='2' configName='f_party_wallLight_lines' locID='249607204' clr='0xFFFFFF' pc='4' crntPos='1' fID='0' spot=''/><item id='249281940' cat='1' configName='o_levelTrophy54' locID='251624512' pc='1' crntPos='0' fID='136219638' spot='0'/><item id='249384793' cat='2' configName='floor_disco1' locID='249607204' clr='0xFF84E0' pc='5' crntPos='' fID='0' spot=''/><item id='249501958' cat='1' configName='f_jacuzzi' locID='253877569' clr='0,0,0' pc='3' crntPos='6' fID='0' spot=''/><item id='249567157' cat='4' configName='ps_Gong_floor' locID='22474294' pc='1' crntPos='' fID='0' spot=''/><item id='250363911' cat='1' configName='floor_bathroomTiles' locID='253877569' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/><item id='250364013' cat='1' configName='wallpaper_NIbook_bathroomTiles' locID='253877569' clr='0,0,0' pc='1' crntPos='' fID='0' spot=''/><item id='250364319' cat='1' configName='f_NIbook_BlingToilet' locID='253877569' clr='0,0,0' pc='1' crntPos='4' fID='0' spot=''/><item id='252131593' cat='2' configName='f_tycoon_wallSpeaker' locID='249607204' clr='0xFFFFFF' pc='6' crntPos='6' fID='0' spot=''/><item id='252131613' cat='2' configName='f_tycoon_wallSpeaker' locID='249607204' clr='0xFFFFFF' pc='6' crntPos='3' fID='0' spot=''/><item id='255818166' cat='3' configName='facade_nightClub_gingerBread' locID='22390189' pc='7' crntPos='4' fID='0' spot=''/><item id='261140125' cat='1' configName='carpet5' locID='22420125' clr='70,10,-50' pc='1' crntPos='' fID='0' spot=''/><item id='261722227' cat='1' configName='f_wallLight1' locID='34518411' clr='-85,-32,0' pc='1' crntPos='1' fID='0' spot=''/><item id='261722626' cat='1' configName='f_spotLight' locID='253877569' clr='-85,-32,0' pc='1' crntPos='1' fID='0' spot=''/><item id='261722645' cat='1' configName='f_spotLight' locID='253877569' clr='-85,-32,0' pc='1' crntPos='11' fID='0' spot=''/><item id='261722674' cat='1' configName='f_spotLight' locID='253877569' clr='-85,-32,0' pc='1' crntPos='15' fID='0' spot=''/><item id='261722693' cat='1' configName='f_spotLight' locID='253877569' clr='-85,-32,0' pc='1' crntPos='5' fID='0' spot=''/><item id='360412477' cat='1' configName='f_burple_5' locID='34518426' pc='1' crntPos='17' fID='0' spot=''/><!-- getPositionedGardenItems  --><item id='1630256' cat='5' fName='fence3' locID='19551437' pc='0' x='0' z='0' r='0'/><item id='2163338' cat='5' fName='solarPanel' locID='19551437' pc='-250' x='117' z='452' r='22'/><item id='2453158' cat='5' fName='wateringCan' locID='19551437' pc='0' x='148' z='406' r='20'/><item id='3496968' cat='5' fName='gardenTable1_orange' locID='19551437' pc='5' x='-42' z='382' r='24'/><item id='3496970' cat='5' fName='deckChairRed' locID='19551437' pc='0' x='-69' z='417' r='16'/><!-- /getPositionedGardenItems  --></nestConfig>";
        }
        
        [HttpPost("php/getSpecialMoves.php")]
        public string GetSpecialMoves()
        {
            return "responseCode=1&result=23";
        }
        
        [HttpPost("weevil/geo")]
        [Produces(MediaTypeNames.Application.FormUrlEncoded)]
        public GeoResponse GetGeo()
        {
            return new GeoResponse
            {
                m_l = "uk"
            };
        }
        
        public class GeoResponse
        {
            [FormUrlEncodedPropertyName("l")] public string m_l { get; set; }
        }
        
        [HttpGet("site/server-time")]
        public string GetServerTime()
        {
            return "res=1&t=1597760479&x=y";
        }
    }
}