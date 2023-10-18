using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Utils 
{
    static string dataFolder;
    public static List<Dictionary<string, string>> GetCSVRecords(string localFilePath)
    {
        List<Dictionary<string, string>> records = new List<Dictionary<string, string>>();
        string[] split = File.ReadAllText(localFilePath).Split(new string[] { ",", "\r", "\n" }, System.StringSplitOptions.RemoveEmptyEntries);
        string[] headers = File.ReadAllLines(localFilePath)[0].Split(","[0]);

        int j = 0;

        Dictionary<string, string> currentRecord = new Dictionary<string, string>();

        for (int i = headers.Length; i < split.Length; i++)
        {
            currentRecord.Add(headers[j], split[i]);
           
            j++;

            if (j >= headers.Length)
            {
                records.Add(currentRecord);
                currentRecord = new Dictionary<string, string>();
                j = 0;
            }

        }

        return records;
    }

    public static long MiliTime()
    {
        return System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static string GetOrCreateDataFolder()
    {
        if (string.IsNullOrEmpty(dataFolder))
        {
            dataFolder = Path.Combine(Application.dataPath, "Text Files", System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"));
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }
        }
        return dataFolder;
    }
}
