using StackXML;

namespace BinWeevils.Protocol.Xml
{
    [XmlCls("locationDefinitions")]
    public partial class LocationDefinitions
    {
        [XmlField("version")] public string m_version;
        [XmlField("timestamp")] public string m_timestamp;

        [XmlBody] public List<LocationDefinition> m_locations;
    }

    [XmlCls("location")]
    public partial class LocationDefinition
    {
        [XmlField("id")] public int m_id;
        [XmlField("name")] public string m_name;
        [XmlField("type")] public int m_type;
        
        [XmlField("boundType")] public string m_boundType;
        [XmlField("boundary")] public string m_boundary;
        [XmlField("xWall")] public int m_xWall;
        [XmlField("zWall")] public int m_zWall;
        
        [XmlField("camMode")] public int m_camMode;
        [XmlField("camPos")] public string m_camPos;
        [XmlField("camAim")] public string m_camAim;
        [XmlField("camBounds")] public string m_camBounds;
        
        [XmlField("entryPos")] public string m_entryPos;
        [XmlField("entryDir")] public int m_entryDir;
        [XmlField("weevilScale")] public double m_weevilScale;
        
        [XmlField("timerID")] public int m_timerID;
        [XmlField("noZoom")] public bool m_noZoom;
        [XmlField("maintainY")] public bool m_maintainY;
        [XmlField("roomEvents")] public bool m_roomEvents;
        [XmlField("clickAnywhere")] public bool m_clickAnywhere;
        [XmlField("slippery")] public bool m_slippery;
        
        [XmlField("hasAudio")] public bool m_hasAudio;
        [XmlField("playList")] [XmlSplitStr] public List<int> m_playList;

        [XmlBody] public List<LocationCTA> m_ctas;
        [XmlBody] public List<LocationDoor> m_doors;
        [XmlBody] public List<LocationObject> m_objects;
        [XmlBody] public List<LocationInteractive> m_interactives;
        [XmlBody] public List<LocationNoGoArea> m_noGoAreas;
        [XmlBody] public List<LocationPreRend3D> m_preRend3Ds;
        [XmlBody] public List<LocationGameSlot> m_gameSlots;
        [XmlBody] public List<LocationSign> m_signs;
        [XmlBody] public List<LocationAnim> m_anims;
        [XmlBody] public List<LocationKart> m_karts;
        [XmlBody] public List<LocationTimeTrial> m_timeTrials;
        [XmlBody] public List<LocationTarget> m_targets;
        [XmlBody] public List<LocationBackdrop> m_backdrops;
        [XmlBody] public List<LocationTeleporter> m_teleporters;
        [XmlBody] public List<LocationSpinner> m_spinners;

        [XmlBody] public LocationRoomBG m_roomBG;
        [XmlBody] public LocationFloor m_floor;
        [XmlBody] public LocationLogic m_logic;
        [XmlBody] public LocationColMap m_colMap;

        [XmlBody] public List<LocationLibrary> m_libraries;
    }

    [XmlCls(null)]
    public partial class LocationSWFRef
    {
        [XmlField("path")] public string m_path;

        public virtual ReadOnlySpan<char> GetNodeName()
        {
            throw new NotSupportedException();
        }
    }
    
    [XmlCls("logic")] public partial class LocationLogic : LocationSWFRef
    {
    }
    
    [XmlCls("colMap")] public partial class LocationColMap : LocationSWFRef
    {
    }
    
    [XmlCls("roomBG")] public partial class LocationRoomBG : LocationSWFRef
    {
    }
    
    [XmlCls("floor")] public partial class LocationFloor : LocationSWFRef
    {
    }
    
    [XmlCls("cta")]
    public partial class LocationCTA
    {
        [XmlField("id")] public int m_id;
        [XmlField("extUIData")] public string? m_extUIData;
        [XmlField("x")] public int m_x;
        [XmlField("z")] public int m_z;
    }

    [XmlCls("door")]
    public partial class LocationDoor
    {
        [XmlField("id")] public int m_id;
        [XmlField("type")] public string m_type; // "vid" for cinema
        
        [XmlField("extUIData")] public string? m_extUIData;
        
        [XmlField("toLoc")] public int m_toLoc;
        [XmlField("toDoor")] public int m_toDoor;
        
        [XmlField("tycoonOnly")] public bool m_tycoonOnly;
        [XmlField("nonTyconOverlay")] public string m_nonTyconOverlay;
        
        [XmlField("y")] public int m_y; // ???

        [XmlField("x1")] public int m_x1;
        [XmlField("y1")] public int m_y1;
        [XmlField("z1")] public int m_z1;
        
        [XmlField("x2")] public int m_x2;
        [XmlField("y2")] public int m_y2;
        [XmlField("z2")] public int m_z2;
        
        [XmlField("entryDir")] public int m_entryDir;
    }

    [XmlCls("object")]
    public partial class LocationObject
    {
        [XmlField("name")] public string m_name;
        [XmlField("x")] public int m_x;
        [XmlField("y")] public int m_y;
        [XmlField("z")] public int m_z;
        
        [XmlField("type")] public string m_type;
        
        // gameSlot
        // todo: duplicate with gameslot
        [XmlField("lbl")] public string m_lbl;
        [XmlField("gamePath")] public string m_gamePath;
        [XmlField("slot")] public int m_slot;
        [XmlField("arrivalPoints")] public string m_arrivalPoints;
        [XmlField("playerPositions")] public string m_playerPositions;
    }

    [XmlCls("interactive")]
    public partial class LocationInteractive
    {
        [XmlField("type")] public string m_type;
        [XmlField("path")] public string m_path;
        [XmlField("actRect")] public string m_actRect;
    }

    [XmlCls("noGoArea")]
    public partial class LocationNoGoArea
    {
        [XmlField("type")] public string m_type;
        
