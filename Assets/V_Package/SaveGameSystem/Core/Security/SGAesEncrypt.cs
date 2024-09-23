using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace VPackage.SaveGameSystem.Security
{
    public class SGAesEncrypt
    {
        static string m_defaultIV = "ky_si_bong_dem06";
        static byte[] m_defaultIVBytes;

        public static byte[] GetIV()
        {
            if(m_defaultIVBytes == null)
            {
                if (m_defaultIV.Length == 0)
                    throw new Exception("Default IV không được để trống");

                if (m_defaultIV.Length < 16)
                    m_defaultIV = m_defaultIV.PadRight(16);
                else if (m_defaultIV.Length > 16)
                    m_defaultIV = m_defaultIV.Substring(0, 16);
                
                m_defaultIVBytes = Encoding.ASCII.GetBytes(m_defaultIV);
            } 
            return m_defaultIVBytes;
        }
        
        static string ValidateAndFixKeyString(string key)
        {
            if(string.IsNullOrEmpty(key))
                throw new Exception("Key cannot null or empty");
        
            int keyLength = key.Length;

            if (keyLength < 32)
                key = key.PadRight(32);
            else if (keyLength > 32)
                key = key.Substring(0, 32);

            return key;
        }
        
        static void ValidateKeyBytes(byte[] key)
        {
            int keyLength = key.Length;
            if(key == null || keyLength == 0)
                throw new Exception("Key cannot null or empty");

            if (key.Length != 32)
                throw new Exception("Key must have a length of 32");
        }
        
        public static byte[] GetKeyBytes(string key)
        {
            key = ValidateAndFixKeyString(key);
            return Encoding.ASCII.GetBytes(key);
        }

        #region Cryptor

        static ICryptoTransform CreateEncryptor(string key)
        {
            key = ValidateAndFixKeyString(key);
            Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.ASCII.GetBytes(key);
            aesAlg.IV = GetIV();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            return aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        }
        
        static ICryptoTransform CreateEncryptor(byte[] keyBytes)
        {
            ValidateKeyBytes(keyBytes);
            Aes aesAlg = Aes.Create();
            aesAlg.Key = keyBytes;
            aesAlg.IV = GetIV();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            return aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        }
        
        static ICryptoTransform CreateDecryptor(string key)
        {
            key = ValidateAndFixKeyString(key);
            Aes aesAlg = Aes.Create();
            aesAlg.Key = Encoding.ASCII.GetBytes(key);
            aesAlg.IV = GetIV();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            return aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        }
    
        static ICryptoTransform CreateDecryptor(byte[] keyBytes)
        {
            ValidateKeyBytes(keyBytes);
            Aes aesAlg = Aes.Create();
            aesAlg.Key = keyBytes;
            aesAlg.IV = GetIV();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            return aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
        }

        #endregion
        
        static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            MemoryStream ms = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(ms, cryptoTransform, CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();

            return ms.ToArray();
        }
        
        #region Encrypt Bytes

        public static byte[] EncryptBytes(byte[] plainBytes, byte[] keyBytes)
        {
            if (plainBytes == null)
                throw new Exception("PlainBytes null!");
            
            ICryptoTransform encryptor = CreateEncryptor(keyBytes);
            return PerformCryptography(plainBytes, encryptor);
        }
        
        public static byte[] EncryptBytes(byte[] plainBytes, string key)
        {
            if (plainBytes == null)
                throw new Exception("PlainBytes null!");
            
            ICryptoTransform encryptor = CreateEncryptor(key);
            return PerformCryptography(plainBytes, encryptor);
        }
        
        public static byte[] EncryptBytes(byte[] plainBytes, ICryptoTransform encryptor)
        {
            if (plainBytes == null)
                throw new Exception("PlainBytes null!");
            
            return PerformCryptography(plainBytes, encryptor);
        }
        
        #endregion


        #region Decrypt Bytes
        
        public static byte[] DecryptBytes(byte[] cipherBytes, byte[] keyBytes)
        {
            if (cipherBytes == null || cipherBytes.Length <= 0)
                throw new Exception("CipherBytes null!");
            
            ICryptoTransform decryptor = CreateDecryptor(keyBytes);
            return PerformCryptography(cipherBytes, decryptor);
        }
        
        /// <summary>
        /// Key phải là các ký tự trong bộ mã ASCII
        /// </summary>
        public static byte[] DecryptBytes(byte[] cipherBytes, string key)
        {
            if (cipherBytes == null || cipherBytes.Length <= 0)
                throw new Exception("CipherBytes null!");
            
            ICryptoTransform decryptor = CreateDecryptor(key);
            return PerformCryptography(cipherBytes, decryptor);
        }
        
        static byte[] DecryptBytes(byte[] cipherBytes, ICryptoTransform decryptor)
        {
            if (cipherBytes == null || cipherBytes.Length <= 0)
                throw new Exception("CipherBytes null!");
            
            return PerformCryptography(cipherBytes, decryptor);
        }
        
        #endregion


        #region Encrypt String to String
        
        /// <summary>
        /// Key phải là các ký tự trong bộ mã ASCII
        /// </summary>
        public static string EncryptString(string plainText, string key)
        {
            if (plainText == null)
                return null;

            if (plainText.Length == 0)
                return plainText;
            
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = EncryptBytes(plainBytes, key);
            return Convert.ToBase64String(cipherBytes);
        }
        
        public static string EncryptString(string plainText, byte[] keyBytes)
        {
            if (plainText == null)
                return null;

            if (plainText.Length == 0)
                return plainText;
            
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = EncryptBytes(plainBytes, keyBytes);
            return Convert.ToBase64String(cipherBytes);
        }
        
        static string EncryptString(string plainText, ICryptoTransform encryptor)
        {
            if (plainText == null)
                return null;

            if (plainText.Length == 0)
                return plainText;
            
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] cipherBytes = EncryptBytes(plainBytes, encryptor);
            return Convert.ToBase64String(cipherBytes);
        }
        
        #endregion
        
        #region Decrypt String to String
        
        /// <summary>
        /// Key phải là các ký tự trong bộ mã ASCII.
        /// </summary>
        public static string DecryptString(string cipherText, string key)
        {
            if (cipherText == null)
                return null;

            if (cipherText.Length == 0)
                return cipherText;
            
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] plainBytes = DecryptBytes(cipherBytes, key);
            return Encoding.UTF8.GetString(plainBytes);
        }
        
        public static string DecryptString(string cipherText, byte[] keyBytes)
        {
            if (cipherText == null)
                return null;

            if (cipherText.Length == 0)
                return cipherText;
            
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] plainBytes = DecryptBytes(cipherBytes, keyBytes);
            return Encoding.UTF8.GetString(plainBytes);
        }
        
        public static string DecryptString(string cipherText, ICryptoTransform decryptor)
        {
            if (cipherText == null)
                return null;

            if (cipherText.Length == 0)
                return cipherText;
            
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] plainBytes = DecryptBytes(cipherBytes, decryptor);
            return Encoding.UTF8.GetString(plainBytes);
        }

        #endregion
        

        #region Encrypt String To Bytes

        public static byte[] EncryptStringToBytes(string plainText, string key)
        {
            if (plainText == null)
                return null;

            if (plainText.Length == 0)
                return new byte[0];
            
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            return EncryptBytes(plainBytes, key);
        }
        
        public static byte[] EncryptStringToBytes(string plainText, byte[] keyBytes)
        {
            if (plainText == null)
                return null;

            if (plainText.Length == 0)
                return new byte[0];
            
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            return EncryptBytes(plainBytes, keyBytes);
        }

        #endregion

        #region Decrypt Bytes to String

        public static string DecryptBytesToString(byte[] cipherBytes, string key)
        {
            if (cipherBytes == null)
                return null;

            if (cipherBytes.Length == 0)
                return "";
            
            byte[] plainBytes = DecryptBytes(cipherBytes, key);
            return Encoding.UTF8.GetString(plainBytes);
        }
        
        public static string DecryptBytesToString(byte[] cipherBytes, byte[] keyBytes)
        {
            if (cipherBytes == null)
                return null;

            if (cipherBytes.Length == 0)
                return "";
            
            byte[] plainBytes = DecryptBytes(cipherBytes, keyBytes);
            return Encoding.UTF8.GetString(plainBytes);
        }

        #endregion

        #region Int

        public static byte[] EncryptInt(int plainValue, byte[] keyBytes)
        {
            byte[] plainBytes = BitConverter.GetBytes(plainValue);
            byte[] cipherBytes = EncryptBytes(plainBytes, keyBytes);
            return cipherBytes;
        }
        
        public static byte[] EncryptInt(int plainValue, string key)
        {
            byte[] plainBytes = BitConverter.GetBytes(plainValue);
            byte[] cipherBytes = EncryptBytes(plainBytes, key);
            return cipherBytes;
        }
    
        public static int DecryptInt(byte[] cipherBytes, byte[] keyBytes)
        {
            byte[] plainBytes = DecryptBytes(cipherBytes, keyBytes);
            return BitConverter.ToInt32(plainBytes, 0);
        }
        
        public static int DecryptInt(byte[] cipherBytes, string key)
        {
            byte[] plainBytes = DecryptBytes(cipherBytes, key);
            return BitConverter.ToInt32(plainBytes, 0);
        }

        #endregion
        
        
    }
}