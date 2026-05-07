using System.Security.Cryptography;
using System.Text;

namespace chatApplication.Application.Security
{
    public static class AesHelper
    {
        // Sohbet başlarken rastgele 256-bit (32 byte) bir Oturum Anahtarı üretiriz
        public static string GenerateSessionKey()
        {
            var key = new byte[32];
            RandomNumberGenerator.Fill(key);
            return Convert.ToBase64String(key);
        }

        // Mesajı (Metin) Şifreler
        public static string EncryptText(string plainText, string base64SessionKey)
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(base64SessionKey);
            aes.GenerateIV(); // Güvenlik için her mesaja özel rastgele başlangıç vektörü (IV)

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length); // Çözerken lazım olacak, en başa IV'yi ekliyoruz

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
            return Convert.ToBase64String(ms.ToArray());
        }

        // Şifreli Mesajı (Metin) Çözer
        public static string DecryptText(string cipherText, string base64SessionKey)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(base64SessionKey);

            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length); // En baştaki IV'yi okuyoruz
            aes.IV = iv;

            using var ms = new MemoryStream(fullCipher, iv.Length, fullCipher.Length - iv.Length);
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        // Resim Şifreler (Resimler bilgisayarda byte[] olarak tutulur)
        public static byte[] EncryptImage(byte[] imageBytes, string base64SessionKey)
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(base64SessionKey);
            aes.GenerateIV();

            var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(imageBytes, 0, imageBytes.Length);
            }
            return ms.ToArray();
        }

        // Şifreli Resmi Çözer
        public static byte[] DecryptImage(byte[] encryptedImageBytes, string base64SessionKey)
        {
            using var aes = Aes.Create();
            aes.Key = Convert.FromBase64String(base64SessionKey);

            var iv = new byte[16];
            Array.Copy(encryptedImageBytes, 0, iv, 0, iv.Length);
            aes.IV = iv;

            using var ms = new MemoryStream(encryptedImageBytes, iv.Length, encryptedImageBytes.Length - iv.Length);
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var outputMs = new MemoryStream();

            cs.CopyTo(outputMs);
            return outputMs.ToArray();
        }
    }
}