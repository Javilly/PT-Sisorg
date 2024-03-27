using System;
using System.Collections.Generic;

namespace GitSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            server = new Server();
            Client client = new Client();

            remoteCommits = server.LoadCommits();

            bool exit = false;
            while (!exit)
            {
                Console.WriteLine(
                    "Ingrese un comando (add, commit, push, pull, merge, revert, log, help, exit):"
                );
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
    }
}
