using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;

namespace LMS.HelpingMethods
{
    public static class Helper
    {
        public static string GetHash(string rawData)
        {
            using (var hashed = SHA256.Create())
            {
                var bytes = hashed.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
