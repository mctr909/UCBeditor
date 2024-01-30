using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;

namespace UCB {
	class Wire : Item {
		static readonly Pen REVERSE = new Pen(Color.FromArgb(215, 215, 215), 2.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		static readonly Pen BLACK = new Pen(Color.FromArgb(71, 71, 71), 2.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		static readonly Pen RED = new Pen(Color.FromArgb(211, 63, 63), 2.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		static readonly Pen GREEN = new Pen(Color.FromArgb(47, 167, 47), 2.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		static readonly Pen BLUE = new Pen(Color.FromArgb(63, 63, 221), 2.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		static readonly Pen MAGENTA = new Pen(Color.FromArgb(167, 0, 167), 2.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		static readonly Pen YELLOW = new Pen(Color.FromArgb(191, 191, 0), 2.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

		public enum Colors {
			BLACK,
			RED,
			GREEN,
			BLUE,
			MAGENTA,
			YELLOW
		}

		Pen mPen;
		readonly Colors mColor;
		readonly bool mReverse;

		void SetColor() {
			switch (mColor) {
			case Colors.BLACK:
				mPen = BLACK;
				break;
			case Colors.RED:
				mPen = RED;
				break;
			case Colors.GREEN:
				mPen = GREEN;
				break;
			case Colors.BLUE:
				mPen = BLUE;
				break;
			case Colors.MAGENTA:
				mPen = MAGENTA;
				break;
			case Colors.YELLOW:
				mPen = YELLOW;
				break;
			default:
				return;
			}
		}

		PointF NearPoint(Point point) {
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
			mReverse = "WRAP" == cols[0];
			Height = mReverse ? -100 : 100;
			Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			End = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
			mColor = (Colors)Enum.Parse(typeof(Colors), cols[5]);
			SetColor();
		}

		public Wire(Point begin, Point end, Colors color, bool reverse) {
			mReverse = reverse;
			Height = mReverse ? -100 : 100;
			Begin = begin;
			End = end;
			mColor = color;
			SetColor();
		}

		public bool OnMiddle(Point point) {
			var abX = End.X - Begin.X;
			var abY = End.Y - Begin.Y;
			var apX = point.X - Begin.X;
			var apY = point.Y - Begin.Y;
			var abL2 = abX * abX + abY * abY;
			if (0 == abL2) {
				return false;
			}
			var r = (double)(abX * apX + abY * apY) / abL2;
			if (0.0 < r && r < 1.0) {
				var px = apX - r * abX;
				var py = apY - r * abY;
				return 0 == (px * px + py * py);
			} else {
				return false;
			}
		}

		public override Item Clone() {
			return new Wire(Begin, End, mColor, mReverse);
		}

		public override Point[] GetTerminals() {
			return new Point[] { Begin, End };
		}

		public override bool IsSelected(Point point) {
			return Wire && mReverse == Reverse && Distance(point) < 8.0;
		}

		public override double Distance(Point point) {
			var n = NearPoint(point);
			var sx = point.X - n.X;
			var sy = point.Y - n.Y;
			return Math.Sqrt(sx * sx + sy * sy);
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
				mReverse ? "WRAP" : "WIRE",
				Begin.X, Begin.Y,
				End.X, End.Y,
				mColor
			);
		}

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			if (mReverse == Reverse && !Wire) {
				return;
			}
			var x1 = Begin.X + dx;
			var y1 = Begin.Y + dy;
			var x2 = End.X + dx;
			var y2 = End.Y + dy;
			if (selected) {
				g.DrawLine(SELECT_COLOR, x1, y1, x2, y2);
			} else {
				if (mReverse == Reverse) {
					g.DrawLine(mPen, x1, y1, x2, y2);
				} else {
					g.DrawLine(REVERSE, x1, y1, x2, y2);
				}
			}
		}
	}
}
