using System.Text;
using System.Text.RegularExpressions;

namespace BinWeevils.Tools.CoreDiff;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");

        var root = @"D:\re\bw\archive_decompiled\cdn.binw.net";
        Diff(Path.Combine(root, "play", "core.swf.as"));
        // the core.swf in root is some other newer core
        
        for (int i = 0; i < 391; i++)
        {
            var path = Path.Combine(root, $"core{i}.swf.as");
            if (!Path.Exists(path))
            {
                path = Path.Combine(root, "play", $"core{i}.swf.as");
            }
            
            if (!Path.Exists(path)) continue;
            Diff(path);
        }
    }
    
    private static void Diff(string file)
    {
        var repo = new GitRepo("coreDiff");
        foreach (var existingFile in Directory.GetFiles(repo.m_path, "*.as", SearchOption.AllDirectories))
        {
            File.Delete(existingFile);
        }
        
        using var sr = new StreamReader(file);
        
        string? package = null;
        string? type = null;
        StringBuilder? typeBuilder = null;
        
        while (true)
        {
            var line = sr.ReadLine();
            if (line == null) break;
            
            if (line.StartsWith("package "))
            {
                package = line.Substring("package ".Length);
                if (package.EndsWith("_fla"))
                {
                    package = "fla";
                }
                typeBuilder = new StringBuilder();
            }
            if (line.StartsWith("   public "))
            {
                var typeNameMatch = Regex.Match(line, "^   public (?:interface|(?:dynamic )?class) ([^ ]+)");
                if (!typeNameMatch.Success)
                {
                    // constants/globals
                    continue;
                    throw new Exception($"type name match failed: \"{line}\"");
                }
                
                type = typeNameMatch.Groups[1].Value;
            }
            
            if (typeBuilder == null) continue;
            typeBuilder.AppendLine(line);
            
            if (line.StartsWith("}"))
            {
                if (type != null)
                {
                    var outputPath = Path.Combine(repo.m_path, package!.Replace(".", "/"), $"{type}.as");
                    Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
                    File.WriteAllText(outputPath, typeBuilder.ToString());
                }
                
                package = null;
                type = null;
                typeBuilder = null;
            }
        }
        
        //File.Copy(file, Path.Combine(repo.m_path, "core.swf.as"), true);
        repo.StageAll();
        repo.Commit(Path.GetFileName(file));
    }
}