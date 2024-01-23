using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

class PDF {
	const string FontName = "Arial";

	public struct PAGE_SIZE {
		public double Scale;
		public PointF Size;
		public PointF Pixel;

		static PAGE_SIZE ToPix(double dpi, double width, double height) {
			var scale = dpi / 25.4;
			return new PAGE_SIZE() {
				Scale = scale,
				Size = new PointF((float)width, (float)height),
				Pixel = new PointF((float)(width * scale), (float)(height * scale))
			};
		}

		public static PAGE_SIZE A4_H = ToPix(72, 297, 210);
		public static PAGE_SIZE A4_V = ToPix(72, 210, 297);
		public static PAGE_SIZE A5_H = ToPix(72, 210, 148);
		public static PAGE_SIZE A5_V = ToPix(72, 148, 210);
		public static PAGE_SIZE L_H = ToPix(72, 127, 89);
		public static PAGE_SIZE L_V = ToPix(72, 89, 127);
	}

	public class Page {
		public readonly PAGE_SIZE Size;

		const float FONT_SCALE = 1.2f;
		const float PIX_SCALE = FONT_SCALE * 0.65f;

		MemoryStream mMs;
		StreamWriter mSw;
		Bitmap mBmp;
		Graphics mG;
		double mOfsX;
		double mOfsY;

		Font mFont = new Font(FontName, 9.0f);

		public Color DrawColor {
			set {
				mSw.WriteLine("{0} {1} {2} RG",
					(value.R / 255.0).ToString("0.##"),
					(value.G / 255.0).ToString("0.##"),
					(value.B / 255.0).ToString("0.##")
				);
				mSw.WriteLine("{0} {1} {2} rg",
					(value.R / 255.0).ToString("0.##"),
					(value.G / 255.0).ToString("0.##"),
					(value.B / 255.0).ToString("0.##")
				);
			}
		}

		public float FontSize {
			get { return mFont.Size; }
			set { mFont = new Font(mFont.Name, value); }
		}

		public double Scale { get; set; } = 1.0;

