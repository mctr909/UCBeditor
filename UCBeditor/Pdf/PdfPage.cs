using System;
using System.Drawing;
using System.IO;

namespace Pdf {
	public class PdfPage : IDrawer {
		public readonly PageSize Size;

		private const float FONT_SCALE = 1.2f;
		private const float PIX_SCALE = FONT_SCALE * 0.65f;

		private readonly MemoryStream mMs;
		private readonly StreamWriter mSw;
		private readonly Bitmap mBmp;
		private readonly Graphics mG;
		private readonly double mScale;
		private double mTx;
		private double mTy;

		private Font mFont = new Font(PdfFile.FontName, 9.0f);

		public Color DrawColor {
			set {
				mSw.WriteLine("{0} {1} {2} RG",
					(value.R / 255.0).ToString("0.##"),
					(value.G / 255.0).ToString("0.##"),
					(value.B / 255.0).ToString("0.##")
				);
			}
		}
		public Color FillColor {
			set {
				mSw.WriteLine("{0} {1} {2} rg",
					(value.R / 255.0).ToString("0.##"),
					(value.G / 255.0).ToString("0.##"),
					(value.B / 255.0).ToString("0.##")
				);
			}
		}
		public double FontSize {
			get { return mFont.Size; }
			set { mFont = new Font(mFont.Name, (float)value); }
		}

		public PdfPage(PageSize size, double scale) {
			Size = size;
			mMs = new MemoryStream();
			mSw = new StreamWriter(mMs);
			mBmp = new Bitmap((int)size.Pixel.X, (int)size.Pixel.Y);
			mG = Graphics.FromImage(mBmp);
			mScale = Size.Scale * scale;
			mTx = 0.0;
			mTy = 0.0;
		}

		public void Dispose() {
			mG.Dispose();
			mBmp.Dispose();
			mSw.Dispose();
			mMs.Dispose();
		}

		internal void Flush(FileStream fs) {
			mSw.Flush();
			mMs.Seek(0, SeekOrigin.Begin);
			var ms = new MemoryStream();
			var tmp = new StreamWriter(ms);
			tmp.WriteLine("q");
			tmp.WriteLine("0 w");
			tmp.WriteLine("1 0 0 -1 0 {0} cm", Size.Pixel.Y);
			tmp.WriteLine("BT");
			var sr = new StreamReader(mMs);
			while (!sr.EndOfStream) {
				tmp.WriteLine(sr.ReadLine());
			}
			tmp.WriteLine("ET");
			tmp.WriteLine("Q");
			tmp.Flush();

			var enc = Deflate.Compress(ms.ToArray());
			var sw = new StreamWriter(fs);
			sw.NewLine = "\n";
			sw.WriteLine("<</Filter /FlateDecode /Length {0}>>stream", enc.Length + 2);
			sw.Flush();
			fs.WriteByte(0x68);
			fs.WriteByte(0xDE);
			fs.Write(enc, 0, enc.Length);
			fs.Flush();
			sw.WriteLine();
			sw.WriteLine("endstream");
			sw.Flush();
		}

		public SizeF GetTextSize(string s) {
			return mG.MeasureString(s, mFont);
		}

		public void ClearTranslate() {
			mTx = 0.0;
			mTy = 0.0;
		}

		public void SetTranslate(double x, double y) {
			mTx = x;
			mTy = y;
		}

		public void DrawText(string s, double x, double y, bool centering = false) {
			WriteText(s, x, y, centering ? (GetTextSize(s).Width * 0.5f) : 0);
		}

		public void DrawText(string s, PointF p, bool centering = false) {
			WriteText(s, p.X, p.Y, centering ? (GetTextSize(s).Width * 0.5f) : 0);
		}

		public void DrawText(string s, double x, double y, double rotateAngle, bool centering = false) {
			WriteText(s, x, y, centering ? (GetTextSize(s).Width * 0.5f) : 0, rotateAngle);
		}

