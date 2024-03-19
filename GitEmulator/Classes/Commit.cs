using System;

class Commit
{
    public string Message { get; }
    public DateTime Date { get; }
    public string[] Files { get; }

    public Commit(string message, string[] files)
    {
        Message = message;
        Date = DateTime.Now;
        Files = files;
    }
}