		public Page(PAGE_SIZE size) {
			Size = size;
			mMs = new MemoryStream();
			mSw = new StreamWriter(mMs);
			mBmp = new Bitmap((int)size.Pixel.X, (int)size.Pixel.Y);
			mG = Graphics.FromImage(mBmp);
			mOfsX = 0.0;
			mOfsY = 0.0;
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

		public void SetTransrate(double x, double y) {
			mOfsX = x;
			mOfsY = y;
		}

		public void ClearTransform() {
			mOfsX = 0.0;
			mOfsY = 0.0;
		}

		public void DrawLeftText(string s, double x, double y) {
			writeText(s, x, y);
		}

		public void DrawLeftText(string s, PointF pos) {
			writeText(s, pos.X, pos.Y);
		}

		public void DrawRightText(string s, double x, double y) {
			writeText(s, x, y, GetTextSize(s).Width);
		}

		public void DrawRightText(string s, PointF pos) {
			writeText(s, pos.X, pos.Y, GetTextSize(s).Width);
		}

		public void DrawCenteredText(string s, PointF p, double rotateAngle) {
			writeTextR(s, p.X, p.Y, rotateAngle, GetTextSize(s).Width * 0.5f);
		}

		public void DrawLine(double ax, double ay, double bx, double by) {
			writeM(ax, ay);
			writeLS(bx, by);
		}

		public void DrawLine(PointF a, PointF b) {
			DrawLine(a.X, a.Y, b.X, b.Y);
		}

		public void DrawPolyline(PointF[] poly) {
			var pa = poly[0];
			for (int i = 1; i < poly.Length; i++) {
				var pb = poly[i];
				DrawLine(pa, pb);
				pa = pb;
			}
		}

		public void DrawPolygon(PointF[] poly) {
			var p = poly[0];
			writeM(p.X, p.Y);
			for (int i = 1; i < poly.Length; i++) {
				p = poly[i];
				writeL(p);
			}
			p = poly[0];
			writeLS(p.X, p.Y);
		}

		public void DrawCircle(PointF c, double radius) {
			var poly = polyCircle(c.X, c.Y, radius);
			var p = poly[0];
			writeM(p.X, p.Y);
			for (int i = 1; i < poly.Length; i++) {
				p = poly[i];
				writeL(p);
			}
			p = poly[0];
			writeLS(p.X, p.Y);
		}

		public void DrawArc(PointF c, double diameter, double start, double sweep) {
			var poly = polyCircle(c.X, c.Y, diameter * 0.5, start, sweep);
			var p = poly[0];
			writeM(p.X, p.Y);
			for (int i = 1; i < poly.Length - 1; i++) {
				p = poly[i];
				writeL(p);
			}
			p = poly[poly.Length - 1];
			writeLS(p.X, p.Y);
		}

		public void FillPolygon(PointF[] poly) {
			var p = poly[0];
			writeM(p.X, p.Y);
			for (int i = 1; i < poly.Length; i++) {
				p = poly[i];
				writeL(p);
			}
			p = poly[0];
			writeLF(p);
		}

		public void FillCircle(PointF c, double radius) {
			fillCircleF(c.X, c.Y, radius);
		}

		public void FillCircle(double cx, double cy, double radius) {
			fillCircleF(cx, cy, radius);
		}

		void fillCircleF(double cx, double cy, double radius) {
			var poly = polyCircle(cx, cy, radius);
			var p = poly[0];
			writeM(p.X, p.Y);
			for (int i = 1; i < poly.Length; i++) {
				p = poly[i];
				writeL(p);
			}
			p = poly[0];
			writeLF(p);
		}

		PointF[] polyCircle(double cx, double cy, double radius, double start = 0, double sweep = 360) {
			var poly = new PointF[16];
			var sRad = Math.PI * start / 180;
			var ssweep = sweep / 360.0;
			for (int i = 0; i < poly.Length; i++) {
				var th = 2 * Math.PI * (i + 0.5) * ssweep / poly.Length + sRad;
				poly[i] = new PointF(
					(float)(cx + radius * Math.Cos(th)),
					(float)(cy + radius * Math.Sin(th))
				);
			}
			return poly;
		}

		SizeF GetTextSize(string s) {
			return mG.MeasureString(s, mFont);
		}

		void writeFontSize(double size) {
			mSw.WriteLine("/F0 {0} Tf", (size * FONT_SCALE).ToString("0.##"));
		}

		void writeText(string text) {
			mSw.WriteLine("({0}) Tj", text);
		}

		void writeText(string s, double x, double y, double ofsX = 0.0) {
			writeFontSize(FontSize);
			var ofsY = FontSize * PIX_SCALE * 0.5;
			var strs = s.Replace("\r", "").Split('\n');
			x += mOfsX;
			y += mOfsY;
			foreach (var str in strs) {
				mSw.WriteLine("1 0 0 -1 {0} {1} Tm",
					(x - ofsX * PIX_SCALE).ToString("0.##"),
					(y + ofsY).ToString("0.##")
				);
				writeText(str.Replace("\n", ""));
				ofsY += FontSize * (PIX_SCALE + 0.2);
			}
		}

		void writeTextR(string s, double x, double y, double theta, double ofsX = 0.0) {
			writeFontSize(FontSize);
			x += mOfsX;
			y += mOfsY;
			var strs = s.Replace("\r", "").Split('\n');
			var ofsY = FontSize * (2 - strs.Length) * 0.5;
			var cos = Math.Cos(theta);
			var sin = Math.Sin(theta);
			foreach (var str in strs) {
				var rx = ofsX * cos + ofsY * sin;
				var ry = ofsX * sin - ofsY * cos;
				mSw.WriteLine("{0} {1} {2} {3} {4} {5} Tm",
					cos.ToString("0.##"), sin.ToString("0.##"),
					sin.ToString("0.##"), (-cos).ToString("0.##"),
					(x - rx * PIX_SCALE).ToString("0.##"),
					(y - ry * PIX_SCALE).ToString("0.##")
				);
				writeText(str);
				ofsY += FontSize + 0.5;
			}
		}

		void writeM(double x, double y) {
			mSw.WriteLine("{0} {1} m",
				((x + mOfsX) * Size.Scale * Scale).ToString("0.00000"),
				((y + mOfsY) * Size.Scale * Scale).ToString("0.00000")
			);
		}

		void writeL(double x, double y) {
			mSw.WriteLine("{0} {1} l",
				((x + mOfsX) * Size.Scale * Scale).ToString("0.00000"),
				((y + mOfsY) * Size.Scale * Scale).ToString("0.00000")
			);
		}

		void writeL(PointF p) {
			writeL(p.X, p.Y);
		}

		void writeLS(double x, double y) {
			mSw.WriteLine("{0} {1} l S",
				((x + mOfsX) * Size.Scale * Scale).ToString("0.00000"),
				((y + mOfsY) * Size.Scale * Scale).ToString("0.00000")
			);
		}

		void writeLF(double x, double y) {
			mSw.WriteLine("{0} {1} l f",
				((x + mOfsX) * Size.Scale * Scale).ToString("0.00000"),
				((y + mOfsY) * Size.Scale * Scale).ToString("0.00000")
			);
		}

		void writeLF(PointF p) {
			writeLF(p.X, p.Y);
		}
	}

