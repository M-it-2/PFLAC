using System;
using System.Collections.Generic;
using System.Text;

namespace PFLAC_WPF.Models
{
    public class MilitaryPerson
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }

        public string Gender { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public int Category { get; set; } = 3;
        public int AgeGroup { get; set; }

        public List<int> Norms { get; set; } = new();
        public List<double> Results { get; set; } = new();
        public List<int> Scores { get; set; } = new();

        public GradeResponse GradeResponse { get; set; } = new();
        public int Grade { get; set; }
    }
}
