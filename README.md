# Cursor Studio

**Public version: 1.0.0** · [English](#english) · [简体中文](#chinese)

<img src="Source/AppIcon.svg" width="72" alt="Cursor Studio pointer icon" />

## English

Cursor Studio **1.0.0** is a free, MIT-licensed **Windows 11 cursor manager** for custom mouse pointers, CUR/ANI cursor themes and per-application cursor scaling. Import complete CUR/ANI themes, save a desktop size for each theme, and automatically apply a different size while a matching game or application is in the foreground. Switching away restores the theme's desktop size.

### Highlights

- **Automatic foreground sizing:** match an EXE by full path or process name. Full-path matches take priority. Rules resize the current theme; they do not assign a separate theme to each application.
- **Multiple themes with remembered sizes:** import folders or ZIP archives, switch themes quickly, and adjust sizes in 15 steps from 32 to 256 pixels.
- **Designed for lightweight background use:** hiding or minimizing releases the UI, preview images, thumbnails and shared UI font cache. The existing single collection runs after the UI has been released; reopening recreates the font cache. This trades some reopen work for more predictable cleanup without adding a periodic background task. Reopening prepares the complete first frame before showing it. Foreground events are used normally, with a one-second fallback only if the event hook fails. Memory use varies with themes, usage and Windows memory management.
- **Native and transparent:** built with C#, WinForms and Windows APIs, without WebView2, third-party runtime libraries, DLL injection or a cursor overlay.
- **Free and recoverable:** MIT source code, local settings, an original-cursor backup and a restore entry point are included.

### Download and use

Download the Windows ZIP from [Releases](https://github.com/wzhs-114/CursorStudio/releases), extract the entire archive, and run `CursorStudio.exe`. Administrator privileges are not required. The application defaults to English. Use the Interface language selector at the top right to switch between English and Simplified Chinese; your choice is saved.

Use “导入当前系统指针” (Import current system cursors) to copy the configured cursor files into an independent theme, including ANI animations and the desktop size, without switching the current scheme. This captures Windows system cursor files, not game-rendered pointers.

The public package starts with an empty theme library. Import CUR/ANI files that you have permission to use; no character artwork or third-party themes are bundled. Importing a theme does not execute its EXE, BAT or installation scripts.

Select the actual game EXE in the application rules page, rather than its launcher. You can also pick a running application. Update full-path rules if the game moves. Optional sign-in startup uses a stable executable copy and the real user's Windows startup entry. Short-lived, non-elevated scheduled tasks handle startup registration/query and are removed afterward.

In the application rules page, choose whether closing the window sends it to the tray or exits fully. The default is to keep running in the tray, independently of whether rules are enabled. Double-click the tray icon to reopen it. Direct exit stops application rules and restores the desktop size; sign-in startup remains a separate preference.

### Data and recovery

Data stays locally in `%USERPROFILE%\.CursorStudio`: `Library` stores imported themes, `settings.json` stores preferences and rules, `Backup` preserves the original cursor scheme, and `Bin` stores the stable startup executable. The application does not upload cursor files, settings or usage records.

Use the in-app restore action or the included `恢复原指针.bat` (restore original cursors) script. Before removing the application completely, restore your cursors, disable sign-in startup and exit it.

### Build and verify

On Windows, run `./Source/Build.ps1`, then `./Tools/Package.ps1`. The build uses the Windows .NET Framework 4.x C# compiler and needs no NuGet packages. Run `./Tests/Test.ps1` for an isolated self-test. The optional `-Integration` test temporarily applies generated geometric cursors and restores the previous state afterward. Packaging uses an explicit allowlist to keep local themes and personal test data out of public ZIPs.

### Scope and licensing

Cursor Studio controls Windows system cursors. Games that draw their own pointers, crosshairs or actively replace system settings may not be supported. Adjusting size directly in Windows Settings may still overwrite a custom theme; use this application's slider or reapply action. Mixed-DPI displays and individual games may need further testing. No universal game compatibility or comparative memory-performance claim is made.

Source code, the original geometric app icon and generated test assets are covered by the [MIT License](LICENSE). Imported third-party themes retain their own licenses.

### Contributions and acknowledgements

**[wzhs-114](https://github.com/wzhs-114)** is the project author and maintainer, responsible for requirements, design decisions, real-world feedback and acceptance. **Codex (OpenAI)** provided AI-assisted implementation, debugging, testing, packaging and documentation. Maintenance and publication decisions remain with the author. See [Contributors and acknowledgements](CONTRIBUTORS.md).

---

<a id="chinese"></a>

## 简体中文

Windows 11 鼠标指针主题与应用大小管理器。管理多套 CUR/ANI 指针，为主题记住桌面大小，并根据前台游戏或软件自动调整大小；切出后恢复桌面预设。

免费、MIT 开源。使用 C#、WinForms 和 Windows 原生接口，不需要 WebView2 或额外安装 .NET 运行时。

### 为什么选择 Cursor Studio

- **进入应用自动缩放，切出自动恢复**：按前台 EXE 匹配规则，游戏大小和桌面大小分别设置。
- **多主题，各记各的大小**：整套导入 CUR/ANI，快速切换，无需每次重新调整。
- **为轻量后台设计**：收起后释放界面和预览资源，正常使用前台窗口事件检测；没有常驻网页界面或游戏注入。
- **免费且可自行检查**：MIT 开源，无第三方运行库依赖，提供构建和恢复说明。

以上为本项目已有能力，不代表对所有游戏兼容，也不是未经实测的竞品性能排名。

### 功能

- 从文件夹或 ZIP 整套导入，核对 Windows 的 17 个指针角色；支持重命名、删除与缺少角色时使用系统默认。
- 每套主题记住自己的大小；15 档滑条覆盖 32–256 像素，调整时保留自定义主题。
- 支持 ANI 动画预览、角色切换及浅色、深色、棋盘格预览背景。
- 按 EXE 完整路径或进程名设置应用规则；完整路径匹配优先。进入匹配应用时调整大小，切出或退出时恢复桌面预设。
- 规则修改的是当前主题的大小，不是给每个游戏另绑定一套主题。
- 正常后台使用前台窗口事件；仅事件钩子注册失败时回退到 1 秒检测。游戏没有处于前台时不会持续改写大小。
- 可选登录启动和托盘操作；自动备份首次应用前的指针方案，提供恢复入口。
- Win11 风格深浅色界面和性能／画面两种外观预设。

### 下载与使用

从 [Releases](https://github.com/wzhs-114/CursorStudio/releases) 下载 Windows ZIP，完整解压后运行 `CursorStudio.exe`，无需管理员权限。

点击“导入当前系统指针”，可将当前系统方案的指针文件（含 ANI 动画）和桌面大小保存为独立主题，导入时不切换当前方案；游戏自行绘制的指针不属于系统方案。

公共版不内置人物或其他第三方主题。首次打开是空主题库，请选择自己有权使用的 CUR/ANI 文件夹或 ZIP 导入。导入时不执行主题包中的 EXE、BAT 或安装脚本。

在“应用规则”页选择真正的游戏 EXE，而不是启动器；也可以从运行中的应用选取。游戏移动目录后请重新选择路径，或明确使用进程名匹配。

“登录 Windows 后启动到托盘”注册真实用户启动项，并读取系统实际启用状态；可在任务管理器的启动应用里找到 CursorStudio。为避免从打包宿主启动时落入隔离注册表，注册／查询会使用无触发器的临时任务，完成后移除，不留下常驻计划任务，不请求管理员权限。

在“应用规则”页可选择关闭窗口后收进托盘或直接退出，默认收进托盘，独立于应用规则开关。双击托盘图标重新打开；直接退出会停止应用规则并恢复桌面大小。登录启动是独立设置。

### 后台内存与界面恢复

收起或最小化会释放控件树、预览资源、主题缩略图和界面字体缓存；释放后执行原有的一次内存清理，再次打开时重建字体缓存。这会增加少量重建工作，不增加周期性后台任务。再次打开时先完成布局、样式与可见首帧准备，再完整呈现；临时绘制合成在首帧完成后撤销，不保留整窗截图或额外动画计时器。

后台占用随主题、打开历史、Windows 内存回收和测量指标变化。某次任务管理器中的低工作集读数不等于固定的总内存需求；本项目不通过定时清空工作集来修饰读数。

### 恢复与本地数据

- 数据目录：`%USERPROFILE%\.CursorStudio`
- `Library`：导入主题的独立副本；删除原始下载文件不会影响它们。
- `settings.json`：主题大小、应用规则和外观偏好。
- `Backup`：首次应用前的方案及必要文件备份。
- `Bin`：启用登录启动时使用的稳定程序位置。
- `error.log`：本地错误日志。程序不上传配置、指针文件或使用记录。

可在窗口中点击“恢复管理器使用前”，或双击分发包中的 `恢复原指针.bat`。彻底移除前请先恢复指针、关闭登录启动并退出程序。

### 构建

需要 Windows 11 和 Windows 自带的 .NET Framework 4.x C# 编译器；无第三方运行库或 NuGet 依赖。

```powershell
.\Source\Build.ps1
.\Tools\Package.ps1
```

程序输出到 `dist\CursorStudio\CursorStudio.exe`；公开分发 ZIP 由固定文件清单生成，不会把现有本地主题或测试数据打包进去。

应用图标为自行绘制的通用几何箭头，矢量源文件为 `Source\AppIcon.svg`。修改后可运行 `.\Tools\GenerateIcon.ps1` 生成多尺寸 ICO；未复制 Windows 系统光标资源。

### 验证

```powershell
.\Tests\Test.ps1
# 会临时应用测试指针，结束后恢复原状态：
.\Tests\Test.ps1 -Integration
```

测试素材由程序绘制简单几何图形，不依赖任何人物主题。集成检查覆盖真实系统指针与动画、多个大小、规则匹配、备份恢复及隐藏／最小化后的界面生命周期。

### 已知范围

- 仅控制 Windows 系统指针。游戏自行绘制的光标、准星或主动覆盖系统设置的行为不保证能控制。
- 不向游戏注入 DLL，不修改游戏文件，没有独立光标叠加渲染层；这也限制了可覆盖的游戏类型，不构成所有游戏的兼容性承诺。
- Windows 设置页直接调大小仍可能覆盖自定义主题，建议使用本程序的滑条或“重新应用”。
- 界面默认英文，可在右上角“Interface language / 界面语言”切换简体中文并自动保存；不同显示器混合 DPI、不同 Windows 更新和特殊游戏环境需要更多反馈。

### 许可证与素材

源码、自绘图标及测试几何素材使用 [MIT License](LICENSE)。用户导入的第三方 CUR/ANI 主题仍适用其原作者的授权，本项目不替这些素材授予再分发许可。

本项目由个人实际使用需求驱动，使用 AI 辅助实现和调试。欢迎通过 Issues 提交复现步骤；请不要上传私人配置、令牌或未获许可的素材。

### 贡献与致谢

**[wzhs-114](https://github.com/wzhs-114)**：项目作者与维护者，提出需求、决定设计，并进行实际使用反馈与验收。

**Codex（OpenAI）**：AI 开发辅助，参与代码实现、问题排查、测试、打包和文档编写。项目维护及发布决定由作者负责。详见 [贡献与致谢](CONTRIBUTORS.md)。
