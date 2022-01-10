using System;
using System.Collections.Generic;

namespace TankWars
{
    // World class used as a model by the server and client. Stores all objects and monitors 
    // their interactions within the world.
    // Created December 2021 by Bryce Gillespie and Sam Christensen 
    public class World
    {
        // World constants. Initially set but can be modified if provided in the settings file

        // Hits before a tank dies
        public static int MaxHP = 3;
        // Speed of tank
        public static double EnginePower = 3;
        // Tank model size
        public static int TankSize = 60;
        // How frequently a tank can shoot
        public static int FramesPerShot = 80;
        // Speed of projectiles when fired
        public static double ProjectileSpeed = 25;
        // The most powerups allowed in the world at the same time
        public static int MaxPowerUps = 2;
        // The most frames the world will wait before spawning a new powerup
        public static int MaxPowerUpDelay = 1650;
        // Wall model size
        public static int WallSize = 50;
        // How many frames a player will wait before respawning after death
        public static int RespawnRate = 300;

        // Dictionary of all Tanks in the model
        public Dictionary<int, Tank> Tanks;
        // Dictionary of all Beams in the model
        public Dictionary<int, Beam> Beams;
        // Dictionary of all Powerups in the model
        public Dictionary<int, Powerup> Powerups;
        // Dictionary of all Walls in the model
        public Dictionary<int, Wall> Walls;
        // Dictionary of all Projectiles in the model
        public Dictionary<int, Projectile> Projectiles;
        // Dictionary of all ControlCommands in the model
        public Dictionary<int, ControlCommand> ControlCommands;

        // Lists used to store the IDS of dead objects needing to be removed from the world.
        public List<int> deadProjectiles;
        public List<int> deadPowerups;
        public List<int> deadBeams;
        public List<int> disconnectedTanks;

        // Frame counter used to decide if a powerup can be spawned
        public int powerupFrameCounter;

        // Clients personal tank
        public Tank player;

        // Random used for random spawns and time between powerup respawns
        private Random r;

        // World size
        public int size { get; private set; }

        /// <summary>
        /// Constructor for the world class without a settings file. Uses the set constants.
        /// </summary>
        /// <param name="_size"> the size of the world </param>
        public World(int _size)
        {
            Tanks = new Dictionary<int, Tank>();
            Beams = new Dictionary<int, Beam>();
            Powerups = new Dictionary<int, Powerup>();
            Walls = new Dictionary<int, Wall>();
            Projectiles = new Dictionary<int, Projectile>();
            ControlCommands = new Dictionary<int, ControlCommand>();
            deadProjectiles = new List<int>();
            deadPowerups = new List<int>();
            deadBeams = new List<int>();
            disconnectedTanks = new List<int>();
            powerupFrameCounter = MaxPowerUpDelay;
            r = new Random();

            player = new Tank();

            size = _size;
        }

        /// <summary>
        /// Constructor for the world class that uses a settings file. Constants are
        /// set to those in the file.
        /// </summary>
        /// <param name="settings"> Settings file with constants and wall information </param>
        public World(Settings settings)
        {
            Tanks = new Dictionary<int, Tank>();
            Beams = new Dictionary<int, Beam>();
            Powerups = new Dictionary<int, Powerup>();
            Walls = new Dictionary<int, Wall>();
            Projectiles = new Dictionary<int, Projectile>();
            ControlCommands = new Dictionary<int, ControlCommand>();
            deadProjectiles = new List<int>();
            deadPowerups = new List<int>();
            deadBeams = new List<int>();
            disconnectedTanks = new List<int>();
            r = new Random();

            // Sets constants to those found in the settings file
            MaxHP = settings.MaxHP;
            EnginePower = settings.EnginePower;
            TankSize = settings.TankSize;
            FramesPerShot = settings.FramesPerShot;
            ProjectileSpeed = settings.ProjectileSpeed;
            MaxPowerUps = settings.MaxPowerUps;
            MaxPowerUpDelay = settings.MaxPowerUpDelay;
            WallSize = settings.WallSize;
            RespawnRate = settings.RespawnRate;
            MaxPowerUpDelay = settings.MaxPowerUpDelay;


            powerupFrameCounter = MaxPowerUpDelay;

            player = new Tank();

            size = settings.UniverseSize;
        }

        /// <summary>
        /// Setter for the world size
        /// </summary>
        /// <param name="size"> the new world size to be set </param>
        public void SetSize(int size)
        {
            this.size = size;
        }

        /// <summary>
        /// Updates all objects and their interactions within the worlds. 
        /// </summary>
        public void Update()
        {
            UpdateTankStatus();
            ProcessCommands();
            UpdateTankMovement();
            UpdateProjectiles();
            UpdatePowerUps();
        }

