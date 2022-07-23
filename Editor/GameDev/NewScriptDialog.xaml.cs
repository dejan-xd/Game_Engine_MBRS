using Editor.GameProject.ViewModel;
using Editor.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Editor.GameDev
{
    /// <summary>
    /// Interaction logic for NewScriptDialog.xaml
    /// </summary>
    public partial class NewScriptDialog : Window
    {
        private static readonly string _cppCode = @"#include ""{0}.h""

namespace {1} {{
	REGISTER_SCRIPT({0});
    void {0}::begin_play()
    {{
        // TODO: begin play
    }}
    void {0}::update(float x)
    {{
        // TODO: update
    }}
}}   // namespace {1}
";

        private static readonly string _hCode = @"#pragma once

namespace {1} {{
	class {0} : public primal::script::entity_script {{
	public:
		constexpr explicit {0}(primal::game_entity::entity entity) : primal::script::entity_script{{ entity }} {{}}
        void begin_play() override;
	    void update(float x) override;
    private:
	}};
}}	// namespace {1}
";

        private static readonly string _namespace = GetNamespaceFromProjectName();

        private static string GetNamespaceFromProjectName()
        {
            string projectName = Project.Current.Name.Trim();
            if (string.IsNullOrEmpty(projectName)) return string.Empty;

            return projectName;
        }

        private bool Validate()
        {
            bool isValid = false;
            string name = scriptName.Text.Trim();
            string path = scriptPath.Text.Trim();
            string errorMsg = string.Empty;
            Regex nameRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$");

            if (string.IsNullOrEmpty(name))
            {
                errorMsg = "Type in a script name.";
            }
            else if (!nameRegex.IsMatch(name))
            {
                errorMsg = "Invalid character(s) used in script name.";
            }
            else if (char.IsDigit(name[0]))
            {
                errorMsg = "Script name must start with a letter.";
            }
            else if (string.IsNullOrEmpty(path))
            {
                errorMsg = "Select a valid script folder.";
            }
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                errorMsg = "Invalid character(s) used in the script path.";
            }
            else if (!Path.GetFullPath(Path.Combine(Project.Current.Path, path)).Contains(Path.Combine(Project.Current.Path, @"GameCode\")))
            {
                errorMsg = "Script must be added to (a sub-folder) of GameCode.";
            }
            else if (File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.cpp"))) ||
                File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.h"))))
            {
                errorMsg = $"Script {name} already exists in this folder.";
            }
            else
            {
                isValid = true;
            }

            messageTextBlock.Foreground = !isValid ? FindResource("Editor.RedBrush") as Brush : FindResource("Editor.FontBrush") as Brush;
            messageTextBlock.Text = errorMsg;

            return isValid;
        }
        private void OnScriptName_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Validate()) return;
            string name = scriptName.Text.Trim();
            messageTextBlock.Text = $"{name}.h and {name}.cpp will be added to {Project.Current.Name}";
        }

        private void OnScriptPath_TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

        private async void OnCreate_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;
            IsEnabled = false;

            busyAnimation.Opacity = 0;
            busyAnimation.Visibility = Visibility.Visible;
            DoubleAnimation fadeIn = new(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));
            busyAnimation.BeginAnimation(OpacityProperty, fadeIn);

            try
            {
                string name = scriptName.Text.Trim();
                string path = Path.GetFullPath(Path.Combine(Project.Current.Path, scriptPath.Text.Trim()));
                string solution = Project.Current.Solution;
                string projectName = Project.Current.Name;
                await Task.Run(() => CreateScript(name, path, solution, projectName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to create script {scriptName.Text}");
            }
            finally
            {
                DoubleAnimation fadeOut = new(1, 0, new Duration(TimeSpan.FromMilliseconds(200)));
                fadeOut.Completed += (s, e) =>
                {
                    busyAnimation.Opacity = 0;
                    busyAnimation.Visibility = Visibility.Hidden;
                    Close();
                };
                busyAnimation.BeginAnimation(OpacityProperty, fadeOut);
            }
        }

        private static void CreateScript(string name, string path, string solution, string projectName)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            string cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));
            string h = Path.GetFullPath(Path.Combine(path, $"{name}.h"));

            using (StreamWriter sw = File.CreateText(cpp))
            {
                sw.Write(string.Format(_cppCode, name, _namespace));
            }
            using (StreamWriter sw = File.CreateText(h))
            {
                sw.Write(string.Format(_hCode, name, _namespace));
            }

            string[] files = new string[] { cpp, h };

            VisualStudio.AddFilesToSolution(solution, projectName, files);
        }

        public NewScriptDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            scriptPath.Text = @"GameCode\";
        }
    }
}