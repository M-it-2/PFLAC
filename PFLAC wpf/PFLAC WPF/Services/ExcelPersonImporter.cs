using PFLAC_WPF.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using OfficeOpenXml;

namespace PFLAC_WPF.Services
{
    public class ExcelPersonImporter : IExcelPersonImporter
    {
        public List<MilitaryPerson> Import(string filePath)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Betcor");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Файл не знайдено", filePath);

            var persons = new List<MilitaryPerson>();

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int row = 2;

                while (!string.IsNullOrWhiteSpace(worksheet.Cells[row, 1].Text))
                {
                    var name = worksheet.Cells[row, 1].Text;
                    var ageText = worksheet.Cells[row, 2].Text;

                    if (!int.TryParse(ageText, out var age))
                        throw new InvalidDataException($"Некоректний вік у рядку {row}");

                    var person = new MilitaryPerson
                    {
                        Name = name,
                        Age = age
                    };

                    persons.Add(person);
                    row++;
                }
            }

            return persons;
        }
    }
}