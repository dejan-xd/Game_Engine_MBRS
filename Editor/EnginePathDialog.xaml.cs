using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Editor
{
    /// <summary>
    /// Interaction logic for EnginePathDialog.xaml
    /// </summary>
    public partial class EnginePathDialog : Window
    {
        public string GameEnginePath { get; private set; }

        public EnginePathDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void OnOk_Button_Click(object sender, RoutedEventArgs e)
        {
            string path = pathTextBox.Text.Trim();
            messageTextBlock.Text = string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                messageTextBlock.Text = "Invalid path.";
            }
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                messageTextBlock.Text = "Invalid character(s) used in the Game Engine path.";
            }
            else if (!Directory.Exists(Path.Combine(path, @"Engine\EngineAPI\")))
            {
                messageTextBlock.Text = "Unable to find the engine at the specified location.";
            }

            if (string.IsNullOrEmpty(messageTextBlock.Text))
            {
                if (!Path.EndsInDirectorySeparator(path)) path += @"\";
                GameEnginePath = path;
                DialogResult = true;
                Close();
            }
        }
    }
}
