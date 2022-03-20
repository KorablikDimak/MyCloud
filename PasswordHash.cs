using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace MyCloud
{
    public static class PasswordHash
    {
        public static string HashPassword(this string password)
        {
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
            
            var cryptoService = new MD5CryptoServiceProvider();
            byte[] byteHash = cryptoService.ComputeHash(passwordBytes);

            return byteHash.Aggregate(string.Empty, (current, b) => current + $"{b:x2}");
        }
    }
}