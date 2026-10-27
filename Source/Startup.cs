using System;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Microsoft.Win32;
namespace CursorStudio {
public sealed class StartupState {public bool Enabled;public string Command="",Error="";}
public static class StartupRegistration {
 const string RunKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
 public static string StableExe {get{return Path.Combine(Store.Root,@"Bin\CursorStudio.exe");}}
 static string Command {get{return "\""+StableExe+"\" --startup";}}
 // A process started by a packaged host can inherit its private registry view.
 // The Task Scheduler service runs this short worker in the normal user context.
 // The temporary task has no trigger, never elevates, and is removed after use.
 public static StartupState Run(string operation){string report=Path.Combine(Store.Root,"startup-"+Guid.NewGuid().ToString("N")+".json"),name="CursorStudio-Registration-"+Guid.NewGuid().ToString("N");Directory.CreateDirectory(Store.Root);dynamic service=null,folder=null;var objects=new List<object>();bool registered=false;
  try{service=Activator.CreateInstance(Type.GetTypeFromProgID("Schedule.Service"));objects.Add(service);service.Connect();folder=service.GetFolder("\\");objects.Add(folder);dynamic task=service.NewTask(0);objects.Add(task);task.RegistrationInfo.Description="Cursor Studio 启动项注册（临时任务，无触发器）";task.Settings.ExecutionTimeLimit="PT1M";task.Settings.DisallowStartIfOnBatteries=false;task.Settings.StopIfGoingOnBatteries=false;string sid=System.Security.Principal.WindowsIdentity.GetCurrent().User.Value;task.Principal.UserId=sid;task.Principal.LogonType=3;task.Principal.RunLevel=0;dynamic action=task.Actions.Create(0);objects.Add(action);action.Path=System.Windows.Forms.Application.ExecutablePath;action.Arguments="--startup-state "+operation+" \""+report+"\"";dynamic saved=folder.RegisterTaskDefinition(name,task,6,sid,null,3);objects.Add(saved);registered=true;dynamic running=saved.Run(null);objects.Add(running);
   for(int i=0;i<200;i++){if(File.Exists(report)){var result=Store.Json.Deserialize<StartupState>(File.ReadAllText(report));if(result==null)throw new Exception("启动项检查未返回结果。");if(result.Error!="")throw new Exception(result.Error);return result;}Thread.Sleep(50);}throw new Exception("Windows 启动项注册超时，请检查任务计划程序服务。");
  }finally{if(registered)try{folder.DeleteTask(name,0);}catch{}for(int i=objects.Count-1;i>=0;i--)if(Marshal.IsComObject(objects[i]))try{Marshal.ReleaseComObject(objects[i]);}catch{}if(File.Exists(report))File.Delete(report);}
 }
 public static bool IsEnabled(){try{return Run("status").Enabled;}catch(Exception e){Store.Log(e);return false;}}
 public static int Worker(string operation,string report){var state=new StartupState();try{if(operation!="status"&&operation!="enable"&&operation!="disable")throw new Exception("未知启动项操作。");using(var key=Registry.CurrentUser.CreateSubKey(RunKey)){if(operation=="enable"){if(!File.Exists(StableExe))throw new FileNotFoundException("启动程序不存在。",StableExe);key.SetValue("CursorStudio",Command,RegistryValueKind.String);foreach(string view in new[]{"Run","Run32"})using(var approved=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\"+view,true))if(approved!=null)approved.DeleteValue("CursorStudio",false);}else if(operation=="disable")key.DeleteValue("CursorStudio",false);state.Command=Convert.ToString(key.GetValue("CursorStudio",""));}
   bool disabled=false;using(var key=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run")){var value=key==null?null:key.GetValue("CursorStudio") as byte[];disabled=value!=null&&value.Length>0&&(value[0]==3||value[0]==7);}state.Enabled=state.Command==Command&&File.Exists(StableExe)&&!disabled;
  }catch(Exception e){state.Error=e.Message;}Store.Atomic(report,Store.Json.Serialize(state));return state.Error==""?0:1;
 }
}
}
