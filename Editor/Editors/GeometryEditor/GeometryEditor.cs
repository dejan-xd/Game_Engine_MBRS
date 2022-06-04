﻿using Editor.Common;
using Editor.Content;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Editor.Editors
{

    // NOTE: the purpose of this class is to enable viewing 3D geometry in WPF while we don't have a graphics renderer
    //       in the game engine. When we have a renderer, this class and the WPF viewer will become obsolite.
    class MeshRendererVertexData : ViewModelBase
    {
        private Brush _specular = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff111111"));
        public Brush Specular
        {
            get => _specular;
            set
            {
                if (_specular != value)
                {
                    _specular = value;
                    OnPropertyChanged(nameof(Specular));
                }
            }
        }

        private Brush _diffuse = Brushes.White;
        public Brush Diffuse
        {
            get => _diffuse;
            set
            {
                if (_diffuse != value)
                {
                    _diffuse = value;
                    OnPropertyChanged(nameof(Diffuse));
                }
            }
        }

        public Point3DCollection Positions { get; } = new Point3DCollection();
        public Vector3DCollection Normals { get; } = new Vector3DCollection();
        public PointCollection UVs { get; } = new PointCollection();
        public Int32Collection Indices { get; } = new Int32Collection();
    }

    // NOTE: the purpose of this class is to enable viewing 3D geometry in WPF while we don't have a graphics renderer
    //       in the game engine. When we have a renderer, this class and the WPF viewer will become obsolite.
    class MeshRenderer : ViewModelBase
    {
        public ObservableCollection<MeshRendererVertexData> Meshes { get; } = new ObservableCollection<MeshRendererVertexData>();

        private Vector3D _cameraDirection = new(0, 0, -10);
        public Vector3D CameraDirection
        {
            get => _cameraDirection;
            set
            {
                if (_cameraDirection != value)
                {
                    _cameraDirection = value;
                    OnPropertyChanged(nameof(CameraDirection));
                }
            }
        }

        private Point3D _cameraPosition = new(0, 0, 10);
        public Point3D CameraPosition
        {
            get => _cameraPosition;
            set
            {
                if (_cameraPosition != value)
                {
                    _cameraPosition = value;
                    OnPropertyChanged(nameof(OffsetCameraPosition));
                    OnPropertyChanged(nameof(CameraPosition));
                }
            }
        }

        private Point3D _cameraTarget = new(0, 0, 0);
        public Point3D CameraTarget
        {
            get => _cameraTarget;
            set
            {
                if (_cameraTarget != value)
                {
                    _cameraTarget = value;
                    OnPropertyChanged(nameof(OffsetCameraPosition));
                    OnPropertyChanged(nameof(CameraTarget));
                }
            }
        }

        public Point3D OffsetCameraPosition => new(CameraPosition.X + CameraTarget.X, CameraPosition.Y + CameraTarget.Y, CameraPosition.Z + CameraTarget.Z);

        private Color _keyLight = (Color)ColorConverter.ConvertFromString("#ffaeaeae");
        public Color KeyLight
        {
            get => _keyLight;
            set
            {
                if (_keyLight != value)
                {
                    _keyLight = value;
                    OnPropertyChanged(nameof(KeyLight));
                }
            }
        }

        private Color _skyLight = (Color)ColorConverter.ConvertFromString("#ff111b30");
        public Color SkyLight
        {
            get => _skyLight;
            set
            {
                if (_skyLight != value)
                {
                    _skyLight = value;
                    OnPropertyChanged(nameof(SkyLight));
                }
            }
        }

        private Color _groundLight = (Color)ColorConverter.ConvertFromString("#ff3f2f1e");
        public Color GroundLight
        {
            get => _groundLight;
            set
            {
                if (_groundLight != value)
                {
                    _groundLight = value;
                    OnPropertyChanged(nameof(GroundLight));
                }
            }
        }

        private Color _ambientLight = (Color)ColorConverter.ConvertFromString("#ff3b3b3b");
        public Color AmbientLight
        {
            get => _ambientLight;
            set
            {
                if (_ambientLight != value)
                {
                    _ambientLight = value;
                    OnPropertyChanged(nameof(AmbientLight));
                }
            }
        }

        public MeshRenderer(MeshLOD lod, MeshRenderer old)
        {
            Debug.Assert(lod?.Meshes.Any() == true);

            // Calculate vertex size minus the position and normal vectors (by skipping not relevent data)
            int offset = lod.Meshes[0].VertexSize - 3 * sizeof(float) - sizeof(int) - 2 * sizeof(short);

            // In order to set up camera position and target propertly, we need to figure out how big this object is
            // that we're rendering. Hence, we need to know its bounding box.
            double minX, minY, minZ; minX = minY = minZ = double.MaxValue;
            double maxX, maxY, maxZ; maxX = maxY = maxZ = double.MinValue;
            Vector3D avgNormal = new();

            // This is to unpack the packed normals
            float intervals = 2.0f / ((1 << 16) - 1);

            foreach (Mesh mesh in lod.Meshes)
            {
                MeshRendererVertexData vertexData = new();
                // Unpack all vertices
                using (BinaryReader reader = new(new MemoryStream(mesh.Vertices)))
                {
                    for (int i = 0; i < mesh.VertexCount; ++i)
                    {
                        // Read positions
                        float posX = reader.ReadSingle();
                        float posY = reader.ReadSingle();
                        float posZ = reader.ReadSingle();
                        uint signs = (reader.ReadUInt32() >> 24) & 0x000000ff;
                        vertexData.Positions.Add(new Point3D(posX, posY, posZ));

                        // Adjust the bounding box
                        minX = Math.Min(minX, posX); maxX = Math.Max(maxX, posX);
                        minY = Math.Min(minY, posY); maxY = Math.Max(maxY, posY);
                        minZ = Math.Min(minZ, posZ); maxZ = Math.Max(maxZ, posZ);

                        // Read normals
                        float nrmX = reader.ReadUInt16() * intervals - 1.0f;
                        float nrmY = reader.ReadUInt16() * intervals - 1.0f;
                        double nrmZ = Math.Sqrt(Math.Clamp(1f - (nrmX * nrmX + nrmY * nrmY), 0f, 1f)) * ((signs & 0x2) - 1f);
                        Vector3D normal = new(nrmX, nrmY, nrmZ);
                        normal.Normalize();
                        vertexData.Normals.Add(normal);
                        avgNormal += normal;

                        // Read UVs (skip tangent and joint data)
                        reader.BaseStream.Position += (offset - sizeof(float) * 2);
                        float u = reader.ReadSingle();
                        float v = reader.ReadSingle();
                        vertexData.UVs.Add(new Point(u, v));
                    }
                }

                using (BinaryReader reader = new(new MemoryStream(mesh.Indices)))
                {
                    if (mesh.IndexSize == sizeof(short))
                        for (int i = 0; i < mesh.IndexCount; ++i) vertexData.Indices.Add(reader.ReadUInt16());
                    else
                        for (int i = 0; i < mesh.IndexCount; ++i) vertexData.Indices.Add(reader.ReadInt32());
                }

                vertexData.Positions.Freeze();
                vertexData.Normals.Freeze();
                vertexData.UVs.Freeze();
                vertexData.Indices.Freeze();
                Meshes.Add(vertexData);
            }

            // set camera target and position
            if (old != null)
            {
                CameraTarget = old.CameraTarget;
                CameraPosition = old.CameraPosition;
            }
            else
            {
                // compute bounding box dimensions
                double width = maxX - minX;
                double height = maxY - minY;
                double depth = maxZ - minZ;
                double radius = new Vector3D(height, width, depth).Length * 1.2;

                if (avgNormal.Length > 0.8)
                {
                    avgNormal.Normalize();
                    avgNormal *= radius;
                    CameraPosition = new Point3D(avgNormal.X, avgNormal.Y, avgNormal.Z);
                }
                else
                {
                    CameraPosition = new Point3D(width, height * 0.5, radius);
                }

                CameraTarget = new Point3D(minX + width * 0.5, minY + height * 0.5, minZ + depth * 0.5);
            }
        }
    }

    class GeometryEditor : ViewModelBase, IAssetEditor
    {
        public Asset Asset => Geometry;

        private Content.Geometry _geometry;
        public Content.Geometry Geometry
        {
            get => _geometry;
            set
            {
                if (_geometry != value)
                {
                    _geometry = value;
                    OnPropertyChanged(nameof(Geometry));
                }
            }
        }

        private MeshRenderer _meshRenderer;
        public MeshRenderer MeshRenderer
        {
            get => _meshRenderer;
            set
            {
                if (_meshRenderer != value)
                {
                    _meshRenderer = value;
                    OnPropertyChanged(nameof(MeshRenderer));
                }
            }
        }

        public void SetAsset(Asset asset)
        {
            Debug.Assert(asset is Content.Geometry);
            if (asset is Content.Geometry geometry)
            {
                Geometry = geometry;
                MeshRenderer = new MeshRenderer(Geometry.GetLODGroup().LODs[0], MeshRenderer);
            }
        }
    }
}