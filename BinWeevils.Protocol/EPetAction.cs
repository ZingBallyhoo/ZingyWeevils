namespace BinWeevils.Protocol
{
    public enum EPetAction
    {
        EYE_THING = -5, // todo: what actually is this
        STOP = -4,
        END_PANT = -3,
        START_PANT = -2,
        SIT = -1,
        DEFAULT,
        CRAWL,
        HOP,
        WALK,
        SPIN,
        JUMP_SPIN,
        WAVE_RIGHT,
        WAVE_LEFT,
        ROCK,
        JUGGLE,
        THROW1,
        THROW2,
        EAT,
        SLEEP,
        WAKE_UP,
        STRETCH,
        GET_IN_BET,
        TURN_TO_FACE,
        PIROUETTE,
        JUMP,
        LOOK,
        LOOK_LEFT_RIGHT,
        ROLL_EYES,
        EXTEND_EYES,
        BLINK,
        THROW_ALL,
        CELEBRATE,
        SUPER_SPIN,
        JUMP_ON,
        STAND,
        FETCH,
        BECOME_SEPIA = EWeevilAction.BECOME_SEPIA, // 44
        BECOME_BLACK = EWeevilAction.BECOME_BLACK, // 46
        BECOME_WHITE = EWeevilAction.BECOME_WHITE, // 47
        
        JUMP_OFF = -JUMP_ON, // -28
    }
}