		public void DrawText(string s, PointF p, double rotateAngle, bool centering = false) {
			WriteText(s, p.X, p.Y, centering ? (GetTextSize(s).Width * 0.5f) : 0, rotateAngle);
		}

		public void FillLine(double ax, double ay, double bx, double by, double weight) {
			var sx = bx - ax;
			var sy = by - ay;
			var th = Math.Atan2(sy, sx) * 180 / Math.PI + 90;
			var a1 = CreateArc(ax, ay, weight, th, 180);
			var a2 = CreateArc(bx, by, weight, th + 180, 180);
			var v = a1[0];
			M(v.X, v.Y);
			for (int i = 1; i < a1.Length; i++) {
				v = a1[i];
				L(v.X, v.Y);
			}
			for (int i = 0; i < a2.Length; i++) {
				v = a2[i];
				L(v.X, v.Y);
			}
			v = a1[0];
			LF(v.X, v.Y);
		}

		public void FillLine(PointF a, PointF b, double weight) {
			var sx = b.X - a.X;
			var sy = b.Y - a.Y;
			var th = Math.Atan2(sy, sx) * 180 / Math.PI + 90;
			var a1 = CreateArc(a.X, a.Y, weight, th, 180);
			var a2 = CreateArc(b.X, b.Y, weight, th + 180, 180);
			var v = a1[0];
			M(v.X, v.Y);
			for (int i = 1; i < a1.Length; i++) {
				v = a1[i];
				L(v.X, v.Y);
			}
			for (int i = 0; i < a2.Length; i++) {
				v = a2[i];
				L(v.X, v.Y);
			}
			v = a1[0];
			LF(v.X, v.Y);
		}

		public void FillCircle(double x, double y, double diameter) {
			var vert = CreateArc(x, y, diameter);
			var v = vert[0];
			M(v.X, v.Y);
			for (int i = 1; i < vert.Length; i++) {
				v = vert[i];
				L(v.X, v.Y);
			}
			v = vert[0];
			LF(v.X, v.Y);
		}

		public void FillCircle(PointF p, double diameter) {
			var vert = CreateArc(p.X, p.Y, diameter);
			var v = vert[0];
			M(v.X, v.Y);
			for (int i = 1; i < vert.Length; i++) {
				v = vert[i];
				L(v.X, v.Y);
			}
			v = vert[0];
			LF(v.X, v.Y);
		}

		public void FillPie(double x, double y, double diameter, double start, double sweep) {
			var vert = CreateArc(x, y, diameter, start, sweep);
			M(x, y);
			for (int i = 0; i < vert.Length; i++) {
				var v = vert[i];
				L(v.X, v.Y);
			}
			LF(x, y);
		}

		public void FillPie(PointF p, double diameter, double start, double sweep) {
			var vert = CreateArc(p.X, p.Y, diameter, start, sweep);
			M(p.X, p.Y);
			for (int i = 0; i < vert.Length; i++) {
				var v = vert[i];
				L(v.X, v.Y);
			}
			LF(p.X, p.Y);
		}

		public void FillArc(double x, double y, double diameter, double start, double sweep, double weight) {
			var outer = CreateArc(x, y, diameter + weight * 0.5, start, sweep);
			var inner = CreateArc(x, y, diameter - weight * 0.5, start, sweep);
			var v = outer[0];
			M(v.X, v.Y);
			for (int i = 1; i < outer.Length; i++) {
				v = outer[i];
				L(v.X, v.Y);
			}
			for (int i = inner.Length - 1; i >= 0; i--) {
				v = inner[i];
				L(v.X, v.Y);
			}
			v = outer[0];
			LF(v.X, v.Y);
		}

