using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CursorStudio;
class SplitProcessQA {
 [DllImport("psapi.dll")]static extern bool QueryWorkingSet(IntPtr handle,IntPtr data,int size);
 static object Memory(Process process){process.Refresh();long pages=0;IntPtr buffer=Marshal.AllocHGlobal(2*1024*1024);try{if(QueryWorkingSet(process.Handle,buffer,2*1024*1024)){long count=Marshal.ReadIntPtr(buffer).ToInt64();for(int i=1;i<=count;i++)if((Marshal.ReadIntPtr(buffer,i*IntPtr.Size).ToInt64()&256)==0)pages++;}}finally{Marshal.FreeHGlobal(buffer);}return new{pid=process.Id,privateWorkingMiB=pages*Environment.SystemPageSize/1048576.0,privateBytesMiB=process.PrivateMemorySize64/1048576.0,handles=process.HandleCount};}
 static void Check(bool value,string text){if(!value)throw new Exception(text);}
 static Process Launch(string exe,string arguments,bool visible=false){return Process.Start(new ProcessStartInfo(exe,arguments){UseShellExecute=false,CreateNoWindow=true,WindowStyle=visible?ProcessWindowStyle.Normal:ProcessWindowStyle.Hidden});}
 static string DataArgs(){return " --data-dir \""+Store.Root+"\"";}
 static Dictionary<string,object> Status(){string file=Path.Combine(Store.Root,"runtime-status.json");if(File.Exists(file))File.Delete(file);for(int n=0;n<80;n++){RuntimeIpc.Signal("Status.v2");Thread.Sleep(50);if(File.Exists(file))return Store.Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(file));}throw new Exception("Background status timed out");}
 static int UiPid(){return Convert.ToInt32(Status()["UiPid"]);}
 static void WaitUiGone(int pid){try{using(var ui=Process.GetProcessById(pid))Check(ui.WaitForExit(8000),"UI process did not exit");}catch(ArgumentException){}}
 static void WaitRule(string id){for(int n=0;n<100;n++){if(Convert.ToString(Status()["CurrentRule"])==id)return;Thread.Sleep(50);}throw new Exception("Foreground rule did not become "+id);}
 [STAThread]static int Main(string[] args){if(args.Length>0&&args[0]=="--target"){Application.EnableVisualStyles();Application.Run(new Form{Text="Cursor Studio foreground test",Size=new Size(280,120),StartPosition=FormStartPosition.CenterScreen});return 0;}Application.EnableVisualStyles();string exe=Path.GetFullPath(args[0]),output=Path.GetFullPath(args[1]);Store.Root=Path.Combine(Path.GetDirectoryName(output),"data-"+Guid.NewGuid().ToString("N"));var result=new Dictionary<string,object>();var samples=new List<object>();Process worker=null,target=null;Snapshot original=Backup.Capture(false);int code=1;try{
  var settings=new Settings{Initialized=true,RulesEnabled=true,Language="en",CloseToTray=true};settings.Themes.Add(TestCursors.Create());var rule=new Rule{Name="Foreground test",Target=Application.ExecutablePath,Mode="Path",Pixels=32};settings.Rules.Add(rule);Store.Save(settings);
  worker=Launch(exe,"--startup"+DataArgs());Check(Convert.ToInt32(Status()["BackgroundPid"])==worker.Id,"Wrong worker");Check(UiPid()==0,"Startup opened a UI");samples.Add(Memory(worker));
  for(int cycle=0;cycle<3;cycle++){RuntimeIpc.Signal("Show.v1");int pid=0;for(int n=0;n<80&&(pid=UiPid())==0;n++)Thread.Sleep(50);Check(pid!=0,"UI did not open");using(var ui=Process.GetProcessById(pid)){for(int n=0;n<100;n++){ui.Refresh();if(ui.MainWindowHandle!=IntPtr.Zero)break;Thread.Sleep(50);}Check(ui.MainWindowHandle!=IntPtr.Zero,"Manager window did not load");RuntimeIpc.Signal("Show.v1");Thread.Sleep(200);Check(UiPid()==pid,"Duplicate UI process");}if(cycle==1){using(var crashed=Process.GetProcessById(pid))crashed.Kill();}else RuntimeIpc.Signal("UI.UserClose.v2");WaitUiGone(pid);Check(!worker.HasExited,"Closing UI killed worker");samples.Add(Memory(worker));}
  result["reopenAndSingleUi"]=true;result["uiCrashIsolation"]=true;
  target=Launch(Application.ExecutablePath,"--target",true);WaitRule(rule.Id);target.CloseMainWindow();Check(target.WaitForExit(5000),"Target did not close");WaitRule("");result["foregroundAfterUiExit"]=true;
  var a=Store.Load();var b=Store.Load();b.RulesEnabled=false;Store.Save(b);a.Language="zh-CN";Store.Save(a);Check(!Store.Load().RulesEnabled,"Stale UI overwrote background rules");for(int n=0;n<80;n++){var state=Status();if(!Convert.ToBoolean(state["RulesEnabled"])&&Convert.ToString(state["Language"])=="zh-CN")break;if(n==79)throw new Exception("Settings did not synchronize");Thread.Sleep(50);}result["settingsSynchronization"]=true;
  TimeSpan cpu=worker.TotalProcessorTime;Thread.Sleep(10000);worker.Refresh();result["idleCpuMs"]=(worker.TotalProcessorTime-cpu).TotalMilliseconds;samples.Add(Memory(worker));var final=Store.Load();final.CloseToTray=false;Store.Save(final);RuntimeIpc.Signal("Show.v1");int finalPid=0;for(int n=0;n<80&&(finalPid=UiPid())==0;n++)Thread.Sleep(50);Check(finalPid!=0,"Final UI did not open");Thread.Sleep(1000);RuntimeIpc.Signal("UI.UserClose.v2");WaitUiGone(finalPid);Check(worker.WaitForExit(5000),"Direct close did not exit worker");result["directExitBothProcesses"]=true;result["success"]=true;code=0;
 }catch(Exception e){result["success"]=false;result["error"]=e.ToString();}finally{RuntimeIpc.Signal("UI.Quit.v2");RuntimeIpc.Signal("Quit.v1");if(worker!=null){if(!worker.HasExited)worker.WaitForExit(5000);worker.Dispose();}if(target!=null){if(!target.HasExited)target.CloseMainWindow();target.Dispose();}Backup.Restore(original);result["samples"]=samples;File.WriteAllText(output,Store.Json.Serialize(result));}return code;}
}