	List<Page> mPageList = new List<Page>();

	public void AddPage(Page page) {
		mPageList.Add(page);
	}

	public void Save(string path) {
		if (string.IsNullOrEmpty(path) || !Directory.Exists(Path.GetDirectoryName(path))) {
			return;
		}
		var fs = new FileStream(path, FileMode.Create);
		var sw = new StreamWriter(fs);
		sw.NewLine = "\n";
		sw.WriteLine("%PDF-1.7");
		sw.Flush();
		fs.WriteByte(0xE2);
		fs.WriteByte(0xE3);
		fs.WriteByte(0xCF);
		fs.WriteByte(0xD3);
		sw.WriteLine();
		sw.WriteLine("1 0 obj");
		sw.WriteLine("<<");
		sw.WriteLine("  /Type /Catalog");
		sw.WriteLine("  /Pages 2 0 R");
		sw.WriteLine(">>");
		sw.WriteLine("endobj");
		sw.WriteLine();
		sw.WriteLine("2 0 obj");
		sw.WriteLine("<<");
		sw.WriteLine("  /Type /Pages");
		sw.Write("  /Kids [");
		for (int pIdx = 0; pIdx < mPageList.Count; pIdx++) {
			sw.Write("{0} 0 R ", pIdx + 4);
		}
		sw.WriteLine("]");
		sw.WriteLine("  /Count {0}", mPageList.Count);
		sw.WriteLine(">>");
		sw.WriteLine("endobj");
		sw.WriteLine();
		sw.WriteLine("3 0 obj");
		sw.WriteLine("<<");
		sw.WriteLine("  /Font <<");
		sw.WriteLine("    /F0 <<");
		sw.WriteLine("      /Type /Font");
		sw.WriteLine("      /BaseFont /{0}", FontName);
		sw.WriteLine("      /Subtype /Type1");
		sw.WriteLine("    >>");
		sw.WriteLine("  >>");
		sw.WriteLine(">>");
		sw.WriteLine("endobj");
		sw.WriteLine();
		for (int pIdx = 0; pIdx < mPageList.Count; pIdx++) {
			var size = mPageList[pIdx].Size.Pixel;
			sw.WriteLine("{0} 0 obj", pIdx + 4);
			sw.WriteLine("<<");
			sw.WriteLine("  /Type /Page");
			sw.WriteLine("  /Parent 2 0 R");
			sw.WriteLine("  /Resources 3 0 R");
			sw.WriteLine("  /MediaBox [0 0 {0} {1}]", size.X, size.Y);
			sw.WriteLine("  /Contents {0} 0 R", mPageList.Count + pIdx + 4);
			sw.WriteLine(">>");
			sw.WriteLine("endobj");
			sw.WriteLine();
		}
		for (int pIdx = 0; pIdx < mPageList.Count; pIdx++) {
			sw.WriteLine("{0} 0 obj", mPageList.Count + pIdx + 4);
			sw.Flush();
			mPageList[pIdx].Flush(fs);
			sw.WriteLine("endobj");
			sw.WriteLine();
		}
		sw.WriteLine("xref");
		sw.WriteLine("trailer");
		sw.WriteLine("<<");
		sw.WriteLine("  /Size {0}", mPageList.Count * 2 + 4);
		sw.WriteLine("  /Root 1 0 R");
		sw.WriteLine(">>");
		sw.WriteLine("startxref");
		sw.WriteLine("0");
		sw.WriteLine("%%EOF");
		sw.Close();
		sw.Dispose();
	}
}
