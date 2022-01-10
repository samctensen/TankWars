using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TankWars
{
    // Class representing settings to be read from XML and passed to World
    public class Settings
    {
        // Settings that can be set (with default values)
        internal int UniverseSize { get; set; } = 2000;
        public int MSPerFrame { get; internal set; } = 17;
        internal int FramesPerShot { get; set; } = 80;
        internal int RespawnRate { get; set; } = 300;
        internal int MaxHP { get; set; } = 3;
        internal int EnginePower { get; set; } = 3;
        internal int TankSize { get; set; } = 60;
        internal double ProjectileSpeed { get; set; } = 25;
        internal int MaxPowerUps { get; set; } = 2;
        internal int WallSize { get; set; } = 50;
        internal int MaxPowerUpDelay { get; set; } = 50;

        // HashSet of Walls parsed from the XML file
        public HashSet<Wall> Walls { get; } = new HashSet<Wall>();

        public Settings(string fileName)
        {
            //Read XML file
            ReadFile(fileName);
        }

        // Parses the settings from a provided XML file
        private void ReadFile(string fileName)
        {
            using (XmlReader reader = XmlReader.Create(fileName))
            {

                //Loop through each element in the document
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            // Universe Size
                            case "UniverseSize":
                                reader.Read();
                                UniverseSize = int.Parse(reader.Value);
                                break;

                            //Movement speed per frame
                            case "MSPerFrame":
                                reader.Read();
                                MSPerFrame = int.Parse(reader.Value);
                                break;

                            //Frames per shot
                            case "FramesPerShot":
                                reader.Read();
                                FramesPerShot = int.Parse(reader.Value);
                                break;

                            //Respawn Rate
                            case "RespawnRate":
                                reader.Read();
                                RespawnRate = int.Parse(reader.Value);
                                break;

                            //Respawn Rate
                            case "MaxHP":
                                reader.Read();
                                MaxHP = int.Parse(reader.Value);
                                break;

                            //Respawn Rate
                            case "EnginePower":
                                reader.Read();
                                EnginePower = int.Parse(reader.Value);
                                break;

                            //Tank Size
                            case "TankSize":
                                reader.Read();
                                TankSize = int.Parse(reader.Value);
                                break;
                            //Projectile Speed
                            case "ProjectileSpeed":
                                reader.Read();
                                ProjectileSpeed = double.Parse(reader.Value);
                                break;
                            // Max Powerups
                            case "MaxPowerUps":
                                reader.Read();
                                MaxPowerUps = int.Parse(reader.Value);
                                break;
                            // Wall Size
                            case "WallSize":
                                reader.Read();
                                WallSize = int.Parse(reader.Value);
                                break;

                            //If 'wall' tag, parse p1 and p2 and add to the HashSet
                            case "Wall":
                                reader.ReadToDescendant("x");
                                reader.Read();
                                int x = int.Parse(reader.Value);
                                reader.ReadToFollowing("y");
                                reader.Read();
                                int y = int.Parse(reader.Value);
                                Vector2D point1 = new Vector2D(x, y);
                                reader.ReadToFollowing("x");
                                reader.Read();
                                x = int.Parse(reader.Value);
                                reader.ReadToFollowing("y");
                                reader.Read();
                                y = int.Parse(reader.Value);
                                Vector2D point2 = new Vector2D(x, y);
                                Walls.Add(new Wall(point1, point2));
                                break;
                        }
                    }
                }
            }
        }
    }
}
