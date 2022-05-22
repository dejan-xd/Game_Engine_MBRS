﻿using Editor.Common;
using Editor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;

namespace Editor.GameProject.ViewModel
{
    [DataContract(Name = "Game", Namespace = "http://schemas.datacontract.org/2004/07/Editor.GameProject")]
    public class Project : ViewModelBase
    {
        public static string Extension { get; } = ".mbrs";

        [DataMember]
        public string Name { get; private set; } = "New Project";

        [DataMember]
        public string Path { get; private set; }

        public string FullPath => $"{Path}{Name}{Extension}";

        [DataMember(Name = "Scenes")]
        private readonly ObservableCollection<Scene> _scenes = new();
        public ReadOnlyObservableCollection<Scene> Scenes { get; private set; }

        private Scene _activeScene;
        public Scene ActiveScene
        {
            get => _activeScene;
            set
            {
                if (_activeScene != value)
                {
                    _activeScene = value;
                    OnPropertyChanged(nameof(ActiveScene));
                }
            }
        }

        public static Project Current => Application.Current.MainWindow.DataContext as Project;

        public static UndoRedo UndoRedo { get; } = new UndoRedo();

        public ICommand Undo { get; private set; }
        public ICommand Redo { get; private set; }
        public ICommand AddScene { get; private set; }
        public ICommand RemoveScene { get; private set; }

        // METHODS

        private void AddSceneInternal(string sceneName)
        {
            Debug.Assert(!string.IsNullOrEmpty(sceneName.Trim()));
            _scenes.Add(new Scene(this, sceneName));
        }

        public void RemoveSceneInternal(Scene scene)
        {
            Debug.Assert(_scenes.Contains(scene));
            _scenes.Remove(scene);
        }

        public static Project Load(string file)
        {
            Debug.Assert(File.Exists(file));
            return Serializer.FromFile<Project>(file);
        }

        public void Unload() { }

        public static void Save(Project project)
        {
            Serializer.ToFile(project, project.FullPath);
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_scenes != null)
            {
                Scenes = new ReadOnlyObservableCollection<Scene>(_scenes);
                OnPropertyChanged(nameof(Scenes));
            }
            ActiveScene = Scenes.FirstOrDefault(x => x.IsActive);

            AddScene = new RelayCommand<object>(x =>
            {
                AddSceneInternal($"New Scene {_scenes.Count}");
                Scene scene = _scenes.Last();
                int index = _scenes.Count - 1;

                UndoRedo.Add(new UndoRedoAction(
                    () => RemoveSceneInternal(scene),
                    () => _scenes.Insert(index, scene),
                    $"Add {scene.Name}"));
            });

            RemoveScene = new RelayCommand<Scene>(x =>
            {
                var index = _scenes.IndexOf(x);
                RemoveSceneInternal(x);

                UndoRedo.Add(new UndoRedoAction(
                    () => _scenes.Insert(index, x),
                    () => RemoveSceneInternal(x),
                    $"Remove {x.Name}"));
            }, x => !x.IsActive);

            Undo = new RelayCommand<object>(x => UndoRedo.Undo());
            Redo = new RelayCommand<object>(x => UndoRedo.Redo());
        }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            OnDeserialized(new StreamingContext());
        }

        public Project() { }
    }
}
