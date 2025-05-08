namespace BinWeevils.Protocol
{
    public static class EnumExtensions
    {
        public static bool IsClientManaged(this EPetSkill skill)
        {
            return skill switch
            {
                EPetSkill.CALL => true,
                EPetSkill.GO_THERE => true,
                EPetSkill.WEEVIL_THROW_BALL => true,
                EPetSkill.STOP_JUGGLING => true,
                _ => false,
            };
        }
    }
}