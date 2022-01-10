// Controller class for processing inputs passed from the view and notifying the view of changes received by the server.
// Created Nov 2021 by Sam Christensen and Bryce Gillespie
using NetworkUtil;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Timers;

namespace TankWars
{
    /// <summary>
    /// Main controller class
    /// </summary>
    public class GameController
    {
        // World representing the model
        public World theWorld;
        public int worldSize;

        // Capture input movement direction
        private bool upPressed = false;
        private bool downPressed = false;
        private bool leftPressed = false;
        private bool rightPressed = false;

        // Capture connection state
        public bool isConnected = false;
        private SocketState server;

        // Encapsulates commands to be passed to the server
        private ControlCommand controlCommand;

        // Event handlers
        public delegate void ServerUpdateHandler();
        public event ServerUpdateHandler UpdateArrived;
        public delegate void ViewShowError(string message);
        public event ViewShowError ErrorOccurred;
        public delegate void ClientIsConnected();
        public event ClientIsConnected Connected;

        public GameController()
        {
            // Initialize world
            theWorld = new World(worldSize);
            theWorld.player = new Tank();
            controlCommand = new ControlCommand();
        }

        /// <summary>
        /// Method to begin server connection event loop
        /// </summary>
        /// <param name="serverName">The server to connect to</param>
        /// <param name="playerName">The player's name</param>
        public void Connect(string serverName, string playerName)
        {
            // Validate player name length and display an errror
            if (playerName.Length > 16)
            {
                ErrorOccurred?.Invoke("Player name must be 16 characters or less");
                return;
            }

            // Begin networking event loop
            Networking.ConnectToServer(OnConnect, serverName, 11000);
            theWorld.player.SetName(playerName);
        }

        /// <summary>
        /// Callback for connecting to the server
        /// </summary>
        public void OnConnect(SocketState state)
        {
            if (state.ErrorOccured)
            {
                ErrorOccurred?.Invoke(state.ErrorMessage);
                return;
            }

            isConnected = true;

            // Inform the view
            Connected?.Invoke();

            // Contine the event loop
            Networking.Send(state.TheSocket, theWorld.player.name + '\n');
            server = state;
            state.OnNetworkAction = ReceiveStartupInfo;
            Networking.GetData(state);
        }

        /// <summary>
        /// Callback for receiving startup info (player ID, world size)
        /// </summary>
        private void ReceiveStartupInfo(SocketState state)
        {
            if (state.ErrorOccured)
            {
                ErrorOccurred?.Invoke(state.ErrorMessage);
                return;
            }

            // Parse data received, splitting by new lines
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            if (parts.Length < 2 || !parts[1].EndsWith("\n"))
            {
                Networking.GetData(state);
                return;
            }

            // Set player ID and world size
            theWorld.player.SetID(int.Parse(parts[0]));
            theWorld.SetSize(int.Parse(parts[1]));

            // Remove processed data
            lock (theWorld)
            {
                state.RemoveData(0, parts[0].Length + parts[1].Length);
            }

            // Contine the event loop
            state.OnNetworkAction = ReceiveJson;
            Networking.GetData(state);
        }

