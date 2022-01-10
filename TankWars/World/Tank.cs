using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    // Class representing a tank object in the World
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        // Unique ID
        [JsonProperty(PropertyName = "tank")]
        public int ID { get; private set; }

        // Vector2D representing location
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; internal set; }

        // Vector2D representing tank body orientation
        [JsonProperty(PropertyName = "bdir")]
        public Vector2D orientation { get; internal set; }

        // Vector2D representing turret direction
        [JsonProperty(PropertyName = "tdir")]
        public Vector2D aiming { get; internal set; }

        // Vector2D representing tank velocity
        public Vector2D velocity { get; internal set; }

        // Player name
        [JsonProperty(PropertyName = "name")]
        public string name { get; private set; }

        // Current hp
        [JsonProperty(PropertyName = "hp")]
        public int hitPoints { get; set; }

        // Current score
        [JsonProperty(PropertyName = "score")]
        public int score { get; internal set; }

        // If the tank died on the given frame
        [JsonProperty(PropertyName = "died")]
        public bool died { get; set; }

        // If the client to which the tank belongs disconnected on the given frame
        [JsonProperty(PropertyName = "dc")]
        public bool disconnected { get; set; }

        // If the client to which the tank belongs connected on the given frame
        [JsonProperty(PropertyName = "join")]
        public bool joined { get; private set; }

        // Represents number of frames between shots, set in settings.xml
        public int shotCounter { get; internal set; }
        
        // Current number of powerups collected
        public int numberOfPowerups { get; internal set; }

        // Number of frames before respawn is allowed
        public int tankRespawnCounter { get; internal set; }

        // Setter for tank ID
        public void SetID(int ID)
        {
            this.ID = ID;
        }

        // Setter for tank name
        public void SetName(string name)
        {
            this.name = name;
        }

        // Constructs a tank object with a given ID and player name
        public Tank(int id, string name)
        {
            this.ID = id;
            this.location = new Vector2D(0, 0);
            this.orientation = new Vector2D(0, -1);
            this.velocity = new Vector2D(0, 0);
            this.aiming = orientation;
            this.name = name;
            this.hitPoints = World.MaxHP;
            this.score = 0;
            this.died = false;
            this.disconnected = false;
            this.joined = true;
            this.shotCounter = 0;
            this.numberOfPowerups = 0;
            this.tankRespawnCounter = World.RespawnRate;
            
        }

        // Constructs a tank object with a given ID, player name and location
        public Tank(int id, string name, Vector2D loc)
        {
            this.ID = id;
            this.location = loc;
            this.orientation = new Vector2D(0, -1);
            this.velocity = new Vector2D(0, 0);
            this.aiming = orientation;
            this.name = name;
            this.hitPoints = World.MaxHP;
            this.score = 0;
            this.died = false;
            this.disconnected = false;
            this.joined = true;
            this.shotCounter = 0;
            this.numberOfPowerups = 0;
            this.tankRespawnCounter = World.RespawnRate;
            
        }

        // Default constructor
        public Tank()
        {
        }

        // Overrides ToString to serialize the object to JSON
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

        // Returns true if a tank is allowed to respawn, false otherwise
        public bool CanSpawn()
        {
            if (tankRespawnCounter <= 0)
            {
                tankRespawnCounter = World.RespawnRate;
                return true;
            }
            return false;
        }
    }
}