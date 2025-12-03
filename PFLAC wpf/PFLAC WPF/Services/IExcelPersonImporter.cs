using PFLAC_WPF.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PFLAC_WPF.Services
{
    public interface IExcelPersonImporter
    {
        List<MilitaryPerson> Import(string filePath);
    }
}
