namespace BinWeevils.Protocol.Enums
{
    public enum EPetExpression : byte
    {
        HAPPY = EWeevilExpression.HAPPY, // 0
        NOT_SO_HAPPY = EWeevilExpression.YIKES, // 1
        SAD = EWeevilExpression.FROWN, // 2
        MOUTH_OPEN = EWeevilExpression.PANT, // 3
        TONGUE_OUT_HUNGRY = EWeevilExpression.HUNGRY, // 4
        TONGUE_OUT_DISOBEDIENT = EWeevilExpression.TONGUE, // 5
        NEUTRAL = EWeevilExpression.NEUTRAL, // 6
        MOUTH_WIDE_OPEN, // 7 - pant but wider...
    }
}