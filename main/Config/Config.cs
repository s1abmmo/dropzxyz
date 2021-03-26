using System.Collections.Generic;
using System.IO;
using System;
using main.JsonConverting;
using System.Windows;

namespace main.Config
{
    public class Config
    {
        public static Config config { get; set; }
        public Dictionary<string, string> ListScript { get; set; }
        public string ScriptNameSelected { get; set; }
        public static void LoadListScript()
        {
            try
            {
                Config.config = new Config();
                config.ListScript = new Dictionary<string, string>();
                DirectoryInfo di = new DirectoryInfo(System.AppDomain.CurrentDomain.BaseDirectory + "\\Auto");
                foreach (FileInfo file in di.GetFiles("*.cts"))
                {
                    string namefile = Path.GetFileNameWithoutExtension(file.Name);
                    string content = File.ReadAllText(file.FullName);
                    config.ListScript.Add(namefile, content);
                }
            }
            catch
            {
                Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "\\Auto");
            }
        }
        public static void SaveConfigs()
        {
            string[] itemList = new string[0];
            for (int a = 0; a < ListItem.ListItem.items.Count; a++)
            {
                Array.Resize(ref itemList, itemList.Length + 1);
                itemList[itemList.Length - 1] = JsonConverting.JsonConverting.EncodeJson(ListItem.ListItem.items[a]);
            }
            string configs = String.Join(Environment.NewLine, itemList);
            File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + "//configs", configs);
        }
        public static void LoadConfigs()
        {
            try
            {
                ListItem.ListItem.items = new List<DropzWindow>();
                string[] itemList = File.ReadAllLines(System.AppDomain.CurrentDomain.BaseDirectory + "//configs");
                for (int a = 0; a < itemList.Length; a++)
                {
                    DropzWindow currentItem = JsonConverting.JsonConverting.DecodeJsonDropWindow(itemList[a]);
                    currentItem.Start = false;
                    currentItem.AutoRunning = false;
                    currentItem.ButtonReady = true;
                    ListItem.ListItem.items.Add(currentItem);
                }
            }
            catch
            {
                ListItem.ListItem.items = new List<DropzWindow>();
            }
        }

    }
}
