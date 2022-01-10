using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    // Class representing a projectile object in the World
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        // Unique identifier
        [JsonProperty(PropertyName = "proj")]
        public int ID { get; private set; } = 0;

        // Auto-incremented ID
        private static int nextID = 0;

        // Vector2D representing location in world coordinates
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; internal set; }

        // Vector2D representing direction in world coordinates
        [JsonProperty(PropertyName = "dir")]
        public Vector2D direction { get; private set; }

        // Vector2D representing velocity
        public Vector2D velocity { get; internal set; }

        // If the projectile has collided with an object (wall or tank)
        [JsonProperty(PropertyName = "died")]
        public bool died { get; internal set; }

        // Owner (tank) of the projectile
        [JsonProperty(PropertyName = "owner")]
        public int ownerID { get; private set; }

        // Creates a new projectile with a given owner, location and direction
        public Projectile(int ownerID, Vector2D location, Vector2D direction)
        {
            this.ID = nextID++;
            this.location = location;
            this.direction = direction;
            this.velocity = direction; 
            this.died = false;
            this.ownerID = ownerID;
        }

        // Default constructor
        public Projectile()
        {
            this.ID = nextID++;
            this.location = null;
            this.direction = null;
            this.died = false;
            this.ownerID = 0;
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