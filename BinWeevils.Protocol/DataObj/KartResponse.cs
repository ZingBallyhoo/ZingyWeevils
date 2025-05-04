using PolyType;

namespace BinWeevils.Protocol.DataObj
{
    [GenerateShape]
    public partial record KartResponse : XtResponse
    {
        [PropertyShape(Name = "command")] public string m_command;
    }
    
    [GenerateShape]
    public partial record KartNotification : XtNotification
    {
        [PropertyShape(Name = "command")] public string m_command;
    }
    
    [GenerateShape]
    public partial record KartPlayerJoinResponse : KartResponse
    {
        [PropertyShape] public KartPendingUpdate m_update;
    }
    
    [GenerateShape]
    public partial record KartPlayerJoinedNotification : KartNotification
    {
        [PropertyShape] public KartPendingUpdate m_update;
    }
    
    [GenerateShape, DataObjInline]
    public partial record KartPendingUpdate
    {
        [PropertyShape(Name = "gameReady")] public bool m_gameReady;
        [PropertyShape(Name = "p0")] public int? m_player0ID;
        [PropertyShape(Name = "p1")] public int? m_player1ID;
        [PropertyShape(Name = "p2")] public int? m_player2ID;
        [PropertyShape(Name = "p3")] public int? m_player3ID;
        [PropertyShape(Name = "p0_kartClr")] public string m_player0KartColor;
        [PropertyShape(Name = "p1_kartClr")] public string m_player1KartColor;
        [PropertyShape(Name = "p2_kartClr")] public string m_player2KartColor;
        [PropertyShape(Name = "p3_kartClr")] public string m_player3KartColor;
    }
    
    [GenerateShape]
    public partial record KartPlayerLeftNotification : KartNotification
    {
        [PropertyShape(Name = "kartID")] public int m_kartID;
    }
    
    [GenerateShape]
    public partial record KartDriveOffNotification : KartNotification
    {
        [PropertyShape(Name = "playerCount")] public int m_playerCount;
    }
    
    [GenerateShape]
    public partial record KartRanksNotification : KartNotification
    {
        [PropertyShape(Name = "p0_pos")] public int? m_player0Pos;
        [PropertyShape(Name = "p1_pos")] public int? m_player1Pos;
        [PropertyShape(Name = "p2_pos")] public int? m_player2Pos;
        [PropertyShape(Name = "p3_pos")] public int? m_player3Pos;
        [PropertyShape(Name = "p0_time")] public uint m_player0Time;
        [PropertyShape(Name = "p1_time")] public uint m_player1Time;
        [PropertyShape(Name = "p2_time")] public uint m_player2Time;
        [PropertyShape(Name = "p3_time")] public uint m_player3Time;
    }
}