        /// <summary>
        /// Helper method called in Update. Uses the JSON commands sent to change the 
        /// velocity of the clients tank based on their key press, and fire projectiles and beams 
        /// when the mouse buttons are clicked.
        /// </summary>
        private void ProcessCommands()
        {
            foreach (KeyValuePair<int, ControlCommand> cmd in ControlCommands)
            {
                Tank tank = Tanks[cmd.Key];
                if (tank.hitPoints < 1)
                {
                    continue;
                }

                // Update tank movement
                switch (cmd.Value.moving)
                {
                    case "up":
                        tank.velocity = new Vector2D(0, -1);
                        break;
                    case "down":
                        tank.velocity = new Vector2D(0, 1);
                        break;
                    case "left":
                        tank.velocity = new Vector2D(-1, 0);
                        break;
                    case "right":
                        tank.velocity = new Vector2D(1, 0);
                        break;
                    default:
                        tank.velocity = new Vector2D(0, 0);
                        break;
                }
                if (!tank.velocity.Equals(new Vector2D(0, 0)))
                {
                    tank.orientation = tank.velocity;
                }
                tank.velocity *= EnginePower;

                // Update turret direction
                tank.aiming = cmd.Value.tdir;

                // Update the beams and projectiles if fired
                switch (cmd.Value.fire)
                {
                    case "main":
                        if (Tanks[cmd.Key].shotCounter > 0)
                            break;
                        // Creates new projectile from the tank
                        Projectile p = new Projectile(tank.ID, tank.location, tank.aiming);
                        p.velocity *= ProjectileSpeed;
                        Projectiles[p.ID] = p;

                        // Resets tanks frames between shot counter
                        Tanks[cmd.Key].shotCounter = FramesPerShot;
                        break;

                    case "alt":
                        if (tank.numberOfPowerups > 0)
                        {
                            // Creates a new beam and immediately detects collision
                            Beam b = new Beam(tank.ID, tank.location, tank.aiming);
                            Beams[b.ID] = b;
                            BeamHitsTank(b);
                            deadBeams.Add(b.ID);
                            tank.numberOfPowerups--;
                        }
                        break;
                }
            }
            ControlCommands.Clear();
        }

        /// <summary>
        /// Helper method called in Update. Updates current tank movement and checks for 
        /// collisions with walls and powerups. Teleports the tank to the other side of 
        /// the world if they drive over the world boundaries.
        /// </summary>
        private void UpdateTankMovement()
        {
            foreach (Tank tank in Tanks.Values)
            {
                // If projectile recently fired
                if (tank.shotCounter > 0)
                    tank.shotCounter--;

                if (tank.velocity.Length() == 0)
                {
                    continue;
                }

                if (tank.hitPoints < 1)
                {
                    continue;
                }

                Vector2D newLocation = tank.location + tank.velocity;
                // Checks for world wrap around on X axis
                if (Math.Abs(tank.location.GetX()) > (size / 2) - TankSize / 2)
                {
                    if (tank.location.GetX() < 0)
                    {
                        newLocation = new Vector2D(-(newLocation.GetX() + TankSize / 2), newLocation.GetY());
                    }
                    else
                    {
                        newLocation = new Vector2D(-(newLocation.GetX() - TankSize / 2), newLocation.GetY());
                    }
                }
                // Checks for world wrap around on Y axis
                if (Math.Abs(tank.location.GetY()) > (size / 2) - TankSize / 2)
                {
                    if (tank.location.GetY() < 0)
                    {
                        newLocation = new Vector2D(newLocation.GetX(), -(newLocation.GetY() + TankSize / 2));
                    }
                    else
                    {
                        newLocation = new Vector2D(newLocation.GetX(), -(newLocation.GetY() - TankSize / 2));
                    }
                }

                // Checks for collisions with walls
                bool collision = false;
                double padding = WallSize / 2 + TankSize / 2;
                foreach (Wall wall in Walls.Values)
                {
                    // Stops tank from moving into wall
                    if (wall.Collision(padding, newLocation))
                    {
                        collision = true;
                        tank.velocity = new Vector2D(0, 0);
                        break;
                    }
                }
                if (!collision)
                {
                    tank.location = newLocation;
                }

                // Checks for collisions with powerups
                foreach (Powerup p in Powerups.Values)
                {
                    if (p.CollidesTank(tank.location))
                    {
                        p.died = true;
                        deadPowerups.Add(p.ID);
                        tank.numberOfPowerups++;
                    }
                }
            }
        }

        /// <summary>
        /// Helper method called in Update. Checks for disconnected and dead tanks.
        /// If a tank is dead, the tank will respawn after the right amount of frames.
        /// </summary>
        private void UpdateTankStatus()
        {
            foreach (Tank tank in Tanks.Values)
            {
                if (tank.disconnected)
                {
                    disconnectedTanks.Add(tank.ID);
                }
                if (tank.CanSpawn() && tank.hitPoints < 1)
                {
                    tank.hitPoints = MaxHP;
                    tank.location = RandomSpawn();
                }
                else
                {
                    tank.tankRespawnCounter--;
                }
                if (tank.died && !tank.disconnected)
                {
                    tank.died = false;
                }
            }
        }

