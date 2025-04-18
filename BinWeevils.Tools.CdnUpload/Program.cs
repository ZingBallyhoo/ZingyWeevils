using Renci.SshNet;
using Renci.SshNet.Sftp;

namespace BinWeevils.Tools.CdnUpload;

class Program
{
    private const string DEST_DIR = "/var/zingyweevils/cdn";
    
    private static string s_sourceRoot;
    
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        
        var host = args[0];
        var username = args[1];
        s_sourceRoot = args[2];
        
        var client = new SftpClient(new PrivateKeyConnectionInfo(host, username, new PrivateKeyFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".ssh", "id_rsa"))));
        await client.ConnectAsync(CancellationToken.None);
        
        // todo: this has to be run multiple times as it fails to recreate directories recursively..
        
        SyncDir(client, "");
        foreach (var directory in Directory.EnumerateDirectories(s_sourceRoot, "*", SearchOption.AllDirectories))
        {
            var relativeDirectory = Path.GetRelativePath(s_sourceRoot, directory);
            
            var testDirectory = relativeDirectory;
            if (testDirectory.StartsWith("play") && testDirectory != "play")
            {
                testDirectory = Path.GetRelativePath("play", testDirectory);
            }
            
            if (testDirectory.StartsWith("WeevilWorld")) continue;
            if (testDirectory.StartsWith(Path.Combine("externalUIs", "adCampaigns"))) continue;
            if (testDirectory.StartsWith(Path.Combine("externalUIs", "comps"))) continue;
            if (testDirectory.StartsWith(Path.Combine("externalUIs", "campaigns"))) continue;
            if (testDirectory.StartsWith(Path.Combine("fixedCam", "adCampaigns"))) continue;
            if (testDirectory.StartsWith(Path.Combine("fixedCam", "campaigns"))) continue;
            if (testDirectory.StartsWith("blog")) continue;
            if (testDirectory.StartsWith("downloads")) continue;
            if (testDirectory.StartsWith("loaderContent")) continue;
            if (testDirectory.StartsWith("videos")) continue;
            if (testDirectory.StartsWith("externalWin")) continue;
            if (testDirectory.StartsWith("js")) continue;
            if (testDirectory.StartsWith("css")) continue;
            if (testDirectory.StartsWith("ads")) continue;
            if (testDirectory.StartsWith("library")) continue;
            if (testDirectory.StartsWith("tycoon")) continue; // random site..
            if (testDirectory.StartsWith("profilePics")) continue; // filter james...
            
            Console.Out.WriteLine(relativeDirectory);
            SyncDir(client, relativeDirectory);
        }
    }
    
    private static void SyncDir(SftpClient client, string relativePath)
    {
        var sourceDir = Path.Combine(s_sourceRoot, relativePath);
        var destDir = Path.Combine(DEST_DIR, relativePath).Replace('\\', '/');
        
        try
        {
            client.CreateDirectory(destDir);
        } catch (Exception)
        {
            // already exists
        }
        var ar = client.BeginSynchronizeDirectories(sourceDir, destDir, "*", ar =>
        {
            // todo: this literally doesnt work...
            // it only logs at the end
            var ar2 = (SftpSynchronizeDirectoriesAsyncResult)ar;
            Console.Out.WriteLine($"{ar2.FilesRead}");
        }, null);
        client.EndSynchronizeDirectories(ar);
    }
}