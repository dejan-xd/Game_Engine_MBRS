using Editor.Components;
using Editor.GameProject.ViewModel;
using Editor.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Editor.Editors
{
    public class NullableBoolToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b == true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool b && b == true;
        }
    }

    /// <summary>
    /// Interaction logic for GameEntityView.xaml
    /// </summary>
    public partial class GameEntityView : UserControl
    {
        private Action _undoAction;
        private string _propertyName;
        public static GameEntityView Instance { get; private set; }

        public GameEntityView()
        {
            InitializeComponent();
            DataContext = null;
            Instance = this;
            DataContextChanged += (x, y) =>
            {
                if (DataContext != null)
                {
                    (DataContext as MSEntity).PropertyChanged += (s, e) => _propertyName = e.PropertyName;
                }
            };
        }

        private Action GetRenameAction()
        {
            MSEntity mse = DataContext as MSEntity;
            var selection = mse.SelectedEntities.Select(entity => (entity, entity.Name)).ToList();

            return new Action(() =>
            {
                selection.ForEach(item => item.entity.Name = item.Name);
                (DataContext as MSEntity).Refresh();
            });
        }

        private Action GetIsEnabledAction()
        {
            MSEntity mse = DataContext as MSEntity;
            var selection = mse.SelectedEntities.Select(entity => (entity, entity.IsEnabled)).ToList();

            return new Action(() =>
            {
                selection.ForEach(item => item.entity.IsEnabled = item.IsEnabled);
                (DataContext as MSEntity).Refresh();
            });
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _propertyName = string.Empty;
            _undoAction = GetRenameAction();
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_propertyName == nameof(MSEntity.Name) && _undoAction != null)
            {
                Action redoAction = GetRenameAction();
                Project.UndoRedo.Add(new UndoRedoAction(_undoAction, redoAction, "Rename game entity"));
                _propertyName = null;
            }
            _undoAction = null;
        }

        private void CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Action undoAction = GetIsEnabledAction();
            MSEntity mse = DataContext as MSEntity;
            mse.IsEnabled = (sender as CheckBox).IsChecked == true;
            Action redoAction = GetIsEnabledAction();

            Project.UndoRedo.Add(new UndoRedoAction(undoAction, redoAction, mse.IsEnabled == true ? "Enable game entity" : "Disable game entity"));
        }

        private void OnAddComponent_Button_PreviewMouse_LBD(object sender, MouseButtonEventArgs e)
        {
            ContextMenu menu = FindResource("addComponentMenu") as ContextMenu;
            ToggleButton btn = sender as ToggleButton;
            btn.IsChecked = true;
            menu.Placement = PlacementMode.Bottom;
            menu.PlacementTarget = btn;
            menu.MinWidth = btn.ActualWidth;
            menu.IsOpen = true;
        }

        private void AddComponent(ComponentType componentType, object data)
        {
            var creationFunction = ComponentFactory.GetCreationFunction(componentType);
            var changedEntities = new List<(GameEntity entity, Component component)>();
            MSEntity msEntity = DataContext as MSEntity;

            foreach (GameEntity entity in msEntity.SelectedEntities)
            {
                Component component = creationFunction(entity, data);
                if (entity.AddComponent(component))
                {
                    changedEntities.Add((entity, component));
                }
            }

            if (changedEntities.Any())
            {
                msEntity.Refresh();
                Project.UndoRedo.Add(new UndoRedoAction(
                    () =>
                    {
                        changedEntities.ForEach(x => x.entity.RemoveComponent(x.component));
                        (DataContext as MSEntity).Refresh();
                    },
                    () =>
                    {
                        changedEntities.ForEach(x => x.entity.AddComponent(x.component));
                        (DataContext as MSEntity).Refresh();
                    },
                    $"Add {componentType.ToString().ToLower()} component"));
            }
        }

        private void OnAddScriptComponent(object sender, System.Windows.RoutedEventArgs e)
        {
            AddComponent(ComponentType.Script, (sender as MenuItem).Header.ToString());
        }
    }
}
