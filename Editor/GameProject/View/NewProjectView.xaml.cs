using Editor.GameProject.ViewModel;
using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Editor.GameProject
{
    /// <summary>
    /// Interaction logic for NewProjectView.xaml
    /// </summary>
    public partial class NewProjectView : UserControl
    {
        public NewProjectView()
        {
            InitializeComponent();
        }

        private void CreateBtn_Click(object sender, RoutedEventArgs e)
        {
            NewProject newProject = DataContext as NewProject;
            string projectPath = newProject.CreateProject(templateListBox.SelectedItem as ProjectTemplate);

            bool dialogueResult = false;
            Window window = Window.GetWindow(this);

            if (!string.IsNullOrEmpty(projectPath))
            {
                dialogueResult = true;
                Project project = OpenProject.Open(new ProjectData() { ProjectName = newProject.ProjectName, ProjectPath = projectPath });
                window.DataContext = project;
            }
            window.DialogResult = dialogueResult;
            window.Close();
        }

        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new();
            if (sfd.ShowDialog() == true)
            {
                NewProject newProject = DataContext as NewProject;
                newProject.ProjectPath = Path.GetDirectoryName(sfd.FileName) + @"";
                newProject.ProjectName = Path.GetFileNameWithoutExtension(sfd.FileName);
            }
        }
    }
}