using PFLAC_WPF.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace PFLAC_WPF.Services
{
    public class PflacApiService  : IPflacApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:8000";

        public PflacApiService(HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("PFLACApp/1.0");
        }

        private static int AdjustAge(int ageGroup, string gender)
        {
            var isMan = string.Equals(gender, "man", StringComparison.OrdinalIgnoreCase);

            if (isMan)
            {
                return ageGroup switch
                {
                    25 or 30 or 35 or 40 => 40,
                    45 => 44,
                    50 => 49,
                    55 or 60 or 70 => 50,
                    _ => ageGroup
                };
            }

            return ageGroup switch
            {
                25 or 30 => 30,
                35 or 40 => 39,
                45 or 50 or 55 or 60 or 70 => 40,
                _ => ageGroup
            };
        }

        public async Task<IReadOnlyList<PhysicalRecord>> GetPhysicalTableAsync(int ageGroup, string gender)
        {
            var adjustedAgeGroup = AdjustAge(ageGroup, gender);
            var url = $"{BaseUrl}/table_physical/?age_group={adjustedAgeGroup}&gender={gender}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var records = JsonSerializer.Deserialize<List<PhysicalRecord>>(body) ?? new List<PhysicalRecord>();

            return records;
        }

        public async Task<IReadOnlyList<ScoreResponse>> GetScoresAsync(
            string gender,
            int exerciseNumber,
            double result)
        {
            var resultString = result.ToString(CultureInfo.InvariantCulture);
            var url = $"{BaseUrl}/table_scoring/?gender={gender}&exercise_num={exerciseNumber}&result={resultString}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();

            try
            {
                var scores = JsonSerializer.Deserialize<List<ScoreResponse>>(body)
                             ?? new List<ScoreResponse>();

                return scores;
            }
            catch (JsonException)
            {
                return Array.Empty<ScoreResponse>();
            }
        }

        public async Task<GradeResponse?> GetGradeAsync(int category, int ageGroup)
        {
            var url = $"{BaseUrl}/table_standarts/?category={category}&age_group={ageGroup}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync();
            var grades = JsonSerializer.Deserialize<List<GradeResponse>>(body);

            if (grades == null || grades.Count == 0)
                return null;

            return grades[0];
        }
    }
}
