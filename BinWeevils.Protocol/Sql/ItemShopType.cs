using System.Runtime.Serialization;

namespace BinWeevils.Protocol.Sql
{
    public enum ItemShopType
    {
        [DataMember(Name = "")] None,
        [DataMember(Name = "nestco")] nestco,
        [DataMember(Name = "tycoon")] tycoon,
        [DataMember(Name = "garden")] Garden,
        [DataMember(Name = "nightClub")] nightClub,
        [DataMember(Name = "binPetShop")] BinPetShop,
        [DataMember(Name = "photoStudio")] photoStudio,
        [DataMember(Name = "popUpShop")] PopUpShop,
    }
}