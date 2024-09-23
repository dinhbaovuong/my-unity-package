using System;
using System.IO;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VPackage.SaveGameSystem.Security
{
    internal static class SgPassManager
    {
        private const string characters = "qwertyuiopasdfghjklzxcvbnm1234567890!@#$%^&*()QWERTYUIOPASDFGHJKLZXCVBNM";
        //private static readonly string p = "!@#ZeRoX$%^WingOfFreedom&*(";
        //private static readonly string pFormat = "!@#{0}ZeRoX{1}&*(";
        private const string pF = "uPB{0}f2+rh9{0}nqRg&@{0}_!PB#Se%CJgNQZ";

        static string GetP(string databaseId)
        {
            return string.Format(pF, databaseId);
        }
        
        
        public static string GetOrCreateDbPassInPlayerPref(string databaseId)
        {
            string p = GetP(databaseId);
            string plainKey = string.Format("sg.pass_manager.db.{0}", databaseId);
            string cipherKey = SGAesEncrypt.EncryptString(plainKey, p);

            string cipherPass;
            if (PlayerPrefs.HasKey(cipherKey))
            {
                cipherPass = PlayerPrefs.GetString(cipherKey);
                if(string.IsNullOrWhiteSpace(cipherPass) == false)
                    return SGAesEncrypt.DecryptString(cipherPass, p);
            }
            
            string plainPass = RandomDbPass();
            cipherPass = SGAesEncrypt.EncryptString(plainPass, p);
            PlayerPrefs.SetString(cipherKey, cipherPass);
            return plainPass;
        }

        

        public static string GetOrCreateDbPassInFile(string filePath, string databaseId)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new Exception("File path cannot empty!");
            }

            string p = GetP(databaseId);
            string cipherPass;
            if (File.Exists(filePath))
            {
                cipherPass = File.ReadAllText(filePath);
                if(string.IsNullOrWhiteSpace(cipherPass) == false)
                    return SGAesEncrypt.DecryptString(cipherPass, p);
            }
            
            
            string directory = Path.GetDirectoryName(filePath);
            if(string.IsNullOrEmpty(directory))
                throw new Exception("Directory path cannot empty!");
            
            if (Directory.Exists(directory) == false)
                Directory.CreateDirectory(directory);

            string plainPass = RandomDbPass();
            cipherPass = SGAesEncrypt.EncryptString(plainPass, p);
            File.WriteAllText(filePath, cipherPass);
            return plainPass;
        }

        private static string RandomDbPass()
        {
            System.Random random = new System.Random();
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < 32; i++)
            {
                int randomIndex = random.Next(0, characters.Length);
                sb.Append(characters[randomIndex]);
            }

            return sb.ToString();
        }
    }
}