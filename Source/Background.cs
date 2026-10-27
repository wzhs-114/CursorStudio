using System;
using System.Threading;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
namespace CursorStudio {
// Foreground events and IPC waits replace the old permanent 300ms UI poll.
public sealed class BackgroundWatch:IDisposable {
 readonly Control owner;readonly Action changed;readonly Action show,quit,restore;
 readonly WinEvent callback;IntPtr hook;bool disposed,queued;readonly System.Windows.Forms.Timer fallback;
 readonly RegisteredWaitHandle[] waits=new RegisteredWaitHandle[3];
 delegate void WinEvent(IntPtr h,uint ev,IntPtr window,int obj,int child,uint thread,uint time);
 [DllImport("user32.dll",SetLastError=true)]static extern IntPtr SetWinEventHook(uint min,uint max,IntPtr module,WinEvent callback,uint process,uint thread,uint flags);
 [DllImport("user32.dll")]static extern bool UnhookWinEvent(IntPtr hook);
 public bool IsFallback {get{return hook==IntPtr.Zero;}}
 public BackgroundWatch(Control c,Action onChange,Action onShow,Action onQuit,Action onRestore){owner=c;changed=onChange;show=onShow;quit=onQuit;restore=onRestore;callback=OnForeground;hook=SetWinEventHook(3,3,IntPtr.Zero,callback,0,0,0);fallback=new System.Windows.Forms.Timer{Interval=1000};fallback.Tick+=delegate{Queue();};if(hook==IntPtr.Zero)fallback.Start();var signals=new[]{Program.ShowEvent,Program.QuitEvent,Program.RestoreEvent};for(int i=0;i<3;i++){int n=i;if(signals[i]!=null)waits[i]=ThreadPool.RegisterWaitForSingleObject(signals[i],delegate(object state,bool timedOut){Post(n==0?show:n==1?quit:restore);},null,Timeout.Infinite,false);}SystemEvents.SessionSwitch+=SessionChanged;SystemEvents.PowerModeChanged+=PowerChanged;}
 void OnForeground(IntPtr h,uint ev,IntPtr window,int obj,int child,uint thread,uint time){Queue();}
 void Queue(){if(disposed||queued)return;queued=true;if(!Post(delegate{queued=false;changed();}))queued=false;}
 bool Post(Action action){if(disposed||owner.IsDisposed)return false;try{owner.BeginInvoke((Action)(()=>{if(!disposed)action();}));return true;}catch(InvalidOperationException){return false;}}
 void SessionChanged(object sender,SessionSwitchEventArgs e){if(e.Reason==SessionSwitchReason.SessionUnlock||e.Reason==SessionSwitchReason.ConsoleConnect||e.Reason==SessionSwitchReason.RemoteConnect)Post(changed);}
 void PowerChanged(object sender,PowerModeChangedEventArgs e){if(e.Mode==PowerModes.Resume)Post(changed);}
 public void Dispose(){if(disposed)return;disposed=true;SystemEvents.SessionSwitch-=SessionChanged;SystemEvents.PowerModeChanged-=PowerChanged;fallback.Dispose();if(hook!=IntPtr.Zero){UnhookWinEvent(hook);hook=IntPtr.Zero;}foreach(var w in waits)if(w!=null)w.Unregister(null);GC.KeepAlive(callback);}
}
}
