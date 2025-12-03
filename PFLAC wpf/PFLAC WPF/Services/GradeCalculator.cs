using PFLAC_WPF.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PFLAC_WPF.Services
{
    public class GradeCalculator : IGradeCalculator
    {
        public int CalculateGrade(IReadOnlyList<int> scores, GradeResponse gradeResponse)
        {
            if (scores == null || scores.Count == 0 || gradeResponse == null)
                return 0;

            if (scores.Any(s => s < gradeResponse.Score))
                return 2;

            var ratings = ExtractRatings(gradeResponse);
            var totalScore = scores.Sum();

            if (ratings[0] <= totalScore && ratings[1] > totalScore)
                return 3;

            if (ratings[1] <= totalScore && ratings[2] > totalScore)
                return 4;

            return 5;
        }

        private static List<int> ExtractRatings(GradeResponse gradeResponse)
        {
            return new List<int>
            {
                ParseRating(gradeResponse.Rating3),
                ParseRating(gradeResponse.Rating4),
                ParseRating(gradeResponse.Rating5)
            };
        }

        private static int ParseRating(string rating)
        {
            if (!string.IsNullOrWhiteSpace(rating) && rating.Contains('/'))
            {
                var parts = rating.Split('/');
                if (int.TryParse(parts[0], out var result))
                    return result;
            }

            return 0;
        }
    }
}