        [XmlField("x")] public int m_x;
        [XmlField("z")] public int m_z;
        
        // rect
        [XmlField("w")] public int m_w;
        [XmlField("h")] public int m_h;
        
        // rad
        [XmlField("r")] public int m_r;
    }

    [XmlCls("preRend3D")]
    public partial class LocationPreRend3D
    {
        [XmlField("path")] public string m_path;
        [XmlField("path1")] public string m_initAnglesPath;
        
        [XmlField("logicID")] public int m_logicID;
        [XmlField("doorID")] public int m_doorID;
        [XmlField("extUIData")] public string? m_extUIData;
        
        [XmlField("boundary")] public string m_boundary;
        [XmlField("clr")] public string m_clr;
        [XmlField("bg")] public bool m_bg;
        
        [XmlField("x")] public double m_x;
        [XmlField("y")] public double m_y;
        [XmlField("z")] public double m_z;
        [XmlField("depthOffset")] public int m_depthOffset;
        [XmlField("scale")] public double m_scale;
        
        [XmlField("rx")] public double m_rx;
        [XmlField("ry")] public double m_ry;
        [XmlField("rxMin")] public double m_rxMin;
        [XmlField("rxMax")] public double m_rxMax;
        [XmlField("ryMin")] public double m_ryMin;
        [XmlField("ryMax")] public double m_ryMax;
        
        [XmlField("framesY")] public int m_framesY;
        [XmlField("symAxes")] public int m_symAxes;
        [XmlField("rIncr")] public double m_rIncr;
    }

    [XmlCls("gameSlot")]
    public partial class LocationGameSlot : LocationPreRend3D
    {
        // todo: duplicate with object
        [XmlField("lbl")] public string m_lbl;
        [XmlField("gamePath")] public string m_gamePath;
        [XmlField("slot")] public int m_slot;
        [XmlField("arrivalPoints")] public string m_arrivalPoints;
        [XmlField("playerPositions")] public string m_playerPositions;
    }
    
    [XmlCls("sign")]
    public partial class LocationSign : LocationPreRend3D
    {
        [XmlField("txt")] public string m_text;
    }

    [XmlCls("anim")]
    public partial class LocationAnim
    {
        [XmlField("path")] public string m_path;
    }

    [XmlCls("kart")]
    public partial class LocationKart
    {
        [XmlField("id")] public int m_id;
        [XmlField("hatch")] public bool m_hatch;
        [XmlField("path")] public string m_path;
        [XmlField("gamePath")] public string m_gamePath;
        [XmlField("track")] public string m_track;
        [XmlField("clr")] public string m_clr;
        [XmlField("numPlayers")] public int m_numPlayers;
        [XmlField("playerID")] public int m_playerID;
        
        [XmlField("x")] public int m_x;
        [XmlField("y")] public int m_y;
        [XmlField("z")] public int m_z;
        [XmlField("ry")] public int m_ry;
        [XmlField("scale")] public double m_scale;
        
        [XmlField("arrivalPos")] public string m_arrivalPos;
        [XmlField("exitDest")] public string m_exitDest;
    }
    
    [XmlCls("timeTrial")]
    public partial class LocationTimeTrial
    {
        [XmlField("id")] public int m_id;
        [XmlField("path")] public string m_path;
        [XmlField("gamePath")] public string m_gamePath;
        [XmlField("track")] public string m_track;
        [XmlField("doorID")] public int m_doorID;
    }
    
    [XmlCls("target")]
    public partial class LocationTarget
    {
        [XmlField("x")] public double m_x;
        [XmlField("y")] public double m_y;
        [XmlField("z")] public double m_z;
        [XmlField("rad")] public int m_rad;
        [XmlField("ori")] public int m_ori;
        [XmlField("dynam")] public bool m_dynam;
        [XmlField("indestructible")] public bool m_indestructible;
        [XmlField("useShape")] public bool m_useShape;
        [XmlField("autoPlay")] public bool m_autoPlay;
        [XmlField("useChildren")] public bool m_useChildren;
    }

    [XmlCls("library")]
    public partial class LocationLibrary
    {
        [XmlBody] public List<LocationLibraryPin> m_pins;
        [XmlBody] public LocationLibraryLightning m_lightning;

        [XmlField("id")] public int m_id;
        [XmlField("path")] public string m_path;
    }

    [XmlCls("pin")]
    public partial class LocationLibraryPin
    {
        [XmlField("x1")] public double m_x1;
        [XmlField("y1")] public double m_y1;
        [XmlField("z1")] public double m_z1;
        
        [XmlField("x2")] public double m_x2;
        [XmlField("y2")] public double m_y2;
        [XmlField("z2")] public double m_z2;
        
        [XmlField("scale")] public double m_scale;
    }

    [XmlCls("lightning")]
    public partial class LocationLibraryLightning
    {
        
    }
    
    [XmlCls("backdrop")]
    public partial class LocationBackdrop
    {
        [XmlField("path")] public string m_path;
        [XmlField("x")] public int m_x;
        [XmlField("y")] public int m_y;
        [XmlField("z")] public int m_z;
    }
    
    [XmlCls("teleporter")]
    public partial class LocationTeleporter
    {
        [XmlField("x")] public int m_x;
        [XmlField("y")] public int m_y;
        [XmlField("z")] public int m_z;
        [XmlField("scale")] public double m_scale;
        [XmlField("destLocID")] public int m_destLocID;
        [XmlField("logicID")] public int m_logicID;
    }
    
    [XmlCls("spinner")]
    public partial class LocationSpinner
    {
        [XmlField("x")] public int m_x;
        [XmlField("y")] public int m_y;
        [XmlField("z")] public int m_z;
        [XmlField("scale")] public double m_scale;
    }
}