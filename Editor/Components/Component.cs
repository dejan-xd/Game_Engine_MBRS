using Editor.Common;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Editor.Components
{
    interface IMultiSelectComponent { }

    [DataContract]
    abstract class Component : ViewModelBase
    {
        [DataMember]
        public GameEntity Owner { get; private set; }

        public Component(GameEntity owner)
        {
            Debug.Assert(owner != null);
            Owner = owner;
        }
    }

    abstract class MultiSelectComponent<T> : ViewModelBase, IMultiSelectComponent where T : Component { }
}
