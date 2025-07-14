using System.Text.Json.Serialization;

namespace PFLAC
{
  internal class PhysicalRecord
  {
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("age_group")]
    public int AgeGroup { get; set; }

    [JsonPropertyName("gender")]
    public string Gender { get; set; }

    [JsonPropertyName("exercise_number")]
    public int ExerciseNumber { get; set; }

    [JsonPropertyName("exercise_name")]
    public string ExerciseName { get; set; }
  }
}
