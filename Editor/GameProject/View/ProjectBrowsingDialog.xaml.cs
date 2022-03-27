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
