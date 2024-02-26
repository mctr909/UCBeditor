using System.Drawing;

namespace Pdf {
	public readonly struct PageSize {
		public readonly double Scale;
		public readonly PointF Size;
		public readonly PointF Pixel;

		private PageSize(double dpi, double width, double height) {
			var scale = dpi / 25.4;
			Scale = scale;
			Size = new PointF((float)width, (float)height);
			Pixel = new PointF((float)(width * scale), (float)(height * scale));
		}

		public static readonly PageSize A4_H = new PageSize(72, 297, 210);
		public static readonly PageSize A4_V = new PageSize(72, 210, 297);
		public static readonly PageSize A5_H = new PageSize(72, 210, 148);
		public static readonly PageSize A5_V = new PageSize(72, 148, 210);
		public static readonly PageSize POST_H = new PageSize(72, 148, 100);
		public static readonly PageSize POST_V = new PageSize(72, 100, 148);
		public static readonly PageSize L_H = new PageSize(72, 127, 89);
		public static readonly PageSize L_V = new PageSize(72, 89, 127);
	}
}
