using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TankWars
{
    [JsonObject(MemberSerialization.OptIn)]
    // Represents wall objects in the world
    public class Wall
    {
        // Locations different coordinates of the wall
        private double top, bottom, left, right;

        // Unique identifier for beam objects
        [JsonProperty(PropertyName = "wall")]
        public int ID { get; private set; } = 0;

        // ID increment used to automatically set unique IDs for each wall
        private static int nextID = 0;

        // Vector2D location of the first point of the wall
        [JsonProperty(PropertyName = "p1")]
        public Vector2D p1 { get; private set; } = null;

        // Vector2D location of the second point of the wall
        [JsonProperty(PropertyName = "p2")]
        public Vector2D p2 { get; private set; } = null;

        /// <summary>
        /// Default constructor for the wall
        /// </summary>
        public Wall()
        {

        }

        /// <summary>
        /// Constructor for the wall which takes two vector locations to be placed between
        /// </summary>
        /// <param name="P1"> first point of the wall </param>
        /// <param name="P2"> second point of the wall </param>
        public Wall(Vector2D P1, Vector2D P2)
        {
            ID = nextID++;
            p1 = P1;
            p2 = P2;

            // Sets point values
            top = Math.Min(P1.GetY(), P2.GetY());
            bottom = Math.Max(P1.GetY(), P2.GetY());
            left = Math.Min(P1.GetX(), P2.GetX());
            right = Math.Max(P1.GetX(), P2.GetX());
            
        }

        // Overrides ToString to serialize the object to JSON
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

        /// <summary>
        /// Detects if a wall collides with the location
        /// </summary>
        /// <param name="padding"> the width of the colliding object </param>
        /// <param name="Location"> the location of the object colliding </param>
        /// <returns></returns>
        public bool Collision(double padding, Vector2D Location)
        {
            return left < Location.GetX() + padding &&
                right > Location.GetX() - padding &&
                top < Location.GetY() + padding &&
                bottom > Location.GetY() - padding;
        }
    }
}