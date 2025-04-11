using System;
using System.ComponentModel;
using Avalonia.Threading;
using Microsoft.AspNetCore.Mvc.Formatters;

public class Clock : INotifyPropertyChanged
{
    private static Clock _instance;
    private DispatcherTimer _disTimer = new DispatcherTimer();

    public event PropertyChangedEventHandler? PropertyChanged;
    public EventHandler<DateTime> OnEveryHour = (s , e) => { };

    public string CurrentTime { get; set; } = "";
    public string NextDownloadCycle { get; set; } = "Downloading in: ";

    private DateTime _nextCycle;
    private int hours = 0;
    private int minutes = 30;
    private bool isRunning;

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

    public void setNextCycle()
    {
        _nextCycle = DateTime.Now.AddHours(this.hours).AddMinutes(this.minutes);
    }

    public void startClock()
    {
        _disTimer.Start();
    }

    public void stopClock()
    {
        _disTimer.Stop();
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
            "Next update check (in {0}h{1}m{2}s)",
            ts.Hours,
            ts.Minutes,
            ts.Seconds
            
        );
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentTime)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextDownloadCycle)));
    }
}