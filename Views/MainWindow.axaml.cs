using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using GetStartedApp;
using Components;

namespace GetStartedApp.Views;

public partial class MainWindow : Window
{
    private bool _mouseDownForWindowMoving = false;
    private PointerPoint _originalPoint;
    public MainWindow()
    {
        InitializeComponent();
    }
    
    private void InputElement_OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_mouseDownForWindowMoving) return;

        PointerPoint currentPoint = e.GetCurrentPoint(this);
        Position = new PixelPoint(Position.X + (int)(currentPoint.Position.X - _originalPoint.Position.X),
            Position.Y + (int)(currentPoint.Position.Y - _originalPoint.Position.Y));
    }

    private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (WindowState == WindowState.Maximized || WindowState == WindowState.FullScreen) return;

        _mouseDownForWindowMoving = true;
        _originalPoint = e.GetCurrentPoint(this);
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _mouseDownForWindowMoving = false;
    }

    // Minimize Window
    private void Minimize_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    // Maximize or Restore Window
    private void Maximize_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    // Close Window
    private void Close_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Close();
    }

    //Change Connection
    private void ChangeConnection(object? sender, RoutedEventArgs e)
    {
        if (Connection.Text != null)
        {
            ConnectionController connectionController = ConnectionController.GetInstance();
            connectionController.SetCurrentConnection(Connection.Text);
        }
    }
    
    private void AddAllowedConnection(object? sender, RoutedEventArgs e)
    {
        if (AllowedConnections.Text != null)
        {
            ConnectionController connectionController = ConnectionController.GetInstance();
            connectionController.addAllowedConnection(AllowedConnections.Text);
        }
    }
    
    public async void DFS(IStorageFolder rootFolder)
    {
        var visited = new HashSet<IStorageItem>();
        var stack = new Stack<(IStorageItem item, StackPanel parentPanel)>();

        var rootPanel = this.FindControl<StackPanel>("FileTree");
        visited.Add(rootFolder);
        stack.Push((rootFolder, rootPanel));

        while (stack.Count > 0)
        {
            var (node, parentPanel) = stack.Pop();

            if (node is IStorageFile file)
            {
                var fileText = new TextBlock
                {
                    Text = file.Name,
                    Margin = new Thickness(5, 0, 0, 0),
                    Foreground = Brushes.LightGray
                };
                parentPanel.Children.Add(fileText);
            }
            else if (node is IStorageFolder folder)
            {
                // Folder name label
                var folderText = new TextBlock
                {
                    Text = folder.Name,
                    FontWeight = FontWeight.Bold,
                    Margin = new Thickness(5, 5, 0, 0),
                };

                // Nested panel for folder contents
                var nestedPanel = new StackPanel
                {
                    Margin = new Thickness(20, 0, 0, 0)
                };

                parentPanel.Children.Add(folderText);
                parentPanel.Children.Add(nestedPanel);
                

                var items = folder.GetItemsAsync().ToBlockingEnumerable().ToArray();
                for (int i = items.Length - 1; i >= 0; i--)
                {
                    var item = items[i];
                    if (!visited.Contains(item))
                    {
                        visited.Add(item);
                        stack.Push((item, nestedPanel)); // 👈 This is the key!
                    }
                }
            }
        }
        
    }

    private async void OpenFileButton_Clicked(object sender, RoutedEventArgs args)
    {
        var rootPanel = this.FindControl<StackPanel>("FileTree");
        rootPanel.Children.Clear();
        var topLevel = TopLevel.GetTopLevel(this);
        // Get top level from the current control. Alternatively, you can use Window reference instead.
        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select a Folder",
            AllowMultiple = false
        });
        if (!folders.Any()) { return;}
        DFS(folders.First());
        
        /*var filesandfolders = folders[0].GetItemsAsync().ToBlockingEnumerable().ToList();
        foreach (var item in filesandfolders)
        {
            if (item is IStorageFile file)
            {
                Debug.WriteLine($"File: {file.Name}");
            }
            else if (item is IStorageFolder folder)
            {
                Debug.WriteLine($"Folder: {folder.Name}");
            }
            else
            {
                Debug.WriteLine("Unknown item type");
            }
        }
        
        if (folders.Count > 0)
        {
            var folder = folders[0];
            var fullPath = folder.Path?.LocalPath;
    
            FolderName.Text = folder.Name;
        }*/
    }
}