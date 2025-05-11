using PolyType;

namespace BinWeevils.Protocol.Amf
{
    [GenerateShape]
    public partial class SubmitLapTimesRequest
    {
        public string m_userID;
        public double m_lap1;
        public double m_lap2;
        public double m_lap3;
        public byte m_trackID;
    }
    
    [GenerateShape]
    public partial class SubmitLapTimesResponse
    {
        [PropertyShape(Name = "lap1")] public double m_pbLap1;
        [PropertyShape(Name = "lap2")] public double m_pbLap2;
        [PropertyShape(Name = "lap3")] public double m_pbLap3;
        [PropertyShape(Name = "total")] public double m_pbTotal;
        
        [PropertyShape(Name = "medalInfo")] public MedalInfo m_medalInfo;
        [PropertyShape(Name = "unlock")] public bool m_unlock;
        
        [GenerateShape]
        public partial class MedalInfo
        {
            [PropertyShape(Name = "hasWonMedal")] public bool m_hasWonMedal;
            [PropertyShape(Name = "medalType")] public string m_medalType;
            [PropertyShape(Name = "clr")] public uint m_color;
        }
    }
}