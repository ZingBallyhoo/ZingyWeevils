namespace BinWeevils.Protocol.Enums
{
    public enum EPetExpression : byte
    {
        HAPPY = EWeevilExpression.HAPPY, // 0
        YIKES = EWeevilExpression.YIKES, // 1
        FROWN = EWeevilExpression.FROWN, // 2
        PANT = EWeevilExpression.PANT, // 3
        HUNGRY = EWeevilExpression.HUNGRY, // 4
        TONGUE = EWeevilExpression.TONGUE, // 5
        NEUTRAL = EWeevilExpression.NEUTRAL, // 6
        SHOCK, // 7 - pant but wider...
    }
}