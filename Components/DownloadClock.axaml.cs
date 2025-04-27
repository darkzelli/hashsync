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

public class DownloadClock : INotifyPropertyChanged
{
    private static DownloadClock _instance;
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
    private int minutes = 2;
    
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
    public string GetSrcPath()
    {
        return srcPath;
    }
    public void setSrcPath(string path)
    {
        srcPath = path;
    }
    public void setSrcName(string path)
    {
        srcName = path;
    }
    private DownloadClock(int hours, int minutes)
    {
        this.hours = hours;
        this.minutes = minutes;
        setNextCycle();
        _disTimer.Interval = TimeSpan.FromSeconds(1);
        _disTimer.Tick += DispatcherTimer_Tick;
        startClock();
    }

    private DownloadClock()
    {
        setNextCycle();
        _disTimer.Interval = TimeSpan.FromSeconds(1);
        _disTimer.Tick += DispatcherTimer_Tick;
        startClock();
    }
    

    public static DownloadClock GetInstance(int hours, int minutes)
    {
        if (_instance == null)
        {
            _instance = new DownloadClock(hours, minutes);
        }
        return _instance;
    }

    public static DownloadClock GetInstance()
    {
        if (_instance == null)
        {
            _instance = new DownloadClock();
        }
        return _instance;
    }

    public async static void onEveryHour()
    {
       ConnectionController.CheckConnectionHash();

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