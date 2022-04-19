using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Dictionaries
{
    public partial class ControlTemplates : ResourceDictionary
    {
        private void OnTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            var expression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (expression == null) return;
            if (e.Key == Key.Enter)
            {
                if (textBox.Tag is ICommand command && command.CanExecute(textBox.Text))
                {
                    command.Execute(textBox.Text);
                }
                else
                {
                    expression.UpdateSource();
                }
                Keyboard.ClearFocus();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                expression.UpdateTarget();
                Keyboard.ClearFocus();
            }
        }

        private void OnTextBoxRename_KeyDown(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            var expression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (expression == null) return;
            if (e.Key == Key.Enter)
            {
                if (textBox.Tag is ICommand command && command.CanExecute(textBox.Text))
                {
                    command.Execute(textBox.Text);
                }
                else
                {
                    expression.UpdateSource();
                }
                textBox.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                expression.UpdateTarget();
                textBox.Visibility = Visibility.Collapsed;
            }
        }

        private void OnTextBoxRename_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            var expression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (expression != null)
            {
                expression.UpdateTarget();
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                textBox.Visibility = Visibility.Collapsed;
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Window window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.Close();
        }

        private void RestoreBtn_Click(object sender, RoutedEventArgs e)
        {
            Window window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = (window.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            Window window = (Window)((FrameworkElement)sender).TemplatedParent;
            window.WindowState = WindowState.Minimized;
        }
    }
}