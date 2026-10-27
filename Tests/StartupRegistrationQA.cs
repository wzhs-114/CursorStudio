using System;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using CursorStudio;
class StartupRegistrationQA {
 [STAThread]static int Main(string[] args){if(args.Length==3&&args[0]=="--startup-state"){if(args[1]=="test-disabled"){using(var k=Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run"))k.SetValue("CursorStudio",new byte[]{3,0,0,0,0,0,0,0,0,0,0,0},RegistryValueKind.Binary);return StartupRegistration.Worker("status",args[2]);}return StartupRegistration.Worker(args[1],args[2]);}bool before=StartupRegistration.Run("status").Enabled;try{if(!before)throw new Exception("Expected enabled baseline");if(StartupRegistration.Run("disable").Enabled||StartupRegistration.Run("status").Enabled)throw new Exception("Disable failed");if(!StartupRegistration.Run("enable").Enabled||!StartupRegistration.Run("status").Enabled)throw new Exception("Enable failed");if(StartupRegistration.Run("test-disabled").Enabled||StartupRegistration.Run("status").Enabled)throw new Exception("Windows disabled state ignored");if(!StartupRegistration.Run("enable").Enabled)throw new Exception("Explicit re-enable failed");File.WriteAllText(args[0],"PASS: registration queried outside host; disable/status/enable/status match Windows state; Windows-disabled startup is detected and explicit enable restores it; temporary registration tasks removed.");return 0;}finally{StartupRegistration.Run(before?"enable":"disable");}}
}
