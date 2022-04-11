using Editor.GameProject.ViewModel;
using System.Linq;
using System.Windows;

namespace Editor.GameProject
{
    /// <summary>
    /// Interaction logic for ProjectBrowsingDialog.xaml
    /// </summary>
    public partial class ProjectBrowsingDialog : Window
    {
        public ProjectBrowsingDialog()
        {
            InitializeComponent();
            Loaded += OnProjectBrowserDialogLoaded;
        }

        private void OnProjectBrowserDialogLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnProjectBrowserDialogLoaded;
            if (!OpenProject.Projects.Any())
            {
                openProjBtn.IsEnabled = false;
                openProjectView.Visibility = Visibility.Hidden;
                OpenProjBtn_Click(createProjBtn, new RoutedEventArgs());
            }
        }

        private void OpenProjBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == openProjBtn)
            {
                if (createProjBtn.IsChecked == true)
                {
                    createProjBtn.IsChecked = false;
                    browserStackPanel.Margin = new Thickness(0);
                }
                openProjBtn.IsChecked = true;
            }
            else
            {
                if (openProjBtn.IsChecked == true)
                {
                    openProjBtn.IsChecked = false;
                    browserStackPanel.Margin = new Thickness(-800, 0, 0, 0);
                }
                createProjBtn.IsChecked = true;
            }
        }
    }
}
