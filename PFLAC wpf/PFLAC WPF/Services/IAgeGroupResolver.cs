using System;
using System.Collections.Generic;
using System.Text;

namespace PFLAC_WPF.Services
{
    public interface IAgeGroupResolver
    {
        int ResolveAgeGroup(int age);
    }
}
