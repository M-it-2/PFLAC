using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;

namespace PFLAC
{
  internal class FileReader
  {
    public List<MilitaryPerson> ReadFromExcel(string filePath)
    {
      ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
      if (!File.Exists(filePath))
      {
        throw new FileNotFoundException("Файл не знайдено!!", filePath);
      }

      var militaryPersons = new List<MilitaryPerson>();

      using (var package = new ExcelPackage(new FileInfo(filePath)))
      {
        var worksheet = package.Workbook.Worksheets[0]; // take the first sheet

        int row = 2; // assume that the first line is the headings
        while (!string.IsNullOrEmpty(worksheet.Cells[row, 1].Text))
        {
          string fullName = worksheet.Cells[row, 1].Text;
          int age = int.Parse(worksheet.Cells[row, 2].Text);

          militaryPersons.Add(new MilitaryPerson(fullName, age));
          row++;
        }
      }

      return militaryPersons;
    }
  }
}
