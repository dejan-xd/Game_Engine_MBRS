﻿using Editor.Utilities;
using System;
using System.Numerics;
using System.Runtime.Serialization;

namespace Editor.Components
{
    [DataContract]
    class Transform : Component
    {
        private Vector3 _position;
        [DataMember]
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        private Vector3 _rotation;
        [DataMember]
        public Vector3 Rotation
        {
            get => _rotation;
            set
            {
                if (_rotation != value)
                {
                    _rotation = value;
                    OnPropertyChanged(nameof(Rotation));
                }
            }
        }

        private Vector3 _scale;
        [DataMember]
        public Vector3 Scale
        {
            get => _scale;
            set
            {
                if (_scale != value)
                {
                    _scale = value;
                    OnPropertyChanged(nameof(Scale));
                }
            }
        }

        public override IMultiSelectComponent GetMultiSelectComponent(MultiSelectEntity multiSelectEntity) => new MultiSelectTransform(multiSelectEntity);

        public Transform(GameEntity owner) : base(owner) { }
    }

    sealed class MultiSelectTransform : MultiSelectComponent<Transform>
    {
        private float? _posX;
        public float? PosX
        {
            get => _posX;
            set
            {
                if (!_posX.IsTheSameAs(value))
                {
                    _posX = value;
                    OnPropertyChanged(nameof(PosX));
                }
            }
        }

        private float? _posY;
        public float? PosY
        {
            get => _posY;
            set
            {
                if (!_posY.IsTheSameAs(value))
                {
                    _posY = value;
                    OnPropertyChanged(nameof(PosY));
                }
            }
        }

        private float? _posZ;
        public float? PosZ
        {
            get => _posZ;
            set
            {
                if (!_posZ.IsTheSameAs(value))
                {
                    _posZ = value;
                    OnPropertyChanged(nameof(PosZ));
                }
            }
        }

        private float? _rotX;
        public float? RotX
        {
            get => _rotX;
            set
            {
                if (!_rotX.IsTheSameAs(value))
                {
                    _rotX = value;
                    OnPropertyChanged(nameof(RotX));
                }
            }
        }

        private float? _rotY;
        public float? RotY
        {
            get => _rotY;
            set
            {
                if (!_rotY.IsTheSameAs(value))
                {
                    _rotY = value;
                    OnPropertyChanged(nameof(RotY));
                }
            }
        }

        private float? _rotZ;
        public float? RotZ
        {
            get => _rotZ;
            set
            {
                if (!_rotZ.IsTheSameAs(value))
                {
                    _rotZ = value;
                    OnPropertyChanged(nameof(RotZ));
                }
            }
        }

        private float? _scaleX;
        public float? ScaleX
        {
            get => _scaleX;
            set
            {
                if (!_scaleX.IsTheSameAs(value))
                {
                    _scaleX = value;
                    OnPropertyChanged(nameof(ScaleX));
                }
            }
        }

        private float? _scaleY;
        public float? ScaleY
        {
            get => _scaleY;
            set
            {
                if (!_scaleY.IsTheSameAs(value))
                {
                    _scaleY = value;
                    OnPropertyChanged(nameof(ScaleY));
                }
            }
        }

        private float? _scaleZ;
        public float? ScaleZ
        {
            get => _scaleZ;
            set
            {
                if (!_scaleZ.IsTheSameAs(value))
                {
                    _scaleZ = value;
                    OnPropertyChanged(nameof(ScaleZ));
                }
            }
        }

        protected override bool UpdateComponents(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(PosX):
                case nameof(PosY):
                case nameof(PosZ):
                    SelectedComponents.ForEach(c => c.Position = new Vector3(_posX ?? c.Position.X, _posY ?? c.Position.Y, _posZ ?? c.Position.Z));
                    return true;

                case nameof(RotX):
                case nameof(RotY):
                case nameof(RotZ):
                    SelectedComponents.ForEach(c => c.Rotation = new Vector3(_rotX ?? c.Rotation.X, _rotY ?? c.Rotation.Y, _rotZ ?? c.Rotation.Z));
                    return true;

                case nameof(ScaleX):
                case nameof(ScaleY):
                case nameof(ScaleZ):
                    SelectedComponents.ForEach(c => c.Scale = new Vector3(_scaleX ?? c.Scale.X, _scaleY ?? c.Scale.Y, _scaleZ ?? c.Scale.Z));
                    return true;
            }
            return false;
        }

        protected override bool UpdateMultiSelectComponent()
        {
            PosX = MultiSelectEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(c => c.Position.X));
            PosY = MultiSelectEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(c => c.Position.Y));
            PosZ = MultiSelectEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(c => c.Position.Z));

            RotX = MultiSelectEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(c => c.Rotation.X));
            RotY = MultiSelectEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(c => c.Rotation.Y));
            RotZ = MultiSelectEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(c => c.Rotation.Z));

            ScaleX = MultiSelectEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(c => c.Scale.X));
            ScaleY = MultiSelectEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(c => c.Scale.Y));
            ScaleZ = MultiSelectEntity.GetMixedValue(SelectedComponents, new Func<Transform, float>(c => c.Scale.Z));

            return true;
        }

        public MultiSelectTransform(MultiSelectEntity multiSelectEntity) : base(multiSelectEntity)
        {
            Refresh();
        }
    }
}
