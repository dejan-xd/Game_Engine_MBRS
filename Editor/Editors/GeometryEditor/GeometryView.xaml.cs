using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for GeometryView.xaml
    /// </summary>
    public partial class GeometryView : UserControl
    {
        private Point _clickedPosition;
        private bool _capturedLeft;
        private bool _capturedRight;

        public void SetGeometry(int index = -1)
        {
            if (DataContext is not MeshRenderer vm) return;

            if (vm.Meshes.Any() && viewport.Children.Count == 2)
            {
                viewport.Children.RemoveAt(1);
            }

            int meshIndex = 0;
            Model3DGroup modelGroup = new();
            foreach (MeshRendererVertexData mesh in vm.Meshes)
            {
                // Skip over meshes that we don't want to display
                if (index != -1 && meshIndex != index)
                {
                    ++meshIndex;
                    continue;
                }

                MeshGeometry3D mesh3D = new()
                {
                    Positions = mesh.Positions,
                    Normals = mesh.Normals,
                    TriangleIndices = mesh.Indices,
                    TextureCoordinates = mesh.UVs
                };

                DiffuseMaterial diffuse = new(mesh.Diffuse);
                SpecularMaterial specular = new(mesh.Specular, 50);
                MaterialGroup matGroup = new();
                matGroup.Children.Add(diffuse);
                matGroup.Children.Add(specular);

                GeometryModel3D model = new(mesh3D, matGroup);
                modelGroup.Children.Add(model);

                Binding binding = new(nameof(mesh.Diffuse)) { Source = mesh };
                BindingOperations.SetBinding(diffuse, DiffuseMaterial.BrushProperty, binding);

                if (meshIndex == index) break;
            }

            var visual = new ModelVisual3D() { Content = modelGroup };
            viewport.Children.Add(visual);
        }

        public GeometryView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => SetGeometry();
        }

        private void OnGrid_Mouse_LBD(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _clickedPosition = e.GetPosition(this);
            _capturedLeft = true;
            Mouse.Capture(sender as UIElement);
        }

        private void OnGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (_capturedLeft && !_capturedRight) return;

            Point pos = e.GetPosition(this);
            Vector d = pos - _clickedPosition;

            if (_capturedLeft && !_capturedRight)
            {
                //MoveCamera(d.X, d.Y, 0);
            }
            else if (!_capturedLeft && _capturedRight)
            {
                MeshRenderer vm = DataContext as MeshRenderer;
                Point3D cp = vm.CameraPosition;
                double yOffset = d.Y * 0.001 * Math.Sqrt(cp.X * cp.X + cp.Z * cp.Z);
                vm.CameraTarget = new Point3D(vm.CameraTarget.X, vm.CameraTarget.Y + yOffset, vm.CameraTarget.Z);
            }
        }

        private void OnGrid_Mouse_LBU(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _capturedLeft = false;
            if (!_capturedRight) Mouse.Capture(null);
        }

        private void OnGrid_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {

        }

        private void OnGrid_Mouse_RBD(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }

        private void OnGrid_Mouse_RBU(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}