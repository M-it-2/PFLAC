using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Net.NetworkInformation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Google.Protobuf.Compiler;

namespace PFLAC
{
    public partial class Form1 : Form
    {
    private List<MilitaryPerson> persons = new List<MilitaryPerson>();
    private FileReader fileReader = new FileReader();
    private OutputHandler outputHandler = new OutputHandler();
    private DataBaseHandler dataHandler = new DataBaseHandler();

    private int currentIndex = 0;

    public Form1()
    {
      InitializeComponent();
    }
        
    private void Form1_Load(object sender, EventArgs e)
    {
      //string lastUpdated = GetLastChangeDate();
      //string _updateLocal = db.HashGet(key, "last_updated");

      //if (lastUpdated == _updateLocal)
      //{
      //    // connect to db with error))
      //    // MySqlConnect conn = new MySqlConnect();

      //    Messages.Error("1");

      //    GetUpdateDatabase();
      //}

      //if (!EthernetCheck.IsEthernetAvailable())
      //{
      //  // TODO DOCKER REDIS ;)
      //}
    }

    private void LoadFileBtn_Click(object sender, EventArgs e)
    {
      using (OpenFileDialog openFileDialog = new OpenFileDialog())
      {
        openFileDialog.Filter = "Excel Files|*.xlsx;*.xls";
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
          persons = fileReader.ReadFromExcel(openFileDialog.FileName);
          if (persons.Count > 0)
          {
            currentIndex = 0;
            DisplayPerson(persons[currentIndex]);
          }
        }


      }
    }

    //public string GetUpdateDataBase() {
    //   // todo
    //   string connectionString = "Server=localhost;Database=mydatabase;User ID=myuser;Password=mypassword;";

    //    using (MySqlConnection conn = new MySqlConnection(connectionString))
    //    {
    //        conn.Open();
    //        string query = "SELECT date FROM last_change LIMIT 1";

    //        using (MySqlCommand cmd = new MySqlCommand(query, conn))
    //        {
    //            var result = cmd.ExecuteScalar();
    //            return result != null ? result.ToString() : null;
    //        }
    //    }
    //}
    
    private void DisplayPerson(MilitaryPerson person)
    {
      fullNameTxtBox.Text = person.Name;
      ageTxtBox.Text = person.Age.ToString();

      if (person.Gender == "man")
      {
        maleRdBtn.Checked = true;
      }
      else if (person.Gender == "woman")
      {
        femaleRdBtn.Checked = true;
      }

      if (person.Status == "Officer")
      {
        oficerRdBtn.Checked = true;
      }
      else if (person.Status == "Contract")
      {
        soldierRdBtn.Checked = true;
      }
    }

    private void SavePersonData(MilitaryPerson person)
    {
      person.Name = fullNameTxtBox.Text;
      person.Age = int.Parse(ageTxtBox.Text);
      person.AgeGroup = MilitaryPerson.GetAgeGroup(person.Age);
      person.Gender = maleRdBtn.Checked ? "man" : femaleRdBtn.Checked ? "woman" : "";
      person.Status = oficerRdBtn.Checked ? "Officer" : soldierRdBtn.Checked ? "Contract" : "";
      if (cat1RdBtn.Checked)
      {
        person.Category = 1;
      }
      else if (cat2RdBtn.Checked)
      {
        person.Category = 2;
      }
      VisibleFalse();
    }

    private void NextBtn_Click(object sender, EventArgs e)
    {
      if (currentIndex < persons.Count - 1)
      {
        SavePersonData(persons[currentIndex]);
        currentIndex++;
        DisplayPerson(persons[currentIndex]);
      }
    }

    private void PreviousBtn_Click(object sender, EventArgs e)
    {
      if (currentIndex > 0)
      {
        SavePersonData(persons[currentIndex]);
        currentIndex--;
        DisplayPerson(persons[currentIndex]);
      }
    }

    private async void GetNormsBtn_Click(object sender, EventArgs e)
    {
      GetNormsBtn.Enabled = false;

      try
      {
        SavePersonData(persons[currentIndex]);

        var norms = await DataBaseHandler.GetPhysicalTableAsync(persons[currentIndex]);

        if (norms == null || norms.Count == 0)
        {
          this.Invoke((Action)(() =>
          {
            norm1Lbl.Text = "Нет данных";
            norm2Lbl.Text = "Нет данных";
            norm3Lbl.Text = "Нет данных";

            VisibleTrue();
          }));
          return;
        }

        var normList = new List<string>();

        foreach (var kvp in norms)
        {
          normList.Add($"№{kvp.Key}: {kvp.Value}");
        }

        this.Invoke((Action)(() =>
        {
          norm1Lbl.Text = normList.Count > 0 ? normList[0] : "Нет данных";
          norm2Lbl.Text = normList.Count > 1 ? normList[1] : "Нет данных";
          norm3Lbl.Text = normList.Count > 2 ? normList[2] : "Нет данных";

          VisibleTrue();
        }));
      }
      catch (Exception ex)
      {
        Messages.Error("Ошибка: " + ex.Message);
      }
      finally
      {
        GetNormsBtn.Enabled = true;
      }
    }
    private void VisibleTrue()
    {
      norm1Lbl.Visible = true;
      norm2Lbl.Visible = true;
      norm3Lbl.Visible = true;

      norm1TxtBox.Visible = true;
      norm2TxtBox.Visible = true;
      norm3TxtBox.Visible = true;
    }
    private void VisibleFalse()
    {
      norm1Lbl.Visible = false;
      norm2Lbl.Visible = false;
      norm3Lbl.Visible = false;

      norm1TxtBox.Visible = false;
      norm2TxtBox.Visible = false;
      norm3TxtBox.Visible = false;
    }

    private async void СalcGradeBtn_Click(object sender, EventArgs e)
    {
      try
      {
        if (double.TryParse(norm1TxtBox.Text.Trim(), out double norm1))
        {
          persons[currentIndex].Results.Add(norm1);
        }
        if (double.TryParse(norm2TxtBox.Text.Trim(), out double norm2))
        {
          persons[currentIndex].Results.Add(norm2);
        }
        if (double.TryParse(norm3TxtBox.Text.Trim(), out double norm3))
        {
          persons[currentIndex].Results.Add(norm3);
        }

        else
        {
          Messages.Error("Введите корректные числовые значения!");
        }

        await DataBaseHandler.GetScoresAsync(persons[currentIndex]);
        await DataBaseHandler.GetGradeTabAsync(persons[currentIndex]);

        persons[currentIndex].CalculateGrade();
        GradeLbl.Text = persons[currentIndex].Grade.ToString();
      }
      catch (Exception ex)
      {
        Messages.Error($"Ошибка при добавлении данных: {ex.Message}");
      }
    }
  }
}
