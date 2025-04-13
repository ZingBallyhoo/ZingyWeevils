using System.Diagnostics;

namespace BinWeevils.Tools.CoreDiff
{
    public class GitRepo
    {
        public readonly string m_path;
    
        public GitRepo(string path)
        {
            m_path = path;
            Directory.CreateDirectory(m_path);
            EnsureInitialized();
        }
    
        private void RunGitCmd(string cmd)
        {
            var process = Process.Start("git.exe", $"-C \"{m_path}\" {cmd}");
            process.WaitForExit();
        }
    
        public void EnsureInitialized()
        {
            var gitDir = Path.Combine(m_path, ".git");
            if (Directory.Exists(gitDir)) return;
        
            RunGitCmd("init");
            RunGitCmd("config user.name \"Bot\"");
            RunGitCmd("config user.email \"\"");
            RunGitCmd("config commit.gpgsign false");
            RunGitCmd("commit -m \"Initial commit\" --allow-empty");
        }
    
        public void SwitchBranch(string branch, string? basedOff=null)
        {
            if (branch == basedOff) basedOff = null;
            basedOff ??= "master";
            RunGitCmd("reset HEAD --hard");
            RunGitCmd("clean -f"); // remove any unstaged files
            RunGitCmd($"checkout -b {branch} {basedOff}");
            RunGitCmd($"checkout {branch}"); // prev command fails if existing
        }
    
        public void StageAll()
        {
            RunGitCmd("add .");
        }
    
        public void Commit(string message)
        {
            RunGitCmd($"commit -m \"{message}\"");
        }
        
        // push without checkout
        // git push -u origin branch:branch --force
    }
}