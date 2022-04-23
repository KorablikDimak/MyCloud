using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MyCloud.Security
{
    public static class StringHasher
    {
        public static string HashString(this string password)
        {
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
            
            var cryptoService = new MD5CryptoServiceProvider();
            byte[] byteHash = cryptoService.ComputeHash(passwordBytes);

            return byteHash.Aggregate(string.Empty, (current, b) => current + $"{b:x2}");
        }
    }
}