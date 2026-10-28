using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
[assembly:AssemblyTitle("Cursor Studio Cursor Manager")]
[assembly:AssemblyVersion("1.0.0.0")]
[assembly:AssemblyFileVersion("1.0.0.0")]
namespace CursorStudio {
static class Program {
 internal static EventWaitHandle ShowEvent,QuitEvent,RestoreEvent;
 internal static void Initialize(Settings s){if(s.Initialized)return;s.Initialized=true;Store.Save(s);}
 [STAThread]static int Main(string[] args){if(args.Length==3&&args[0]=="--startup-state")return StartupRegistration.Worker(args[1],args[2]);Application.EnableVisualStyles();Application.SetCompatibleTextRenderingDefault(false);try{
 if(args.Contains("--self-test"))return Tests.Run(false,args.Length>1?args[1]:Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"self-test.json"));
 if(args.Contains("--integration-test"))return Tests.Run(true,args.Length>1?args[1]:Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"integration-test.json"));
 if(args.Contains("--resource-test"))return Tests.Resources(args.Length>1?args[1]:Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"resource-test.json"));
 if(args.Length==2&&args[0]=="--startup-status"){Store.Atomic(args[1],Store.Json.Serialize(StartupRegistration.Run("status")));return 0;}
 ShowEvent=new EventWaitHandle(false,EventResetMode.AutoReset,@"Local\CursorStudio.Show.v1");QuitEvent=new EventWaitHandle(false,EventResetMode.AutoReset,@"Local\CursorStudio.Quit.v1");RestoreEvent=new EventWaitHandle(false,EventResetMode.AutoReset,@"Local\CursorStudio.Restore.v1");if(args.Contains("--quit")){QuitEvent.Set();return 0;}
 bool created;using(var mutex=new Mutex(true,@"Local\CursorStudio.Instance.v1",out created)){if(!created){if(args.Contains("--restore"))RestoreEvent.Set();else if(!args.Contains("--startup"))ShowEvent.Set();return 0;}try{Settings s=Store.Load();L.Set(s.Language);Initialize(s);
 if(args.Contains("--migrate-endfield")){Migration.Run(s);return 0;}
 if(args.Contains("--refresh-startup")){Store.Startup(s.AutoStart);return 0;}
 if(args.Contains("--restore")){Backup.Original();s.ActiveId="";s.RulesEnabled=false;Store.Save(s);return 0;}
 bool startup=args.Contains("--startup");if(startup){var t=s.Themes.FirstOrDefault(x=>x.Id==s.ActiveId);if(t!=null)Native.Apply(t);}
 using(var form=new MainForm(s,startup)){if(args.Contains("--capture")){form.Show();Application.DoEvents();form.CaptureView(args[1]);form.ExitNow();}else Application.Run(form);}return 0;
 }finally{mutex.ReleaseMutex();}}}catch(Exception e){Store.Log(e);if(args.Length==0||args.Contains("--restore"))MessageBox.Show(e.Message,"Cursor Studio",MessageBoxButtons.OK,MessageBoxIcon.Error);return 1;}}
}
}
