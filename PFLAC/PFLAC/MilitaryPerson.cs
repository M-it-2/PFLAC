using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PFLAC
{
  internal class MilitaryPerson
  {
    public string Name {  get; set; }
    public int Age { get; set; }
    public string Gender { get; set; }
    public string Status { get; set; }

    public int Category { get; set; }
    public int AgeGroup { get; set; }

    public List<int> Norms { get; set; }
    public List<double> Results { get; set; }
    public List<int> Scores;
    public GradeResponse GradeResponse { get; set; }
    public int Grade;

    public MilitaryPerson()
    {
      Results = new List<double>();
      Scores = new List<int>();
    }
    public MilitaryPerson(string name, int age)
    {
      Name = name;
      Age = age;
      AgeGroup = GetAgeGroup(age);
      Category = 3;
      Norms = new List<int>();
      Results = new List<double>();
      Scores = new List<int>();
      GradeResponse = new GradeResponse();
    }
    public static int GetAgeGroup(int age)
    {
      var ageGroups = new Dictionary<Func<int, bool>, int>
        {
            { a => a >= 16 && a <= 24, 25 },
            { a => a >= 25 && a <= 29, 30 },
            { a => a >= 30 && a <= 34, 35 },
            { a => a >= 35 && a <= 39, 40 },
            { a => a >= 40 && a <= 44, 45 },
            { a => a >= 45 && a <= 49, 50 },
            { a => a >= 50 && a <= 54, 55 },
            { a => a >= 55 && a <= 59, 60 },
            { a => a >= 60, 70 }
        };

      foreach (var group in ageGroups)
      {
        if (group.Key(age))
          return group.Value;
      }
      return 0;
    }
    public void CalculateGrade()
    {
      List<int> ratings = new List<int>();
      ratings = ExtractRatings();

      int totalCount = 0;
      foreach (var score in Scores)
      {
        totalCount += score;
        if (score < GradeResponse.Score)
        {
          Grade = 2;
          return;
        }
      }
      if (ratings[0] <= totalCount && ratings[1] > totalCount)
      {
        Grade = 3;
      }
      else if (ratings[1] <= totalCount && ratings[2] > totalCount)
      {
        Grade = 4;
      }
      else
      {
        Grade = 5;
      }
    }
    private List<int> ExtractRatings()
    {
      return new List<int>
    {
        ParseRating(GradeResponse.Rating3),
        ParseRating(GradeResponse.Rating4),
        ParseRating(GradeResponse.Rating5)
    };
    }

    private int ParseRating(string rating)
    {
      if (!string.IsNullOrWhiteSpace(rating) && rating.Contains("/"))
      {
        var parts = rating.Split('/');
        if (int.TryParse(parts[0], out int result))
        {
          return result;
        }
      }
      return 0;
    }
  }
}
