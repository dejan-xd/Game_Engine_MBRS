using Editor.Common;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace Editor.Components
{
    interface IMultiSelectComponent { }

    [DataContract]
    abstract class Component : ViewModelBase
    {
        public abstract IMultiSelectComponent GetMultiSelectComponent(MultiSelectEntity multiSelectEntity);

        [DataMember]
        public GameEntity Owner { get; private set; }

        public Component(GameEntity owner)
        {
            Debug.Assert(owner != null);
            Owner = owner;
        }
    }

    abstract class MultiSelectComponent<T> : ViewModelBase, IMultiSelectComponent where T : Component
    {
        private bool _enableUpdates = true;
        public List<T> SelectedComponents { get; }

        protected abstract bool UpdateComponents(string propertyName);
        protected abstract bool UpdateMultiSelectComponent();

        public void Refresh()
        {
            _enableUpdates = false;
            UpdateMultiSelectComponent();
            _enableUpdates = true;
        }

        public MultiSelectComponent(MultiSelectEntity multiSelectEntity)
        {
            Debug.Assert(multiSelectEntity?.SelectedEntities?.Any() == true);
            SelectedComponents = multiSelectEntity.SelectedEntities.Select(entity => entity.GetComponent<T>()).ToList();
            PropertyChanged += (s, e) => { if (_enableUpdates) { UpdateComponents(e.PropertyName); } };
        }
    }
}
