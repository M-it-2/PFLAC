using Microsoft.Win32;
using PFLAC_WPF.Infrastructure;
using PFLAC_WPF.Models;
using PFLAC_WPF.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;

namespace PFLAC_WPF.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly PersonEvaluationService _evaluationService;
        private readonly IMessageService _messages;

        public ObservableCollection<MilitaryPerson> Persons { get; } = new();

        private int _currentIndex = -1;
        private string _exercise1Name;
        public string Exercise1Name
        {
            get => _exercise1Name;
            set { _exercise1Name = value; OnPropertyChanged(); }
        }

        private string _exercise2Name;
        public string Exercise2Name
        {
            get => _exercise2Name;
            set { _exercise2Name = value; OnPropertyChanged(); }
        }

        private string _exercise3Name;
        public string Exercise3Name
        {
            get => _exercise3Name;
            set { _exercise3Name = value; OnPropertyChanged(); }
        }
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (_currentIndex == value)
                    return;

                _currentIndex = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentPerson));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanGoPrevious));

                OnPropertyChanged(nameof(FullName));
                OnPropertyChanged(nameof(AgeText));
                OnPropertyChanged(nameof(IsMale));
                OnPropertyChanged(nameof(IsFemale));
                OnPropertyChanged(nameof(IsOfficer));
                OnPropertyChanged(nameof(IsContract));
                OnPropertyChanged(nameof(IsCategory1));
                OnPropertyChanged(nameof(IsCategory2));
                OnPropertyChanged(nameof(IsCategory3));

                SyncPersonResultsToUi();
            }
        }

        public MilitaryPerson? CurrentPerson =>
            Persons.Count > 0 &&
            CurrentIndex >= 0 &&
            CurrentIndex < Persons.Count
                ? Persons[CurrentIndex]
                : null;

        public bool CanGoNext => CurrentIndex < Persons.Count - 1;
        public bool CanGoPrevious => CurrentIndex > 0;


        public string FullName
        {
            get => CurrentPerson?.Name ?? string.Empty;
            set
            {
                if (CurrentPerson == null) return;
                if (CurrentPerson.Name == value) return;
                CurrentPerson.Name = value;
                OnPropertyChanged();
            }
        }

        public string AgeText
        {
            get => CurrentPerson?.Age.ToString() ?? string.Empty;
            set
            {
                if (CurrentPerson == null) return;
                if (int.TryParse(value, out var age))
                {
                    if (CurrentPerson.Age == age) return;
                    CurrentPerson.Age = age;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsMale
        {
            get => string.Equals(CurrentPerson?.Gender, "man", StringComparison.OrdinalIgnoreCase);
            set
            {
                if (CurrentPerson == null) return;
                if (value)
                {
                    if (CurrentPerson.Gender == "man") return;
                    CurrentPerson.Gender = "man";
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsFemale));
                }
            }
        }

        public bool IsFemale
        {
            get => string.Equals(CurrentPerson?.Gender, "woman", StringComparison.OrdinalIgnoreCase);
            set
            {
                if (CurrentPerson == null) return;
                if (value)
                {
                    if (CurrentPerson.Gender == "woman") return;
                    CurrentPerson.Gender = "woman";
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsMale));
                }
            }
        }

        public bool IsOfficer
        {
            get => string.Equals(CurrentPerson?.Status, "Officer", StringComparison.OrdinalIgnoreCase);
            set
            {
                if (CurrentPerson == null) return;
                if (value)
                {
                    if (CurrentPerson.Status == "Officer") return;
                    CurrentPerson.Status = "Officer";
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsContract));
                }
            }
        }

        public bool IsContract
        {
            get => string.Equals(CurrentPerson?.Status, "Contract", StringComparison.OrdinalIgnoreCase);
            set
            {
                if (CurrentPerson == null) return;
                if (value)
                {
                    if (CurrentPerson.Status == "Contract") return;
                    CurrentPerson.Status = "Contract";
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsOfficer));
                }
            }
        }

        public bool IsCategory1
        {
            get => CurrentPerson?.Category == 1;
            set
            {
                if (CurrentPerson == null) return;
                if (value && CurrentPerson.Category != 1)
                {
                    CurrentPerson.Category = 1;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCategory2));
                    OnPropertyChanged(nameof(IsCategory3));
                }
            }
        }

        public bool IsCategory2
        {
            get => CurrentPerson?.Category == 2;
            set
            {
                if (CurrentPerson == null) return;
                if (value && CurrentPerson.Category != 2)
                {
                    CurrentPerson.Category = 2;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCategory1));
                    OnPropertyChanged(nameof(IsCategory3));
                }
            }
        }

        public bool IsCategory3
        {
            get => CurrentPerson?.Category == 3;
            set
            {
                if (CurrentPerson == null) return;
                if (value && CurrentPerson.Category != 3)
                {
                    CurrentPerson.Category = 3;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCategory1));
                    OnPropertyChanged(nameof(IsCategory2));
                }
            }
        }


        private string _norm1Text = string.Empty;
        public string Norm1Text
        {
            get => _norm1Text;
            set
            {
                if (_norm1Text == value) return;
                _norm1Text = value;
                OnPropertyChanged();
            }
        }

        private string _norm2Text = string.Empty;
        public string Norm2Text
        {
            get => _norm2Text;
            set
            {
                if (_norm2Text == value) return;
                _norm2Text = value;
                OnPropertyChanged();
            }
        }

        private string _norm3Text = string.Empty;
        public string Norm3Text
        {
            get => _norm3Text;
            set
            {
                if (_norm3Text == value) return;
                _norm3Text = value;
                OnPropertyChanged();
            }
        }

        private string _gradeText = string.Empty;
        public string GradeText
        {
            get => _gradeText;
            set
            {
                if (_gradeText == value) return;
                _gradeText = value;
                OnPropertyChanged();
            }
        }


        public ICommand LoadFromExcelCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PreviousCommand { get; }
        public ICommand LoadNormsCommand { get; }
        public ICommand CalculateGradeCommand { get; }

        public MainViewModel(PersonEvaluationService evaluationService, IMessageService messages)
        {
            _evaluationService = evaluationService;
            _messages = messages;

            LoadFromExcelCommand = new RelayCommand(async _ => await LoadFromExcelAsync());
            NextCommand = new RelayCommand(_ => GoNext(), _ => CanGoNext);
            PreviousCommand = new RelayCommand(_ => GoPrevious(), _ => CanGoPrevious);
            LoadNormsCommand = new RelayCommand(async _ => await LoadNormsAsync(), _ => CurrentPerson != null);
            CalculateGradeCommand = new RelayCommand(async _ => await CalculateGradeAsync(), _ => CurrentPerson != null);
        }

        private async Task LoadFromExcelAsync()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls"
            };

            if (dialog.ShowDialog() != true)
                return;

            try
            {
                Persons.Clear();
                var persons = _evaluationService.LoadPersonsFromExcel(dialog.FileName);
                foreach (var p in persons)
                    Persons.Add(p);

                if (Persons.Count > 0)
                {
                    CurrentIndex = 0;
                    ClearUiForNewPerson();
                }
            }
            catch (Exception ex)
            {
                _messages.ShowError($"Помилка при завантаженні файлу: {ex.Message}");
            }
        }

        private void GoNext()
        {
            if (CanGoNext)
                CurrentIndex++;
        }

        private void GoPrevious()
        {
            if (CanGoPrevious)
                CurrentIndex--;
        }

        private async Task LoadNormsAsync()
        {
            if (CurrentPerson == null) return;

            try
            {
                var records = await _evaluationService.LoadNormsAsync(CurrentPerson);

                Exercise1Name = records.Count > 0
                    ? $"№{records[0].ExerciseNumber}: {records[0].ExerciseName}"
                    : "Немає даних";

                Exercise2Name = records.Count > 1
                    ? $"№{records[1].ExerciseNumber}: {records[1].ExerciseName}"
                    : "Немає даних";

                Exercise3Name = records.Count > 2
                    ? $"№{records[2].ExerciseNumber}: {records[2].ExerciseName}"
                    : "Немає даних";

                _messages.ShowInfo("Нормативи успішно завантажено.");
            }
            catch (Exception ex)
            {
                _messages.ShowError($"Помилка при завантаженні нормативів: {ex.Message}");
            }
        }


        private async Task CalculateGradeAsync()
        {
            if (CurrentPerson == null)
                return;

            if (!TryParseDouble(Norm1Text, out var n1) ||
                !TryParseDouble(Norm2Text, out var n2) ||
                !TryParseDouble(Norm3Text, out var n3))
            {
                _messages.ShowError("Введи коректні числові значення для трьох результатів.");
                return;
            }

            try
            {
                var grade = await _evaluationService.CalculateGradeAsync(CurrentPerson, n1, n2, n3);
                GradeText = grade.ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                _messages.ShowError($"Помилка при розрахунку: {ex.Message}");
            }
        }

        private static bool TryParseDouble(string text, out double value)
        {
            return double.TryParse(
                text.Replace(',', '.'),
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out value);
        }

        private void ClearUiForNewPerson()
        {
            Norm1Text = string.Empty;
            Norm2Text = string.Empty;
            Norm3Text = string.Empty;
            GradeText = string.Empty;
        }

        private void SyncPersonResultsToUi()
        {
            var person = CurrentPerson;
            if (person == null)
            {
                ClearUiForNewPerson();
                return;
            }

            if (person.Results.Count > 0)
                Norm1Text = person.Results[0].ToString(CultureInfo.InvariantCulture);
            else
                Norm1Text = string.Empty;

            if (person.Results.Count > 1)
                Norm2Text = person.Results[1].ToString(CultureInfo.InvariantCulture);
            else
                Norm2Text = string.Empty;

            if (person.Results.Count > 2)
                Norm3Text = person.Results[2].ToString(CultureInfo.InvariantCulture);
            else
                Norm3Text = string.Empty;

            GradeText = person.Grade > 0
                ? person.Grade.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}