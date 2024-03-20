using System;
using System.Collections.Generic;

namespace GitSimulation
{
    class Program
    {
        static List<string> stagingArea = new List<string>();
        static List<Commit> commits = new List<Commit>();
        static List<Commit> localCommits = new List<Commit>();
        private static Server server;

        static Dictionary<string, Action<string>> actions = new Dictionary<string, Action<string>>
        {
            { "add", AddFile },
            { "commit", Commit },
            { "push", PushCommit },
            { "log", LogCommits },
            { "help", Help },
            { "exit", Exit }
        };

        static void Main(string[] args)
        {
            server = new Server(); // Inicializar server dentro del método Main

            commits = server.LoadCommits();

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

            Commit commit = new Commit(message, stagingArea.ToArray(), true);
            localCommits.Add(commit);
            commits.Add(commit);
            stagingArea.Clear();
            Console.WriteLine("Se ha realizado el commit con éxito.\n");
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
            commits = server.LoadCommits();
            Console.WriteLine("Los commits locales han sido enviados al servidor remoto.\n");
        }

        static void LogCommits(string parameter)
        {
            Console.WriteLine("Historial de commits:\n");
            if (string.IsNullOrWhiteSpace(parameter) || parameter.ToLower() != "local")
            {
                foreach (var commit in commits)
                {
                    Console.WriteLine($"Commit: {commit.Message}");
                    Console.WriteLine($"Fecha: {commit.Date}");
                    Console.WriteLine($"Es local: {(commit.IsLocal ? "Si" : "No")}");
                    Console.WriteLine("Archivos modificados:");
                    foreach (var file in commit.Files)
                    {
                        Console.WriteLine($"- {file}");
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
                    foreach (var file in commit.Files)
                    {
                        Console.WriteLine($"- {file}");
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
    }
}
