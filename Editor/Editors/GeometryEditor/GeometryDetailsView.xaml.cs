using System.Windows.Controls;

namespace Editor.Editors
{
    /// <summary>
    /// Interaction logic for GeometryDetailsView.xaml
    /// </summary>
    public partial class GeometryDetailsView : UserControl
    {
        public GeometryDetailsView()
        {
            InitializeComponent();
        }

        private void OnHighlight_CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GeometryEditor vm = DataContext as GeometryEditor;
            foreach (MeshRendererVertexData m in vm.MeshRenderer.Meshes)
            {
                m.IsHighlighted = false;
            }

            CheckBox checkBox = sender as CheckBox;
            (checkBox.DataContext as MeshRendererVertexData).IsHighlighted = checkBox.IsChecked == true;
        }

        private void OnIsolate_CheckBox_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            GeometryEditor vm = DataContext as GeometryEditor;
            foreach (MeshRendererVertexData m in vm.MeshRenderer.Meshes)
            {
                m.IsIsolated = false;
            }

            CheckBox checkBox = sender as CheckBox;
            MeshRendererVertexData mesh = checkBox.DataContext as MeshRendererVertexData;
            mesh.IsIsolated = checkBox.IsChecked == true;

            if (Tag is GeometryView geometryView)
            {
                geometryView.SetGeometry(mesh.IsIsolated ? vm.MeshRenderer.Meshes.IndexOf(mesh) : -1);
            }
        }
    }
}
