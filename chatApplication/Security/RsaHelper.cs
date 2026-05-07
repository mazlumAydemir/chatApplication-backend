using System.Security.Cryptography;
using System.Text;

namespace chatApplication.Application.Security
{
    public static class RsaHelper
    {
        // 1. İstemci (Client/Sysadmin) uygulamayı açtığında kendine bir anahtar çifti üretir
        public static (string PublicKey, string PrivateKey) GenerateKeys()
        {
            using var rsa = RSA.Create(2048);
            return (
                Convert.ToBase64String(rsa.ExportRSAPublicKey()),
                Convert.ToBase64String(rsa.ExportRSAPrivateKey())
            );
        }

        // 2. Ahmet, Ayşe'nin Public Key'ini kullanarak Oturum Anahtarını (Session Key) şifreler
        public static string Encrypt(string plainText, string base64PublicKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(Convert.FromBase64String(base64PublicKey), out _);

            var dataToEncrypt = Encoding.UTF8.GetBytes(plainText);
            var encryptedData = rsa.Encrypt(dataToEncrypt, RSAEncryptionPadding.OaepSHA256);

            return Convert.ToBase64String(encryptedData);
        }

        // 3. Ayşe, Ahmet'ten gelen şifreli Oturum Anahtarını kendi Private Key'i ile çözer
        public static string Decrypt(string cipherText, string base64PrivateKey)
        {
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(Convert.FromBase64String(base64PrivateKey), out _);

            var dataToDecrypt = Convert.FromBase64String(cipherText);
            var decryptedData = rsa.Decrypt(dataToDecrypt, RSAEncryptionPadding.OaepSHA256);

            return Encoding.UTF8.GetString(decryptedData);
        }
    }
}