using System;
using System.IO;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
namespace CursorStudio {
// This process never constructs MainForm, previews, thumbnails or UI fonts.
public sealed class BackgroundHost:ApplicationContext {
 readonly Control dispatch=new Control();readonly NativeWindow menuOwner=new NativeWindow();readonly NotifyIcon tray;
 readonly BackgroundWatch watcher;readonly EventWaitHandle reload,status;
 readonly RegisteredWaitHandle reloadWait,statusWait;Settings settings;RuleEngine engine;Process editor;bool exiting;
 [DllImport("user32.dll")]static extern IntPtr CreatePopupMenu();
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern bool AppendMenuW(IntPtr menu,uint flags,UIntPtr id,string text);
 [DllImport("user32.dll")]static extern uint TrackPopupMenuEx(IntPtr menu,uint flags,int x,int y,IntPtr owner,IntPtr parameters);
 [DllImport("user32.dll")]static extern bool DestroyMenu(IntPtr menu);
 [DllImport("user32.dll")]static extern bool GetCursorPos(out Point point);
 [DllImport("user32.dll")]static extern bool SetForegroundWindow(IntPtr window);
 [DllImport("user32.dll")]static extern bool PostMessage(IntPtr window,uint msg,IntPtr w,IntPtr l);
 public BackgroundHost(Settings value,bool open){settings=value;engine=new RuleEngine(settings);IntPtr handle=dispatch.Handle;menuOwner.CreateHandle(new CreateParams{Caption="CursorStudio.Background",Style=unchecked((int)0x80000000)});reload=RuntimeIpc.Event("Reload.v2");status=RuntimeIpc.Event("Status.v2");reloadWait=ThreadPool.RegisterWaitForSingleObject(reload,delegate{Post(Reload);},null,Timeout.Infinite,false);statusWait=ThreadPool.RegisterWaitForSingleObject(status,delegate{Post(WriteStatus);},null,Timeout.Infinite,false);
  tray=new NotifyIcon{Icon=Skin.AppIcon,Text="Cursor Studio",Visible=true};tray.DoubleClick+=delegate{OpenEditor();};tray.MouseUp+=delegate(object sender,MouseEventArgs e){if(e.Button==MouseButtons.Right)ShowMenu();};watcher=new BackgroundWatch(dispatch,()=>Evaluate(false),OpenEditor,ExitBackground,Restore);Evaluate(true);if(open)dispatch.BeginInvoke((Action)OpenEditor);
 }
 void Post(Action action){if(exiting||dispatch.IsDisposed)return;try{dispatch.BeginInvoke((Action)(()=>{if(!exiting)action();}));}catch(InvalidOperationException){}}
 void Reload(){try{var fresh=Store.Load();engine.Restore();settings=fresh;L.Set(settings.Language);engine=new RuleEngine(settings);Evaluate(true);}catch(Exception e){Store.Log(e);}}
 void Evaluate(bool force){try{bool enabled=settings.RulesEnabled;if(enabled||engine.Current!=null)engine.Tick(force);if(enabled&&!settings.RulesEnabled){Store.Save(settings);RuntimeIpc.Signal("UI.Reload.v2");}}catch(Exception e){Store.Log(e);}}
 string EditorArguments(){return "--ui --data-dir \""+Store.Root+"\"";}
 void OpenEditor(){if(exiting)return;try{if(editor!=null&&!editor.HasExited){RuntimeIpc.Signal("UI.Show.v2");return;}if(editor!=null)editor.Dispose();editor=Process.Start(new ProcessStartInfo(Application.ExecutablePath,EditorArguments()){UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Normal});}catch(Exception e){Store.Log(e);tray.ShowBalloonTip(5000,"Cursor Studio",e.Message,ToolTipIcon.Error);}}
 void ToggleRules(){try{var fresh=Store.Load();fresh.RulesEnabled=!fresh.RulesEnabled;Store.Save(fresh);Reload();RuntimeIpc.Signal("UI.Reload.v2");}catch(Exception e){Store.Log(e);}}
 void Restore(){try{engine.Restore();Backup.Original();var fresh=Store.Load();fresh.ActiveId="";fresh.RulesEnabled=false;Store.Save(fresh);Reload();RuntimeIpc.Signal("UI.Reload.v2");OpenEditor();}catch(Exception e){Store.Log(e);tray.ShowBalloonTip(5000,"Cursor Studio",e.Message,ToolTipIcon.Error);}}
 void ShowMenu(){IntPtr menu=CreatePopupMenu();try{AppendMenuW(menu,0,new UIntPtr(1),L.T("打开管理器"));AppendMenuW(menu,settings.RulesEnabled?8u:0u,new UIntPtr(2),L.T("启用 / 暂停应用规则"));AppendMenuW(menu,0x800,UIntPtr.Zero,null);AppendMenuW(menu,0,new UIntPtr(3),L.T("退出并恢复桌面大小"));Point point;GetCursorPos(out point);SetForegroundWindow(menuOwner.Handle);uint command=TrackPopupMenuEx(menu,0x102,point.X,point.Y,menuOwner.Handle,IntPtr.Zero);PostMessage(menuOwner.Handle,0,IntPtr.Zero,IntPtr.Zero);if(command==1)OpenEditor();else if(command==2)ToggleRules();else if(command==3)ExitBackground();}finally{DestroyMenu(menu);}}
 void WriteStatus(){try{Store.Atomic(Path.Combine(Store.Root,"runtime-status.json"),Store.Json.Serialize(new{BackgroundPid=Process.GetCurrentProcess().Id,UiPid=editor!=null&&!editor.HasExited?editor.Id:0,RulesEnabled=settings.RulesEnabled,CurrentRule=engine.Current==null?"":engine.Current.Id,Language=settings.Language,Fallback=watcher.IsFallback,Error=engine.Error}));}catch(Exception e){Store.Log(e);}}
 public void ExitBackground(){if(exiting)return;exiting=true;RuntimeIpc.Signal("UI.Quit.v2");try{engine.Restore();}catch(Exception e){Store.Log(e);}tray.Visible=false;ExitThread();}
 protected override void Dispose(bool disposing){if(disposing){if(watcher!=null)watcher.Dispose();reloadWait.Unregister(null);statusWait.Unregister(null);reload.Dispose();status.Dispose();tray.Dispose();menuOwner.DestroyHandle();dispatch.Dispose();if(editor!=null)editor.Dispose();}base.Dispose(disposing);}
}
}
