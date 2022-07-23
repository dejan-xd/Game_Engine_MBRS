using Editor.GameProject.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Editor.GameProject
{
    /// <summary>
    /// Interaction logic for OpenProjectView.xaml
    /// </summary>
    public partial class OpenProjectView : UserControl
    {
        public OpenProjectView()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                ListBoxItem item = projectsListBox.ItemContainerGenerator.ContainerFromIndex(projectsListBox.SelectedIndex) as ListBoxItem;
                item?.Focus();
            };
        }

        private void ListBoxItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OpenBtn_Click(sender, e);
        }

        private void OpenBtn_Click(object sender, RoutedEventArgs e)
        {
            Project project = OpenProject.Open(projectsListBox.SelectedItem as ProjectData);
            bool dialogueResult = false;
            Window window = Window.GetWindow(this);

            if (project != null)
            {
                dialogueResult = true;
                window.DataContext = project;
            }
            window.DialogResult = dialogueResult;
            window.Close();
        }
    }
}