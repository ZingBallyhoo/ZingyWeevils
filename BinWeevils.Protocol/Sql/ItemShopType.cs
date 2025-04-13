using System.Runtime.Serialization;

namespace BinWeevils.Protocol.Sql
{
    public enum ItemShopType
    {
        [DataMember(Name = "")] None,
        [DataMember(Name = "nestco")] Nestco,
        [DataMember(Name = "tycoon")] Tycoon,
        [DataMember(Name = "garden")] Garden,
        [DataMember(Name = "nightClub")] NightClub,
        [DataMember(Name = "binPetShop")] BinPetShop,
        [DataMember(Name = "photoStudio")] PhotoStudio,
        [DataMember(Name = "popUpShop")] PopUpShop,
    }
}