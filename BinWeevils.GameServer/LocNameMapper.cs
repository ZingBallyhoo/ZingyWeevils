using System.Text.RegularExpressions;
using BinWeevils.Database;
using BinWeevils.Protocol.Xml;
using Microsoft.Extensions.DependencyInjection;

namespace BinWeevils.GameServer
{
    public partial class LocNameMapper
    {
        private readonly IServiceProvider m_provider;
        private readonly HashSet<string> m_allowedLocNames = [];
        
        public LocNameMapper(IServiceProvider provider, LocationDefinitions locationDefinitions)
        {
            m_provider = provider;
            
            m_allowedLocNames.Add("at home");
            m_allowedLocNames.Add("in a secret location");
            m_allowedLocNames.Add("loyaltyCard");
            m_allowedLocNames.Add("magazineViewer");
            m_allowedLocNames.Add("map");
            m_allowedLocNames.Add("videoAdsUI");
            m_allowedLocNames.Add("test"); // okay why (weevilPost_18032011.swf)

            foreach (var location in locationDefinitions.m_locations)
            {
                foreach (var door in location.m_doors)
                {
                    AddExtUI(door.m_extUIData);
                }
                foreach (var preRend3D in location.m_preRend3Ds)
                {
                    AddExtUI(preRend3D.m_extUIData);
                }
                foreach (var sign in location.m_signs)
                {
                    AddExtUI(sign.m_extUIData);
                }
                foreach (var gameSlot in location.m_gameSlots)
                {
                    AddExtUI(gameSlot.m_extUIData);
                }
                foreach (var cta in location.m_ctas)
                {
                    AddExtUI(cta.m_extUIData);
                }
            }
            
            Console.Out.WriteLine("h");
        }
        
        [GeneratedRegex("(?:,|^)locName:([^,]+)(?:,|$)")]
        private partial Regex ExtUIRegex { get; }
        
        private void AddExtUI(string? extUIData)
        {
            if (extUIData == null) return;
            
            var match = ExtUIRegex.Match(extUIData);
            if (!match.Success)
            {
                if (extUIData.Contains("locName:"))
                {
                    throw new InvalidDataException("sanity");
                }
                return;
            }
            
            m_allowedLocNames.Add(match.Groups[1].Value);
        }
        
        public async ValueTask<string> MapName(string text)
        {
            if (m_allowedLocNames.Contains(text))
            {
                return text switch
                {
                    "map" => "looking at the map",
                    "loyaltyCard" => "stamping their BinCard",
                    "videoAdsUI" => "watching an Ad",
                    "magazineViewer" => "reading a magazine",
                    "test" => "reading a magazine",
                    
                    "FurnitureShop" => "at the Furniture shop",
                    "GadgetShop" => "at the Gadget shop",
                    "BathroomAndKitchenShop" => "at the Bathroom and Kitchen shop",
                    "PropertyShop" => "at the Property shop",
                    "WallpaperShop" => "at the Wallpaper shop",
                    "GardeningShop" => "at the Gardening shop",
                    "PhotoStudioShop" => "at the Photo Studio shop",
                    "NightClubShop" => "at the Night Club shop",
                    "GardenPlotsShop" => "at the Garden Plots shop",
                    "TycoonShop" => "at the Tycoon shop",
                    
                    "wevMod" => "changing their look",
                    "HaggleHut" => "at the Haggle Hut",
                    "Rums Cove Interia" => "in Rum's Airport",
                    "LabsLab" => "in Lab's Lab",
                    
                    _ => text
                };
            }
            
            var ownerMatch = NestRegex.Match(text);
            if (!ownerMatch.Success)
            {
                ownerMatch = PlazaRegex.Match(text);
            }
            
            if (ownerMatch.Success)
            {
                if (await ValidateUserName(ownerMatch.Groups[1].Value))
                {
                    return text;
                }

                throw new InvalidDataException($"user gave invalid owner \"{ownerMatch.Groups[1].Value}\" for nest bVar loCame");
            }
            
            throw new InvalidDataException($"unknown locName: {text}");
        }
        
        private async Task<bool> ValidateUserName(string userName) 
        {
            await using var scope = m_provider.CreateAsyncScope();
            var context = scope.ServiceProvider.GetRequiredService<WeevilDBContext>();
            
            return context.m_weevilDBs.Any(x => x.m_name == userName);
        }
        
        [GeneratedRegex("^in (.+)'s nest$")]
        private partial Regex NestRegex { get; }
        
        [GeneratedRegex("^in (.+)'s plaza$")]
        private partial Regex PlazaRegex { get; }
    }
}