using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using Newtonsoft.Json;

namespace DealFinder.Network.Models
{
    public class TcgCard
    {
        [JsonProperty]
        public decimal HiPrice { get; private set; }

        [JsonProperty]
        public decimal AvgPrice { get; private set; }

        [JsonProperty]
        public decimal LowPrice { get; private set; }

        //[JsonProperty]
        //public int TcgId { get; private set; }

        [JsonProperty]
        public string Link { get; private set; }

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public string CardSetName { get; private set; }

        [JsonProperty]
        public string Rarity { get; private set; }
        
        //[JsonProperty]
        //public List<Format> Formats { get; private set; }
    }

    public class Format
    {
        [JsonProperty]
        public string Name { get; private set; }
        
        [JsonProperty]
        public string Legality { get; private set; } 
    }
}


