using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Supabase;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.IO.Compression;
using GetStartedApp.Views;

namespace Components;

public class ConnectionController : INotifyPropertyChanged
{
    private static ConnectionController? controllerInstance { get; set; }
    private const string UuidFilePath = "./uuid.txt";
    private const string ConnectionFilePath = "./connection.txt";
    private const string ConnectionHashFilePath = "./connectionhash.txt";
    private string ConnectionDirectoryPath { get; set; } = string.Empty;
    
    public  string instanceUUID { get; private set; }
    private string _currentConnection;
    public string[] allowedConnections { get; private set; }
    public string[] downloadhistory { get; private set; }
    public Clock clock { get; set; } = Clock.GetInstance();
    public DownloadClock downdloadclock { get; set; } = DownloadClock.GetInstance();


    public event PropertyChangedEventHandler? PropertyChanged;

    public string GetDownloadDirectory()
    {
        return ConnectionDirectoryPath;
    }
    public string[] getAllowedConnections()
    {
        return allowedConnections;
    }
    public  string getUUID()
    {
        return instanceUUID;
    }
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
    [Table("sync")]
    class Sync : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; }

        [Column("allowed_connections")] 
        public string[] allowedConnections { get; set; } = Array.Empty<string>();

        [Column("filename")]
        public string filename { get; set; }
    }
    
    public async static void CheckConnectionHash()
    {
        if (File.Exists(ConnectionFilePath))
        { 
            Debug.WriteLine("Connection hash file found");
           string connectionUUID = File.ReadAllText(ConnectionFilePath).Trim();
           var supabase  = MainWindow.GetSupabase();
           var result = await supabase.From<Sync>().Where(x => x.Id == connectionUUID).Get();
           Debug.WriteLine("Found new connection hash");
           var syncs = result.Models;
           var sync = syncs.FirstOrDefault();
           if (File.Exists(ConnectionHashFilePath))
           {
               Debug.WriteLine("3");
               string connectionHashUUID = File.ReadAllText(ConnectionHashFilePath).Trim();
               if (connectionHashUUID != sync.filename)
               {
                   Debug.WriteLine("4");
                   string downloadpath = DownloadClock.GetInstance().GetSrcPath();
                   File.WriteAllText(ConnectionHashFilePath, sync.filename);
                   Debug.WriteLine(downloadpath);
                   if (Directory.Exists(downloadpath))
                   {
                       Debug.WriteLine("5");
                       string parentDirectory = Directory.GetParent(downloadpath)?.FullName;
                       Directory.Delete(downloadpath, recursive: true);
                       var bytes = await supabase.Storage
                           .From("sync")
                           .Download(sync.filename, null);
                       
                       string savePath = parentDirectory +  ".zip";
                       Debug.WriteLine(savePath);
                       await File.WriteAllBytesAsync(savePath, bytes);
                       ZipFile.ExtractToDirectory(savePath, parentDirectory);
                       if (File.Exists(savePath))
                       {
                           File.Delete(savePath);
                       }
                       Debug.WriteLine("Download Complete");

                   }
                   Debug.WriteLine("6");
                   
                   
               }
           }
           else
           {
               string downloadpath = DownloadClock.GetInstance().GetSrcPath();
               File.WriteAllText(ConnectionHashFilePath, sync.filename);
               Debug.WriteLine(downloadpath);
               if (Directory.Exists(downloadpath))
               {
                   Debug.WriteLine("5");
                   string parentDirectory = Directory.GetParent(downloadpath)?.FullName;
                   Directory.Delete(downloadpath, recursive: true);
                   var bytes = await supabase.Storage
                       .From("sync")
                       .Download(sync.filename, null);
                       
                   string savePath = parentDirectory +  ".zip";
                   Debug.WriteLine(savePath);
                   await File.WriteAllBytesAsync(savePath, bytes);
                   ZipFile.ExtractToDirectory(savePath, parentDirectory);
                   if (File.Exists(savePath))
                   {
                       File.Delete(savePath);
                   }
                   Debug.WriteLine("Download Complete");

               }
               Debug.WriteLine("6");

           }
           
        }
    }

    public void SetCurrentConnection(string connection)
    {
        CurrentConnection = connection;
        File.WriteAllText(ConnectionFilePath, connection);
            
    }

    public void addAllowedConnection(string connection)
    {
        allowedConnections = allowedConnections.Concat(new[] { connection }).ToArray();
        OnPropertyChanged(nameof(allowedConnections));
    }

    public void addDownloadhistory(string connection)
    {
        downloadhistory = downloadhistory.Concat(new[] { connection }).ToArray();
        OnPropertyChanged(nameof(downloadhistory));
    }

    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    
    
}