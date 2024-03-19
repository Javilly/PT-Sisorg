using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class Server
{
    private const string dataFilePath = "data.json";

    public List<Commit> LoadCommits()
    {
        if (File.Exists(dataFilePath))
        {
            string jsonData = File.ReadAllText(dataFilePath);
            return JsonConvert.DeserializeObject<List<Commit>>(jsonData);
        }
        else
        {
            return new List<Commit>();
        }
    }

    public void SaveCommits(List<Commit> commits)
    {
        string jsonData = JsonConvert.SerializeObject(commits);
        File.WriteAllText(dataFilePath, jsonData);
    }
}
