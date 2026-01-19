---
name: integrating-wpf-media
description: Integrates multimedia content in WPF including MediaElement video/audio playback, Image control display, and SoundPlayerAction effects. Use when building media players, galleries, or multimedia UIs.
---

# WPF Media Integration Patterns

Integrating multimedia content such as images, video, and audio in WPF.

## 1. Image Control

### 1.1 Basic Image Display

```xml
<!-- Resource image -->
<Image Source="/Assets/logo.png" Width="100" Height="100"/>

<!-- Absolute path -->
<Image Source="C:\Images\photo.jpg"/>

<!-- URI -->
<Image Source="https://example.com/image.png"/>

<!-- Pack URI (embedded resource) -->
<Image Source="pack://application:,,,/MyAssembly;component/Images/icon.png"/>
```

### 1.2 Stretch Options

```xml
<!-- None: maintain original size -->
<Image Source="/photo.jpg" Stretch="None"/>

<!-- Fill: stretch to fit area (ignore aspect ratio) -->
<Image Source="/photo.jpg" Stretch="Fill"/>

<!-- Uniform: maintain aspect ratio, maximum size within area -->
<Image Source="/photo.jpg" Stretch="Uniform"/>

<!-- UniformToFill: maintain aspect ratio, fill area (may crop) -->
<Image Source="/photo.jpg" Stretch="UniformToFill"/>
```

### 1.3 Dynamic Image Loading

```csharp
namespace MyApp.Helpers;

using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public static class ImageHelper
{
    /// <summary>
    /// Load image from file
    /// </summary>
    public static BitmapImage LoadFromFile(string filePath)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze(); // Can be used outside UI thread
        return bitmap;
    }

    /// <summary>
    /// Load image from stream
    /// </summary>
    public static BitmapImage LoadFromStream(Stream stream)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.StreamSource = stream;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    /// <summary>
    /// Load thumbnail (memory optimization)
    /// </summary>
    public static BitmapImage LoadThumbnail(string filePath, int maxWidth, int maxHeight)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
        bitmap.DecodePixelWidth = maxWidth;
        bitmap.DecodePixelHeight = maxHeight;
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    /// <summary>
    /// Load image from Base64
    /// </summary>
    public static BitmapImage LoadFromBase64(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        using var stream = new MemoryStream(bytes);
        return LoadFromStream(stream);
    }
}
```

### 1.4 Image Load Events

```csharp
// XAML
// <Image x:Name="DynamicImage" ImageFailed="OnImageFailed"/>

private void LoadImageAsync(string url)
{
    var bitmap = new BitmapImage();
    bitmap.BeginInit();
    bitmap.UriSource = new Uri(url);
    bitmap.EndInit();

    // Loading complete event
    bitmap.DownloadCompleted += (s, e) =>
    {
        // Image load complete
    };

    // Loading failed event
    bitmap.DownloadFailed += (s, e) =>
    {
        // Error handling
    };

    // Loading progress
    bitmap.DownloadProgress += (s, e) =>
    {
        var progress = e.Progress; // 0-100
    };

    DynamicImage.Source = bitmap;
}
```

---

## 2. MediaElement (Video/Audio)

### 2.1 Basic Usage

```xml
<!-- Auto-play video -->
<MediaElement Source="/Videos/intro.mp4"
              LoadedBehavior="Play"
              UnloadedBehavior="Stop"/>

<!-- Manual control video -->
<MediaElement x:Name="VideoPlayer"
              Source="/Videos/movie.mp4"
              LoadedBehavior="Manual"
              UnloadedBehavior="Stop"
              MediaOpened="OnMediaOpened"
              MediaEnded="OnMediaEnded"
              MediaFailed="OnMediaFailed"/>
```

### 2.2 LoadedBehavior Options

| Value | Description |
|-------|-------------|
| **Play** | Auto-play |
| **Pause** | Pause after load |
| **Stop** | Stop after load |
| **Manual** | Control via code |
| **Close** | Close media |

### 2.3 Video Player Implementation

```csharp
namespace MyApp.Controls;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

public sealed partial class VideoPlayerControl : UserControl
{
    private readonly DispatcherTimer _positionTimer;
    private bool _isDragging;

    public VideoPlayerControl()
    {
        InitializeComponent();

        _positionTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(200)
        };
        _positionTimer.Tick += OnPositionTimerTick;
    }

    private void OnMediaOpened(object sender, RoutedEventArgs e)
    {
        // Display total duration
        if (VideoPlayer.NaturalDuration.HasTimeSpan)
        {
            var duration = VideoPlayer.NaturalDuration.TimeSpan;
            PositionSlider.Maximum = duration.TotalSeconds;
            TotalTimeText.Text = FormatTime(duration);
        }

        _positionTimer.Start();
    }

    private void OnMediaEnded(object sender, RoutedEventArgs e)
    {
        _positionTimer.Stop();
        VideoPlayer.Stop();
        VideoPlayer.Position = TimeSpan.Zero;
    }

    private void OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
    {
        // Media load failed
        MessageBox.Show($"Media load failed: {e.ErrorException.Message}");
    }

    private void OnPositionTimerTick(object? sender, EventArgs e)
    {
        if (!_isDragging)
        {
            PositionSlider.Value = VideoPlayer.Position.TotalSeconds;
            CurrentTimeText.Text = FormatTime(VideoPlayer.Position);
        }
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        VideoPlayer.Play();
        _positionTimer.Start();
    }

    private void PauseButton_Click(object sender, RoutedEventArgs e)
    {
        VideoPlayer.Pause();
    }

    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        VideoPlayer.Stop();
        _positionTimer.Stop();
    }

    private void PositionSlider_DragStarted(object sender, EventArgs e)
    {
        _isDragging = true;
    }

    private void PositionSlider_DragCompleted(object sender, EventArgs e)
    {
        _isDragging = false;
        VideoPlayer.Position = TimeSpan.FromSeconds(PositionSlider.Value);
    }

    private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        VideoPlayer.Volume = e.NewValue;
    }

    private static string FormatTime(TimeSpan time)
    {
        return time.Hours > 0
            ? $"{time:hh\\:mm\\:ss}"
            : $"{time:mm\\:ss}";
    }
}
```