        /// <summary>
        /// Parses JSON received from the server to update the model
        /// </summary>
        private void ReceiveJson(SocketState state)
        {
            if (state.ErrorOccured)
            {
                ErrorOccurred?.Invoke(state.ErrorMessage);
                return;
            }

            // Parse the data received, splitting by newline
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            lock (theWorld)
            {
                foreach (string part in parts)
                {
                    //Ignore empty strings added by regex splitter
                    if (parts.Length == 0)
                        continue;

                    if (part.Length < 1 || part[part.Length - 1] != '\n')
                        break;

                    // JSON objects for deserialization
                    JObject obj = JObject.Parse(part);
                    JToken wallToken = obj["wall"];
                    JToken tankToken = obj["tank"];
                    JToken projToken = obj["proj"];
                    JToken beamToken = obj["beam"];
                    JToken powToken = obj["power"];

                    // Populate the model (World) with the correct objects as parsed from JSON
                    if (wallToken != null)
                    {
                        Wall wall = JsonConvert.DeserializeObject<Wall>(part);
                        theWorld.Walls[wall.ID] = wall;
                    }

                    if (tankToken != null)
                    {
                        Tank tank = JsonConvert.DeserializeObject<Tank>(part);

                        // Separate player
                        if (tank.ID == theWorld.player.ID)
                        {
                            theWorld.player = tank;
                        }
                        theWorld.Tanks[tank.ID] = tank;

                        if (tank.disconnected)
                            theWorld.disconnectedTanks.Add(tank.ID);
                    }

                    if (projToken != null)
                    {
                        Projectile proj = JsonConvert.DeserializeObject<Projectile>(part);
                        if (proj.died)
                        {
                            theWorld.Projectiles.Remove(proj.ID);
                        }
                        else
                        {
                            theWorld.Projectiles[proj.ID] = proj;
                        }
                    }

                    if (beamToken != null)
                    {
                        Beam beam = JsonConvert.DeserializeObject<Beam>(part);
                        theWorld.Beams[beam.ID] = beam;
                    }

                    if (powToken != null)
                    {
                        Powerup pow = JsonConvert.DeserializeObject<Powerup>(part);
                        if (pow.died)
                        {
                            theWorld.Powerups.Remove(pow.ID);
                        }
                        else
                        {
                            theWorld.Powerups[pow.ID] = pow;
                        }
                    }
                }
            }

            // Inform the view
            UpdateArrived?.Invoke();

            // Process client inputs
            ProcessInputs();

            // Clear the socket data and continue the event loop
            if (totalData[totalData.Length - 1] == '\n')
            {
                state.ClearData();
            }
            Networking.GetData(state);
        }

        /// <summary>
        /// Sends input commands to the server
        /// </summary>
        public void ProcessInputs()
        {
            if (server != null)
            {
                //Determine moving direction
                if (upPressed)
                    controlCommand.moving = "up";
                else if (downPressed)
                    controlCommand.moving = "down";
                else if (leftPressed)
                    controlCommand.moving = "left";
                else if (rightPressed)
                    controlCommand.moving = "right";
                else
                    controlCommand.moving = "none";

                // Serialize input commands and send to the server
                string command = JsonConvert.SerializeObject(controlCommand) + "\n";
                Networking.Send(server.TheSocket, command);
            }
        }

        /// <summary>
        /// Handles movement request
        /// </summary>
        public void HandleMoveRequest(string direction)
        {
            if (direction == "up")
                upPressed = true;
            else if (direction == "down")
                downPressed = true;
            else if (direction == "left")
                leftPressed = true;
            else if (direction == "right")
                rightPressed = true;
        }

        /// <summary>
        /// Cancels movement request
        /// </summary>
        public void HandleKeyUp(string direction)
        {
            if (direction == "up")
                upPressed = false;
            else if (direction == "down")
                downPressed = false;
            else if (direction == "left")
                leftPressed = false;
            else if (direction == "right")
                rightPressed = false;
        }

        /// <summary>
        /// Handles mouse input
        /// </summary>
        public void HandleMouseRequest(string fireType)
        {
            controlCommand.fire = fireType;
        }

        /// <summary>
        /// Handles mouse button release
        /// </summary>
        public void HandleMouseUp()
        {
            controlCommand.fire = "none";
        }

        /// <summary>
        /// Handles mouse movement
        /// </summary>
        public void HandleMouseMoveRequest(System.Drawing.Point mouseLocation, int viewSize)
        {
            controlCommand.tdir = new Vector2D(mouseLocation.X - (viewSize / 2), mouseLocation.Y - (viewSize / 2));
            controlCommand.tdir.Normalize();
        }
    }
}