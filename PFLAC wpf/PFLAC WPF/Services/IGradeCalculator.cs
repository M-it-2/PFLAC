using PFLAC_WPF.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PFLAC_WPF.Services
{
    public interface IGradeCalculator
    {
        int CalculateGrade(IReadOnlyList<int> scores, GradeResponse gradeResponse);
    }
}
