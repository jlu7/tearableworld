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

        [JsonProperty]
        public int TcgId { get; private set; }

        [JsonProperty]
        public string Link { get; private set; }

        [JsonProperty]
        public string ProductName { get; private set; }
    }
}


