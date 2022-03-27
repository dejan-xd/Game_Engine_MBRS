using Editor.GameProject.View_Model;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Editor.GameProject
{
    /// <summary>
    /// Interaction logic for CreateProjectView.xaml
    /// </summary>
    public partial class CreateProjectView : UserControl
    {
        public CreateProjectView()
        {
            InitializeComponent();
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            NewProject newProject = DataContext as NewProject;
            string projectPath = newProject.CreateProject(templateListBox.SelectedItem as ProjectTemplate);

            bool dialogueResult = false;
            Window window = Window.GetWindow(this);

            if (!string.IsNullOrEmpty(projectPath)) dialogueResult = true;
            window.DialogResult = dialogueResult;
            window.Close();
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new();
            if (sfd.ShowDialog() == true)
            {
                NewProject newProject = DataContext as NewProject;
                newProject.ProjectPath = Path.GetDirectoryName(sfd.FileName) + @"\";
                newProject.ProjectName = Path.GetFileNameWithoutExtension(sfd.FileName);
            }
        }
    }
}
