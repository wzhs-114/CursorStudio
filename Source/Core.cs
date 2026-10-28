using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Web.Script.Serialization;
using Microsoft.Win32;

namespace CursorStudio {
public sealed class Theme {
 public string Id=Guid.NewGuid().ToString("N"),Name=L.T("新主题");
 public int Pixels=32;
 public Dictionary<string,string> Files=new Dictionary<string,string>();
 public override string ToString(){return Name;}
}
public sealed class Rule {
 public string Id=Guid.NewGuid().ToString("N"),Name=L.T("应用"),Target="",Mode="Path";
 public bool Enabled=true;public int Pixels=32;
 public override string ToString(){return Name;}
}
public sealed class Settings {
 public List<Theme> Themes=new List<Theme>(); public List<Rule> Rules=new List<Rule>();
 public string ActiveId="";public bool RulesEnabled=false,AutoStart=false,Migrated=false,Initialized=false;
 public string Appearance="Performance";
 public bool CloseToTray=true;
 public string Language="en";
}
public sealed class RegValue {public string Name;public int Kind;public object Value;}
public sealed class Snapshot {
 public List<RegValue> Cursors=new List<RegValue>(),Access=new List<RegValue>();
 public List<RegValue> Schemes=new List<RegValue>();
 public Dictionary<string,string> Assets=new Dictionary<string,string>();
}
public static class Store {
 // A profile-root data directory avoids packaged-host AppData filesystem redirection.
 public static string Root=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".CursorStudio");
 public static string Config {get{return Path.Combine(Root,"settings.json");}}
 public static string Library {get{return Path.Combine(Root,"Library");}}
 public static readonly JavaScriptSerializer Json=new JavaScriptSerializer{MaxJsonLength=16000000};
 public static Settings Load(){Directory.CreateDirectory(Library);if(!File.Exists(Config))return new Settings();var s=Json.Deserialize<Settings>(File.ReadAllText(Config));if(s==null)throw new Exception(L.T("主题库配置损坏，未覆盖原文件。请查看备份 settings.json.bak。"));return s;}
 public static void Save(Settings s){Atomic(Config,Json.Serialize(s));}
 public static void Atomic(string path,string text){Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path+".tmp",text,System.Text.Encoding.UTF8);if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");else File.Move(path+".tmp",path);}
 public static void Log(Exception e){try{Directory.CreateDirectory(Root);File.AppendAllText(Path.Combine(Root,"error.log"),DateTime.Now.ToString("s")+" "+e+Environment.NewLine);}catch{}}
 public static void Startup(bool on){if(on){Directory.CreateDirectory(Path.GetDirectoryName(StartupRegistration.StableExe));if(!String.Equals(StartupRegistration.StableExe,System.Windows.Forms.Application.ExecutablePath,StringComparison.OrdinalIgnoreCase))File.Copy(System.Windows.Forms.Application.ExecutablePath,StartupRegistration.StableExe,true);}var result=StartupRegistration.Run(on?"enable":"disable");if(result.Enabled!=on)throw new Exception(L.T("Windows 启动项状态与请求不一致。"));}
}
public static class Native {
 public static string[] Labels {get{return RoleLabelKeys.Select(L.T).ToArray();}}
 public static readonly string[] Roles={"Arrow","Help","AppStarting","Wait","Crosshair","IBeam","NWPen","No","SizeNS","SizeWE","SizeNWSE","SizeNESW","SizeAll","UpArrow","Hand","Pin","Person"};
 public static readonly string[] RoleLabelKeys={"正常选择","帮助选择","后台运行","忙","精确选择","文本选择","手写","不可用","垂直调整大小","水平调整大小","沿对角线调整大小1","沿对角线调整大小2","移动","候选","链接选择","位置选择","个人选择"};
 public static readonly int[] Ids={32512,32651,32650,32514,32515,32513,32631,32648,32645,32644,32642,32643,32646,32516,32649,32671,32672};
 public const string CursorKey=@"Control Panel\Cursors",AccessKey=@"Software\Microsoft\Accessibility";
 [DllImport("user32.dll",SetLastError=true)]public static extern bool SystemParametersInfoW(uint action,uint param,IntPtr value,uint flags);
 [DllImport("user32.dll",CharSet=CharSet.Unicode,SetLastError=true)]public static extern IntPtr LoadImageW(IntPtr inst,string path,uint type,int width,int height,uint flags);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]public static extern IntPtr LoadCursorFromFileW(string path);
 [DllImport("user32.dll")]public static extern IntPtr LoadCursorW(IntPtr instance,IntPtr id);
 [DllImport("user32.dll")]public static extern bool DestroyCursor(IntPtr cursor);
 [DllImport("user32.dll")]public static extern bool DrawIconEx(IntPtr dc,int x,int y,IntPtr icon,int w,int h,uint step,IntPtr brush,uint flags);
 [DllImport("user32.dll")]public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]public static extern bool IsWindowVisible(IntPtr window);
 [DllImport("user32.dll")]public static extern bool ShowWindow(IntPtr window,int command);
 [DllImport("user32.dll")]public static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint pid);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]static extern bool QueryFullProcessImageName(IntPtr p,int flags,System.Text.StringBuilder path,ref int count);
 [DllImport("kernel32.dll")]static extern IntPtr OpenProcess(uint access,bool inherit,uint pid);
 [DllImport("kernel32.dll")]static extern bool CloseHandle(IntPtr p);
 public static int DesktopSize(){using(var k=Registry.CurrentUser.OpenSubKey(CursorKey))return Math.Max(32,Math.Min(256,Convert.ToInt32(k.GetValue("CursorBaseSize",32))));}
 public static int Snap(int p){return Math.Max(32,Math.Min(256,32+(int)Math.Round((p-32)/16.0)*16));}
 public static Dictionary<string,string> CurrentPaths(){var r=new Dictionary<string,string>();using(var k=Registry.CurrentUser.OpenSubKey(CursorKey))foreach(string role in Roles)r[role]=Environment.ExpandEnvironmentVariables(Convert.ToString(k.GetValue(role,"",RegistryValueOptions.DoNotExpandEnvironmentNames)));return r;}
 public static string ProcessPath(uint pid){IntPtr h=OpenProcess(0x1000,false,pid);if(h==IntPtr.Zero)return "";try{int n=32768;var b=new System.Text.StringBuilder(n);return QueryFullProcessImageName(h,0,b,ref n)?b.ToString():"";}finally{CloseHandle(h);}}
 public static Rule Match(IEnumerable<Rule> rules,uint pid){string path=ProcessPath(pid),name=Path.GetFileNameWithoutExtension(path);if(name=="")try{using(var p=Process.GetProcessById((int)pid))name=p.ProcessName;}catch{}return MatchTarget(rules,path,name);}
 public static Rule MatchTarget(IEnumerable<Rule> rules,string path,string name){return rules.Where(r=>r.Enabled).OrderBy(r=>r.Mode=="Path"?0:1).FirstOrDefault(r=>r.Mode=="Path"?path!=""&&String.Equals(r.Target,path,StringComparison.OrdinalIgnoreCase):String.Equals(Path.GetFileNameWithoutExtension(r.Target),name,StringComparison.OrdinalIgnoreCase));}
 public static void SetSize(int size){if(!SystemParametersInfoW(0x2029,0,new IntPtr(size),0))throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(),L.T("Windows 未能应用实时指针大小。"));}
 public static void Reload(){if(!SystemParametersInfoW(0x57,0,IntPtr.Zero,2))throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error(),L.T("Windows 未能重载指针方案。"));}
 public static bool Valid(string path){IntPtr h=LoadCursorFromFileW(path);if(h==IntPtr.Zero)return false;DestroyCursor(h);return true;}
 public static string DefaultPath(string role){string file="";switch(role){case "Arrow":file="aero_arrow.cur";break;case "Help":file="aero_helpsel.cur";break;case "AppStarting":file="aero_working.ani";break;case "Wait":file="aero_busy.ani";break;case "Crosshair":file="cross_r.cur";break;case "IBeam":file="beam_r.cur";break;case "NWPen":file="aero_pen.cur";break;case "No":file="aero_unavail.cur";break;case "SizeNS":file="aero_ns.cur";break;case "SizeWE":file="aero_ew.cur";break;case "SizeNWSE":file="aero_nwse.cur";break;case "SizeNESW":file="aero_nesw.cur";break;case "SizeAll":file="aero_move.cur";break;case "UpArrow":file="aero_up.cur";break;case "Hand":file="aero_link.cur";break;case "Pin":file="aero_pin.cur";break;case "Person":file="aero_person.cur";break;}string p=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"Cursors",file);return File.Exists(p)?p:"";}
 public static Dictionary<string,string> Paths(Theme theme){var paths=new Dictionary<string,string>();foreach(string role in Roles){string f;paths[role]=theme.Files.TryGetValue(role,out f)&&!String.IsNullOrEmpty(f)?Path.Combine(Store.Library,theme.Id,f):DefaultPath(role);}return paths;}
 public static void PersistSize(int pixels){using(var k=Registry.CurrentUser.CreateSubKey(CursorKey))k.SetValue("CursorBaseSize",pixels,RegistryValueKind.DWord);using(var k=Registry.CurrentUser.CreateSubKey(AccessKey))k.SetValue("CursorSize",1+(pixels-32)/16,RegistryValueKind.DWord);}
 public static void WritePaths(Dictionary<string,string> paths,string name){using(var k=Registry.CurrentUser.CreateSubKey(CursorKey)){foreach(var p in paths)k.SetValue(p.Key,p.Value,RegistryValueKind.String);k.SetValue("",name,RegistryValueKind.String);}using(var k=Registry.CurrentUser.CreateSubKey(CursorKey+@"\Schemes"))k.SetValue(name,String.Join(",",Roles.Select(r=>paths[r])),RegistryValueKind.String);}
 public static void Apply(Theme t){foreach(var p in Paths(t))if(p.Value!=""&&(!File.Exists(p.Value)||!Valid(p.Value)))throw new Exception(L.T("指针文件不可用：")+p.Value);Backup.Ensure();var old=Backup.Capture(false);try{PersistSize(t.Pixels);SetSize(t.Pixels);WritePaths(Paths(t),"CursorStudio · "+t.Name);Reload();SetSize(t.Pixels);}catch{Backup.Restore(old);throw;}}
}
public static class Backup {
 static string FilePath {get{return Path.Combine(Store.Root,"Backup","original.json");}}
 static List<RegValue> Read(string path){var list=new List<RegValue>();using(var k=Registry.CurrentUser.OpenSubKey(path)){if(k!=null)foreach(string n in k.GetValueNames())list.Add(new RegValue{Name=n,Kind=(int)k.GetValueKind(n),Value=k.GetValue(n,null,RegistryValueOptions.DoNotExpandEnvironmentNames)});}return list;}
 public static Snapshot Capture(bool assets){var s=new Snapshot{Cursors=Read(Native.CursorKey),Access=Read(Native.AccessKey),Schemes=Read(Native.CursorKey+@"\Schemes")};if(assets){string dir=Path.Combine(Store.Root,"Backup","Files");Directory.CreateDirectory(dir);foreach(var p in Native.CurrentPaths()){if(File.Exists(p.Value)){string to=Path.Combine(dir,p.Key+Path.GetExtension(p.Value));File.Copy(p.Value,to,true);s.Assets[p.Key]=to;}}}return s;}
 public static void Ensure(){if(!File.Exists(FilePath))Store.Atomic(FilePath,Store.Json.Serialize(Capture(true)));}
 static void Write(string path,List<RegValue> values,bool access){using(var k=Registry.CurrentUser.CreateSubKey(path)){IEnumerable<string> keys=access?new[]{"CursorSize","CursorType","CursorColor"}:Native.Roles.Concat(new[]{"","CursorBaseSize","Scheme Source"});foreach(string n in keys){var v=values.FirstOrDefault(x=>x.Name==n);if(v==null){k.DeleteValue(n,false);continue;}var kind=(RegistryValueKind)v.Kind;object o=v.Value;if(kind==RegistryValueKind.DWord)o=Convert.ToInt32(o);else if(kind==RegistryValueKind.QWord)o=Convert.ToInt64(o);else o=Convert.ToString(o);k.SetValue(n,o,kind);}}}
 public static void Restore(Snapshot s){Write(Native.AccessKey,s.Access,true);Write(Native.CursorKey,s.Cursors,false);if(s.Schemes!=null)using(var schemes=Registry.CurrentUser.CreateSubKey(Native.CursorKey+@"\Schemes"))foreach(var v in s.Schemes)schemes.SetValue(v.Name,Convert.ToString(v.Value),(RegistryValueKind)v.Kind);using(var k=Registry.CurrentUser.CreateSubKey(Native.CursorKey))foreach(var a in s.Assets){string orig=Convert.ToString(s.Cursors.First(v=>v.Name==a.Key).Value);if(!File.Exists(Environment.ExpandEnvironmentVariables(orig)))k.SetValue(a.Key,a.Value,RegistryValueKind.String);}Native.Reload();Native.SetSize(Native.DesktopSize());}
 public static void Original(){if(!File.Exists(FilePath))throw new Exception(L.T("尚未应用过主题，无需恢复。"));Restore(Store.Json.Deserialize<Snapshot>(File.ReadAllText(FilePath)));}
}
}
