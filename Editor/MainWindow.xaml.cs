using Editor.Content;
using Editor.GameProject;
using Editor.GameProject.ViewModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
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

        private static void GetEnginePath()
        {
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
            if (DataContext == null)
            {
                e.Cancel = true;
                Application.Current.MainWindow.Hide();
                OpenProjectBrowsingDialog();
                if (DataContext != null)
                {
                    Application.Current.MainWindow.Show();
                }
            }
            else
            {
                Closing -= OnMainWindowClosing;
                Project.Current?.Unload();
                DataContext = null;
            }
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
                Project.Current?.Unload();
                Project project = projectBroser.DataContext as Project;
                Debug.Assert(project != null);
                AssetRegistry.Reset(project.ContentPath);
                DataContext = project;
            }
        }
    }
}