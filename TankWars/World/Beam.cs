using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    // Represents beam objects in the world
    public class Beam
    {
        // Unique identifier for beam objects
        [JsonProperty(PropertyName = "beam")]
        public int ID { get; private set; } = 0;

        // Auto-incremented ID for tracking removal
        private static int nextID = 0;

        // Vector2D representing origin in world coordinates
        [JsonProperty(PropertyName = "org")]
        public Vector2D origin { get; private set; } = null;

        // Vector2D representing direction in world coordinates
        [JsonProperty(PropertyName = "dir")]
        public Vector2D direction { get; private set; } = null;

        // Beam object's owner (a tank)
        [JsonProperty(PropertyName = "owner")]
        public int ownerID { get; private set; } = 0;

        // Constructor with given owner, origin and direction
        public Beam(int ownerID, Vector2D origin, Vector2D direction)
        {
            this.ID = nextID++;
            this.origin = origin;
            this.direction = direction;
            this.ownerID = ownerID;
        }

        // Default constructor
        public Beam()
        {
            this.ID = nextID++;
            this.origin = null;
            this.direction = null;
            this.ownerID = 0;
        }

        // Overrides ToString to serialize the object to JSON
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

        /// <summary>
        /// Determines if a ray interescts a circle
        /// </summary>
        /// <param name="tankLocation">The center of the tank</param>
        /// <returns></returns>
        public bool IntersectsTank(Vector2D tankLocation)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = direction.Dot(direction);
            double b = ((origin - tankLocation) * 2.0).Dot(direction);
            double c = (origin - tankLocation).Dot(origin - tankLocation) - (World.TankSize / 2) * (World.TankSize / 2);
            
            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }
    }
}
