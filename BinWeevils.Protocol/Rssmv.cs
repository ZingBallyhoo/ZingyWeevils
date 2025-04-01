using System.Security.Cryptography;
using System.Text;

namespace BinWeevils.Protocol
{
    public static class Rssmv
    {
        private const string SALT = "P07aJK8soogA815CxjkTcA==";
        
        public static string Hash(string input)
        {
            var result = MD5.HashData(Encoding.UTF8.GetBytes($"{SALT}{input}"));
            return Convert.ToHexStringLower(result);
        }
    }
}