### 2.4 VideoPlayerControl XAML

```xml
<UserControl x:Class="MyApp.Controls.VideoPlayerControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <!-- Video area -->
        <MediaElement x:Name="VideoPlayer"
                      LoadedBehavior="Manual"
                      UnloadedBehavior="Stop"
                      MediaOpened="OnMediaOpened"
                      MediaEnded="OnMediaEnded"
                      MediaFailed="OnMediaFailed"/>

        <!-- Control bar -->
        <Grid Grid.Row="1" Background="#CC000000">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>

            <Button x:Name="PlayButton" Content="▶" Click="PlayButton_Click"
                    Grid.Column="0" Margin="5"/>
            <Button x:Name="PauseButton" Content="⏸" Click="PauseButton_Click"
                    Grid.Column="1" Margin="5"/>
            <Button x:Name="StopButton" Content="⏹" Click="StopButton_Click"
                    Grid.Column="2" Margin="5"/>

            <!-- Position slider -->
            <Slider x:Name="PositionSlider"
                    Grid.Column="3"
                    Margin="5"
                    VerticalAlignment="Center"
                    Thumb.DragStarted="PositionSlider_DragStarted"
                    Thumb.DragCompleted="PositionSlider_DragCompleted"/>

            <!-- Time display -->
            <StackPanel Grid.Column="4" Orientation="Horizontal"
                        VerticalAlignment="Center" Margin="5">
                <TextBlock x:Name="CurrentTimeText" Foreground="White" Text="00:00"/>
                <TextBlock Foreground="White" Text=" / "/>
                <TextBlock x:Name="TotalTimeText" Foreground="White" Text="00:00"/>
            </StackPanel>

            <!-- Volume slider -->
            <Slider x:Name="VolumeSlider"
                    Grid.Column="5"
                    Width="80"
                    Margin="5"
                    Minimum="0" Maximum="1" Value="0.5"
                    VerticalAlignment="Center"
                    ValueChanged="VolumeSlider_ValueChanged"/>
        </Grid>
    </Grid>
</UserControl>
```

---

## 3. Audio Playback

### 3.1 Audio with MediaElement

```xml
<MediaElement x:Name="AudioPlayer"
              Source="/Sounds/background.mp3"
              LoadedBehavior="Manual"
              Volume="0.5"/>
```

### 3.2 SoundPlayer (Simple WAV)

```csharp
namespace MyApp.Services;

using System.Media;

public sealed class SoundService
{
    private readonly SoundPlayer _clickSound;
    private readonly SoundPlayer _notificationSound;

    public SoundService()
    {
        // WAV files only
        _clickSound = new SoundPlayer("Sounds/click.wav");
        _notificationSound = new SoundPlayer("Sounds/notification.wav");

        // Preload
        _clickSound.Load();
        _notificationSound.Load();
    }

    public void PlayClick()
    {
        _clickSound.Play(); // Async playback
    }

    public void PlayNotification()
    {
        _notificationSound.PlaySync(); // Sync playback
    }
}
```

### 3.3 SoundPlayerAction (Sound in XAML)

```xml
<Button Content="Click Me">
    <Button.Triggers>
        <EventTrigger RoutedEvent="Button.Click">
            <SoundPlayerAction Source="/Sounds/click.wav"/>
        </EventTrigger>
    </Button.Triggers>
</Button>
```

---

## 4. Image Gallery Example

### 4.1 Gallery ViewModel

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

### 4.2 Gallery View

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

## 5. Performance Optimization

### 5.1 Image Memory Management

```csharp
// Save memory with DecodePixelWidth/Height
bitmap.DecodePixelWidth = 200; // Decode at 200px instead of original

// Improve performance with Freeze()
bitmap.Freeze();

// Lazy loading for large images
bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
```

### 5.2 Video Performance

```csharp
// Hardware acceleration
RenderOptions.ProcessRenderMode = RenderMode.Default;

// Release video memory
videoPlayer.Close();
videoPlayer.Source = null;
```

---

## 6. References

- [Image Class - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.image)
- [MediaElement Class - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.windows.controls.mediaelement)
- [Multimedia Overview - Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/graphics-multimedia/multimedia-overview)
