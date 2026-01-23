using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SimpleSQLEditor.Services;
using SimpleSQLEditor.Services.EfCore;

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
            // WPF Einstiegspunkt & Root ViewModel
            services.AddSingleton<MainWindow>();
            services.AddSingleton<ViewModels.MainViewModel>();
            services.AddTransient<ViewModels.TableDataViewModel>();

            // Globaler Datenzugriffsmodus (SQL / EF)
            services.AddSingleton<IDataAccessModeService, DataAccessModeService>();

            // Status/Logging Kontext – Quelle pro Operation (SQL / EF / EF->SQL)
            services.AddSingleton<IOperationSourceService, OperationSourceService>();

            // UI-nahe Services (Fenster, Dialoge)
            services.AddSingleton<IWindowService, WindowService>();
            services.AddSingleton<IDialogService, DialogService>();

            // SQL Server – native Administration (reines SQL)
            services.AddSingleton<SqlServerAdminService>();
            services.AddSingleton<IColumnDefinitionService, ColumnDefinitionService>();

            // Entity Framework Core – Laufzeit-DbContext (Lernpfad)
            services.AddSingleton<IEfRuntimeContextFactory, EfRuntimeContextFactory>();
            services.AddSingleton<EfDatabaseAdminService>();
            //services.AddSingleton<IEfDatabaseQueryService, EfDatabaseQueryService>();

            // Use-Case Routing – Datenbankkatalog (SQL oder EF)
            services.AddSingleton<SqlDatabaseCatalogService>();
            services.AddSingleton<EfDatabaseCatalogService>();
            services.AddSingleton<IDatabaseCatalogService, DatabaseCatalogRouter>();

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
