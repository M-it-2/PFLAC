using System;
using System.Collections.Generic;
using System.Text;

namespace PFLAC_WPF.Services
{
    public class AgeGroupResolver : IAgeGroupResolver
    {
        public int ResolveAgeGroup(int age) =>
            age switch
            {
                >= 16 and <= 24 => 25,
                >= 25 and <= 29 => 30,
                >= 30 and <= 34 => 35,
                >= 35 and <= 39 => 40,
                >= 40 and <= 44 => 45,
                >= 45 and <= 49 => 50,
                >= 50 and <= 54 => 55,
                >= 55 and <= 59 => 60,
                >= 60 => 70,
                _ => 0
            };
    }
}
