using System;

public class Commit
{
    public string Message { get; }
    public DateTime Date { get; }
    public string[] Files { get; }
    public bool IsLocal { get; private set; }

    public Commit(string message, string[] files, bool isLocal)
    {
        Message = message;
        Date = DateTime.Now;
        Files = files;
        IsLocal = isLocal;
    }

    public void SetIsLocal(bool isLocal)
    {
        IsLocal = isLocal;
    }
}
