using PolyType;

namespace BinWeevils.Protocol.Form
{
    [GenerateShape]
    public partial class GetPuzzleListResponse
    {
        [PropertyShape(Name = "typeName")] public string m_typeName { get; set; }
        [PropertyShape(Name = "gamePath")] public string m_gamePath { get; set; }
        [PropertyShape(Name = "configBasePath")] public string m_configBasePath { get; set; }
        [PropertyShape(Name = "locName")] public string m_locName { get; set; }
        [PropertyShape(Name = "levelList")] public string m_levelList { get; set; }
        [PropertyShape(Name = "nameList")] public string m_nameList { get; set; }
        [PropertyShape(Name = "configList")] public string m_configList { get; set; }
        [PropertyShape(Name = "completedList")] public string m_completedList { get; set; }
    }
}