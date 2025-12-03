using PFLAC_WPF.Services;
using PFLAC_WPF.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PFLAC_WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var messageService = new MessageService();
            var ageResolver = new AgeGroupResolver();
            var gradeCalculator = new GradeCalculator();
            var excelImporter = new ExcelPersonImporter();
            var apiService = new PflacApiService();
            var personService = new PersonEvaluationService(
                excelImporter,
                apiService,
                ageResolver,
                gradeCalculator
            );

            DataContext = new MainViewModel(personService, messageService);
        }
    }
}
