using PFLAC_WPF.Services;
using PFLAC_WPF.ViewModels;
using PFLAC_WPF.Views;
using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;

namespace PFLAC_WPF
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
        private IServiceProvider _serviceProvider;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new SimpleContainer();
            ConfigureServices(services);

            _serviceProvider = services;

            var mainWindow = new MainWindow
            {
                DataContext = (MainViewModel)_serviceProvider.GetService(typeof(MainViewModel))
            };
            mainWindow.Show();
        }

        private void ConfigureServices(SimpleContainer services)
        {
            var httpClient = new HttpClient();

            services.AddSingleton<IMessageService, MessageService>();
            services.AddSingleton<IAgeGroupResolver, AgeGroupResolver>();
            services.AddSingleton<IGradeCalculator, GradeCalculator>();
            services.AddSingleton<IExcelPersonImporter, ExcelPersonImporter>();
            services.AddSingleton<IPflacApiService>(_ => new PflacApiService(httpClient));
            services.AddSingleton<PersonEvaluationService, PersonEvaluationService>();

            services.AddSingleton<MainViewModel, MainViewModel>();
        }
    }

    public class SimpleContainer : IServiceProvider
    {
        private readonly Dictionary<Type, Func<object>> _registrations = new();

        public void AddSingleton<TService, TImplementation>()
            where TImplementation : TService
        {
            var lazy = new Lazy<object>(() =>
            {
                var ctor = typeof(TImplementation).GetConstructors().Single();
                var parameters = ctor.GetParameters()
                    .Select(p => GetService(p.ParameterType))
                    .ToArray();

                return Activator.CreateInstance(typeof(TImplementation), parameters);
            });

            _registrations[typeof(TService)] = () => lazy.Value;
            _registrations[typeof(TImplementation)] = () => lazy.Value;
        }

        public void AddSingleton<TService>(Func<IServiceProvider, TService> factory)
        {
            var lazy = new Lazy<object>(() => factory(this));
            _registrations[typeof(TService)] = () => lazy.Value;
        }

        public object GetService(Type serviceType)
        {
            if (_registrations.TryGetValue(serviceType, out var factory))
                return factory();

            throw new InvalidOperationException($"Service {serviceType.Name} is not registered");
        }
    }

}
