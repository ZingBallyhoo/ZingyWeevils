using PolyType;

namespace BinWeevils.Protocol.Form.Business
{
    [GenerateShape]
    public partial class GetPlayListIDsForRoomRequest
    {
        [PropertyShape(Name = "locationID")] public uint m_locID;
    }
    
    [GenerateShape]
    public partial class GetPlayListIDsForRoomResponse
    {
        [PropertyShape(Name = "success")] public bool m_success;
        [PropertyShape(Name = "playlistIDs")] public string m_playList;
        // b=r, a classic
    }
}