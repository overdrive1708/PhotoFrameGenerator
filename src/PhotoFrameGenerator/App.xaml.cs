using PhotoFrameGenerator.Models;
using System.Windows;

namespace PhotoFrameGenerator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Startup処理
        /// </summary>
        /// <param name="sender">イベントソース</param>
        /// <param name="e">イベントデータ</param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 未処理の例外が発生したときの処理を登録する｡
            DispatcherUnhandledException += ExceptionHandler.OnDispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += ExceptionHandler.OnUnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += ExceptionHandler.OnUnhandledException;

            // MainWindowを表示する｡
            MainWindow mainWindow = new();
            mainWindow.Show();
        }
    }

}
