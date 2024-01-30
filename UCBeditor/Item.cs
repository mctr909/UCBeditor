using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;

namespace UCB {
	public enum ROTATE {
		NONE,
		DEG90,
		DEG180,
		DEG270
	}

	public abstract class Item {
		protected static readonly Pen SELECT_COLOR = new Pen(Color.Magenta, 2);
		protected static readonly Pen PATTERN = new Pen(Color.FromArgb(147, 147, 147), 2) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		protected static readonly Pen PATTERN_B = new Pen(PATTERN.Color, 9) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		protected static readonly Pen LAND = new Pen(Color.FromArgb(191, 191, 0), 1) { StartCap = LineCap.Round, EndCap = LineCap.Round };

		protected const double TermLandDiameter = 1.7;
		protected const double TermHoleDiameter = 0.5;
		protected const double WireLandDiameter = 0.6;
		protected const double WireHoleDiameter = 0.15;
		protected const double ResistMaskClearance = 0.05;

		public const int GridWidth = 16;
		public const float GridScale = GridWidth / 2.54f;
		public const int SNAP = GridWidth / 2;

		public static bool Reverse { get; set; }
		public static bool Pattern { get; set; }
		public static bool Wire { get; set; }
		public static bool Parts { get; set; }

		public static Item Construct(string line) {
			var cols = line.Split('\t');
			switch (cols[0]) {
			case "TIN":
				return new Pattern(cols);
			case "WIRE":
			case "WRAP":
				return new Wire(cols);
			case "PARTS":
				return new Parts(cols);
			case "TERM":
			default:
				return new Terminal(cols);
			}
		}

		public Point Begin;
		public Point End;
		public double Height { get; protected set; }
		public bool Removed { get; set; }

		public bool IsSelected(Rectangle selectArea) {
			return selectArea.Contains(Begin) || selectArea.Contains(End);
		}
		public void Draw(Graphics g, bool selected) {
			Draw(g, 0, 0, selected);
		}
		public virtual double Distance(Point point) {
			var apX = point.X - Begin.X;
			var apY = point.Y - Begin.Y;
			return Math.Sqrt(apX * apX + apY * apY);
		}
		public virtual bool IsSelected(Point point) {
			if (GetType() == typeof(Terminal)) {
				return Distance(point) < 8.0;
			}
			if (GetType() == typeof(Parts)) {
				return Distance(point) < 8.0;
			}
			return false;
		}
		public virtual Point[] GetTerminals() { return new Point[] { Begin }; }

		public virtual void DrawPattern(PDF.Page page) { }
		public virtual void DrawSolderMask(PDF.Page page) { }

		public abstract Item Clone();
		public abstract void Write(StreamWriter sw);
		public abstract void Draw(Graphics g, int dx, int dy, bool selected);
	}
}