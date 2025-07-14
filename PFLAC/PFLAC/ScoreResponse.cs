using System.Text.Json.Serialization;

namespace PFLAC
{
  internal class ScoreResponse
  {
    [JsonPropertyName("score_count")]
    public int ScoreCount { get; set; }
  }
}
