using Editor.GameProject;
using Editor.GameProject.ViewModel;
using System.ComponentModel;
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
            Closing += OnMainWindowClosing;
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnMainWindowLoaded;
            OpenProjectBrowsingDialog();
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