using System;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.Xml.Linq;
class GenerateIcon {
 static void Main(string[] args){var svg=XDocument.Load(args[0]);var points=svg.Descendants().First(e=>e.Name.LocalName=="polygon").Attribute("points").Value.Split(' ').Select(p=>p.Split(',')).Select(p=>new PointF(float.Parse(p[0],CultureInfo.InvariantCulture),float.Parse(p[1],CultureInfo.InvariantCulture))).ToArray();int[] sizes={16,20,24,32,40,48,64,96,128,256};var images=new byte[sizes.Length][];
  for(int i=0;i<sizes.Length;i++){int size=sizes[i],scale=4;using(var large=new Bitmap(size*scale,size*scale,PixelFormat.Format32bppArgb))using(var g=Graphics.FromImage(large))using(var image=new Bitmap(size,size,PixelFormat.Format32bppArgb)){g.SmoothingMode=SmoothingMode.AntiAlias;g.ScaleTransform(size*scale/64f,size*scale/64f);using(var shape=new GraphicsPath()){shape.AddPolygon(points);g.FillPath(Brushes.White,shape);using(var stroke=new Pen(Color.FromArgb(17,17,17),Math.Max(1.8f,64f/size))){stroke.LineJoin=LineJoin.Round;g.DrawPath(stroke,shape);}}using(var resized=Graphics.FromImage(image)){resized.CompositingMode=CompositingMode.SourceCopy;resized.InterpolationMode=InterpolationMode.HighQualityBicubic;resized.PixelOffsetMode=PixelOffsetMode.HighQuality;resized.DrawImage(large,new Rectangle(0,0,size,size));}using(var stream=new MemoryStream()){image.Save(stream,ImageFormat.Png);images[i]=stream.ToArray();}if(args.Length>2&&size==128)image.Save(args[2],ImageFormat.Png);}}
  using(var stream=File.Create(args[1]))using(var writer=new BinaryWriter(stream)){writer.Write((ushort)0);writer.Write((ushort)1);writer.Write((ushort)sizes.Length);int offset=6+16*sizes.Length;for(int i=0;i<sizes.Length;i++){writer.Write((byte)(sizes[i]%256));writer.Write((byte)(sizes[i]%256));writer.Write((byte)0);writer.Write((byte)0);writer.Write((ushort)1);writer.Write((ushort)32);writer.Write(images[i].Length);writer.Write(offset);offset+=images[i].Length;}foreach(var image in images)writer.Write(image);}
 }
}
