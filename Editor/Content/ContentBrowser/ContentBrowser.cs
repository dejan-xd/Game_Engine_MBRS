using Editor.Common;
using Editor.GameProject.ViewModel;
using Editor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Editor.Content
{
    sealed class ContentInfo
    {
        public static int IconWidth => 90;
        public byte[] Icon { get; }
        public byte[] IconSmall { get; }
        public string FullPath { get; }
        public string FileName => Path.GetFileNameWithoutExtension(FullPath);
        public bool IsDirectory { get; }
        public DateTime DateModified { get; }
        public long? Size { get; }

        public ContentInfo(string fullPath, byte[] icon = null, byte[] smallIcon = null, DateTime? lastModified = null)
        {
            Debug.Assert(File.Exists(fullPath) || Directory.Exists(fullPath));
            FileInfo info = new(fullPath);
            IsDirectory = ContentHelper.IsDirectory(fullPath);
            DateModified = lastModified ?? info.LastWriteTime;
            Size = IsDirectory ? null : info.Length;
            Icon = icon;
            IconSmall = smallIcon ?? icon;
            FullPath = fullPath;
        }
    }

    class ContentBrowser : ViewModelBase, IDisposable
    {
        private readonly DelayEventTimer _refreshTimer = new(TimeSpan.FromMilliseconds(250));

        public string ContentFolder { get; }

        private readonly ObservableCollection<ContentInfo> _folderContent = new();
        public ReadOnlyObservableCollection<ContentInfo> FolderContent { get; }

        private string _selectedFolder;
        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (_selectedFolder != value)
                {
                    _selectedFolder = value;
                    if (!string.IsNullOrEmpty(_selectedFolder))
                    {
                        _ = GetFolderContent();
                    }
                    OnPropertyChanged(nameof(SelectedFolder));
                }
            }
        }

        private void OnContentModified(object sender, ContentModifiedEventArgs e)
        {
            if (Path.GetDirectoryName(e.FullPath) != SelectedFolder) return;
            _refreshTimer.Trigger();
        }

        private void Refresh(object sender, DelayEventTimerArgs e)
        {
            _ = GetFolderContent();
        }

        private async Task GetFolderContent()
        {
            List<ContentInfo> folderContent = new();
            await Task.Run(() =>
            {
                folderContent = GetFolderContent(SelectedFolder);
            });

            _folderContent.Clear();
            folderContent.ForEach(x => _folderContent.Add(x));
        }

        private static List<ContentInfo> GetFolderContent(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            List<ContentInfo> folderContent = new();

            try
            {
                // get sub-folder
                foreach (string dir in Directory.GetDirectories(path))
                {
                    folderContent.Add(new ContentInfo(dir));
                }

                // get files
                foreach (string file in Directory.GetFiles(path, $"*{Asset.AssetFileExtension}"))
                {
                    FileInfo fileInfo = new(file);
                    folderContent.Add(ContentInfoCache.Add(file));
                }
            }
            catch (IOException ex) { Debug.WriteLine(ex.Message); }

            return folderContent;
        }

        public void Dispose()
        {
            ContentWatcher.ContentModified -= OnContentModified;
            ContentInfoCache.Save();
        }

        public ContentBrowser(Project project)
        {
            Debug.Assert(project != null);
            string contentFolder = project.ContentPath;
            Debug.Assert(!string.IsNullOrEmpty(contentFolder.Trim()));
            contentFolder = Path.TrimEndingDirectorySeparator(contentFolder);
            ContentFolder = contentFolder;
            SelectedFolder = contentFolder;
            FolderContent = new ReadOnlyObservableCollection<ContentInfo>(_folderContent);

            ContentWatcher.ContentModified += OnContentModified;
            _refreshTimer.Triggered += Refresh;
        }
    }
}