        /// <summary>
        /// Helper method called in Update. Updates the projectile location after being fired. 
        /// Checks for any collisions with walls or tanks and updates health accordingly. Checks
        /// if the projectile has exited the world and removes it.
        /// </summary>
        private void UpdateProjectiles()
        {
            foreach (Projectile p in Projectiles.Values)
            {
                Vector2D newLocation = p.location + p.velocity;
                bool collision = false;
                double padding = WallSize / 2;
                // Checks for collisions with walls
                foreach (Wall wall in Walls.Values)
                {
                    if (wall.Collision(padding, newLocation) || Math.Abs(p.location.GetX()) > size / 2 ||  Math.Abs(p.location.GetY()) > size / 2)
                    {
                        collision = true;
                        p.died = true;
                        deadProjectiles.Add(p.ID);
                        p.velocity = new Vector2D(0, 0);
                        break;
                    }
                }
                if (!collision)
                {
                    p.location = newLocation;
                }

                // Checks for collisions with tanks
                foreach (Tank tank in Tanks.Values)
                {
                    if (p.CollidesTank(tank.location))
                    {
                        // Updates tank health
                        if (p.ownerID != tank.ID && tank.hitPoints > 0)
                        {
                            p.died = true;
                            deadProjectiles.Add(p.ID);
                            tank.hitPoints--;
                            if (tank.hitPoints < 1)
                            {
                                if(Tanks.ContainsKey(p.ownerID))
                                    Tanks[p.ownerID].score++;
                                tank.died = true;
                                tank.tankRespawnCounter = RespawnRate;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Helper method called in Update. Checks if a powerup can be spawned,
        /// and adds one to the model if it can.
        /// </summary>
        private void UpdatePowerUps()
        {
            // Generates new powerup if enough frames have elapsed
            if (powerupFrameCounter > MaxPowerUpDelay && Powerups.Count < MaxPowerUps)
            {
                powerupFrameCounter = r.Next(1, MaxPowerUpDelay);
                Powerup p = new Powerup(RandomSpawn());
                Powerups[p.ID] = p;
            }
            powerupFrameCounter++;
        }

        /// <summary>
        /// Helper method called immidiately after a beam is fired to detect collisions with tanks
        /// </summary>
        /// <param name="b"> the beam that was fired</param>
        private void BeamHitsTank(Beam b)
        {
            foreach (Tank tank in Tanks.Values)
            {
                if (b.IntersectsTank(tank.location))
                {
                    tank.hitPoints = 0;
                    tank.died = true;
                    tank.tankRespawnCounter = RespawnRate;
                    Tanks[b.ownerID].score++;
                }
            }
        }

        /// <summary>
        /// Method used to generate a random vector location within the world.
        /// Checks that the new location is not colliding with any objects already in the model.
        /// </summary>
        /// <returns></returns>
        public Vector2D RandomSpawn()
        {
            bool validLocation = false;
            bool validTankLocation = false;
            bool validWallLocation = false;
            Vector2D location = new Vector2D(0, 0);

            while (!validLocation)
            {
                int x = r.Next(-(size) / 2, size / 2);
                int y = r.Next(-(size) / 2, size / 2);
                location = new Vector2D(x, y);

                // Checks for collisions with tanks
                if (Tanks.Count > 0)
                {
                    foreach (Tank tank in Tanks.Values)
                    {
                        if ((tank.location - location).Length() > TankSize / 2)
                        {
                            validTankLocation = true;
                        }
                    }
                }
                else
                {
                    validTankLocation = true;
                }

                // Checks for collisions with walls
                double padding = WallSize / 2 + TankSize / 2 + 100;
                foreach (Wall w in Walls.Values)
                {
                    if (w.Collision(padding, location))
                    {
                        validWallLocation = false;
                        break;
                    }


                    else
                        validWallLocation = true;
                }

                if (validWallLocation && validTankLocation)
                    validLocation = true;
            }
            return location;
        }

        /// <summary>
        /// Iterates through the lists of dead objects and removes them from the world dictionaries.
        /// </summary>
        public void RemoveDeadObjects()
        {
            foreach (int p in deadProjectiles)
            {
                Projectiles.Remove(p);
            }

            foreach (int p in deadPowerups)
            {
                Powerups.Remove(p);
            }

            foreach (int b in deadBeams)
            {
                Beams.Remove(b);
            }

            foreach (int t in disconnectedTanks)
            {
                Tanks.Remove(t);
            }

            // Clears lists after removal
            deadProjectiles.Clear();
            deadPowerups.Clear();
            deadBeams.Clear();
            disconnectedTanks.Clear();
        }
    }
}