﻿using Editor.Common;
using Editor.Editors;
using Editor.GameProject.ViewModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Editor.Content
{
    class DataSizeToStringConverter : IValueConverter
    {
        static readonly string[] _sizeSuffixes =
                   { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (value <= 0 || decimalPlaces < 0) return string.Empty;

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, _sizeSuffixes[mag]);
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is long size) ? SizeSuffix(size, 0) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class PlainView : ViewBase
    {
        public static readonly DependencyProperty ItemContainerStyleProperty = ItemsControl.ItemContainerStyleProperty.AddOwner(typeof(PlainView));

        public Style ItemContainerStyle
        {
            get => (Style)GetValue(ItemContainerStyleProperty);
            set => SetValue(ItemContainerStyleProperty, value);
        }

        public static readonly DependencyProperty ItemTemplateProperty =
            ItemsControl.ItemTemplateProperty.AddOwner(typeof(PlainView));

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        public static readonly DependencyProperty ItemWidthProperty =
            WrapPanel.ItemWidthProperty.AddOwner(typeof(PlainView));

        public double ItemWidth
        {
            get => (double)GetValue(ItemWidthProperty);
            set => SetValue(ItemWidthProperty, value);
        }

        public static readonly DependencyProperty ItemHeightProperty =
            WrapPanel.ItemHeightProperty.AddOwner(typeof(PlainView));

        public double ItemHeight
        {
            get => (double)GetValue(ItemHeightProperty);
            set => SetValue(ItemHeightProperty, value);
        }

        protected override object DefaultStyleKey => new ComponentResourceKey(GetType(), "PlainViewResourceId");
    }

    /// <summary>
    /// Interaction logic for ContentBrowserView.xaml
    /// </summary>
    public partial class ContentBrowserView : UserControl, IDisposable
    {
        private string _sortedProperty = nameof(ContentInfo.FileName);
        private ListSortDirection _sortDirection;

        // Dependency Properties

        public SelectionMode SelectionMode
        {
            get => (SelectionMode)GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }

        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register(nameof(SelectionMode), typeof(SelectionMode), typeof(ContentBrowserView), new PropertyMetadata(SelectionMode.Extended));

        public FileAccess FileAccess
        {
            get => (FileAccess)GetValue(FileAccessProperty);
            set => SetValue(FileAccessProperty, value);
        }

        public static readonly DependencyProperty FileAccessProperty =
            DependencyProperty.Register(nameof(FileAccess), typeof(FileAccess), typeof(ContentBrowserView), new PropertyMetadata(FileAccess.ReadWrite));

        internal ContentInfo SelectedItem
        {
            get => (ContentInfo)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(ContentInfo), typeof(ContentBrowserView), new PropertyMetadata(null));

        public ContentBrowserView()
        {
            DataContext = null;
            InitializeComponent();
            Loaded += OnContentBrowserLoaded;
            AllowDrop = true;
        }

        private void OnContentBrowserLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnContentBrowserLoaded;
            if (Application.Current?.MainWindow != null)
            {
                Application.Current.MainWindow.DataContextChanged += OnProjectChanged;
            }

            OnProjectChanged(null, new DependencyPropertyChangedEventArgs(DataContextProperty, null, Project.Current));
            folderListView.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(Thumb_DragDelta), true);
            folderListView.Items.SortDescriptions.Add(new SortDescription(_sortedProperty, _sortDirection));
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (e.OriginalSource is Thumb thumb && thumb.TemplatedParent is GridViewColumnHeader header)
            {
                if (header.Column.ActualWidth < 50)
                {
                    header.Column.Width = 50;
                }
                else if (header.Column.ActualWidth > 250)
                {
                    header.Column.Width = 250;
                }
            }
        }

        private void OnProjectChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (DataContext as ContentBrowser)?.Dispose();
            DataContext = null;
            if (e.NewValue is Project project)
            {
                Debug.Assert(e.NewValue == Project.Current);
                ContentBrowser contentBrowser = new(project);
                contentBrowser.PropertyChanged += OnSelectedFolderChanged;
                DataContext = contentBrowser;
            }
        }

        private void OnSelectedFolderChanged(object sender, PropertyChangedEventArgs e)
        {
            ContentBrowser vm = sender as ContentBrowser;
            if (e.PropertyName == nameof(vm.SelectedFolder) && !string.IsNullOrEmpty(vm.SelectedFolder))
            {
                GeneratePathStackButtons();
            }
        }

        private void GeneratePathStackButtons()
        {
            ContentBrowser vm = DataContext as ContentBrowser;
            string path = Directory.GetParent(Path.TrimEndingDirectorySeparator(vm.SelectedFolder)).FullName;
            string contentPath = Path.TrimEndingDirectorySeparator(vm.ContentFolder);

            pathStack.Children.RemoveRange(1, pathStack.Children.Count - 1);
            if (vm.SelectedFolder == vm.ContentFolder) return;

            string[] paths = new string[3];
            string[] labels = new string[3];

            int i;
            for (i = 0; i < 3; ++i)
            {
                paths[i] = path;
                labels[i] = path[(path.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
                if (path == contentPath) break;
                path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
            }

            if (i == 3) i = 2;
            for (; i >= 0; --i)
            {
                Button btn = new()
                {
                    DataContext = paths[i],
                    Content = new TextBlock() { Text = labels[i], TextTrimming = TextTrimming.CharacterEllipsis }
                };
                pathStack.Children.Add(btn);
                if (i > 0) pathStack.Children.Add(new System.Windows.Shapes.Path());
            }
        }

        private void OnPathStack_Button_Click(object sender, RoutedEventArgs e)
        {
            ContentBrowser vm = DataContext as ContentBrowser;
            vm.SelectedFolder = (sender as Button).DataContext as string;
        }

        private void OnGridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = sender as GridViewColumnHeader;
            string sortBy = column.Tag.ToString();

            folderListView.Items.SortDescriptions.Clear();
            ListSortDirection newDir = ListSortDirection.Ascending;
            if (_sortedProperty == sortBy && _sortDirection == newDir)
            {
                newDir = ListSortDirection.Descending;
            }

            _sortDirection = newDir;
            _sortedProperty = sortBy;

            folderListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void OnContent_Item_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContentInfo info = (sender as FrameworkElement).DataContext as ContentInfo;
            ExecuteSelection(info);
        }

        private void OnContent_Item_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ContentInfo info = (sender as FrameworkElement).DataContext as ContentInfo;
                ExecuteSelection(info);
            }
        }

        private void ExecuteSelection(ContentInfo info)
        {
            if (info == null) return;

            if (info.IsDirectory)
            {
                ContentBrowser vm = DataContext as ContentBrowser;
                vm.SelectedFolder = info.FullPath;
            }
            else if (FileAccess.HasFlag(FileAccess.Read))
            {
                AssetInfo assetInfo = Asset.GetAssetInfo(info.FullPath);
                if (assetInfo != null)
                {
                    OpenAssetEditor(assetInfo);
                }
            }
        }

        private IAssetEditor OpenAssetEditor(AssetInfo info)
        {
            IAssetEditor editor = null;
            try
            {
                switch (info.Type)
                {
                    case AssetType.Animation: break;
                    case AssetType.Audio: break;
                    case AssetType.Material: break;
                    case AssetType.Mesh:
                        editor = OpenEditorPanel<GeometryEditorView>(info, info.Guid, "GeometryEditor");
                        break;
                    case AssetType.Skeleton: break;
                    case AssetType.Texture: break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return editor;
        }

        private IAssetEditor OpenEditorPanel<T>(AssetInfo info, Guid guid, string title) where T : FrameworkElement, new()
        {
            // first look for a window that's already open and is displaying the same asset
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Content is FrameworkElement content && content.DataContext is IAssetEditor editor && editor.Asset.Guid == info.Guid)
                {
                    window.Activate();
                    return editor;
                }
            }

            // if not already open in an asset editor, create a new window and load the asset
            T newEditor = new();
            Debug.Assert(newEditor.DataContext is IAssetEditor);
            (newEditor.DataContext as IAssetEditor).SetAsset(info);

            Window win = new()
            {
                Content = newEditor,
                Title = title,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Style = Application.Current.FindResource("PrimalWindowStyle") as Style
            };

            win.Show();
            return newEditor.DataContext as IAssetEditor;
        }

        private void OnFolderContent_ListView_Drop(object sender, DragEventArgs e)
        {
            ContentBrowser vm = DataContext as ContentBrowser;
            if (vm.SelectedFolder != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length > 0 && Directory.Exists(vm.SelectedFolder))
                {
                    _ = ContentHelper.ImportFilesAsync(files, vm.SelectedFolder);
                    e.Handled = true;
                }
            }
        }

        private void OnFolderContent_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ContentInfo item = folderListView.SelectedItem as ContentInfo;
            SelectedItem = item?.IsDirectory == true ? null : item;
        }

        public void Dispose()
        {
            if (Application.Current?.MainWindow != null)
            {
                Application.Current.MainWindow.DataContextChanged -= OnProjectChanged;
            }

            (DataContext as ContentBrowser)?.Dispose();
            DataContext = null;
        }
    }
}