using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
[assembly:AssemblyTitle("Cursor Studio Cursor Manager")]
[assembly:AssemblyVersion("1.1.0.0")]
[assembly:AssemblyFileVersion("1.1.0.0")]
namespace CursorStudio {
static class Program {
 internal static EventWaitHandle ShowEvent,QuitEvent,RestoreEvent;
 internal static void Initialize(Settings s){if(s.Initialized)return;s.Initialized=true;Store.Save(s);}
 [STAThread]static int Main(string[] args){int dataIndex=Array.IndexOf(args,"--data-dir");if(dataIndex>=0&&dataIndex+1<args.Length){Store.Root=Path.GetFullPath(args[dataIndex+1]);args=args.Where((v,i)=>i!=dataIndex&&i!=dataIndex+1).ToArray();}if(args.Length==3&&args[0]=="--startup-state")return StartupRegistration.Worker(args[1],args[2]);if(args.Contains("--ui")||args.Contains("--capture")||args.Contains("--self-test")||args.Contains("--integration-test")||args.Contains("--resource-test"))Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);try{
 if(args.Contains("--self-test"))return Tests.Run(false,args.Length>1?args[1]:Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"self-test.json"));
 if(args.Contains("--integration-test"))return Tests.Run(true,args.Length>1?args[1]:Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"integration-test.json"));
 if(args.Contains("--ui")){bool uiCreated;using(var uiMutex=new Mutex(true,RuntimeIpc.Name("UI.Instance.v2"),out uiCreated)){if(!uiCreated){RuntimeIpc.Signal("UI.Show.v2");return 0;}try{if(!RuntimeIpc.BackgroundAlive())return 1;var uiSettings=Store.Load();L.Set(uiSettings.Language);using(var ui=new MainForm(uiSettings,false,true))Application.Run(ui);return 0;}finally{uiMutex.ReleaseMutex();}}}
 if(args.Contains("--close-ui")){RuntimeIpc.Signal("UI.UserClose.v2");return 0;}
 if(args.Contains("--runtime-status")){RuntimeIpc.Signal("Status.v2");return 0;}
 if(args.Contains("--resource-test"))return Tests.Resources(args.Length>1?args[1]:Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"resource-test.json"));
 if(args.Length==2&&args[0]=="--startup-status"){Store.Atomic(args[1],Store.Json.Serialize(StartupRegistration.Run("status")));return 0;}
 ShowEvent=RuntimeIpc.Event("Show.v1");QuitEvent=RuntimeIpc.Event("Quit.v1");RestoreEvent=RuntimeIpc.Event("Restore.v1");if(args.Contains("--quit")){QuitEvent.Set();return 0;}
 bool created;using(var mutex=new Mutex(true,RuntimeIpc.Name("Instance.v1"),out created)){if(!created){if(args.Contains("--restore"))RestoreEvent.Set();else if(!args.Contains("--startup"))ShowEvent.Set();return 0;}try{Settings s=Store.Load();L.Set(s.Language);Initialize(s);
 if(args.Contains("--migrate-endfield")){Migration.Run(s);return 0;}
 if(args.Contains("--refresh-startup")){Store.Startup(s.AutoStart);return 0;}
 if(args.Contains("--restore")){Backup.Original();s.ActiveId="";s.RulesEnabled=false;Store.Save(s);return 0;}
 bool startup=args.Contains("--startup");if(startup){var t=s.Themes.FirstOrDefault(x=>x.Id==s.ActiveId);if(t!=null){var current=Native.CurrentPaths();if(Native.Paths(t).Any(p=>!String.Equals(p.Value,current[p.Key],StringComparison.OrdinalIgnoreCase)))Native.Apply(t);}}
 if(args.Contains("--capture")){using(var form=new MainForm(s,false)){form.Show();Application.DoEvents();form.CaptureView(args[1]);form.ExitNow();}}else using(var host=new BackgroundHost(s,!startup&&!args.Contains("--background")))Application.Run(host);return 0;
 }finally{mutex.ReleaseMutex();}}}catch(Exception e){Store.Log(e);if(args.Length==0||args.Contains("--restore"))MessageBox.Show(e.Message,"Cursor Studio",MessageBoxButtons.OK,MessageBoxIcon.Error);return 1;}}
}
}
