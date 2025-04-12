using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Components;

public class ConnectionController : INotifyPropertyChanged
{
    private static ConnectionController? controllerInstance { get; set; }
    private const string UuidFilePath = "./uuid.txt";
    public string instanceUUID { get; private set; }
    private string _currentConnection;
    public string[] allowedConnections { get; private set; }
    public Clock clock { get; set; } = Clock.GetInstance();



    public event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentConnection
    {
        get => _currentConnection;
        set
        {
            if (_currentConnection != value)
            {
                _currentConnection = value;
                OnPropertyChanged(nameof(CurrentConnection)); // Notify the UI
            }
        }
    }

    

    private ConnectionController()
    {
        instanceUUID = LoadOrCreateUUID();
        _currentConnection = "N/A";
        allowedConnections = Array.Empty<string>();
    }

    
    private string LoadOrCreateUUID()
    {
        if (File.Exists(UuidFilePath))
        {
            return File.ReadAllText(UuidFilePath).Trim();
        }
        else
        {
            var newUUID = Guid.NewGuid().ToString("D");
            File.WriteAllText(UuidFilePath, newUUID);
            return newUUID;
        }
    }
    
    public static ConnectionController GetInstance()
    {
        if (controllerInstance == null)
        {
            controllerInstance = new ConnectionController();
        }

        return controllerInstance;
    }

    public void SetCurrentConnection(string connection)
    {
        CurrentConnection = connection;
    }

    public void addAllowedConnection(string connection)
    {
        allowedConnections = allowedConnections.Concat(new[] { connection }).ToArray();
        OnPropertyChanged(nameof(allowedConnections));
    }

    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    
}