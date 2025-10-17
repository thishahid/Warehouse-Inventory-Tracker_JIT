using System.Windows;
using System.Windows.Threading;

namespace WarehouseInventoryTracker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Add this block to catch all unhandled exceptions
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Show the error in a message box
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Prevent the application from crashing
            e.Handled = true;
        }
    }
}