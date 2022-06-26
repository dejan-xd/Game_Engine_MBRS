using Editor.Common;
using Editor.GameProject.ViewModel;
using Editor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

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
        private static readonly object _lock = new();
        private static readonly DelayEventTimer _refreshTimer = new(TimeSpan.FromMilliseconds(250));
        private static readonly FileSystemWatcher _contentWatcher = new()
        {
            IncludeSubdirectories = true,
            Filter = "",
            NotifyFilter = NotifyFilters.CreationTime |
                           NotifyFilters.DirectoryName |
                           NotifyFilters.FileName |
                           NotifyFilters.LastWrite
        };

        private static string _cacheFilePath = string.Empty;
        private static readonly Dictionary<string, ContentInfo> _contentInfoCache = new();
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
                        GetFolderContent();
                    }
                    OnPropertyChanged(nameof(SelectedFolder));
                }
            }
        }

        private async void GetFolderContent()
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
                lock (_lock)
                {
                    foreach (string file in Directory.GetFiles(path, $"*{Asset.AssetFileExtension}"))
                    {
                        FileInfo fileInfo = new(file);

                        if (!_contentInfoCache.ContainsKey(file) || _contentInfoCache[file].DateModified.IsOlder(fileInfo.LastWriteTime))
                        {
                            AssetInfo info = AssetRegistry.GetAssetInfo(file) ?? Asset.GetAssetInfo(file);
                            Debug.Assert(info != null);
                            _contentInfoCache[file] = new ContentInfo(file, info.Icon);
                        }

                        Debug.Assert(_contentInfoCache.ContainsKey(file));
                        folderContent.Add(_contentInfoCache[file]);
                    }
                }
            }
            catch (IOException ex) { Debug.WriteLine(ex.Message); }

            return folderContent;
        }

        private async void OnContentModified(object sender, FileSystemEventArgs e)
        {
            if (Path.GetDirectoryName(e.FullPath) != SelectedFolder) return;

            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _refreshTimer.Trigger();
            }));
        }

        private void Refresh(object sender, DelayEventTimerArgs e)
        {
            GetFolderContent();
        }

        private static void SaveInfoCache(string file)
        {
            lock (_lock)
            {
                using BinaryWriter writer = new(File.Open(file, FileMode.Create, FileAccess.Write));
                writer.Write(_contentInfoCache.Keys.Count);
                foreach (string key in _contentInfoCache.Keys)
                {
                    ContentInfo info = _contentInfoCache[key];

                    writer.Write(key);
                    writer.Write(info.DateModified.ToBinary());
                    writer.Write(info.Icon.Length);
                    writer.Write(info.Icon);
                }
            }
        }

        private static void LoadInfoCache(string file)
        {
            if (!File.Exists(file)) return;

            try
            {
                lock (_lock)
                {
                    using BinaryReader reader = new(File.Open(file, FileMode.Open, FileAccess.Read));
                    int numEntries = reader.ReadInt32();
                    _contentInfoCache.Clear();

                    for (int i = 0; i < numEntries; ++i)
                    {
                        string assetFile = reader.ReadString();
                        DateTime date = DateTime.FromBinary(reader.ReadInt64());
                        int iconSize = reader.ReadInt32();
                        byte[] icon = reader.ReadBytes(iconSize);

                        // cache only the files that still exists
                        if (File.Exists(assetFile))
                        {
                            _contentInfoCache[assetFile] = new ContentInfo(assetFile, icon, null, date);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Warning, "Failed to load Content Browser cache file.");
                _contentInfoCache.Clear();
            }
        }

        public void Dispose()
        {
            ((IDisposable)_contentWatcher).Dispose();
            if (!string.IsNullOrEmpty(_cacheFilePath))
            {
                SaveInfoCache(_cacheFilePath);
                _cacheFilePath = string.Empty;
            }
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

            if (string.IsNullOrEmpty(_cacheFilePath))
            {
                _cacheFilePath = $@"{project.Path}.MBRS\ContentInfoCache.bin";
                LoadInfoCache(_cacheFilePath);
            }

            _contentWatcher.Path = contentFolder;
            _contentWatcher.Changed += OnContentModified;
            _contentWatcher.Created += OnContentModified;
            _contentWatcher.Deleted += OnContentModified;
            _contentWatcher.Renamed += OnContentModified;
            _contentWatcher.EnableRaisingEvents = true;

            _refreshTimer.Triggered += Refresh;
        }
    }
}
