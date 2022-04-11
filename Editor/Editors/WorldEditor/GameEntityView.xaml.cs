using Editor.Components;
using Editor.GameProject.ViewModel;
using Editor.Utilities;
using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace Editor.Editors
{
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
                    (DataContext as MultiSelectEntity).PropertyChanged += (s, e) => _propertyName = e.PropertyName;
                }
            };
        }

        private Action GetRenameAction()
        {
            MultiSelectEntity mse = DataContext as MultiSelectEntity;
            var selection = mse.SelectedEntities.Select(entity => (entity, entity.Name)).ToList();

            return new Action(() =>
            {
                selection.ForEach(item => item.entity.Name = item.Name);
                (DataContext as MultiSelectEntity).Refresh();
            });
        }

        private Action GetIsEnabledAction()
        {
            MultiSelectEntity mse = DataContext as MultiSelectEntity;
            var selection = mse.SelectedEntities.Select(entity => (entity, entity.IsEnabled)).ToList();

            return new Action(() =>
            {
                selection.ForEach(item => item.entity.IsEnabled = item.IsEnabled);
                (DataContext as MultiSelectEntity).Refresh();
            });
        }

        private void TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _undoAction = GetRenameAction();
        }

        private void TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_propertyName == nameof(MultiSelectEntity.Name) && _undoAction != null)
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
            MultiSelectEntity mse = DataContext as MultiSelectEntity;
            mse.IsEnabled = (sender as CheckBox).IsChecked == true;
            Action redoAction = GetIsEnabledAction();

            Project.UndoRedo.Add(new UndoRedoAction(undoAction, redoAction, mse.IsEnabled == true ? "Enable game entity" : "Disable game entity"));
        }
    }
}
