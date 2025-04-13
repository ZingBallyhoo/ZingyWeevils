using System.Runtime.Serialization;

namespace BinWeevils.Protocol.Sql
{
    public enum ItemCurrency
    {
        [DataMember(Name = "")] None,
        [DataMember(Name = "mulch")] Mulch,
        [DataMember(Name = "dosh")] Dosh
    }
}