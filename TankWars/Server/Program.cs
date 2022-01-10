using System;

namespace TankWars
{
    class Program
    {
        static void Main(string[] args)
        {
            // Reads settings from a file at the specified path
            Settings settings = new Settings(@"..\..\..\..\Resources\settings.xml");

            // Creates a new ServerController with passed settings
            ServerController serverController = new ServerController(settings);

            // Starts the controller
            serverController.Start();

            // Keeps console open until input is received
            Console.Read();
        }
    }
}
