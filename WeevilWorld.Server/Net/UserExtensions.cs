using ArcticFox.SmartFoxServer;
using WeevilWorldProtobuf.Objects;

namespace WeevilWorld.Server.Net
{
    public static class UserExtensions
    {
        public static Weevil GetWeevil(this User user)
        {
            return user.GetUserData<WeevilData>().m_object;
        }
        
        public static WeevilData GetWeevilData(this User user)
        {
            return user.GetUserData<WeevilData>();
        }
    }
}