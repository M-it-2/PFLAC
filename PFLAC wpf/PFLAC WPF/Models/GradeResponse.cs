using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace PFLAC_WPF.Models
{
    public class GradeResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("category")]
        public int Category { get; set; }

        [JsonPropertyName("age_group")]
        public int AgeGroup { get; set; }

        [JsonPropertyName("score")]
        public int Score { get; set; }

        [JsonPropertyName("rating_5")]
        public string Rating5 { get; set; } = string.Empty;

        [JsonPropertyName("rating_4")]
        public string Rating4 { get; set; } = string.Empty;

        [JsonPropertyName("rating_3")]
        public string Rating3 { get; set; } = string.Empty;

    }
}
