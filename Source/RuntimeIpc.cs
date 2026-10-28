using System;
using System.IO;
using System.Threading;
using System.Security.Cryptography;
using System.Text;
namespace CursorStudio {
public static class RuntimeIpc {
 public static string Name(string operation){string root=Path.GetFullPath(Store.Root).TrimEnd(Path.DirectorySeparatorChar).ToLowerInvariant();string normal=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".CursorStudio").ToLowerInvariant();string scope="";if(root!=normal)using(var hash=SHA256.Create())scope=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(root))).Replace("-","").Substring(0,20)+".";return @"Local\CursorStudio."+scope+operation;}
 public static EventWaitHandle Event(string operation){return new EventWaitHandle(false,EventResetMode.AutoReset,Name(operation));}
 public static void Signal(string operation){using(var ev=Event(operation))ev.Set();}
 public static void NotifySaved(){EventWaitHandle ev;if(EventWaitHandle.TryOpenExisting(Name("Reload.v2"),out ev))using(ev)ev.Set();}
 public static bool BackgroundAlive(){Mutex mutex;if(!Mutex.TryOpenExisting(Name("Instance.v1"),out mutex))return false;using(mutex)return true;}
}
}
