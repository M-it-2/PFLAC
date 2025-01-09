using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PFLAC
{
  internal class GradeResponse
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
    public string Rating5 { get; set; }

    [JsonPropertyName("rating_4")]
    public string Rating4 { get; set; }

    [JsonPropertyName("rating_3")]
    public string Rating3 { get; set; }
  }
}
