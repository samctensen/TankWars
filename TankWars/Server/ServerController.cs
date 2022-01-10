using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TankWars
{
    // Server Controller class for starting and managing a server accepting clients.
    // Created December 2021 by Bryce Gillespie and Sam Christensen 
    public class ServerController
    {
        // Settings file for walls and constants
        private Settings settings;
        // World model used to store information on objects
        private World theWorld;
        // List of clients to send and recieve information
        private Dictionary<int, SocketState> clients;
        // String used to send wall JSONS anmd world size
        private string startupInfo;

        /// <summary>
        /// Constructor for the ServerController class. Takes a 
        /// settings file to get world information and in-game constants.
        /// </summary>
        /// <param name="settings"></param>
        public ServerController(Settings settings)
        {
            this.settings = settings;
            theWorld = new World(settings);
            clients = new Dictionary<int, SocketState>();   

            foreach (Wall wall in settings.Walls)
            {
                theWorld.Walls[wall.ID] = wall;
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(theWorld.size + "\n");

            foreach (Wall wall in theWorld.Walls.Values)
            {
                sb.Append(wall.ToString());
            }
            this.startupInfo = sb.ToString();
        }

        /// <summary>
        /// Starts the server and creates a thread to constantly update client views
        /// </summary>
        public void Start()
        {
            Networking.StartServer(NewClient, 11000);
            Thread t = new Thread(Update);
            t.Start();
            Console.WriteLine("Server is running. Accepting clients");
        }

        /// <summary>
        /// Method used to update clients view information. Sends JSONS
        /// to every client updating them on every object in the world.
        /// </summary>
        private void Update()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                while (watch.ElapsedMilliseconds < settings.MSPerFrame) { }
                watch.Restart();
                StringBuilder sb = new StringBuilder();
                // Adds all JSONs into one string to be sent
                lock (theWorld)
                {
                    theWorld.Update();
                    foreach (Tank tank in theWorld.Tanks.Values)
                    {
                        sb.Append(tank.ToString());
                    }
                    foreach (Projectile p in theWorld.Projectiles.Values)
                    {
                        sb.Append(p.ToString());
                    }
                    foreach (Beam b in theWorld.Beams.Values)
                    {
                        sb.Append(b.ToString());
                    }
                    foreach (Powerup p in theWorld.Powerups.Values)
                    {
                        sb.Append(p.ToString());
                    }
                }
                string frame = sb.ToString();
                lock (clients)
                {
                    // Attempts to send the JSON information to every client
                    foreach (SocketState client in clients.Values)
                    {
                        try
                        {
                            Networking.Send(client.TheSocket, frame);
                        }
                        catch
                        {
                            DisconnectClient((int)client.ID);
                        }
                    }
                }
                // Remove projectiles and powerups after collision
                theWorld.RemoveDeadObjects();
            }
        }

        /// <summary>
        /// Initial asynchronus callback method used when the server is started.
        /// </summary>
        /// <param name="client"></param>
        private void NewClient(SocketState client)
        {
            // Checks for any errors in the socket
            if (client.ErrorOccured)
            {
                DisconnectClient((int)client.ID);
                return;
            }

            client.OnNetworkAction = ReceviePlayerName;
            Networking.GetData(client);
        }

        /// <summary>
        /// Second asynchronous callback method used for startup information
        /// with the players name and the worlds walls.
        /// </summary>
        /// <param name="client"> SocketState to send information to </param>
        private void ReceviePlayerName(SocketState client)
        {
            // Checks for any errors in the socket
            if (client.ErrorOccured)
            {
                DisconnectClient((int)client.ID);
                return;
            }

            string name = client.GetData();

            // Check if we've received a full line
            if (!name.EndsWith("\n"))
            {
                client.GetData();
                return;
            }

            client.RemoveData(0, name.Length);
            name = name.Trim();

            Networking.Send(client.TheSocket, client.ID + "\n");
            Networking.Send(client.TheSocket, startupInfo);

            // Creates a new tank for the new player
            lock (theWorld)
            {
                theWorld.Tanks[(int)client.ID] = new Tank((int)client.ID, name, theWorld.RandomSpawn());
            }
            // Adds socket to list of clients
            lock (clients)
            {
                clients[(int)client.ID] = client;
                Console.WriteLine("Player: " + name + " has joined.");
            }
            client.OnNetworkAction = ReceiveControlCommand;
            Networking.GetData(client);
        }

        /// <summary>
        /// Asynchronous callback method used after initial connection. 
        /// Deserializes JSONS sent by the client and updates the model.
        /// </summary>
        /// <param name="client"> the SocketState sending control commands </param>
        private void ReceiveControlCommand(SocketState client)
        {
            // Checks for any errors in the socket
            if (client.ErrorOccured)
            {
                DisconnectClient((int) client.ID);
                return;
            }

            string totalData = client.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            foreach (string part in parts)
            {
                //Ignore empty strings added by regex splitter
                if (parts.Length == 0)
                    continue;

                // Checks for incomplete data
                if (part.Length < 1 || part[part.Length - 1] != '\n')
                    break;

                // Deserializes the command and updates the model
                try
                {
                    ControlCommand ctrlCmd = JsonConvert.DeserializeObject<ControlCommand>(part);
                    lock (theWorld)
                    {
                        theWorld.ControlCommands[(int)client.ID] = ctrlCmd;
                    }
                }
                // Removes the client if they send bad JSONs
                catch
                {
                    DisconnectClient((int) client.ID);
                }
                client.RemoveData(0, part.Length);
            }
            Networking.GetData(client);
        }

        /// <summary>
        /// Called when an error is detected in the socket or when faulty JSONS are sent.
        /// Marks the tank for death, and removes the client from the server.
        /// </summary>
        /// <param name="clientID"> the ID of the errored socket </param>
        private void DisconnectClient(int clientID)
        {
            Console.WriteLine("Client " + clientID + " has disconnected.");
            if (theWorld.Tanks.ContainsKey(clientID))
            {
                lock (theWorld)
                {
                    theWorld.Tanks[clientID].hitPoints = 0;
                    theWorld.Tanks[clientID].disconnected = true;
                    theWorld.Tanks[clientID].died = true;
                }
            }

            if (clients.ContainsKey(clientID))
            {
                lock (clients)
                {
                    clients.Remove(clientID);
                }
            }
        }
    }
}
