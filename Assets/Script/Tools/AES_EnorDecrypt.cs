using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class AES_EnorDecrypt
{
    public static byte[] Encrypt(byte[] bytes, string AESKey)
    {
        AESKey = Utils.md5(AESKey);
        RijndaelManaged rijndaelCipher = new RijndaelManaged();
        rijndaelCipher.Key = Convert.FromBase64String(AESKey.Substring(0, 32));
        rijndaelCipher.IV = Encoding.UTF8.GetBytes(AESKey.Substring(0, 16));
        byte[] keyIv = rijndaelCipher.IV;
        byte[] cipherBytes = null;
        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, rijndaelCipher.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(bytes, 0, bytes.Length);
                cs.FlushFinalBlock();
                cipherBytes = ms.ToArray();//得到加密后的字节数组
                cs.Close();
                ms.Close();
            }
        }
        var allEncrypt = new byte[keyIv.Length + cipherBytes.Length];
        Buffer.BlockCopy(keyIv, 0, allEncrypt, 0, keyIv.Length);
        Buffer.BlockCopy(cipherBytes, 0, allEncrypt, keyIv.Length * sizeof(byte), cipherBytes.Length);
        return allEncrypt;
    }

    public static byte[] Decrypt(byte[] bytes, string AESKey)
    {
        AESKey = Utils.md5(AESKey);
        byte[] decryptBytes = null;
        try
        {
            int length = bytes.Length;
            SymmetricAlgorithm rijndaelCipher = Rijndael.Create();
            rijndaelCipher.Key = Convert.FromBase64String(AESKey.Substring(0, 32));//加解密双方约定好的密钥
            byte[] iv = new byte[16];
            Buffer.BlockCopy(bytes, 0, iv, 0, 16);
            rijndaelCipher.IV = iv;
            decryptBytes = new byte[length - 16];
            byte[] passwdText = new byte[length - 16];

            Buffer.BlockCopy(bytes, 16, passwdText, 0, length - 16);
            using (MemoryStream ms = new MemoryStream(passwdText))
            {
                using (CryptoStream cs = new CryptoStream(ms, rijndaelCipher.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cs.Read(decryptBytes, 0, decryptBytes.Length);
                    cs.Close();
                    ms.Close();
                }
                string result = Encoding.UTF8.GetString(decryptBytes).Replace("\0", "");   ///将字符串后尾的'\0'去掉
                decryptBytes = Encoding.UTF8.GetBytes(result);
            }
        }
        catch { }
        return decryptBytes;
    }
}
