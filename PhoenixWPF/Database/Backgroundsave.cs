using PhoenixModel.Helper;
using System;
using System.ComponentModel;
using System.Threading;
using System.Windows.Threading;

public class Backgroundsave
{
    private readonly DispatcherTimer _idleTimer;
  
    public Backgroundsave()
    {     
        _idleTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _idleTimer.Tick += CheckIdleTime;      
    }

    private void CheckIdleTime(object? sender, EventArgs e)
    {
         PerformSave();
    }

    private void PerformSave()
    {
       
    }
}
