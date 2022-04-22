using Editor.GameProject;
using Editor.GameProject.ViewModel;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string EnginePath { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnMainWindowLoaded;
            Closing += OnMainWindowClosing;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnMainWindowLoaded;
            GetEnginePath();
            OpenProjectBrowsingDialog();
        }

        private void GetEnginePath()
        {
            //  enginePath is path to .sln of the project (e.g. ..\Desktop\MBRS\Project)
            string enginePath = Environment.GetEnvironmentVariable("GAME_ENGINE", EnvironmentVariableTarget.User);
            if (enginePath == null || !Directory.Exists(Path.Combine(enginePath, @"Engine\EngineAPI")))
            {
                var dialog = new EnginePathDialog();
                if (dialog.ShowDialog() == true)
                {
                    EnginePath = dialog.GameEnginePath;
                    Environment.SetEnvironmentVariable("GAME_ENGINE", EnginePath.ToUpper(), EnvironmentVariableTarget.User);
                }
                else
                {
                    Application.Current.Shutdown();
                }
            }
            else
            {
                EnginePath = enginePath;
            }
        }

        private void OnMainWindowClosing(object sender, CancelEventArgs e)
        {
            Closing -= OnMainWindowClosing;
            //Project.Current?.Unload();
            Project.Unload();
        }

        private void OpenProjectBrowsingDialog()
        {
            ProjectBrowsingDialog projectBroser = new();
            if (projectBroser.ShowDialog() == false || projectBroser.DataContext == null)
            {
                Application.Current.Shutdown();
            }
            else
            {
                //Project.Current?.Unload();
                Project.Unload();
                DataContext = projectBroser.DataContext;
            }
        }
    }
}