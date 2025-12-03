using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PFLAC_WPF.Models
{
    public class ScoreResponse
    {
        [JsonPropertyName("score_count")]
        public int ScoreCount { get; set; }
    }
}
