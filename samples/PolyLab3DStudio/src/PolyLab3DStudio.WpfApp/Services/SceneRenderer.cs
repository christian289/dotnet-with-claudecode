using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;
using PolyLab3DStudio.Core;

namespace PolyLab3DStudio.Services;

/// <summary>
/// Keeps a Viewport3D in sync with the MainViewModel scene graph.
/// One ModelVisual3D per SceneObjectViewModel; transforms and materials
/// are rebuilt in place when the view model changes.
/// </summary>
public sealed class SceneRenderer
{
    private static readonly Color SelectionGlow = Color.FromRgb(0x3A, 0x1E, 0x14);

    private readonly Dictionary<SceneObjectViewModel, ModelVisual3D> _visuals = [];
    private readonly Dictionary<PrimitiveKind, MeshGeometry3D> _meshCache = [];

    private Viewport3D? _viewport;
    private MainViewModel? _viewModel;

    public void Attach(Viewport3D viewport, MainViewModel viewModel)
    {
        _viewport = viewport;
        _viewModel = viewModel;

        viewModel.SceneObjects.CollectionChanged += OnSceneObjectsChanged;
        viewModel.PropertyChanged += OnViewModelPropertyChanged;

        foreach (SceneObjectViewModel item in viewModel.SceneObjects)
        {
            AddVisual(item);
        }
    }

    public SceneObjectViewModel? FindObject(ModelVisual3D visual)
    {
        foreach ((SceneObjectViewModel vm, ModelVisual3D v) in _visuals)
        {
            if (ReferenceEquals(v, visual))
            {
                return vm;
            }
        }

        return null;
    }

    private void OnSceneObjectsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach ((SceneObjectViewModel vm, ModelVisual3D visual) in _visuals)
            {
                vm.PropertyChanged -= OnObjectPropertyChanged;
                _viewport?.Children.Remove(visual);
            }

            _visuals.Clear();
            return;
        }

        if (e.OldItems is not null)
        {
            foreach (SceneObjectViewModel item in e.OldItems.OfType<SceneObjectViewModel>())
            {
                RemoveVisual(item);
            }
        }

        if (e.NewItems is not null)
        {
            foreach (SceneObjectViewModel item in e.NewItems.OfType<SceneObjectViewModel>())
            {
                AddVisual(item);
            }
        }
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(MainViewModel.SelectedObject))
        {
            return;
        }

        // Re-tint every material so exactly the selected object glows.
        foreach (SceneObjectViewModel item in _visuals.Keys)
        {
            UpdateMaterial(item);
        }
    }

    private void OnObjectPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not SceneObjectViewModel item)
        {
            return;
        }

        if (e.PropertyName == nameof(SceneObjectViewModel.ColorHex))
        {
            UpdateMaterial(item);
            return;
        }

        UpdateTransform(item);
    }

    private void AddVisual(SceneObjectViewModel item)
    {
        if (_viewport is null || _visuals.ContainsKey(item))
        {
            return;
        }

        GeometryModel3D model = new()
        {
            Geometry = GetMesh(item.Kind),
        };

        ModelVisual3D visual = new() { Content = model };
        _visuals[item] = visual;
        _viewport.Children.Add(visual);

        item.PropertyChanged += OnObjectPropertyChanged;
        UpdateMaterial(item);
        UpdateTransform(item);
    }

    private void RemoveVisual(SceneObjectViewModel item)
    {
        if (!_visuals.Remove(item, out ModelVisual3D? visual))
        {
            return;
        }

        item.PropertyChanged -= OnObjectPropertyChanged;
        _viewport?.Children.Remove(visual);
    }

    private void UpdateTransform(SceneObjectViewModel item)
    {
        if (!_visuals.TryGetValue(item, out ModelVisual3D? visual))
        {
            return;
        }

        Transform3DGroup group = new();
        group.Children.Add(new ScaleTransform3D(item.Scale, item.Scale, item.Scale));
        group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), item.RotationX)));
        group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), item.RotationY)));
        group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), item.RotationZ)));
        group.Children.Add(new TranslateTransform3D(item.PositionX, item.PositionY, item.PositionZ));
        group.Freeze();

        visual.Transform = group;
    }

    private void UpdateMaterial(SceneObjectViewModel item)
    {
        if (!_visuals.TryGetValue(item, out ModelVisual3D? visual) ||
            visual.Content is not GeometryModel3D model)
        {
            return;
        }

        Color color = ParseColor(item.ColorHex);
        bool isSelected = ReferenceEquals(_viewModel?.SelectedObject, item);

        MaterialGroup material = new();
        material.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));
        if (isSelected)
        {
            material.Children.Add(new EmissiveMaterial(new SolidColorBrush(SelectionGlow)));
        }

        material.Freeze();
        model.Material = material;
        model.BackMaterial = material;
    }

    private MeshGeometry3D GetMesh(PrimitiveKind kind)
    {
        if (_meshCache.TryGetValue(kind, out MeshGeometry3D? cached))
        {
            return cached;
        }

        MeshData data = PrimitiveMeshFactory.Create(kind);
        MeshGeometry3D mesh = new();

        foreach (System.Numerics.Vector3 p in data.Positions)
        {
            mesh.Positions.Add(new Point3D(p.X, p.Y, p.Z));
        }

        foreach (int index in data.Indices)
        {
            mesh.TriangleIndices.Add(index);
        }

        mesh.Freeze();
        _meshCache[kind] = mesh;
        return mesh;
    }

    private static Color ParseColor(string hex)
    {
        try
        {
            return (Color)ColorConverter.ConvertFromString(hex);
        }
        catch (FormatException)
        {
            return Colors.Gray;
        }
    }
}
