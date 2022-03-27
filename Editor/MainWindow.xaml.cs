using Editor.GameProject;
using System;
using System.Windows;

namespace Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnMainWindowLoaded;

        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnMainWindowLoaded;
            OpenProjectBrowsingDialog();
        }

        private static void OpenProjectBrowsingDialog()
        {
            ProjectBrowsingDialog projBroser = new();
            if (projBroser.ShowDialog() == false)
            {
                Application.Current.Shutdown();
            }
            else
            {
                Console.WriteLine("TODO");
            }
        }
    }
}
