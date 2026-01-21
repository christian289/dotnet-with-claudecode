# WPF Media Gallery Implementation

## 1. Image Gallery Example

### 1.1 Gallery ViewModel

```csharp
namespace MyApp.ViewModels;

using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public sealed partial class GalleryViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<BitmapImage> _images = [];

    [ObservableProperty]
    private BitmapImage? _selectedImage;

    [ObservableProperty]
    private int _selectedIndex;

    [RelayCommand]
    private void LoadFolder(string folderPath)
    {
        Images.Clear();

        var extensions = new[] { "*.jpg", "*.jpeg", "*.png", "*.gif", "*.bmp" };

        foreach (var ext in extensions)
        {
            foreach (var file in Directory.GetFiles(folderPath, ext))
            {
                var thumbnail = LoadThumbnail(file);
                Images.Add(thumbnail);
            }
        }

        if (Images.Count > 0)
        {
            SelectedImage = Images[0];
            SelectedIndex = 0;
        }
    }

    [RelayCommand]
    private void NextImage()
    {
        if (Images.Count is 0)
        {
            return;
        }

        SelectedIndex = (SelectedIndex + 1) % Images.Count;
        SelectedImage = Images[SelectedIndex];
    }

    [RelayCommand]
    private void PreviousImage()
    {
        if (Images.Count is 0)
        {
            return;
        }

        SelectedIndex = (SelectedIndex - 1 + Images.Count) % Images.Count;
        SelectedImage = Images[SelectedIndex];
    }

    private static BitmapImage LoadThumbnail(string filePath)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(filePath);
        bitmap.DecodePixelWidth = 200;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }
}
```

### 1.2 Gallery View

```xml
<Grid>
    <Grid.RowDefinitions>
        <RowDefinition Height="*"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="100"/>
    </Grid.RowDefinitions>

    <!-- Main image -->
    <Image Source="{Binding SelectedImage}"
           Stretch="Uniform"
           Grid.Row="0"/>

    <!-- Navigation buttons -->
    <StackPanel Grid.Row="1" Orientation="Horizontal"
                HorizontalAlignment="Center" Margin="10">
        <Button Content="◀ Previous" Command="{Binding PreviousImageCommand}" Margin="5"/>
        <Button Content="Next ▶" Command="{Binding NextImageCommand}" Margin="5"/>
    </StackPanel>

    <!-- Thumbnail list -->
    <ListBox Grid.Row="2"
             ItemsSource="{Binding Images}"
             SelectedIndex="{Binding SelectedIndex}"
             SelectedItem="{Binding SelectedImage}">
        <ListBox.ItemsPanel>
            <ItemsPanelTemplate>
                <VirtualizingStackPanel Orientation="Horizontal"/>
            </ItemsPanelTemplate>
        </ListBox.ItemsPanel>
        <ListBox.ItemTemplate>
            <DataTemplate>
                <Image Source="{Binding}" Width="80" Height="60"
                       Stretch="UniformToFill" Margin="2"/>
            </DataTemplate>
        </ListBox.ItemTemplate>
    </ListBox>
</Grid>
```

---

## 2. Performance Optimization

### 2.1 Image Memory Management

```csharp
// Save memory with DecodePixelWidth/Height
bitmap.DecodePixelWidth = 200; // Decode at 200px instead of original

// Improve performance with Freeze()
bitmap.Freeze();

// Lazy loading for large images
bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
```

### 2.2 Video Performance

```csharp
// Hardware acceleration
RenderOptions.ProcessRenderMode = RenderMode.Default;

// Release video memory
videoPlayer.Close();
videoPlayer.Source = null;
```
