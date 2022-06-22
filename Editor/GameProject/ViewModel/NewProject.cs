using Editor.Common;
using Editor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Editor.GameProject.ViewModel
{
    [DataContract]
    public class ProjectTemplate
    {
        [DataMember]
        public string ProjectType { get; set; }
        [DataMember]
        public string ProjectFile { get; set; }
        [DataMember]
        public List<string> Folders { get; set; }

        public byte[] Icon { get; set; }
        public byte[] Screenshot { get; set; }
        public string IconPath { get; set; }
        public string ScreenshotPath { get; set; }
        public string ProjectPath { get; set; }
        public string TemplatePath { get; set; }
    }

    public class NewProject : ViewModelBase
    {
        // TODO: get the path from the installation location
        private readonly string _templatePath = @"..\..\Editor\Templates";

        private string _projectName = "NewProject";
        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    ValidateProjectPath();
                    OnPropertyChanged(nameof(ProjectName));
                }
            }
        }

        private string _projectPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\GameEngine\";
        public string ProjectPath
        {
            get => _projectPath;
            set
            {
                if (_projectPath != value)
                {
                    _projectPath = value;
                    ValidateProjectPath();
                    OnPropertyChanged(nameof(ProjectPath));
                }
            }
        }

        private bool _isValid;
        public bool IsValid
        {
            get => _isValid;
            set
            {
                if (_isValid != value)
                {
                    _isValid = value;
                    OnPropertyChanged(nameof(IsValid));
                }
            }
        }

        private string _errorMsg;
        public string ErrorMsg
        {
            get => _errorMsg;
            set
            {
                if (_errorMsg != value)
                {
                    _errorMsg = value;
                    OnPropertyChanged(nameof(ErrorMsg));
                }
            }
        }

        private readonly ObservableCollection<ProjectTemplate> _projectTemplates = new();
        public ReadOnlyObservableCollection<ProjectTemplate> ProjectTemplates { get; }

        private bool ValidateProjectPath()
        {
            string path = ProjectPath;
            if (!Path.EndsInDirectorySeparator(path)) path += @"\";
            path += $@"{ProjectName}\";
            Regex nameRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$");

            IsValid = false;
            if (string.IsNullOrWhiteSpace(ProjectName.Trim()))
            {
                ErrorMsg = "Type in a project name.";
            }
            else if (!nameRegex.IsMatch(ProjectName))
            {
                ErrorMsg = "Invalid character(s) used in project name.";
            }
            else if (string.IsNullOrWhiteSpace(ProjectPath.Trim()))
            {
                ErrorMsg = "Select a valid project folder.";
            }
            else if (ProjectPath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                ErrorMsg = "Invalid character(s) used in project path.";
            }
            else if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
            {
                ErrorMsg = "Selected project folder alderady exists and is not empty.";
            }
            else
            {
                ErrorMsg = string.Empty;
                IsValid = true;
            }

            return IsValid;
        }

        public string CreateProject(ProjectTemplate template)
        {
            ValidateProjectPath();
            if (!IsValid)
            {
                return string.Empty;
            }

            if (!Path.EndsInDirectorySeparator(ProjectPath)) ProjectPath += @"\";
            string path = $@"{ProjectPath}{ProjectName}\";

            try
            {
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                foreach (string folder in template.Folders)
                {
                    Directory.CreateDirectory(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path), folder)));
                }
                DirectoryInfo dirInfo = new(path + @".MBRS\");
                dirInfo.Attributes |= FileAttributes.Hidden;
                File.Copy(template.IconPath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Icon.ico")));
                File.Copy(template.ScreenshotPath, Path.GetFullPath(Path.Combine(dirInfo.FullName, "Screenshot.png")));

                string projectXml = File.ReadAllText(template.ProjectPath);
                projectXml = string.Format(projectXml, ProjectName, path);
                string projectPath = Path.GetFullPath(Path.Combine(path, $"{ProjectName}{Project.Extension}"));
                File.WriteAllText(projectPath, projectXml);

                CreateMSVCSolution(template, path);

                return path;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to create {ProjectName}");
                throw;
            }
        }

        private void CreateMSVCSolution(ProjectTemplate template, string projectPath)
        {
            Debug.Assert(File.Exists(Path.Combine(template.TemplatePath, "MSVCSolution")));
            Debug.Assert(File.Exists(Path.Combine(template.TemplatePath, "MSVCProject")));

            string engineAPIPath = @"$(GAME_ENGINE)Engine\EngineAPI\";
            Debug.Assert(Directory.Exists(engineAPIPath));

            string _0 = ProjectName;
            string _1 = "{" + Guid.NewGuid().ToString().ToUpper() + "}";
            string _2_solution = "{" + Guid.NewGuid().ToString().ToUpper() + "}";
            string _2_project = engineAPIPath;
            string _3 = "$(GAME_ENGINE)";

            string solution = File.ReadAllText(Path.Combine(template.TemplatePath, "MSVCSolution"));
            solution = string.Format(solution, _0, _1, _2_solution);
            File.WriteAllText(Path.GetFullPath(Path.Combine(projectPath, $"{_0}.sln")), solution);

            string project = File.ReadAllText(Path.Combine(template.TemplatePath, "MSVCProject"));
            project = string.Format(project, _0, _1, _2_project, _3);
            File.WriteAllText(Path.GetFullPath(Path.Combine(projectPath, $@"GameCode\{_0}.vcxproj")), project);
        }

        public NewProject()
        {
            ProjectTemplates = new ReadOnlyObservableCollection<ProjectTemplate>(_projectTemplates);
            try
            {
                string[] templateFiles = Directory.GetFiles(_templatePath, "template.xml", SearchOption.AllDirectories);
                Debug.Assert(templateFiles.Any());
                foreach (string file in templateFiles)
                {
                    ProjectTemplate template = Serializer.FromFile<ProjectTemplate>(file);
                    template.TemplatePath = Path.GetDirectoryName(file);
                    template.IconPath = Path.GetFullPath(Path.Combine(template.TemplatePath, "Icon.png"));
                    template.Icon = File.ReadAllBytes(template.IconPath);
                    template.ScreenshotPath = Path.GetFullPath(Path.Combine(template.TemplatePath, "Screenshot.png"));
                    template.Screenshot = File.ReadAllBytes(template.ScreenshotPath);
                    template.ProjectPath = Path.GetFullPath(Path.Combine(template.TemplatePath, template.ProjectFile));

                    _projectTemplates.Add(template);
                }
                ValidateProjectPath();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to read project templates");
                throw;
            }
        }
    }
}