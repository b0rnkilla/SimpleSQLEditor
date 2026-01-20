using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleSQLEditor.Services;

namespace SimpleSQLEditor
{
    public partial class App : Application
    {
        #region Fields

        private readonly IHost _host;

        #endregion

        #region Constructor

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();
        }

        #endregion

        #region Methods & Events

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ViewModels.MainViewModel>();
            services.AddSingleton<SqlServerAdminService>();
            services.AddTransient<Views.StatusLogWindow>();
            services.AddSingleton<IWindowService, WindowService>();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();

            base.OnExit(e);
        }

        #endregion
    }
}
