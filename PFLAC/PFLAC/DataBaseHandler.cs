using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Google.Protobuf.Compiler;
using System.Text.Json.Serialization;

namespace PFLAC
{
  class DataBaseHandler
  {
    private static readonly HttpClient _httpClient = new HttpClient();
    private static int AdjustAge(int age, string gender)
    {
      int result = 0;
      if (gender == "man")
      {
        switch (age)
        {
          case 25:
          case 30:
          case 35:
          case 40:
            result = 40;
            break;
          case 45:
            result = 44;
            break;
          case 50:
            result = 49;
            break;
          case 55:
          case 60:
          case 70:
            result = 50;
            break;
        }
      }
      else
      {
        switch (age)
        {
          case 25:
          case 30:
            result = 30;
            break;
          case 35:
          case 40:
            result = 39;
            break;
          case 45:
          case 50:
          case 55:
          case 60:
          case 70:
            result = 40;
            break;
        }
      }
      return result;
    }

    public static async Task<Dictionary<int, string>> GetPhysicalTableAsync(MilitaryPerson person)
    {
      int ageGroup = AdjustAge(person.AgeGroup, person.Gender);

      string apiUrl = $"https://pflac-api.onrender.com/table_physical/?age_group={ageGroup}&gender={person.Gender}";

      try
      {
        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        Messages.Info(responseBody);

        var records = JsonSerializer.Deserialize<List<PhysicalRecord>>(responseBody);

        Dictionary<int, string> result = new Dictionary<int, string>();
        foreach (var record in records)
        {
          result[record.ExerciseNumber] = record.ExerciseName;
          person.Norms.Add(record.ExerciseNumber);
        }

        return result;
      }
      catch (Exception ex)
      {
        Messages.Error($"Ошибка при запросе: {ex.Message}");
        return null;
      }
    }
    public static async Task GetScoresAsync(MilitaryPerson person)
    {
      for (int i = 0; i < 3; i++)
      {
        var exerciseNum = person.Norms[i];
        var resultValue = person.Results[i];

        string url = $"https://pflac-api.onrender.com/table_scoring/?gender={person.Gender}&exercise_num={exerciseNum}&result={resultValue}";

        try
        {
          HttpResponseMessage response = await _httpClient.GetAsync(url);

          response.EnsureSuccessStatusCode();

          string responseBody = await response.Content.ReadAsStringAsync();
          var scores = JsonSerializer.Deserialize<List<ScoreResponse>>(responseBody);

          foreach (var score in scores)
          {
            person.Scores.Add(score.ScoreCount);
            Messages.Info($"Score (1 - 100) {score.ScoreCount}");
          }
        }
        catch (Exception ex)
        {
          Messages.Error($"Error while calling API for URL: {url}. Exception: {ex.Message}");
        }
      }
    }
    public static async Task GetGradeTabAsync(MilitaryPerson person)
    {
      string apiUrl = $"https://pflac-api.onrender.com/table_standarts/?category={person.Category}&age_group={person.AgeGroup}";

      try
      {
        HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

        response.EnsureSuccessStatusCode();

        string responseBody = await response.Content.ReadAsStringAsync();
        Messages.Info(responseBody);

        var resList = JsonSerializer.Deserialize<List<GradeResponse>>(responseBody);
        person.GradeResponse = resList[0];
      }
      catch (Exception ex)
      {
        Messages.Error($"Ошибка при запросе: {ex.Message}");
      }
    }
  }
} 
