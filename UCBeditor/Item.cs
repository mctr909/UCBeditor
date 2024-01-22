using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;

namespace UCBeditor {
	abstract class Item {
		protected static readonly Pen SELECT_COLOR = Pens.Turquoise;

		public static bool Reverse { get; set; }

		public static Item Construct(string line) {
			var cols = line.Split('\t');
			switch (cols[0]) {
			case "TIN":
				return new Tin(cols);
			case "WIRE":
				return new Wire(cols);
			case "WRAP":
				return new Wrap(cols);
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
			if (Reverse) {
				if (GetType() == typeof(Wire)) {
					return false;
				}
			} else {
				if (GetType() == typeof(Wrap)) {
					return false;
				}
			}
			if (Tin.Enable) {
				if (GetType() == typeof(Wire)) {
					return false;
				}
				if (GetType() == typeof(Wrap)) {
					return false;
				}
			} else {
				if (GetType() == typeof(Tin)) {
					return false;
				}
			}
			return Distance(point) < 8.0;
		}
		public virtual Point[] GetTerminals() { return new Point[0]; }

		public abstract Item Clone();
		public abstract void Write(StreamWriter sw);
		public abstract void Draw(Graphics g, int dx, int dy, bool selected);
	}

	class Terminal : Item {
		static readonly Pen COLOR = new Pen(Color.FromArgb(191, 191, 0), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		static readonly Pen REVERSE = new Pen(Color.FromArgb(191, 191, 191), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

		protected Terminal() { }

		public Terminal(string[] cols) {
			Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			End = Begin;
			Height = -0.01;
		}

		public Terminal(Point pos) {
			Begin = pos;
			End = pos;
			Height = -0.01;
		}

		public override Item Clone() {
			return new Terminal(Begin);
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine("TERM\t{0}\t{1}", Begin.X, Begin.Y);
		}

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			var x1 = Begin.X + dx - 5;
			var y1 = Begin.Y + dy - 5;
			var x2 = Begin.X + dx - 2;
			var y2 = Begin.Y + dy - 2;
			if (selected) {
				g.DrawArc(SELECT_COLOR, x1, y1, 10, 10, 0, 360);
				g.DrawArc(SELECT_COLOR, x2, y2, 4, 4, 0, 360);
			} else {
				if (Reverse) {
					g.FillEllipse(COLOR.Brush, x1, y1, 10, 10);
					g.FillEllipse(Brushes.White, x2, y2, 4, 4);
				} else {
					g.FillEllipse(Brushes.White, x2, y2, 4, 4);
					g.DrawEllipse(REVERSE, x2, y2, 4, 4);
				}
			}
		}
	}

	class Land : Terminal {
		public readonly Item Parent;

		public Land(Point pos, Item parent) {
			Begin = pos;
			End = pos;
			Height = -0.01;
			Parent = parent;
		}

		public override Item Clone() { return null; }

		public override void Write(StreamWriter sw) { }

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			if (Parent is Tin) {
				return;
			} else {
				base.Draw(g, dx, dy, selected);
			}
		}
	}

	class Wire : Item {
		static readonly Pen REVERSE = new Pen(Color.FromArgb(211, 211, 211), 4.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };

