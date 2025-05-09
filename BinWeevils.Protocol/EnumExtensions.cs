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
        
        public static bool IsValidPose(this EPetAction action)
        {
            return action switch
            {
                EPetAction.DEFAULT => true,
                EPetAction.SLEEP => true,
                EPetAction.JUMP_ON => true,
                _ => false
            };
        }
    }
}