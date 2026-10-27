using System;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
namespace CursorStudio {
// Self-contained test artwork: simple geometry authored for this project.
// No user themes, Windows cursor files or third-party illustrations are bundled.
static class TestCursors {
 static byte[] Frame(int step){using(var image=new Bitmap(32,32)){using(var g=Graphics.FromImage(image)){g.SmoothingMode=SmoothingMode.AntiAlias;g.Clear(Color.Transparent);using(var path=new GraphicsPath()){path.AddPolygon(new[]{new Point(3,2),new Point(3,24),new Point(9,18),new Point(14,29),new Point(19,27),new Point(13,16),new Point(23,16)});g.FillPath(Brushes.White,path);g.DrawPath(Pens.Black,path);}if(step>=0){double angle=step*Math.PI/18;using(var pen=new Pen(Color.DeepSkyBlue,2))g.DrawEllipse(pen,16,3,12,12);g.FillEllipse(Brushes.RoyalBlue,(float)(21+6*Math.Cos(angle))-2,(float)(9+6*Math.Sin(angle))-2,4,4);}}
   using(var stream=new MemoryStream())using(var w=new BinaryWriter(stream)){int maskStride=4,payload=40+32*32*4+maskStride*32;w.Write((ushort)0);w.Write((ushort)2);w.Write((ushort)1);w.Write((byte)32);w.Write((byte)32);w.Write((ushort)0);w.Write((ushort)3);w.Write((ushort)2);w.Write(payload);w.Write(22);w.Write(40);w.Write(32);w.Write(64);w.Write((ushort)1);w.Write((ushort)32);w.Write(0);w.Write(payload-40);for(int n=0;n<4;n++)w.Write(0);for(int y=31;y>=0;y--)for(int x=0;x<32;x++){Color c=image.GetPixel(x,y);w.Write(c.B);w.Write(c.G);w.Write(c.R);w.Write(c.A);}for(int y=31;y>=0;y--)for(int b=0;b<4;b++){byte value=0;for(int bit=0;bit<8;bit++)if(image.GetPixel(b*8+bit,y).A==0)value|=(byte)(128>>bit);w.Write(value);}return stream.ToArray();}
  }}
 static void Chunk(BinaryWriter w,string tag,byte[] data){w.Write(System.Text.Encoding.ASCII.GetBytes(tag));w.Write(data.Length);w.Write(data);if(data.Length%2!=0)w.Write((byte)0);}
 static byte[] Animation(){using(var body=new MemoryStream())using(var w=new BinaryWriter(body)){w.Write(System.Text.Encoding.ASCII.GetBytes("ACON"));using(var header=new MemoryStream())using(var h=new BinaryWriter(header)){foreach(int value in new[]{36,36,36,0,0,32,1,2,1})h.Write(value);Chunk(w,"anih",header.ToArray());}using(var frames=new MemoryStream())using(var f=new BinaryWriter(frames)){f.Write(System.Text.Encoding.ASCII.GetBytes("fram"));for(int i=0;i<36;i++)Chunk(f,"icon",Frame(i));Chunk(w,"LIST",frames.ToArray());}using(var result=new MemoryStream())using(var r=new BinaryWriter(result)){r.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));r.Write((int)body.Length);r.Write(body.ToArray());return result.ToArray();}}}
 public static Theme Create(){string input=Path.Combine(Store.Root,"test-input");Directory.CreateDirectory(input);string still=Path.Combine(input,"arrow.cur"),animated=Path.Combine(input,"wait.ani");File.WriteAllBytes(still,Frame(-1));File.WriteAllBytes(animated,Animation());var map=new Dictionary<string,string>();foreach(string role in Native.Roles)map[role]=role=="Wait"||role=="AppStarting"?animated:still;return ImportSession.Commit("Geometric test theme",96,map);}
}
}
