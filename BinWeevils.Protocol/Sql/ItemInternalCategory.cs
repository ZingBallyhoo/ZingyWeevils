using System.Runtime.Serialization;

namespace BinWeevils.Protocol.Sql
{
    public enum ItemInternalCategory
    {
        [DataMember(Name = "")] None,
        
        [DataMember(Name = "unavailable")] Unavailable,
        [DataMember(Name = "retired")] Retired,
        [DataMember(Name = "available")] Available,
        [DataMember(Name = "trophy")] Trophy,
        [DataMember(Name = "campaign")] Campaign,
        
        [DataMember(Name = "halloween")] Halloween,
        [DataMember(Name = "xmas")] Xmas,
        [DataMember(Name = "holiday")] Holiday,
        [DataMember(Name = "easter")] Easter,
        
        [DataMember(Name = "prize")] Prize,
        [DataMember(Name = "collectibleprize")] CollectiblePrize,
        
        [DataMember(Name = "merch")] Merch,
        [DataMember(Name = "nestco")] Nestco
    }
}