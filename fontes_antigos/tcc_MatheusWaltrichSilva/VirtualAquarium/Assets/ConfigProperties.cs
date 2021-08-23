using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[System.Serializable]
public class ConfigProperties {

    public string token { get; set; }
    public string ssid { get; set; }
    public string password { get; set; }
    public int timeSpeed { get; set; }
    public string moduleIP { get; set; }

    public ConfigProperties()
    {
        this.token = "";
        this.ssid = "";
        this.password = "";
        this.timeSpeed = 1;
        this.moduleIP = "";
    }

    public void saveConfig()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/aquarium.cfg";
        FileStream fileStream = new FileStream(path, FileMode.Create);
        formatter.Serialize(fileStream, this);
        fileStream.Close();
    }

    public static ConfigProperties loadConfig()
    {
        string path = Application.persistentDataPath + "/aquarium.cfg";
        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream fileStream = new FileStream(path, FileMode.Open);

            ConfigProperties configs = formatter.Deserialize(fileStream) as ConfigProperties;
            fileStream.Close();
            return configs;
        } else
        {
            return new ConfigProperties();
        }
    }

}
