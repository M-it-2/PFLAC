using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PFLAC
{
  internal class ScoreResponse
  {
    [JsonPropertyName("score_count")]
    public int ScoreCount { get; set; }
  }
}
