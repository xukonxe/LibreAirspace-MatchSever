using System;//Action
using System.Security.Cryptography;
using System.Text;

namespace CMKZ {
    public static partial class LocalStorage {
        public static byte[] Encrypt(this byte[] X) {
            for (var i = 0; i < X.Length; i++) {
                X[i] ^= 0x78;
            }
            return X;
        }
        public static byte[] Decrypt(this byte[] X) {
            for (var i = 0; i < X.Length; i++) {
                X[i] ^= 0x78;
            }
            return X;
        }
        public static string ToSHA256(this string X) {
            var S = new SHA256Managed();
            var H = S.ComputeHash(Encoding.UTF8.GetBytes(X));
            return BitConverter.ToString(H).Replace("-", "").ToLower();
        }
        public static string ToMD5(this string X) {
            var S = new MD5CryptoServiceProvider();
            var H = S.ComputeHash(Encoding.UTF8.GetBytes(X));
            return BitConverter.ToString(H).Replace("-", "").ToLower();
        }
    }
}