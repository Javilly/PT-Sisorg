using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class Server
{
    private const string dataFilePath = "commits.json";

    public List<Commit> LoadCommits()
    {
        if (File.Exists(dataFilePath))
        {
            string jsonData = File.ReadAllText(dataFilePath);
            if (jsonData != null)
            {
                List<Commit>? deserializedCommits = JsonConvert.DeserializeObject<List<Commit>>(
                    jsonData
                );
                return deserializedCommits ?? new List<Commit>();
            }
        }
        return new List<Commit>();
    }

    public void SaveCommits(List<Commit> newCommits)
    {
        List<Commit> allCommits = LoadCommits();

        allCommits.AddRange(newCommits);

        string jsonData = JsonConvert.SerializeObject(allCommits);
        File.WriteAllText(dataFilePath, jsonData);
    }
}
