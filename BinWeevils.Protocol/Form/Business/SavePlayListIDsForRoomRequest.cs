using PolyType;

namespace BinWeevils.Protocol.Form.Business
{
    [GenerateShape]
    public partial class SavePlayListIDsForRoomRequest
    {
        [PropertyShape(Name = "locationID")] public uint m_locID;
        [PropertyShape(Name = "playlistIDs")] public string m_playListIDs;
    }
}