public class Client
{
    public List<string> stagingArea = new List<string>();
    public List<Commit> remoteCommits = new List<Commit>();
    public List<Commit> localCommits = new List<Commit>();
    private Server server;

    public Dictionary<string, Action<string>> actions = new Dictionary<string, Action<string>>
    {
        { "add", AddFile },
        { "commit", Commit },
        { "push", PushCommit },
        { "log", LogCommits },
        { "pull", PullLatestCommit },
        { "merge", Merge },
        { "revert", Revert },
        { "help", Help },
        { "exit", Exit }
    };

    public void AddFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            Console.WriteLine("Uso: add <nombre_archivo>\n");
            return;
        }

        string repoDirectory = "repo";
        string filePath = Path.Combine(repoDirectory, fileName);

        if (!File.Exists(filePath))
        {
            Console.WriteLine(
                $"El archivo '{fileName}' no existe en la carpeta '{repoDirectory}'.\n"
            );
            return;
        }

        if (stagingArea.Contains(fileName))
        {
            Console.WriteLine(
                $"El archivo '{fileName}' ya ha sido agregado al área de preparación.\n"
            );
            return;
        }

        stagingArea.Add(fileName);
        Console.WriteLine($"El archivo '{fileName}' ha sido agregado al área de preparación.\n");
    }

    public void Commit(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            Console.WriteLine("Uso: commit <mensaje>\n");
            return;
        }

        if (stagingArea.Count == 0)
        {
            Console.WriteLine("No hay archivos en el área de preparación para hacer commit.\n");
            return;
        }

        Dictionary<string, string> filesContent = new Dictionary<string, string>();
        foreach (var fileName in stagingArea)
        {
            string filePath = Path.Combine("repo", fileName);
            string fileContent = File.ReadAllText(filePath);
            filesContent.Add(fileName, fileContent);
        }

        Commit commit = new Commit(message, filesContent, true);
        localCommits.Add(commit);
        stagingArea.Clear();
        Console.WriteLine("Se ha realizado el commit con éxito.\n");
        LogCommits("local");
    }

    public void PushCommit(string parameter)
    {
        if (!string.IsNullOrWhiteSpace(parameter))
        {
            Console.WriteLine("El comando 'push' no acepta parámetros adicionales.\n");
            return;
        }

        if (localCommits.Count == 0)
        {
            Console.WriteLine("No hay commits para enviar al servidor remoto.\n");
            return;
        }

        foreach (var commit in localCommits)
        {
            commit.SetIsLocal(false);
        }
        server.SaveCommits(localCommits);
        localCommits.Clear();
        remoteCommits = server.LoadCommits();
        Console.WriteLine("Los commits locales han sido enviados al servidor remoto.\n");
    }

    public void LogCommits(string parameter)
    {
        Console.WriteLine("Historial de commits:\n");
        if (string.IsNullOrWhiteSpace(parameter) || parameter.ToLower() != "local")
        {
            foreach (var commit in remoteCommits)
            {
                Console.WriteLine($"Commit: {commit.Message}");
                Console.WriteLine($"Fecha: {commit.Date}");
                Console.WriteLine($"Es local: {(commit.IsLocal ? "Si" : "No")}");
                Console.WriteLine("Archivos modificados:");
                foreach (var fileName in commit.Files.Keys)
                {
                    Console.WriteLine($"- {fileName}");
                }
                Console.WriteLine();
            }
            foreach (var commit in localCommits)
            {
                Console.WriteLine($"Commit: {commit.Message}");
                Console.WriteLine($"Fecha: {commit.Date}");
                Console.WriteLine($"Es local: {(commit.IsLocal ? "Si" : "No")}");
                Console.WriteLine("Archivos modificados:");
                foreach (var fileName in commit.Files.Keys)
                {
                    Console.WriteLine($"- {fileName}");
                }
                Console.WriteLine();
            }
        }
        else
        {
            foreach (var commit in localCommits)
            {
                Console.WriteLine($"Commit: {commit.Message}");
                Console.WriteLine($"Fecha: {commit.Date}");
                Console.WriteLine("Archivos modificados:");
                foreach (var fileName in commit.Files.Keys)
                {
                    Console.WriteLine($"- {fileName}");
                }
                Console.WriteLine();
            }
        }
    }

    public void Help(string parameter)
    {
        Console.WriteLine("Comandos disponibles:\n");
        Console.WriteLine("\tadd <nombre_archivo>: Agrega un archivo al área de preparación.\n");
        Console.WriteLine(
            "\tcommit <mensaje>: Realiza un nuevo commit con los archivos en el área de preparación.\n"
        );
        Console.WriteLine("\tpush: Envía los commits locales al servidor remoto.\n");
        Console.WriteLine(
            "\tpull: Trae todos los cambios del último commit remoto sin sobreescribir archivos locales.\n"
        );
        Console.WriteLine(
            "\tmerge: Trae todos los cambios del último commit remoto sobreescribiendo archivos locales.\n"
        );
        Console.WriteLine("\trevert: Borra el último commit local.\n");
        Console.WriteLine(
            "\tlog: Muestra el historial de commits (Parametro 'local' para mostrar solo los commits locales).\n"
        );
        Console.WriteLine("\thelp: Muestra esta ayuda.\n");
        Console.WriteLine("\texit: Sale del programa.\n");
    }

    public void Exit(string parameter)
    {
        Environment.Exit(0);
    }

    public void PullLatestCommit(string parameter)
    {
        Commit latestRemoteCommit = remoteCommits.LastOrDefault();
        Commit latestLocalCommit = localCommits.LastOrDefault();

        if (
            latestRemoteCommit != null
            && (latestLocalCommit == null || latestRemoteCommit.Date > latestLocalCommit.Date)
        )
        {
            Console.WriteLine("Hay nuevos commits remotos. Actualizando archivos...");
            Dictionary<string, string> filesToUpdate = latestRemoteCommit.Files;
            UpdateRepo(filesToUpdate, false);
            Console.WriteLine("Archivos actualizados correctamente.");
        }
        else
        {
            Console.WriteLine("No hay nuevos commits remotos para extraer.");
        }
    }

    public void Merge(string parameter)
    {
        Commit latestRemoteCommit = remoteCommits.LastOrDefault();

        if (latestRemoteCommit != null)
        {
            Dictionary<string, string> filesToUpdate = latestRemoteCommit.Files;
            UpdateRepo(filesToUpdate, true);
            Console.WriteLine("Merge realizado con éxito.");
        }
        else
        {
            Console.WriteLine("No se encontró ningún commit remoto para realizar el merge.");
        }
    }

    public void UpdateRepo(Dictionary<string, string> filesToUpdate, bool isMerge)
    {
        string repoPath = @"repo";
        if (!Directory.Exists(repoPath))
        {
            Directory.CreateDirectory(repoPath);
        }

        foreach (var kvp in filesToUpdate)
        {
            string fileName = kvp.Key;
            string fileContent = kvp.Value;

            string filePath = Path.Combine(repoPath, fileName);

            if (isMerge)
            {
                File.WriteAllText(filePath, fileContent);
            }
            else
            {
                if (!File.Exists(filePath))
                {
                    File.WriteAllText(filePath, fileContent);
                }
            }
        }
    }

    public void Revert(string parameter)
    {
        if (localCommits.Count > 0)
        {
            Commit lastLocalCommit = localCommits[localCommits.Count - 1];
            localCommits.RemoveAt(localCommits.Count - 1);
            Console.WriteLine($"Se ha revertido el último commit local: {lastLocalCommit.Message}");
        }
        else
        {
            Console.WriteLine("No hay commits locales para revertir.");
        }
    }
}
