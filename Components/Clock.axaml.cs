using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Avalonia.Threading;
using Microsoft.AspNetCore.Mvc.Formatters;
using Components;
using GetStartedApp.Views;
using Supabase;
using System.Threading.Tasks;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.IO.Compression;

public class Clock : INotifyPropertyChanged
{
    private static Clock _instance;
    private DispatcherTimer _disTimer = new DispatcherTimer();
    public static string srcPath { get; set; } = string.Empty;
    public static string srcName { get; set; } = string.Empty;
    public const  string CurrentHashPath = "./currenthash.txt";
    private bool _isRunning { get; set; } = false;
    public event PropertyChangedEventHandler? PropertyChanged;
    public EventHandler<DateTime> OnEveryHour = (s, e) => { onEveryHour();};

    public string CurrentTime { get; set; } = "";
    public string NextDownloadCycle { get; set; } = "Downloading in: ";

    private DateTime _nextCycle;
    private int hours = 0;
    private int minutes = 1;
    
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
    public void setSrcPath(string path)
    {
        srcPath = path;
    }
    public void setSrcName(string path)
    {
        srcName = path;
    }
    private Clock(int hours, int minutes)
    {
        this.hours = hours;
        this.minutes = minutes;
        setNextCycle();
        _disTimer.Interval = TimeSpan.FromSeconds(1);
        _disTimer.Tick += DispatcherTimer_Tick;
    }

    private Clock()
    {
        setNextCycle();
        _disTimer.Interval = TimeSpan.FromSeconds(1);
        _disTimer.Tick += DispatcherTimer_Tick;
    }
    

    public static Clock GetInstance(int hours, int minutes)
    {
        if (_instance == null)
        {
            _instance = new Clock(hours, minutes);
        }
        return _instance;
    }

    public static Clock GetInstance()
    {
        if (_instance == null)
        {
            _instance = new Clock();
        }
        return _instance;
    }

    public async static void onEveryHour()
    {
        if (File.Exists(CurrentHashPath))
        {
            string hash = DirectoryHasher.CreateDirectorySha256(srcPath);
            string currentHash = File.ReadAllText(CurrentHashPath).Trim();
            if (!DirectoryHasher.CompareHash(hash, currentHash))
            {
                File.WriteAllText(CurrentHashPath, hash);
                Debug.WriteLine("Hash changed: " + hash);
                var supabase  = MainWindow.GetSupabase();
                var model = new Sync
                {
                   Id = ConnectionController.GetInstance().getUUID(),
                   allowedConnections = ConnectionController.GetInstance().getAllowedConnections(),
                   filename = hash,
                   
                   
                };
        
        
                string sourceFolder = srcPath;
                string zipPath = "./" + srcName + ".zip";

                if (File.Exists(zipPath))
                    File.Delete(zipPath); 

                ZipFile.CreateFromDirectory(sourceFolder, zipPath);
                Debug.WriteLine("Folder compressed to: " + zipPath);
                Debug.WriteLine("Uploading: " + zipPath + " as key: " + hash);
                await supabase!.Storage
                    .From("sync")
                    .Upload(zipPath, hash, new Supabase.Storage.FileOptions
                    {
                        Upsert = true
                    });

                var response = await supabase!.From<Sync>().Upsert(model);
                Debug.WriteLine("Inserted city with status: " + response?.ResponseMessage?.StatusCode);

            }
            else
            {
                Debug.WriteLine("Hash has not changed: " + hash);
            }
            
        }
        else
        {
            
            string hash = DirectoryHasher.CreateDirectorySha256(srcPath);
            Debug.WriteLine("Created new has file and added hash:" + hash);
            File.WriteAllText(CurrentHashPath, hash);
        }
        
    }
    public void setNextCycle()
    {
        _nextCycle = DateTime.Now.AddHours(this.hours).AddMinutes(this.minutes);
    }

    public bool IsRunning()
    {
        return _isRunning;
    }
    public void startClock()
    {
        _disTimer.Start();
        _isRunning = true;
    }

    public void stopClock()
    {
        _disTimer.Stop();
        _isRunning = false;
    }

    private void DispatcherTimer_Tick(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        if (now >= _nextCycle)
        {
            setNextCycle();
            OnEveryHour(this, now);
        }
        TimeSpan ts = _nextCycle - now;
        NextDownloadCycle = String.Format(
            "Check in {0}h {1}m {2}s ",
            ts.Hours,
            ts.Minutes,
            ts.Seconds
            
        );
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextDownloadCycle)));
    }
}