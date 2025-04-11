using System;
using System.ComponentModel;
using System.Linq;

namespace Components;

public class ConnectionController : INotifyPropertyChanged
{
    private static ConnectionController? controllerInstance { get; set; }
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
        instanceUUID = Guid.NewGuid().ToString("D");
        _currentConnection = "N/A";
        allowedConnections = new string[0];

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