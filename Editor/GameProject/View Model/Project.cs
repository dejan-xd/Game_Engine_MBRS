using Editor.GameProject.View_Model;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;

namespace Editor.GameProject.View
{
    [DataContract(Name = "Game", Namespace = "http://schemas.datacontract.org/2004/07/Editor.GameProject")]
    public class Project : ViewModelBase
    {
        public static string Extension { get; } = ".mbrs";

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string Path { get; private set; }

        public string FullPath => $"{Path}{Name}{Extension}";

        [DataMember(Name = "Scenes")]
        private ObservableCollection<Scene> _scenes = new();
        public ReadOnlyObservableCollection<Scene> Scenes { get; }

        public Project(string name, string path)
        {
            Name = name;
            Path = path;

            _scenes.Add(new Scene(this, "Default Scene"));
        }
    }
}
