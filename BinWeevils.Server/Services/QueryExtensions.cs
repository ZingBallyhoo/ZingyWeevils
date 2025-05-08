using BinWeevils.Common.Database;
using BinWeevils.Protocol.Sql;
using BinWeevils.Protocol.Xml;
using Microsoft.EntityFrameworkCore;

namespace BinWeevils.Server.Services
{
    public static class QueryExtensions
    {
        public static IQueryable<NestStockItem> ToStockItem(this IQueryable<ItemType> queryable, WeevilDBContext dbContext, EconomySettings economySettings) 
        {
            return queryable.Select(itemType => new NestStockItem
            {
                m_id = itemType.m_itemTypeID,
                m_level = (uint)itemType.m_minLevel,
                m_name = itemType.m_name,
                m_probability = itemType.m_probability,
                m_price = economySettings.GetItemCost(itemType.m_price, itemType.m_currency),
                m_xp = economySettings.GetItemXp(itemType.m_expPoints),
                m_tycoon = itemType.m_tycoonOnly ? 1 : 0,
                m_fileName = itemType.m_configLocation,
                m_description = itemType.m_description,
                m_deliveryTime = 0,
                m_color = /*itemType.m_defaultHexColor == "-1" && */itemType.m_paletteID != -1 ? 
                    dbContext.m_paletteEntries
                        .Where(y => y.m_paletteID == itemType.m_paletteID)
                        .OrderBy(y => EF.Functions.Random())
                        .Select(x => x.m_colorString)
                        .First()
                    : itemType.m_defaultHexColor
            });
        }
    }
}