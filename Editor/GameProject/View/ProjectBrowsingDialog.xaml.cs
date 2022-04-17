using Editor.GameProject.ViewModel;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Editor.GameProject
{
    /// <summary>
    /// Interaction logic for ProjectBrowsingDialog.xaml
    /// </summary>
    public partial class ProjectBrowsingDialog : Window
    {
        private readonly CubicEase _easing = new() { EasingMode = EasingMode.EaseInOut };

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

        private void AnimateToCreateProject()
        {
            DoubleAnimation highlightAnimation = new(200, 400, new Duration(TimeSpan.FromSeconds(0.2)));
            highlightAnimation.EasingFunction = _easing;
            highlightAnimation.Completed += (s, e) =>
            {
                ThicknessAnimation animation = new(new Thickness(0), new Thickness(-1600, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.5)));
                animation.EasingFunction = _easing;
                browserStackPanel.BeginAnimation(MarginProperty, animation);
            };
            highlightRect.BeginAnimation(Canvas.LeftProperty, highlightAnimation);
        }

        private void AnimateToOpenProject()
        {
            DoubleAnimation highlightAnimation = new(400, 200, new Duration(TimeSpan.FromSeconds(0.2)));
            highlightAnimation.EasingFunction = _easing;
            highlightAnimation.Completed += (s, e) =>
            {
                ThicknessAnimation animation = new(new Thickness(-1600, 0, 0, 0), new Thickness(0), new Duration(TimeSpan.FromSeconds(0.5)));
                animation.EasingFunction = _easing;
                browserStackPanel.BeginAnimation(MarginProperty, animation);
            };
            highlightRect.BeginAnimation(Canvas.LeftProperty, highlightAnimation);
        }

        private void OpenProjBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender == openProjBtn)
            {
                if (createProjBtn.IsChecked == true)
                {
                    createProjBtn.IsChecked = false;
                    AnimateToOpenProject();
                    openProjectView.IsEnabled = true;
                    newProjectView.IsEnabled = false;
                }
                openProjBtn.IsChecked = true;
            }
            else
            {
                if (openProjBtn.IsChecked == true)
                {
                    openProjBtn.IsChecked = false;
                    AnimateToCreateProject();
                    openProjectView.IsEnabled = false;
                    newProjectView.IsEnabled = true;
                }
                createProjBtn.IsChecked = true;
            }
        }

    }
}