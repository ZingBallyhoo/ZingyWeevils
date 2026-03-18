using PolyType;

namespace BinWeevils.Protocol.Form.Puzzle
{
    [GenerateShape]
    public partial class GetPuzzleListRequest
    {
        [PropertyShape(Name = "userID")] public string m_userID { get; set; }
        [PropertyShape(Name = "typeID")] public PuzzleTypeID m_typeID { get; set; }
    }

    public enum PuzzleTypeID
    {
        WordSearch = 1,
        Crossword = 2,
    }
    
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