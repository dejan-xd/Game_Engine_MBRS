using System.Windows;
using System.Windows.Controls;

namespace Editor.Utilities
{
    /// <summary>
    /// Interaction logic for LoggerView.xaml
    /// </summary>
    public partial class LoggerView : UserControl
    {
        public LoggerView()
        {
            InitializeComponent();

            // TODO: remove this
            Loaded += (s, e) =>
            {
                Logger.Log(MessageType.Info, "Information message");
                Logger.Log(MessageType.Warning, "Warning message");
                Logger.Log(MessageType.Error, "Error message");
            };
        }

        private void ClearBtn_Click(object sender, RoutedEventArgs e)
        {
            Logger.Clear();
        }

        private void OnFilter_Click(object sender, RoutedEventArgs e)
        {
            int filter = 0x0;
            if (toggleInfo.IsChecked == true) filter |= (int)MessageType.Info;
            if (toggleWarn.IsChecked == true) filter |= (int)MessageType.Warning;
            if (toggleError.IsChecked == true) filter |= (int)MessageType.Error;
            Logger.SetMessageFilter(filter);
        }
    }
}
