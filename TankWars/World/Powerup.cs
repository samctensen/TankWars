using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    // Class representing powerup objects in the World
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        // Unique identifier
        [JsonProperty(PropertyName = "power")]
        public int ID { get; private set; } = 0;

        // Vector2D representing location
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; internal set; }

        // Whether the powerup has been collected
        [JsonProperty(PropertyName = "died")]
        public bool died { get; internal set; }

        // Auto-incremented ID
        private static int nextID = 0;

        // Default constructor
        public Powerup()
        {
            this.location = new Vector2D(0, 0);
            this.died = false;
            this.ID = nextID++;
        }

        // Constructor to create a powerup at a specified location
        public Powerup(Vector2D v)
        {
            this.location = v;
            this.died = false;
            this.ID = nextID++;

        }

        // Overrides ToString to serialize the object to JSON
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

        // Detects collision with a tank object
        public bool CollidesTank(Vector2D tankLocation)
        {
            return (this.location - tankLocation).Length() < World.TankSize / 2;
        }
    }
}