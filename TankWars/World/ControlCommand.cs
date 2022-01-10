// Class for handling inputs to send to the server
// Created Nov 2021 by Sam Christensen and Bryce Gillespie
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TankWars
{
    // Class representing JSON-serializable commands sent from the client to the server
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommand
    {
        // Movement direction
        [JsonProperty(PropertyName = "moving")]
        public string moving { get; set; } = "none";

        // Firing mode (none, main, alt)
        [JsonProperty(PropertyName = "fire")]
        public string fire { get; set; } = "none";

        // Turret direction vector
        [JsonProperty(PropertyName = "tdir")]
        public Vector2D tdir { get; set; } = new Vector2D(0, 0);
    }
}