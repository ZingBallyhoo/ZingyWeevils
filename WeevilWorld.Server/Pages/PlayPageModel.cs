namespace WeevilWorld.Server.Pages
{
    public class PlayPageModel
    {
        public string BuildName { get; set; }
        public string BuildJsonFilename { get; set; } = "04";

        public string UnityLoaderPath => $"/WeevilWorld/{BuildName}/Build/UnityLoader.js";
        public string UnityJsonPath => $"/WeevilWorld/{BuildName}/Build/{BuildJsonFilename}.json";
    }
}