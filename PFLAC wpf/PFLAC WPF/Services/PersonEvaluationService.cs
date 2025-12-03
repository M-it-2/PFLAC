using PFLAC_WPF.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PFLAC_WPF.Services
{
    public class PersonEvaluationService
    {
        private readonly IExcelPersonImporter _excelImporter;
        private readonly IPflacApiService _api;
        private readonly IAgeGroupResolver _ageGroupResolver;
        private readonly IGradeCalculator _gradeCalculator;

        public PersonEvaluationService(
            IExcelPersonImporter excelImporter,
            IPflacApiService api,
            IAgeGroupResolver ageGroupResolver,
            IGradeCalculator gradeCalculator)
        {
            _excelImporter = excelImporter;
            _api = api;
            _ageGroupResolver = ageGroupResolver;
            _gradeCalculator = gradeCalculator;
        }

        public List<MilitaryPerson> LoadPersonsFromExcel(string filePath)
        {
            var persons = _excelImporter.Import(filePath);

            foreach (var p in persons)
            {
                p.AgeGroup = _ageGroupResolver.ResolveAgeGroup(p.Age);
                p.Category = 3;
            }

            return persons;
        }

        public void UpdatePersonBasicInfo(
            MilitaryPerson person,
            string name,
            int age,
            string gender, 
            string status, 
            int category)  
        {
            person.Name = name;
            person.Age = age;
            person.Gender = gender;
            person.Status = status;
            person.Category = category;
            person.AgeGroup = _ageGroupResolver.ResolveAgeGroup(age);
        }

        public async Task<IReadOnlyList<PhysicalRecord>> LoadNormsAsync(MilitaryPerson person)
        {
            var records = await _api.GetPhysicalTableAsync(person.AgeGroup, person.Gender);

            person.Norms.Clear();
            foreach (var r in records)
                person.Norms.Add(r.ExerciseNumber);

            return records;
        }

        public async Task<int> CalculateGradeAsync(
            MilitaryPerson person,
            double norm1,
            double norm2,
            double norm3)
        {
            if (person.Norms.Count < 3)
                throw new InvalidOperationException("Нормативи ще не завантажені.");

            person.Results.Clear();
            person.Results.Add(norm1);
            person.Results.Add(norm2);
            person.Results.Add(norm3);

            person.Scores.Clear();

            for (int i = 0; i < 3; i++)
            {
                var exerciseNum = person.Norms[i];
                var resultValue = person.Results[i];

                var scores = await _api.GetScoresAsync(person.Gender, exerciseNum, resultValue);
                foreach (var s in scores)
                    person.Scores.Add(s.ScoreCount);
            }

            var gradeResponse = await _api.GetGradeAsync(person.Category, person.AgeGroup);
            if (gradeResponse == null)
                throw new InvalidOperationException("Не вдалося отримати таблицю оцінок.");

            person.GradeResponse = gradeResponse;

            var grade = _gradeCalculator.CalculateGrade(person.Scores, person.GradeResponse);
            person.Grade = grade;

            return grade;
        }
    }
}
