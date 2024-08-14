using System;
using System.IO;
using System.Security.Cryptography;

namespace TelegramServer.Verifier.Provider;

public static class AesEncryptionProvider
{
    public static string Encrypt(string plainText, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
 
        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            using (var sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }
        }
 
        return Convert.ToBase64String(ms.ToArray());
    }
 
    public static string Decrypt(string cipherText, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
 
        var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
 
        var bytes = Convert.FromBase64String(cipherText);

        using var ms = new MemoryStream(bytes);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var sr = new StreamReader(cs);
        return sr.ReadToEnd();
    }
}