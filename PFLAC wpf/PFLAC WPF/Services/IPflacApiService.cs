using PFLAC_WPF.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PFLAC_WPF.Services
{
    public interface IPflacApiService
    {
        Task<IReadOnlyList<PhysicalRecord>> GetPhysicalTableAsync(int ageGroup, string gender);
        Task<IReadOnlyList<ScoreResponse>> GetScoresAsync(string gender, int exerciseNumber, double result);
        Task<GradeResponse?> GetGradeAsync(int category, int ageGroup);
    }
}
