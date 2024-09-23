using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using VPackage.SaveGameSystem;
using VPackage.SaveGameSystem.Security;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace ZeroX.SaveGameSystem.Examples
{
    public class TestSaveGame : MonoBehaviour
    {
        EString es;
        private void Start()
        {
            //SaveGame.Default.TryGetC
            //SaveGame.Default.DeleteChild("daily_reward");
            // var a = SaveGame.Default["lucky_wheel"]["lastTimeSpin"];
            // var b = SaveGame.Default["lucky_wheel"]["spinedCount"];
            // var c = SaveGame.Default["daily_reward"]["lastTimeReceived"];
            // var d = SaveGame.Default["daily_reward"]["receivedToDay"];
            // var e = SaveGame.Default["lucky_wheel"]["spinedCount"]["some_thing"];
            // var f = SaveGame.Default["lucky_wheel"]["spinedCount"]["woa_woa"];
            //var jNode = SGJson.Parse("đâsCaSCÂcS@3@!1");
            //Debug.Log(jNode);
            //var a = SaveGame.Default["lucky_wheel"]["lastTimeSpin"];
            //a.StringValue = "06-11-1997";
            //Debug.Log(a.StringValue);
            //a.DeleteChild("0");
            //SaveGame.Default["lucky_wheel"]["lastTimeSpin"].SetString("06-11-1997");
            //Debug.Log(a.GetString());
            // SaveGame.Default["lucky_wheel"]["spinnedCount"].SetInt(232);
            // SaveGame.Default["lucky_wheel"].SetString("hế lô");
            // var d = SaveGame.Default["lucky_wheel"];
            // var b = SaveGame.Default["lucky_wheel"]["spinnedCount"];
            // var c = b.ChildKeySeq;
            // b.AddChild("hello các bạn");
            //Debug.Log(SaveGame.Default["lucky_wheel"]["spinnedCount"].GetInt());

            // SaveGame.Default["inventory"].AddChild("coin");
            // SaveGame.Default["inventory"].AddChild("gem");
            //SaveGame.Default["lucky_wheel"].DeleteChild("spinnedCount");
            
            
            // SaveGame.Default["lucky_wheel"]["last_time_spin"].SetDateTime(DateTime.Now);
            // SaveGame.Default["lucky_wheel"]["spinned_count"].SetInt(5);
            //
            // var moneyNode = SaveGame.Default["money"];
            // moneyNode["coin"].LongValue = 155000;
            // moneyNode["gem"].LongValue = 1000;
            //EHistory.PlusAmountOfDate(DateTime.Now.AddDays(-1), "hello", 3);
            //UEHistory.PlusAmountOfDate(DateTime.Now, "hello", 5);
            //Debug.Log(UEHistory.GetAmountInDateRange(DateTime.Now.AddDays(-1), DateTime.Now, "hello"));
            //SaveGame.Default.TryGetChild(out var childNode, "inventory", "ship_1", "bullet_level");
            //Debug.Log(childNode.FloatValue);

            // List<string> list = new List<string>(){"a1", "a2", "a3", "a4", "a5"};
            // var listTestNode = SaveGame.Default["list_test"];
            // listTestNode.AddChildValueRange(list);
            // listTestNode.AddChildWithSeq("bottle_1");
            // listTestNode.AddChildWithSeq("bottle_2");
            // listTestNode.AddChildWithSeq("bottle_3");
            // listTestNode.AddChildWithSeq("bottle_3");
            // listTestNode.AddChildWithSeq("bottle_5");
            //int a = listTestNode.DeleteAllChild(n => n.StringValue == "bottle_3");
            //Debug.Log(a);

            SaveGame.Login();
            var a = SaveGame.Default["cmn"];
            a["cc"].IntValue = 2;
            Debug.Log(a["cc"].IntValue);
            // Aes aesAlg = Aes.Create();
            // Debug.Log(aesAlg.Mode);
            // Debug.Log(aesAlg.Padding);
            //Firebase.Crashlytics.Crashlytics.LogException(new Exception("Nothing"));
            
            DateTime dateTime = DateTime.Now;
            //Debug.Log(dateTime.);
        }

        IEnumerator WaitTask()
        {
            Task task = new Task(() =>
            {
                Thread.Sleep(1000);
                throw new Exception("nonono");
            });
            task.Start();
            
            yield return new WaitForTaskEnd(task);
            Debug.Log("Task end: " + task.IsFaulted);
        }

        public void Login()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            SaveGame.Login();
            st.Stop();
            Debug.Log("Login time: " + st.ElapsedMilliseconds);
        }

        public void Logout()
        {
            SaveGame.Logout();
        }

        [ContextMenu("Test Encrypt Type")]
        public void TestEncryptTypePerformance()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            EString e = new EString();
            for (int i = 0; i < 10000; i++)
            {
                e.Value = "hello các bạn nhé, dài chút nào";
            }
            st.Stop();
            Debug.Log("EString: " + st.ElapsedMilliseconds);
            
            st.Restart();
            for (int i = 0; i < 10000; i++)
            {
                SGAesEncrypt.EncryptString("hello các bạn nhé, dài chút nào", "123456789000");
            }
            st.Stop();
            Debug.Log("AES: " + st.ElapsedMilliseconds);
        }

        [ContextMenu("Test Save One File")]
        public void TestSaveGame10kKey_1File()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 0; i < 10000; i++)
            {
                SaveGame.Default["hello"]["key_" + i].StringValue = "hello các bạn nhé";
            }
            st.Stop();
            Debug.Log("Save 10k key trong 1 file: " + st.ElapsedMilliseconds);
        }
        
        [ContextMenu("Test Save Multi File")]
        public void TestSaveGame100kKey_10kFile()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            //SGNode helloNode = SaveGame.Default["hello"];
            for (int i = 0; i < 10000; i++)
            {
                //var nodeA = helloNode["A_" + i];
                for (int j = 0; j < 10; j++)
                {
                    SaveGame.Default["A_" + i]["B_" + j].StringValue = "hello các bạn nhé";
                    //nodeA["B_" + j].StringValue = "hello các bạn nhé";
                }
            }
            st.Stop();
            Debug.Log("Save 100k key trong 10k file: " + st.ElapsedMilliseconds);
        }
        
        [ContextMenu("Test Save Multi File")]
        public void TestSaveGame10kKey_10kFile()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 0; i < 10000; i++)
            {
                SaveGame.Default["A_" + i]["B_1"].StringValue = "hello các bạn nhé";
            }
            st.Stop();
            Debug.Log("Save 100k key trong 10k file: " + st.ElapsedMilliseconds);
        }

        public void TestSet100Level()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var nodeCampaignLevel = SaveGame.Default["campaign_level"];
            for (int i = 1; i <= 100; i++)
            {
                var nodeLevelData = nodeCampaignLevel[i.ToString()];
                nodeLevelData["isCompleted"].IntValue = 1;
                nodeLevelData["starReached"].IntValue = 3;
                nodeLevelData["data_1"].StringValue = "hello các bạn";
                nodeLevelData["data_2"].StringValue = "hello các bạn";
                nodeLevelData["data_3"].StringValue = "hello các bạn";
                nodeLevelData["data_4"].StringValue = "hello các bạn";
                nodeLevelData["data_5"].StringValue = "hello các bạn";
                nodeLevelData["data_6"].StringValue = "hello các bạn";
            }
            st.Stop();
            Debug.Log("Set 100 level: " + st.ElapsedMilliseconds);
        }

        public void TestGet100Level()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var nodeCampaignLevel = SaveGame.Default["campaign_level"];
            for (int i = 1; i <= 100; i++)
            {
                var nodeLevelData = nodeCampaignLevel[i.ToString()];
                int starReached = nodeLevelData["starReached"].IntValue;
            }
            st.Stop();
            Debug.Log("Get 100 level: " + st.ElapsedMilliseconds);
        }

        public void TestSet1000Level()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var nodeCampaignLevel = SaveGame.Default["campaign_level"];
            for (int i = 1; i <= 10000; i++)
            {
                var nodeLevelData = nodeCampaignLevel[i.ToString()];
                nodeLevelData["isCompleted"].IntValue = 1;
                nodeLevelData["starReached"].IntValue = 3;
                nodeLevelData["data_1"].StringValue = "hello các bạn";
                nodeLevelData["data_2"].StringValue = "hello các bạn";
                nodeLevelData["data_3"].StringValue = "hello các bạn";
                nodeLevelData["data_4"].StringValue = "hello các bạn";
                nodeLevelData["data_5"].StringValue = "hello các bạn";
                nodeLevelData["data_6"].StringValue = "hello các bạn";
            }
            st.Stop();
            Debug.Log("Set 10000 level: " + st.ElapsedMilliseconds);
        }
        
        public void TestGet1000Level()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var nodeCampaignLevel = SaveGame.Default["campaign_level"];
            for (int i = 1; i <= 10000; i++)
            {
                var nodeLevelData = nodeCampaignLevel[i.ToString()];
                int starReached = nodeLevelData["starReached"].IntValue;
            }
            st.Stop();
            Debug.Log("Get 10000 level: " + st.ElapsedMilliseconds);
        }
        
        public void TestInitialize1000Level()
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var nodeCampaignLevel = SaveGame.Default["campaign_level"];
            nodeCampaignLevel.InitializeAllChild();
            // for (int i = 1; i <= 10000; i++)
            // {
            //     var nodeLevelData = nodeCampaignLevel[i.ToString()];
            //     int starReached = nodeLevelData["starReached"].IntValue;
            // }
            st.Stop();
            Debug.Log("Initialize 10000 level: " + st.ElapsedMilliseconds);
        }
        
        public void TestSimpleJson()
        {
            Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();
            for (int i = 0; i < 120; i++)
            {
                string key = "level_" + i;
                Dictionary<string, string> dict2 = new Dictionary<string, string>();
                dict2["isCompleted"] = "1";
                dict2["starReached"] = "3";
                dict2["data_1"] = "hello các bạn";
                dict2["data_2"] = "hello các bạn";
                dict2["data_3"] = "hello các bạn";
                dict2["data_4"] = "hello các bạn";
                dict2["data_5"] = "hello các bạn";
                dict2["data_6"] = "hello các bạn";
            }

            string json = JsonConvert.SerializeObject(dict);
            var jObject = SGJson.Parse(json);
            
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 0; i < 120; i++)
            {
                string j = jObject.ToString();
            }
            st.Stop();
            Debug.Log("To Json: " + st.ElapsedMilliseconds);
            
            st.Restart();
            for (int i = 0; i < 120; i++)
            {
                var j = SGJson.Parse(json);
            }
            st.Stop();
            Debug.Log("From Json: " + st.ElapsedMilliseconds);
        }

        string GenerateSmallString()
        {
            Dictionary<string, Dictionary<string, string>> dictSmall = new Dictionary<string, Dictionary<string, string>>();
            for (int i = 0; i < 100; i++)
            {
                string key1 = "key_" + i;
                Dictionary<string, string> dict2 = new Dictionary<string, string>();
                dictSmall.Add(key1, dict2);
                for (int j = 0; j < 10; j++)
                {
                    string key2 = "key_" + j;
                    dict2.Add(key2, "hello các bạn nhé ahihihihi");
                }
            }
            
            return JsonConvert.SerializeObject(dictSmall);
        }
        
        string GenerateBigString()
        {
            Dictionary<string, Dictionary<string, string>> dictBig = new Dictionary<string, Dictionary<string, string>>();
            for (int i = 0; i < 1000; i++)
            {
                string key1 = "key_" + i;
                Dictionary<string, string> dict2 = new Dictionary<string, string>();
                dictBig.Add(key1, dict2);
                for (int j = 0; j < 100; j++)
                {
                    string key2 = "key_" + j;
                    dict2.Add(key2, "hello các bạn nhé ahihihihi");
                }
            }
            
            return JsonConvert.SerializeObject(dictBig);
        }
        
        string GenerateHugeString()
        {
            Dictionary<string, Dictionary<string, string>> dictBig = new Dictionary<string, Dictionary<string, string>>();
            for (int i = 0; i < 1000; i++)
            {
                string key1 = "key_" + i;
                Dictionary<string, string> dict2 = new Dictionary<string, string>();
                dictBig.Add(key1, dict2);
                for (int j = 0; j < 1000; j++)
                {
                    string key2 = "key_" + j;
                    dict2.Add(key2, "hello các bạn nhé ahihihihi");
                }
            }
            
            return JsonConvert.SerializeObject(dictBig);
        }

        [ContextMenu("Test Write File")]
        public void TestWriteFile()
        {
            string jsonSmall = GenerateSmallString();
            string jsonBig = GenerateBigString();
            string jsonHuge = GenerateHugeString();

            string pathSmall = Path.Combine(Application.persistentDataPath, "smallJson.dat");
            string pathBig = Path.Combine(Application.persistentDataPath, "bigJson.dat");
            string pathHuge = Path.Combine(Application.persistentDataPath, "hugeJson.dat");
            
            
            Stopwatch st = new Stopwatch();
            st.Start();
            File.WriteAllText(pathSmall, jsonSmall);
            st.Stop();
            Debug.Log("Write Small: " + st.ElapsedMilliseconds);
            
            st.Restart();
            File.WriteAllText(pathBig, jsonBig);
            st.Stop();
            Debug.Log("Write Big: " + st.ElapsedMilliseconds);
            
            st.Restart();
            File.WriteAllText(pathHuge, jsonHuge);
            st.Stop();
            Debug.Log("Write Huge: " + st.ElapsedMilliseconds);
        }
        
        [ContextMenu("Test Read File")]
        public void TestReadFile()
        {
            string pathSmall = Path.Combine(Application.persistentDataPath, "smallJson.dat");
            string pathBig = Path.Combine(Application.persistentDataPath, "bigJson.dat");
            string pathHuge = Path.Combine(Application.persistentDataPath, "hugeJson.dat");
            
            
            
            Stopwatch st = new Stopwatch();
            st.Start();
            File.ReadAllText(pathSmall);
            st.Stop();
            Debug.Log("Read Small: " + st.ElapsedMilliseconds);
            
            st.Restart();
            File.ReadAllText(pathBig);
            st.Stop();
            Debug.Log("Read Big: " + st.ElapsedMilliseconds);
            
            st.Restart();
            File.ReadAllText(pathHuge);
            st.Stop();
            Debug.Log("Read Huge: " + st.ElapsedMilliseconds);
        }
        
        
        
        [ContextMenu("Test Write File Encrypt")]
        public void TestWriteFileWithEncrypt()
        {
            string jsonSmall = GenerateSmallString();
            string jsonBig = GenerateBigString();
            string jsonHuge = GenerateHugeString();

            string pathSmall = Path.Combine(Application.persistentDataPath, "smallJson.dat");
            string pathBig = Path.Combine(Application.persistentDataPath, "bigJson.dat");
            string pathHuge = Path.Combine(Application.persistentDataPath, "hugeJson.dat");
            
            
            Stopwatch st = new Stopwatch();
            st.Start();
            File.WriteAllText(pathSmall, SGAesEncrypt.EncryptString(jsonSmall, "12345678"));
            st.Stop();
            Debug.Log("Write Small Encrypt: " + st.ElapsedMilliseconds);
            
            st.Restart();
            File.WriteAllText(pathBig, SGAesEncrypt.EncryptString(jsonBig, "12345678"));
            st.Stop();
            Debug.Log("Write Big Encrypt: " + st.ElapsedMilliseconds);
            
            st.Restart();
            File.WriteAllText(pathHuge, SGAesEncrypt.EncryptString(jsonHuge, "12345678"));
            st.Stop();
            Debug.Log("Write Huge Encrypt: " + st.ElapsedMilliseconds);
        }
        
        [ContextMenu("Test Read File Decrypt")]
        public void TestReadFileWithDecrypt()
        {
            string pathSmall = Path.Combine(Application.persistentDataPath, "smallJson.dat");
            string pathBig = Path.Combine(Application.persistentDataPath, "bigJson.dat");
            string pathHuge = Path.Combine(Application.persistentDataPath, "hugeJson.dat");
            
            
            
            Stopwatch st = new Stopwatch();
            st.Start();
            SGAesEncrypt.DecryptString(File.ReadAllText(pathSmall), "12345678");
            st.Stop();
            Debug.Log("Read Small Decrypt: " + st.ElapsedMilliseconds);
            
            st.Restart();
            SGAesEncrypt.DecryptString(File.ReadAllText(pathBig), "12345678");
            st.Stop();
            Debug.Log("Read Big Decrypt: " + st.ElapsedMilliseconds);
            
            st.Restart();
            SGAesEncrypt.DecryptString(File.ReadAllText(pathHuge), "12345678");
            st.Stop();
            Debug.Log("Read Huge Decrypt: " + st.ElapsedMilliseconds);
        }
        
        [ContextMenu("Test Base 64 Performance")]
        public void TestBase64Performance()
        {
            string jsonSmall = GenerateSmallString();
            string jsonBig = GenerateBigString();
            string jsonHuge = GenerateHugeString();

            
            
            Stopwatch st = new Stopwatch();
            st.Start();
            var s = Convert.ToBase64String(Encoding.UTF8.GetBytes(jsonHuge));
            st.Stop();
            Debug.Log("To Base 64: " + st.ElapsedMilliseconds);
            
            // st.Restart();
            // SGAesEncrypt.DecryptString(File.ReadAllText(pathBig), "12345678");
            // st.Stop();
            // Debug.Log("Read Big Decrypt: " + st.ElapsedMilliseconds);
            //
            // st.Restart();
            // SGAesEncrypt.DecryptString(File.ReadAllText(pathHuge), "12345678");
            // st.Stop();
            // Debug.Log("Read Huge Decrypt: " + st.ElapsedMilliseconds);
        }
        
        public void TestReadFileNormal()
        {
            string pathFolder = Path.Combine(Application.persistentDataPath, "SaveGame/FileSplit/db.0");
            
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 101; i <= 10101; i++)
            {
                string filePath = pathFolder + string.Format("/n.{0}.dat", i);
                File.ReadAllText(filePath);
            }

            st.Stop();
            Debug.Log("Read file normal: " + st.ElapsedMilliseconds);
        }

        [ContextMenu("Read read file parallel")]
        public void TestReadFileParallel()
        {
            string pathFolder = Path.Combine(Application.persistentDataPath, "SaveGame/FileSplit/db.0");
            
            //List<string> listText = new List<string>();
            Stopwatch st = new Stopwatch();
            st.Start();

            Parallel.For(101, 10101, i =>
            {
                string filePath = pathFolder + string.Format("/n.{0}.dat", i);
                string s = File.ReadAllText(filePath);
                // lock (listText)
                // {
                //     listText.Add(s);
                // }
            });
            
            st.Stop();
            Debug.Log("Read file parallel: " + st.ElapsedMilliseconds);

            // Debug.Log(listText.Count);
            // Debug.Log(listText[5000]);
        }

        [ContextMenu("Test read binary vs base 64")]
        public void TestReadBinaryVsBase64()
        {
            string smallString = GenerateSmallString();
            string pathBase64 = Path.Combine(Application.persistentDataPath, "base64.dat");
            string pathBinary = Path.Combine(Application.persistentDataPath, "binary.dat");
            
            File.WriteAllText(pathBase64, SGAesEncrypt.EncryptString(smallString, "12345678"));
            File.WriteAllBytes(pathBinary, SGAesEncrypt.EncryptStringToBytes(smallString, "12345678"));
            
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 0; i < 1000; i++)
            {
                SGAesEncrypt.DecryptString(File.ReadAllText(pathBase64), "12345678");
            }
            st.Stop();
            Debug.Log("Base64: " + st.ElapsedMilliseconds);
            
            st.Restart();
            for (int i = 0; i < 1000; i++)
            {
                var cipherBytes = File.ReadAllBytes(pathBinary);
                var plainBytes = SGAesEncrypt.DecryptBytes(cipherBytes, "12345678");
                Encoding.UTF8.GetString(plainBytes);
            }
            st.Stop();
            Debug.Log("Binary: " + st.ElapsedMilliseconds);
        }

        [ContextMenu("Test key dài ngắn")]
        void TestKeyDaiNgan()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            for (int i = 0; i < 1000; i++)
            {
                dict.Add("linh_tinh_" + i, Random.Range(0, 10000));
            }
            
            dict.Add("a123456789123456789123456789", 1000);
           
            for (int i = 1000; i < 2000; i++)
            {
                dict.Add("linh_tinh_" + i, Random.Range(0, 10000));
            }
            
            dict.Add("a", 999);
            
            for (int i = 2000; i < 3000; i++)
            {
                dict.Add("linh_tinh_" + i, Random.Range(0, 10000));
            }
            
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 0; i < 1000000; i++)
            {
                string key = "a";
                //int value = dict["a"];
            }
            st.Stop();
            Debug.Log("Key ngắn: " + st.ElapsedMilliseconds);
            
            
            st.Restart();
            for (int i = 0; i < 1000000; i++)
            {
                string key = "a123456789123456789123456789";
                //int value = dict["a123456789123456789123456789"];
            }
            st.Stop();
            Debug.Log("Key dài: " + st.ElapsedMilliseconds);
        }

        [ContextMenu("Test key dài ngắn 2")]
        public void TestKeyDaiNgan2()
        {
            Dictionary<string, int> dictKeyNgan = new Dictionary<string, int>();
            Dictionary<string, int> dictKeyDai = new Dictionary<string, int>();
            
            //Set Key Ngắn
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 0; i < 10000; i++)
            {
                dictKeyNgan["a" + i] = i;
            }
            st.Stop();
            Debug.Log("Set Key Ngắn: " + st.ElapsedMilliseconds);
            
            //Get Key Ngắn
            st.Restart();
            for (int i = 0; i < 10000; i++)
            {
                int value = dictKeyNgan["a" + i];
            }
            st.Stop();
            Debug.Log("Get Key Ngắn: " + st.ElapsedMilliseconds);
            
            
            //Set Key Dài
            st.Restart();
            for (int i = 0; i < 10000; i++)
            {
                dictKeyDai["ancaskcnjksanjkfjsaknfjsankjcnkasjcnkjsankfas" + i] = i;
            }
            st.Stop();
            Debug.Log("Set Key Dài: " + st.ElapsedMilliseconds);
            
            
            //Get Key Dài
            st.Restart();
            for (int i = 0; i < 10000; i++)
            {
                int value = dictKeyDai["ancaskcnjksanjkfjsaknfjsankjcnkasjcnkjsankfas" + i];
            }
            st.Stop();
            Debug.Log("Get Key Dài: " + st.ElapsedMilliseconds);
        }
        
        
        
        [ContextMenu("Test key dài ngắn 3")]
        public void TestKeyDaiNgan3()
        {
            Dictionary<string, int> dictKeyNgan = new Dictionary<string, int>();
            Dictionary<string, int> dictKeyDai = new Dictionary<string, int>();
            
            List<string> listKeyNgan = new List<string>();
            List<string> listKeyDai = new List<string>();

            //Cache key
            for (int i = 0; i < 10000; i++)
            {
                listKeyNgan.Add("a" + i);
                listKeyDai.Add("ancaskcnjksanjkfjsaknfjsankjcnkasjcnkjsankfas" + i);
            }
            
            
            Stopwatch st = new Stopwatch();
            st.Start();
            for (int i = 0; i < 10000; i++)
            {
                dictKeyNgan[listKeyNgan[i]] = i;
            }
            st.Stop();
            Debug.Log("Set Key Ngắn cached: " + st.ElapsedMilliseconds);
            
            st.Restart();
            for (int i = 0; i < 10000; i++)
            {
                int value = dictKeyNgan[listKeyNgan[i]];
            }
            st.Stop();
            Debug.Log("Get Key Ngắn cached: " + st.ElapsedMilliseconds);
            
            
            
            st.Restart();
            for (int i = 0; i < 10000; i++)
            {
                dictKeyDai[listKeyDai[i]] = i;
            }
            st.Stop();
            Debug.Log("Set Key Dài cached: " + st.ElapsedMilliseconds);
            
            
            st.Restart();
            for (int i = 0; i < 10000; i++)
            {
                int value = dictKeyDai[listKeyDai[i]];
            }
            st.Stop();
            Debug.Log("Get Key Dài cached: " + st.ElapsedMilliseconds);
        }


        [ContextMenu("Test Multi Thread Read Write")]
        public void TestMultiThreadReadWrite()
        {
            Task task1 = new Task(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    if (i % 2 == 0)
                        SaveGame.Default["test_multi_thread"]["i" + i]["value nhe"].StringValue = "task_1 write: " + i;
                    else
                    {
                        string a = SaveGame.Default["test_multi_thread"]["i" + i]["value nhe"].StringValue;
                    }
                }
            });
            
            Task task2 = new Task(() =>
            {
                for (int i = 0; i < 10000; i++)
                {
                    if (i % 2 == 0)
                        SaveGame.Default["test_multi_thread"]["i" + i]["value nhe"].StringValue = "task_2 write: " + i;
                    else
                    {
                        string a = SaveGame.Default["test_multi_thread"]["i" + i]["value nhe"].StringValue;
                    }
                }
            });

            task1.ContinueWith(t =>
            {
                if(t.IsFaulted)
                    Debug.LogException(t.Exception);
                else
                    Debug.Log("Task 1 completed");
            });
            
            task2.ContinueWith(t =>
            {
                if(t.IsFaulted)
                    Debug.LogException(t.Exception);
                else
                    Debug.Log("Task 2 completed");
            });
            
            task1.Start();
            task2.Start();
        }
    }
}