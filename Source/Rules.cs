using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
namespace CursorStudio {
public sealed class RuleEngine {
 readonly Settings settings;string last="desktop";uint cachedPid;bool cachedEnabled;Rule cachedHit;public Rule Current;public string Error="";
 public RuleEngine(Settings s){settings=s;}
 public void Tick(bool force){uint pid;Native.GetWindowThreadProcessId(Native.GetForegroundWindow(),out pid);if(force||pid!=cachedPid||cachedEnabled!=settings.RulesEnabled){cachedPid=pid;cachedEnabled=settings.RulesEnabled;cachedHit=settings.RulesEnabled&&pid!=0?Native.Match(settings.Rules,pid):null;}ApplyHit(cachedHit,force);}
 public void ApplyHit(Rule hit,bool force){string next=hit==null?"desktop":hit.Id+":"+hit.Pixels;Current=hit;if(next==last&&!force)return;try{if(hit!=null){var theme=settings.Themes.FirstOrDefault(t=>t.Id==settings.ActiveId);if(theme!=null){var paths=Native.CurrentPaths();if(Native.Paths(theme).Any(p=>!String.Equals(paths[p.Key],p.Value,StringComparison.OrdinalIgnoreCase)))Native.Apply(theme);}}Native.SetSize(hit==null?Native.DesktopSize():hit.Pixels);last=next;Error="";}catch(Exception e){Store.Log(e);Error=e.Message;settings.RulesEnabled=false;Current=null;Restore();}}
 public void Restore(){Native.SetSize(Native.DesktopSize());last="desktop";Current=null;}
}
public static class Migration {
 public static bool Available {get{return File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),@".EndfieldCursorFix\settings.json"));}}
 public static void Run(Settings s){if(s.Migrated)return;string dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".EndfieldCursorFix");string cfg=Path.Combine(dir,"settings.json"),exe=Path.Combine(dir,"EndfieldCursorFix.exe");if(!File.Exists(cfg)||!File.Exists(exe))throw new Exception("没有找到旧终末地指针助手。");var data=Store.Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(cfg));bool enabled=data.ContainsKey("Enabled")&&Convert.ToBoolean(data["Enabled"]),startup=data.ContainsKey("AutoStart")&&Convert.ToBoolean(data["AutoStart"]);int pixels=data.ContainsKey("GamePixels")?Convert.ToInt32(data["GamePixels"]):32;
 if(!s.Rules.Any(r=>r.Mode=="Name"&&r.Target.Equals("Endfield",StringComparison.OrdinalIgnoreCase)))s.Rules.Add(new Rule{Name="终末地",Target="Endfield",Mode="Name",Pixels=Native.Snap(pixels)});
 s.RulesEnabled=enabled;s.AutoStart=startup;Store.Save(s);Store.Startup(startup);Directory.CreateDirectory(Path.Combine(Store.Root,"Backup"));File.Copy(cfg,Path.Combine(Store.Root,"Backup","endfield-settings.json"),true);
 RunOld(exe,"--quit");RunOld(exe,"--remove-startup");s.Migrated=true;Store.Save(s);}
 static void RunOld(string exe,string args){using(var p=Process.Start(new ProcessStartInfo(exe,args){UseShellExecute=false,CreateNoWindow=true})){if(!p.WaitForExit(8000)||p.ExitCode!=0)throw new Exception("旧助手未能完成迁移，请查看其日志；新规则已经保存。");}}
}
public sealed class RunningApp {public string Name,Path;public override string ToString(){return Name+"   ·   "+(Path==""?"路径不可读取，请明确选择名称模式":Path);}}
public sealed class RuleDialog:Form {
 readonly TextBox title,target;readonly ComboBox mode;readonly StepSlider size;readonly Label sizeText;public Rule Result;
 public RuleDialog(Rule original){Text=original==null?"添加应用规则":"编辑应用规则";Size=new Size(760,440);MinimumSize=Size;StartPosition=FormStartPosition.CenterParent;Font=new Font("Microsoft YaHei UI",10);var layout=new TableLayoutPanel{Dock=DockStyle.Fill,Padding=new Padding(20),ColumnCount=2,RowCount=7};layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,116));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));Controls.Add(layout);
 title=new TextBox{Dock=DockStyle.Fill,Text=original==null?"":original.Name};target=new TextBox{Dock=DockStyle.Fill,Text=original==null?"":original.Target};mode=new FluentComboBox{Dock=DockStyle.Fill,DropDownStyle=ComboBoxStyle.DropDownList};mode.Items.AddRange(new object[]{"完整 EXE 路径（推荐）","进程名（不含 .exe）"});mode.SelectedIndex=original!=null&&original.Mode=="Name"?1:0;Add(layout,"显示名称",title,0);Add(layout,"匹配方式",mode,1);Add(layout,"目标程序",target,2);
 var actions=new FlowLayoutPanel{Dock=DockStyle.Fill};var browse=new FluentButton{Text="选择 EXE",AutoSize=true};var running=new FluentButton{Text="从运行程序选取",AutoSize=true};actions.Controls.Add(browse);actions.Controls.Add(running);Add(layout,"添加方式",actions,3);
 size=new StepSlider{Minimum=1,Maximum=15,TickFrequency=1,LargeChange=1,Value=1,Dock=DockStyle.Fill};size.Value=(Native.Snap(original==null?32:original.Pixels)-32)/16+1;sizeText=new Label{Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft};size.ValueChanged+=delegate{sizeText.Text="第 "+size.Value+" 档 · "+(16+size.Value*16)+" px";};sizeText.Text="第 "+size.Value+" 档 · "+(16+size.Value*16)+" px";Add(layout,"应用内大小",size,4);Add(layout,"",sizeText,5);
 var bottom=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.RightToLeft};var ok=new FluentButton{Text="保存规则",AutoSize=true};bottom.Controls.Add(ok);var cancel=new FluentButton{Text="取消",DialogResult=DialogResult.Cancel,AutoSize=true};bottom.Controls.Add(cancel);Add(layout,"",bottom,6);AcceptButton=ok;CancelButton=cancel;for(int row=0;row<7;row++)layout.RowStyles.Add(new RowStyle(SizeType.Percent,row==4?22:13));
 browse.Click+=delegate{using(var f=new OpenFileDialog{Filter="应用程序|*.exe"})if(f.ShowDialog(this)==DialogResult.OK){target.Text=f.FileName;mode.SelectedIndex=0;if(String.IsNullOrWhiteSpace(title.Text))title.Text=Path.GetFileNameWithoutExtension(f.FileName);}};
 running.Click+=delegate{var items=new List<RunningApp>();foreach(var p in Process.GetProcesses())using(p){try{if(p.MainWindowHandle==IntPtr.Zero||String.IsNullOrWhiteSpace(p.MainWindowTitle))continue;items.Add(new RunningApp{Name=p.ProcessName,Path=Native.ProcessPath((uint)p.Id)});}catch{}}using(var f=new Form{Text="选择真实应用窗口所属程序",Width=850,Height=420,StartPosition=FormStartPosition.CenterParent}){var list=new ListBox{Dock=DockStyle.Fill,HorizontalScrollbar=true};foreach(var item in items.OrderBy(i=>i.Name))list.Items.Add(item);f.Controls.Add(list);var select=new FluentButton{Text="使用所选程序",Dock=DockStyle.Bottom,Height=40};f.Controls.Add(select);select.Click+=delegate{var item=list.SelectedItem as RunningApp;if(item==null)return;title.Text=item.Name;target.Text=item.Path==""?item.Name:item.Path;mode.SelectedIndex=item.Path==""?1:0;f.Close();};list.DoubleClick+=delegate{select.PerformClick();};Skin.Apply(f);f.ShowDialog(this);}};
 ok.Click+=delegate{if(String.IsNullOrWhiteSpace(title.Text)||String.IsNullOrWhiteSpace(target.Text)){MessageBox.Show("请填写名称并选择目标程序。");return;}if(mode.SelectedIndex==0&&(!Path.IsPathRooted(target.Text)||!File.Exists(target.Text))){MessageBox.Show("完整路径不存在，请重新选择 EXE，或明确改为进程名识别。");return;}Result=new Rule{Id=original==null?Guid.NewGuid().ToString("N"):original.Id,Name=title.Text.Trim(),Mode=mode.SelectedIndex==0?"Path":"Name",Target=mode.SelectedIndex==0?Path.GetFullPath(target.Text):Path.GetFileNameWithoutExtension(target.Text.Trim()),Pixels=16+size.Value*16,Enabled=original==null||original.Enabled};DialogResult=DialogResult.OK;};Skin.Apply(this);
 }
 static void Add(TableLayoutPanel p,string text,Control control,int row){p.Controls.Add(new Label{Text=text,Dock=DockStyle.Fill,TextAlign=ContentAlignment.MiddleLeft},0,row);p.Controls.Add(control,1,row);}
}
}