		static readonly Pen BLACK = new Pen(Color.FromArgb(71, 71, 71), 2.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen RED = new Pen(Color.FromArgb(211, 63, 63), 2.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen GREEN = new Pen(Color.FromArgb(47, 167, 47), 2.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen BLUE = new Pen(Color.FromArgb(63, 63, 221), 2.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen MAGENTA = new Pen(Color.FromArgb(167, 0, 167), 2.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen YELLOW = new Pen(Color.FromArgb(191, 191, 0), 2.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen BLACK_B = new Pen(BLACK.Color, 4.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen RED_B = new Pen(RED.Color, 4.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen GREEN_B = new Pen(GREEN.Color, 4.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen BLUE_B = new Pen(BLUE.Color, 4.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen MAGENTA_B = new Pen(MAGENTA.Color, 4.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen YELLOW_B = new Pen(YELLOW.Color, 4.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };

		public enum Colors {
			BLACK,
			RED,
			GREEN,
			BLUE,
			MAGENTA,
			YELLOW
		}

		protected Colors mColor { get; private set; }
		protected Pen mPen { get; private set; }
		protected Pen mPenB { get; private set; }

		void SetColor() {
			switch (mColor) {
			case Colors.BLACK:
				mPen = BLACK;
				mPenB = BLACK_B;
				break;
			case Colors.RED:
				mPen = RED;
				mPenB = RED_B;
				break;
			case Colors.GREEN:
				mPen = GREEN;
				mPenB = GREEN_B;
				break;
			case Colors.BLUE:
				mPen = BLUE;
				mPenB = BLUE_B;
				break;
			case Colors.MAGENTA:
				mPen = MAGENTA;
				mPenB = MAGENTA_B;
				break;
			case Colors.YELLOW:
				mPen = YELLOW;
				mPenB = YELLOW_B;
				break;
			default:
				return;
			}
		}

		protected PointF NeerPoint(Point point) {
			var abX = End.X - Begin.X;
			var abY = End.Y - Begin.Y;
			var apX = point.X - Begin.X;
			var apY = point.Y - Begin.Y;
			var abL2 = abX * abX + abY * abY;
			if (0.0 == abL2) {
				return Begin;
			}
			var r = (double)(abX * apX + abY * apY) / abL2;
			if (r <= 0.0) {
				return Begin;
			} else if (1.0 <= r) {
				return End;
			} else {
				return new PointF(
					(float)(Begin.X + r * abX),
					(float)(Begin.Y + r * abY)
				);
			}
		}

		protected Wire() { }

		public Wire(string[] cols) {
			Height = 100;
			Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			End = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
			mColor = (Colors)Enum.Parse(typeof(Colors), cols[5]);
			SetColor();
		}

		public Wire(Point begin, Point end, Colors color) {
			Height = 100;
			Begin = begin;
			End = end;
			mColor = color;
			SetColor();
		}

		public override Item Clone() {
			return new Wire(Begin, End, mColor);
		}

		public override Point[] GetTerminals() {
			return new Point[] { Begin, End };
		}

		public override double Distance(Point point) {
			var n = NeerPoint(point);
			var sx = point.X - n.X;
			var sy = point.Y - n.Y;
			return Math.Sqrt(sx * sx + sy * sy);
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"WIRE\t{0}\t{1}\t{2}\t{3}\t{4}",
				Begin.X, Begin.Y,
				End.X, End.Y,
				mColor
			);
		}

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			var nx = End.X - Begin.X;
			var ny = End.Y - Begin.Y;
			var r = Math.Sqrt(nx * nx + ny * ny);
			nx = (int)(nx * 2 / r);
			ny = (int)(ny * 2 / r);
			var x1 = Begin.X + dx + nx;
			var y1 = Begin.Y + dy + ny;
			var x2 = End.X + dx - nx;
			var y2 = End.Y + dy - ny;
			if (selected) {
				g.DrawLine(SELECT_COLOR, x1, y1, x2, y2);
			} else {
				if (Reverse) {
					g.DrawLine(REVERSE, x1, y1, x2, y2);
				} else {
					g.DrawLine(mPenB, x1, y1, x2, y2);
				}
			}
		}
	}

	class Wrap : Wire {
		static readonly Pen REVERSE = new Pen(Color.FromArgb(211, 211, 211), 2.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };

		Wrap() { }

		public Wrap(string[] cols) : base(cols) {
			Height = -100;
		}

		public Wrap(Point begin, Point end, Colors color) : base(begin, end, color) {
			Height = -100;
		}

		public override Item Clone() {
			return new Wrap(Begin, End, mColor);
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"WRAP\t{0}\t{1}\t{2}\t{3}\t{4}",
				Begin.X, Begin.Y,
				End.X, End.Y,
				mColor
			);
		}

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			var x1 = Begin.X + dx;
			var y1 = Begin.Y + dy;
			var x2 = End.X + dx;
			var y2 = End.Y + dy;
			if (selected) {
				g.DrawLine(SELECT_COLOR, x1, y1, x2, y2);
			} else {
				if (Reverse) {
					g.DrawLine(mPen, x1, y1, x2, y2);
				} else {
					g.DrawLine(REVERSE, x1, y1, x2, y2);
				}
			}
		}
	}

	class Tin : Wire {
		static readonly Pen COLOR = new Pen(Color.FromArgb(147, 147, 147), 1.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };
		static readonly Pen COLOR_B = new Pen(COLOR.Color, 4.0f) { StartCap = LineCap.Triangle, EndCap = LineCap.Triangle };

		public static bool Enable { get; set; } = false;

		public Tin(string[] cols) {
			Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			End = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
			Height = -0.02;
		}

		public Tin(Point begin, Point end) {
			Begin = begin;
			End = end;
			Height = -0.02;
		}

		public override Item Clone() {
			return new Tin(Begin, End);
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"TIN\t{0}\t{1}\t{2}\t{3}",
				Begin.X, Begin.Y,
				End.X, End.Y
			);
		}

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			var x1 = Begin.X + dx;
			var y1 = Begin.Y + dy;
			var x2 = End.X + dx;
			var y2 = End.Y + dy;
			if (selected) {
				g.DrawLine(SELECT_COLOR, x1, y1, x2, y2);
			} else {
				if (Enable) {
					g.DrawLine(COLOR_B, x1, y1, x2, y2);
				} else {
					g.DrawLine(COLOR, x1, y1, x2, y2);
				}
			}
		}
	}

	class Parts : Item {
		public enum ROTATE {
			NONE,
			DEG90,
			DEG180,
			DEG270
		}
		public enum EDisplay {
			INVISIBLE,
			TRANSPARENT,
			SOLID,
		}
		public static EDisplay Display { get; set; }

		public readonly ROTATE Rotate;
		public readonly int Center;
		public readonly string Group;
		public readonly string Name;
		public readonly string PackageName;

		readonly Package mPackage;

		public Parts(string[] cols) {
			Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			End = Begin;
			Rotate = (ROTATE)int.Parse(cols[3]);
			Group = cols[4];
			Name = "";
			PackageName = cols[5];
			if (Package.Find(Group, PackageName)) {
				mPackage = Package.Get(Group, PackageName);
				Center = mPackage.Center;
				Height = mPackage.IsSMD ? -mPackage.Height : mPackage.Height;
			} else {
				mPackage = null;
				Center = 0;
				Height = 0;
			}
		}

		public Parts(Point pos, ROTATE rotate, string group, string package) {
			Begin = pos;
			End = pos;
			Rotate = rotate;
			Group = group;
			Name = "";
			PackageName = package;
			if (Package.Find(group, package)) {
				mPackage = Package.Get(group, package);
				Center = mPackage.Center;
				Height = mPackage.IsSMD ? -mPackage.Height : mPackage.Height;
			} else {
				mPackage = null;
				Center = 0;
				Height = 0;
			}
		}

		public override Item Clone() {
			return new Parts(Begin, Rotate, Group, PackageName);
		}

		public override bool IsSelected(Point point) {
			return (!Reverse ^ mPackage.IsSMD) && base.IsSelected(point);
		}

		public override Point[] GetTerminals() {
			if (null == mPackage) {
				return new Point[0];
			}
			var terminals = mPackage.Terminals;
			var points = new Point[terminals.Count];
			var ofs = Center - 1;
			for (int i = 0; i < terminals.Count; i++) {
				var term = terminals[i];
				points[i] = Begin;
				switch (Rotate) {
				case ROTATE.DEG90:
					points[i].X += ofs - term.Y;
					points[i].Y += term.X - ofs;
					break;
				case ROTATE.DEG180:
					points[i].X += ofs - term.X;
					points[i].Y += ofs - term.Y;
					break;
				case ROTATE.DEG270:
					points[i].X += term.Y - ofs;
					points[i].Y += ofs - term.X;
					break;
				default:
					points[i].X += term.X - ofs;
					points[i].Y += term.Y - ofs;
					break;
				}
			}
			return points;
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"PARTS\t{0}\t{1}\t{2}\t{3}\t{4}",
				Begin.X, Begin.Y,
				(int)Rotate,
				Group,
				PackageName
			);
		}

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			if (null == mPackage) {
				return;
			}
			if (!selected && Display == EDisplay.INVISIBLE) {
				return;
			}
			Bitmap bmp;
			if (Display == EDisplay.TRANSPARENT) {
				bmp = (selected ? mPackage.Solid : mPackage.Alpha)[(int)Rotate];
			} else if (selected || (Reverse ^ mPackage.IsSMD)) {
				bmp = mPackage.Alpha[(int)Rotate];
			} else {
				bmp = mPackage.Solid[(int)Rotate];
			}
			var x = Begin.X + dx;
			var y = Begin.Y + dy;
			g.DrawImage(bmp, new Point(x - Center, y - Center));
		}
	}
}