		public void FillArc(PointF p, double diameter, double start, double sweep, double weight) {
			var outer = CreateArc(p.X, p.Y, diameter + weight * 0.5, start, sweep);
			var inner = CreateArc(p.X, p.Y, diameter - weight * 0.5, start, sweep);
			var v = outer[0];
			M(v.X, v.Y);
			for (int i = 1; i < outer.Length; i++) {
				v = outer[i];
				L(v.X, v.Y);
			}
			for (int i = inner.Length - 1; i >= 0; i--) {
				v = inner[i];
				L(v.X, v.Y);
			}
			v = outer[0];
			LF(v.X, v.Y);
		}

		public void FillPolygon(params PointF[] poly) {
			var v = poly[0];
			M(v.X, v.Y);
			for (int i = 1; i < poly.Length; i++) {
				v = poly[i];
				L(v.X, v.Y);
			}
			v = poly[0];
			LF(v.X, v.Y);
		}

		private PointF[] CreateArc(double x, double y, double diameter, double start = 0, double sweep = 360) {
			var divCount = (int)(Math.Abs(sweep) / 15.0 + 0.5);
			if (divCount < 3) {
				divCount = 3;
			}
			var poly = new PointF[divCount];
			var startRad = Math.PI * start / 180;
			var sweepRad = Math.PI * sweep / 180;
			var radius = diameter * 0.5;
			for (int i = 0; i < divCount; i++) {
				var th = (i + 0.5) * sweepRad / divCount + startRad;
				poly[i] = new PointF(
					(float)(x + radius * Math.Cos(th)),
					(float)(y + radius * Math.Sin(th))
				);
			}
			return poly;
		}

		private void WriteText(string s, double x, double y, double ofsX) {
			TF(0, FontSize);
			var ofsY = FontSize * PIX_SCALE * 0.5;
			var strs = s.Replace("\r", "").Split('\n');
			x += mTx;
			y += mTy;
			foreach (var str in strs) {
				TM(
					1, 0,
					0, -1,
					x - ofsX * PIX_SCALE, y + ofsY
				);
				TJ(str.Replace("\n", ""));
				ofsY += FontSize * (PIX_SCALE + 0.2);
			}
			TM(1, 0, 0, 1, 0, 0);
		}

		private void WriteText(string s, double x, double y, double ofsX, double theta) {
			TF(0, FontSize);
			x += mTx;
			y += mTy;
			var strs = s.Replace("\r", "").Split('\n');
			var ofsY = FontSize * (2 - strs.Length) * 0.5;
			var cos = Math.Cos(theta);
			var sin = Math.Sin(theta);
			foreach (var str in strs) {
				var rx = ofsX * cos + ofsY * sin;
				var ry = ofsX * sin - ofsY * cos;
				TM(
					cos, sin,
					sin, -cos,
					x - rx * PIX_SCALE, y - ry * PIX_SCALE
				);
				TJ(str);
				ofsY += FontSize + 0.5;
			}
			TM(1, 0, 0, 1, 0, 0);
		}

		private void M(double x, double y) {
			mSw.WriteLine($"{(x + mTx) * mScale:0.###} {(y + mTy) * mScale:0.###} m");
		}

		private void L(double x, double y) {
			mSw.WriteLine($"{(x + mTx) * mScale:0.###} {(y + mTy) * mScale:0.###} l");
		}

		private void LS(double x, double y) {
			mSw.WriteLine($"{(x + mTx) * mScale:0.###} {(y + mTy) * mScale:0.###} l S");
		}

		private void LF(double x, double y) {
			mSw.WriteLine($"{(x + mTx) * mScale:0.###} {(y + mTy) * mScale:0.###} l f");
		}

		private void TM(double a, double b, double c, double d, double tx, double ty) {
			mSw.WriteLine($"{a:0.###} {b:0.###} {c:0.###} {d:0.###} {tx:0.###} {ty:0.###} Tm");
		}

		private void TF(int fontIndex, double size) {
			mSw.WriteLine($"/F{fontIndex} {size * FONT_SCALE:0.###} Tf");
		}

		private void TJ(string text) {
			mSw.WriteLine($"({text}) Tj");
		}
	}
}
