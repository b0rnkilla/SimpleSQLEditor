using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleSQLEditor.Services.DataAccess;
using SimpleSQLEditor.Services.EfCore;
using SimpleSQLEditor.Services.Sql;
using SimpleSQLEditor.Services.State;
using SimpleSQLEditor.Services.Ui;
using System.Windows;

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
            // WPF Einstiegspunkt & Root ViewModels
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ViewModels.MainViewModel>();
            services.AddTransient<ViewModels.TableDataViewModel>();

            // State (Modus, Logging-Kontext)
            services.AddSingleton<IDataAccessModeService, DataAccessModeService>();
            services.AddSingleton<IOperationSourceService, OperationSourceService>();

            // DataAccess Fassade & Routing
            services.AddSingleton<SqlDataAccessService>();
            services.AddSingleton<EfDataAccessService>();
            services.AddSingleton<IDataAccessService, DataAccessRouterService>();

            // UI Services (Fenster, Dialoge, UI-Events)
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IColumnDefinitionService, ColumnDefinitionService>();

            // SQL Implementierung
            services.AddSingleton<SqlServerAdminService>();

            // EF Core Implementierung
            services.AddSingleton<EfDatabaseAdminService>();

            // Zusätzliche Windows (per Service geöffnet)
            services.AddTransient<Views.StatusLogWindow>();
            services.AddTransient<Views.TableDataWindow>();
            services.AddTransient<Views.SqlDataTypesWindow>();
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
