using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
namespace CursorStudio {
public static class Skin {
 public static Color Back,Card,Ink,Muted,Line,Hover,Accent=Color.FromArgb(0,105,220);public static bool Dark,Visuals,Animate,SystemAnimations=true;
 static Icon icon;
 static readonly System.Collections.Generic.Dictionary<string,Font> fonts=new System.Collections.Generic.Dictionary<string,Font>();
 public static Font TextFont(float size,bool bold){string key=size+":"+bold;Font f;if(!fonts.TryGetValue(key,out f)){f=new Font("Microsoft YaHei UI",size,bold?FontStyle.Bold:FontStyle.Regular);fonts[key]=f;}return f;}
 public static void ReleaseUiFonts(){foreach(var font in fonts.Values)font.Dispose();fonts.Clear();}
 public static Icon AppIcon {get{if(icon==null)using(var stream=typeof(Skin).Assembly.GetManifestResourceStream("CursorStudio.App.ico"))icon=stream==null?Icon.ExtractAssociatedIcon(Application.ExecutablePath):new Icon(stream);return icon;}}
 [DllImport("dwmapi.dll")]static extern int DwmSetWindowAttribute(IntPtr hwnd,int attr,ref int value,int size);
 [StructLayout(LayoutKind.Sequential)]struct Margins {public int Left,Right,Top,Bottom;}
 [DllImport("dwmapi.dll")]static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd,ref Margins margins);
 [DllImport("user32.dll",EntryPoint="SystemParametersInfoW")]static extern bool GetAnimation(uint action,uint parameter,out int value,uint flags);
 public static GraphicsPath Round(RectangleF r,float radius){var p=new GraphicsPath();float d=Math.Min(radius*2,Math.Min(r.Width,r.Height));if(d<=0){p.AddRectangle(r);return p;}p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
 public static Color Blend(Color a,Color b,float amount){return Color.FromArgb((int)(a.R+(b.R-a.R)*amount),(int)(a.G+(b.G-a.G)*amount),(int)(a.B+(b.B-a.B)*amount));}
 public static Color Selected {get{return Blend(Card,Accent,Dark?.22f:.10f);}}
 public static void Read(){using(var k=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))Dark=k!=null&&Convert.ToInt32(k.GetValue("AppsUseLightTheme",1))==0;using(var k=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\DWM")){object value=k==null?null:k.GetValue("ColorizationColor");if(value!=null){int argb=unchecked((int)Convert.ToInt64(value));Accent=Color.FromArgb((argb>>16)&255,(argb>>8)&255,argb&255);}}Back=Dark?Color.FromArgb(32,32,32):Color.FromArgb(243,243,243);Card=Dark?Color.FromArgb(43,43,43):Color.FromArgb(252,252,252);Ink=Dark?Color.FromArgb(244,244,244):Color.FromArgb(27,27,27);Muted=Dark?Color.FromArgb(175,175,175):Color.FromArgb(100,100,100);Line=Dark?Color.FromArgb(64,64,64):Color.FromArgb(224,224,224);Hover=Dark?Color.FromArgb(57,57,57):Color.FromArgb(239,239,239);int animations;Animate=Visuals&&GetAnimation(0x1042,0,out animations,0)&&animations!=0;}
 public static void Apply(Control root){Read();int animations;SystemAnimations=GetAnimation(0x1042,0,out animations,0)&&animations!=0;Color background=Back;var f=root as Form;if(f!=null){f.Icon=AppIcon;int dark=Dark?1:0;try{DwmSetWindowAttribute(f.Handle,20,ref dark,4);int corners=2;DwmSetWindowAttribute(f.Handle,33,ref corners,4);bool transparency=true;using(var k=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"))transparency=k==null||Convert.ToInt32(k.GetValue("EnableTransparency",1))!=0;int backdrop=Visuals&&transparency&&!SystemInformation.HighContrast?2:1;bool material=DwmSetWindowAttribute(f.Handle,38,ref backdrop,4)==0&&backdrop==2;var margins=new Margins();DwmExtendFrameIntoClientArea(f.Handle,ref margins);if(material)background=Blend(Back,Accent,.035f);}catch{}}Paint(root,background);}
 static void Paint(Control c,Color parent){Color color=c is RoundPanel?Card:parent;if(c is TextBox||c is ComboBox||c is ListBox||c is DataGridView)color=Card;c.BackColor=color;c.ForeColor=Ink;if(c is Label&&((Label)c).Tag as string=="muted")c.ForeColor=Muted;
  var b=c as Button;if(b!=null){b.FlatStyle=FlatStyle.Flat;b.FlatAppearance.BorderSize=0;b.BackColor=Card;b.Height=Math.Max(34,b.Height);}
  var cb=c as ComboBox;if(cb!=null){cb.FlatStyle=FlatStyle.Flat;cb.ItemHeight=26;if(cb.DrawMode!=DrawMode.OwnerDrawFixed){cb.DrawMode=DrawMode.OwnerDrawFixed;cb.DrawItem+=delegate(object sender,DrawItemEventArgs e){using(var brush=new SolidBrush((e.State&DrawItemState.Selected)!=0?Selected:Card))e.Graphics.FillRectangle(brush,e.Bounds);if(e.Index>=0)TextRenderer.DrawText(e.Graphics,Convert.ToString(cb.Items[e.Index]),cb.Font,new Rectangle(e.Bounds.X+9,e.Bounds.Y,e.Bounds.Width-12,e.Bounds.Height),Ink,TextFormatFlags.EndEllipsis|TextFormatFlags.VerticalCenter);};}}
  var text=c as TextBox;if(text!=null)text.BorderStyle=BorderStyle.FixedSingle;
  var list=c as ListBox;if(list!=null)list.BorderStyle=BorderStyle.None;
  var grid=c as DataGridView;if(grid!=null){grid.BackgroundColor=Card;grid.BorderStyle=BorderStyle.None;grid.CellBorderStyle=DataGridViewCellBorderStyle.SingleHorizontal;grid.EnableHeadersVisualStyles=false;grid.ColumnHeadersBorderStyle=DataGridViewHeaderBorderStyle.None;grid.ColumnHeadersDefaultCellStyle.BackColor=Card;grid.ColumnHeadersDefaultCellStyle.ForeColor=Muted;grid.DefaultCellStyle.BackColor=Card;grid.DefaultCellStyle.ForeColor=Ink;grid.DefaultCellStyle.SelectionBackColor=Selected;grid.DefaultCellStyle.SelectionForeColor=Ink;grid.DefaultCellStyle.Padding=new Padding(8);grid.GridColor=Line;}
  foreach(Control child in c.Controls)Paint(child,color);c.Invalidate();
 }
 public static void RefreshCursor(Control c){var arrow=new Cursor(Native.LoadCursorW(IntPtr.Zero,new IntPtr(32512)));var text=new Cursor(Native.LoadCursorW(IntPtr.Zero,new IntPtr(32513)));SetCursor(c,arrow,text);}
 static void SetCursor(Control c,Cursor arrow,Cursor text){c.Cursor=c is TextBox?text:arrow;foreach(Control child in c.Controls)SetCursor(child,arrow,text);}
}
public class RoundPanel:Panel {
 public RoundPanel(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);Padding=new Padding(18);}
 protected override void OnPaintBackground(PaintEventArgs e){e.Graphics.Clear(Parent==null?Skin.Back:Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var path=Skin.Round(new RectangleF(.5f,.5f,Math.Max(1,Width-1),Math.Max(1,Height-1)),9*e.Graphics.DpiX/96f)){using(var brush=new SolidBrush(Skin.Card))e.Graphics.FillPath(brush,path);using(var pen=new Pen(Skin.Line))e.Graphics.DrawPath(pen,path);}}
}
public class FluentButton:Button {
 bool hover,down;float progress;readonly Timer transition=new Timer{Interval=16};public bool Accent,Selected;
 public FluentButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw,true);FlatStyle=FlatStyle.Flat;Cursor=Cursors.Hand;transition.Tick+=delegate{float goal=hover?1:0;progress+=Math.Sign(goal-progress)*.22f;if(Math.Abs(goal-progress)<.23f){progress=goal;transition.Stop();}Invalidate();};}
 protected override void OnMouseEnter(EventArgs e){base.OnMouseEnter(e);hover=true;UpdateHover();}
 protected override void OnMouseLeave(EventArgs e){base.OnMouseLeave(e);hover=false;down=false;UpdateHover();}
 void UpdateHover(){if(Skin.Animate&&Visible)transition.Start();else{transition.Stop();progress=hover?1:0;Invalidate();}}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);down=true;Invalidate();}
 protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);down=false;Invalidate();}
 protected override void OnVisibleChanged(EventArgs e){base.OnVisibleChanged(e);if(!Visible){transition.Stop();hover=down=false;progress=0;}}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(Parent==null?Skin.Back:Parent.BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;Color normal=Accent?Skin.Accent:Selected?Skin.Selected:Skin.Card;Color fill=down?Skin.Blend(normal,Skin.Ink,.12f):Skin.Blend(normal,Accent?Skin.Ink:Skin.Hover,progress*(Accent?.12f:1));using(var p=Skin.Round(new RectangleF(.5f,.5f,Width-1,Height-1),5)){using(var b=new SolidBrush(fill))e.Graphics.FillPath(b,p);using(var pen=new Pen(Accent?Skin.Accent:Skin.Line))e.Graphics.DrawPath(pen,p);}TextRenderer.DrawText(e.Graphics,Text,Font,ClientRectangle,Enabled?(Accent?Color.White:Skin.Ink):Skin.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,new Rectangle(4,4,Width-8,Height-8),Skin.Ink,fill);}
 protected override void Dispose(bool disposing){if(disposing)transition.Dispose();base.Dispose(disposing);}
}
public sealed class StepSlider:Control {
 int value=1;bool dragging;public int Minimum=1,Maximum=15,TickFrequency=1,LargeChange=1;public event EventHandler ValueChanged;
 public int Value {get{return value;}set{int next=Math.Max(Minimum,Math.Min(Maximum,value));if(next==this.value)return;this.value=next;Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
 public StepSlider(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.Selectable,true);Height=48;TabStop=true;AccessibleRole=AccessibleRole.Slider;AccessibleName=L.T("指针大小档位");}
 void PointValue(int x){float margin=12*DeviceDpi/96f;Value=Minimum+(int)Math.Round((x-margin)/Math.Max(1,Width-2*margin)*(Maximum-Minimum));}
 protected override void OnMouseDown(MouseEventArgs e){base.OnMouseDown(e);if(e.Button!=MouseButtons.Left)return;Focus();dragging=true;Capture=true;PointValue(e.X);}
 protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(dragging)PointValue(e.X);}
 protected override void OnMouseUp(MouseEventArgs e){dragging=false;Capture=false;base.OnMouseUp(e);}
 protected override bool IsInputKey(Keys key){return key==Keys.Left||key==Keys.Right||key==Keys.Up||key==Keys.Down||base.IsInputKey(key);}
 protected override void OnKeyDown(KeyEventArgs e){if(e.KeyCode==Keys.Right||e.KeyCode==Keys.Up)Value++;if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Down)Value--;if(e.KeyCode==Keys.Home)Value=Minimum;if(e.KeyCode==Keys.End)Value=Maximum;base.OnKeyDown(e);}
 protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;float s=DeviceDpi/96f,start=12*s,end=Width-12*s,y=Height/2f-3*s,x=start+(end-start)*(Value-Minimum)/Math.Max(1,Maximum-Minimum);using(var p=new Pen(Skin.Line,4*s)){p.StartCap=p.EndCap=LineCap.Round;e.Graphics.DrawLine(p,start,y,end,y);}using(var p=new Pen(Enabled?Skin.Accent:Skin.Muted,4*s)){p.StartCap=p.EndCap=LineCap.Round;e.Graphics.DrawLine(p,start,y,x,y);}using(var b=new SolidBrush(Skin.Muted))for(int i=Minimum;i<=Maximum;i++)e.Graphics.FillEllipse(b,start+(end-start)*(i-Minimum)/Math.Max(1,Maximum-Minimum)-s,y+14*s,2*s,2*s);using(var b=new SolidBrush(Skin.Card))e.Graphics.FillEllipse(b,x-10*s,y-10*s,20*s,20*s);using(var p=new Pen(Skin.Line))e.Graphics.DrawEllipse(p,x-10*s,y-10*s,20*s,20*s);using(var b=new SolidBrush(Enabled?Skin.Accent:Skin.Muted))e.Graphics.FillEllipse(b,x-6*s,y-6*s,12*s,12*s);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,ClientRectangle);}
}
public class ToggleCheckBox:CheckBox {
 public ToggleCheckBox(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);Height=36;}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.Clear(BackColor);e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;float s=DeviceDpi/96f,y=(Height-20*s)/2;using(var p=Skin.Round(new RectangleF(s,y,39*s,20*s),10*s)){using(var b=new SolidBrush(Checked?Skin.Accent:Skin.Hover))e.Graphics.FillPath(b,p);using(var pen=new Pen(Checked?Skin.Accent:Skin.Muted))e.Graphics.DrawPath(pen,p);}using(var b=new SolidBrush(Checked?Color.White:Skin.Muted))e.Graphics.FillEllipse(b,(Checked?24:5)*s,y+4*s,12*s,12*s);int text=(int)(54*s);TextRenderer.DrawText(e.Graphics,Text,Font,new Rectangle(text,0,Math.Max(1,Width-text),Height),Enabled?Skin.Ink:Skin.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);if(Focused&&ShowFocusCues)ControlPaint.DrawFocusRectangle(e.Graphics,ClientRectangle);}
 protected override void OnCheckedChanged(EventArgs e){base.OnCheckedChanged(e);Invalidate();}
}
public sealed class FluentComboBox:ComboBox {
 public FluentComboBox(){DrawMode=DrawMode.OwnerDrawFixed;FlatStyle=FlatStyle.Flat;ItemHeight=26;}
 protected override void OnDrawItem(DrawItemEventArgs e){using(var brush=new SolidBrush((e.State&DrawItemState.Selected)!=0&&DroppedDown?Skin.Selected:Skin.Card))e.Graphics.FillRectangle(brush,e.Bounds);if(e.Index>=0)TextRenderer.DrawText(e.Graphics,Convert.ToString(Items[e.Index]),Font,new Rectangle(e.Bounds.X+9,e.Bounds.Y,e.Bounds.Width-12,e.Bounds.Height),Skin.Ink,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);}
 protected override void WndProc(ref Message m){base.WndProc(ref m);if(IsHandleCreated&&(m.Msg==0xF||m.Msg==0x318)){using(var g=m.Msg==0x318&&m.WParam!=IntPtr.Zero?Graphics.FromHdc(m.WParam):Graphics.FromHwnd(Handle)){float scale=DeviceDpi/96f;int arrow=(int)(28*scale);g.Clear(Parent==null?Skin.Card:Parent.BackColor);g.SmoothingMode=SmoothingMode.AntiAlias;using(var p=Skin.Round(new RectangleF(.5f,.5f,Width-1,Height-1),5*scale)){using(var b=new SolidBrush(Skin.Card))g.FillPath(b,p);using(var pen=new Pen(Focused?Skin.Accent:Skin.Line))g.DrawPath(pen,p);}TextRenderer.DrawText(g,SelectedIndex>=0?GetItemText(SelectedItem):Text,Font,new Rectangle((int)(10*scale),1,Math.Max(1,Width-arrow-(int)(10*scale)),Height-2),Skin.Ink,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);float x=Width-arrow/2f,y=Height/2f;using(var p=new Pen(Skin.Ink,1.2f*scale)){g.DrawLines(p,new[]{new PointF(x-3*scale,y-1*scale),new PointF(x,y+2*scale),new PointF(x+3*scale,y-1*scale)});}}}}
}
}
