using System;
using System.Collections.Generic;

namespace GitSimulation
{
    class Program
    {
        static List<string> stagingArea = new List<string>();
        static List<Commit> remoteCommits = new List<Commit>();
        static List<Commit> localCommits = new List<Commit>();
        private static Server server;

        static Dictionary<string, Action<string>> actions = new Dictionary<string, Action<string>>
        {
            { "add", AddFile },
            { "commit", Commit },
            { "push", PushCommit },
            { "log", LogCommits },
            { "pull", PullLatestCommit },
            { "merge", Merge },
            { "help", Help },
            { "exit", Exit }
        };

        static void Main(string[] args)
        {
            server = new Server(); // Inicializar server dentro del método Main

            remoteCommits = server.LoadCommits();

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine("Ingrese un comando (add, commit, push, log, help, exit):");
                string? input = Console.ReadLine();
                string[] parts = input?.Split(' ', 2) ?? new string[] { "", "" }; // Dividir el input o enviar string vacíos
                string commandName = parts[0].Trim();
                string parameter = parts.Length > 1 ? parts[1].Trim() : "";

                if (actions.ContainsKey(commandName))
                {
                    actions[commandName].Invoke(parameter);
                }
                else
                {
                    Console.WriteLine(
                        "Comando no reconocido. Por favor, ingrese 'help' para ver la lista de comandos disponibles.\n"
                    );
                }
            }
        }

        static void AddFile(string fileName)
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
            Console.WriteLine(
                $"El archivo '{fileName}' ha sido agregado al área de preparación.\n"
            );
        }

        static void Commit(string message)
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

        static void PushCommit(string parameter)
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

        static void LogCommits(string parameter)
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

        static void Help(string parameter)
        {
            Console.WriteLine("Comandos disponibles:\n");
            Console.WriteLine(
                "\tadd <nombre_archivo>: Agrega un archivo al área de preparación.\n"
            );
            Console.WriteLine(
                "\tcommit <mensaje>: Realiza un nuevo commit con los archivos en el área de preparación.\n"
            );
            Console.WriteLine("\tpush: Envía los commits locales al servidor remoto.\n");
            Console.WriteLine(
                "\tlog: Muestra el historial de commits (Parametro 'local' para mostrar solo los commits locales).\n"
            );
            Console.WriteLine("\thelp: Muestra esta ayuda.\n");
            Console.WriteLine("\texit: Sale del programa.\n");
        }

        static void Exit(string parameter)
        {
            Environment.Exit(0);
        }

        static void PullLatestCommit(string parameter)
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

        static void Merge(string parameter)
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

        static void UpdateRepo(Dictionary<string, string> filesToUpdate, bool isMerge)
        {
            string repoPath = @"repo";

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
    }
}
