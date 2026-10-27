using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
namespace CursorStudio {
public sealed class CursorPreview:Control {
 IntPtr cursor;int pixels=32,step,steps=1,jiffies=6;int[] rates;readonly Timer timer;bool active,playing=true;public string PathName="";public int BackgroundStyle;
 public bool IsAnimating {get{return timer.Enabled;}}public bool HasResource {get{return cursor!=IntPtr.Zero;}}
 public void RefreshAnimation(){timer.Stop();Schedule();}
 public void SetPlaying(bool value){if(playing==value)return;playing=value;RefreshAnimation();}
 public CursorPreview(){SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.UserPaint|ControlStyles.ResizeRedraw,true);timer=new Timer();timer.Tick+=delegate{timer.Stop();if(!active||cursor==IntPtr.Zero||steps<=1)return;step=(step+1)%steps;Invalidate();Schedule();};}
 public void Set(string path,int size){path=path??"";if(path==PathName&&size==pixels)return;Release();PathName=path;pixels=size;if(active)Load();Invalidate();}
 public void SetActive(bool value){if(active==value)return;active=value;if(active)Load();else Release();Invalidate();}
 void Release(){timer.Stop();if(cursor!=IntPtr.Zero){Native.DestroyCursor(cursor);cursor=IntPtr.Zero;}step=0;steps=1;rates=null;jiffies=6;}
 void Load(){if(cursor!=IntPtr.Zero||!File.Exists(PathName))return;try{cursor=Native.LoadImageW(IntPtr.Zero,PathName,2,pixels,pixels,16);if(Path.GetExtension(PathName).Equals(".ani",StringComparison.OrdinalIgnoreCase))ReadTiming();Schedule();}catch(Exception e){Store.Log(e);}}
 void ReadTiming(){// Skip frame chunks instead of allocating the entire 10 MiB ANI.
  using(var stream=File.OpenRead(PathName))using(var reader=new BinaryReader(stream)){if(stream.Length<12)return;stream.Position=12;while(stream.Position+8<=stream.Length){string tag=System.Text.Encoding.ASCII.GetString(reader.ReadBytes(4));uint length=reader.ReadUInt32();long start=stream.Position,end=start+length;if(end>stream.Length)break;if(tag=="anih"&&length>=36){reader.ReadUInt32();reader.ReadUInt32();steps=(int)Math.Max(1,Math.Min(1000,reader.ReadUInt32()));stream.Position=start+28;jiffies=(int)Math.Max(1,Math.Min(60000,reader.ReadUInt32()));}else if(tag=="rate"&&length<=4000){rates=new int[length/4];for(int i=0;i<rates.Length;i++)rates[i]=(int)Math.Max(1,Math.Min(60000,reader.ReadUInt32()));}stream.Position=end+(length%2);}}}
 void Schedule(){if(!active||!playing||steps<=1||cursor==IntPtr.Zero||!Skin.SystemAnimations)return;int ticks=rates!=null&&step<rates.Length?rates[step]:jiffies;timer.Interval=Math.Max(10,Math.Min(1000000,(int)(ticks*1000L/60)));timer.Start();}
 protected override void OnPaint(PaintEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;bool dark=BackgroundStyle==2||(BackgroundStyle==0&&Skin.Dark);Color back=dark?Color.FromArgb(29,29,29):Color.FromArgb(247,247,247);e.Graphics.Clear(Parent==null?Skin.Card:Parent.BackColor);using(var rounded=Skin.Round(new RectangleF(.5f,.5f,Math.Max(1,Width-1),Math.Max(1,Height-1)),7)){var state=e.Graphics.Save();e.Graphics.SetClip(rounded);using(var b=new SolidBrush(back))e.Graphics.FillRectangle(b,ClientRectangle);if(BackgroundStyle==3){using(var b=new SolidBrush(Color.FromArgb(211,211,211)))for(int y=0;y<Height;y+=18)for(int x=0;x<Width;x+=18)if((x/18+y/18)%2==0)e.Graphics.FillRectangle(b,x,y,18,18);}if(cursor!=IntPtr.Zero){int draw=Math.Min(pixels,Math.Max(16,Math.Min(Width,Height)-32));IntPtr dc=e.Graphics.GetHdc();try{Native.DrawIconEx(dc,(Width-draw)/2,(Height-draw)/2,cursor,draw,draw,(uint)step,IntPtr.Zero,3);}finally{e.Graphics.ReleaseHdc(dc);}if(draw<pixels)TextRenderer.DrawText(e.Graphics,"适配预览 · 实际 "+pixels+" px",Font,new Point(12,Height-28),dark?Color.LightGray:Color.DimGray);}else TextRenderer.DrawText(e.Graphics,"选择主题，预览你的指针",Font,ClientRectangle,Skin.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);e.Graphics.Restore(state);using(var p=new Pen(Skin.Line))e.Graphics.DrawPath(p,rounded);}}
 protected override void Dispose(bool disposing){if(disposing){Release();timer.Dispose();}base.Dispose(disposing);}
}
}
