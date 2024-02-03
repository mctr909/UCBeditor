using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;

namespace Items {
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

		protected Point mEnd;

		readonly Pen mPen;
		readonly Colors mColor;
		readonly bool mReverse;

		void SetColor(out Pen pen) {
			switch (mColor) {
			case Colors.BLACK:
				pen = BLACK;
				break;
			case Colors.RED:
				pen = RED;
				break;
			case Colors.GREEN:
				pen = GREEN;
				break;
			case Colors.BLUE:
				pen = BLUE;
				break;
			case Colors.MAGENTA:
				pen = MAGENTA;
				break;
			case Colors.YELLOW:
				pen = YELLOW;
				break;
			default:
				pen = BLACK;
				break;
			}
		}

		PointF NearPoint(Point point) {
			var abX = mEnd.X - mPosition.X;
			var abY = mEnd.Y - mPosition.Y;
			var apX = point.X - mPosition.X;
			var apY = point.Y - mPosition.Y;
			var abL2 = abX * abX + abY * abY;
			if (0.0 == abL2) {
				return mPosition;
			}
			var r = (double)(abX * apX + abY * apY) / abL2;
			if (r <= 0.0) {
				return mPosition;
			} else if (1.0 <= r) {
				return mEnd;
			} else {
				return new PointF(
					(float)(mPosition.X + r * abX),
					(float)(mPosition.Y + r * abY)
				);
			}
		}

		protected Wire() { }

		public Wire(string[] cols) {
			mReverse = "WRAP" == cols[0];
			Height = mReverse ? -100 : 100;
			mPosition = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			mEnd = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
			mColor = (Colors)Enum.Parse(typeof(Colors), cols[5]);
			SetColor(out mPen);
		}
		public Wire(Point begin, Point end, Colors color, bool reverse) {
			mReverse = reverse;
			Height = mReverse ? -100 : 100;
			mPosition = begin;
			mEnd = end;
			mColor = color;
			SetColor(out mPen);
		}

		public bool OnMiddle(Point point, double limit = 0) {
			var abX = mEnd.X - mPosition.X;
			var abY = mEnd.Y - mPosition.Y;
			var apX = point.X - mPosition.X;
			var apY = point.Y - mPosition.Y;
			var abL2 = abX * abX + abY * abY;
			if (0 == abL2) {
				return false;
			}
			var r = (double)(abX * apX + abY * apY) / abL2;
			if (0.0 < r && r < 1.0) {
				var px = apX - r * abX;
				var py = apY - r * abY;
				return (px * px + py * py) <= limit;
			} else {
				return false;
			}
		}

		public override void Move(int dx, int dy) {
			mPosition.X += dx;
			mPosition.Y += dy;
			mEnd.X += dx;
			mEnd.Y += dy;
		}
		public override double Distance(Point point) {
			var n = NearPoint(point);
			var sx = point.X - n.X;
			var sy = point.Y - n.Y;
			return Math.Sqrt(sx * sx + sy * sy);
		}
		public override bool IsSelected(Point point) {
			return Wire && mReverse == SolderFace && Distance(point) < SNAP;
		}
		public override bool IsSelected(Rectangle selectArea) {
			return selectArea.Contains(mPosition) || selectArea.Contains(mEnd);
		}
		public override Point[] GetTerminals() {
			return new Point[] { mPosition, mEnd };
		}

		public override Item Clone() {
			return new Wire(mPosition, mEnd, mColor, mReverse);
		}
		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
				mReverse ? "WRAP" : "WIRE",
				mPosition.X, mPosition.Y,
				mEnd.X, mEnd.Y,
				mColor
			);
		}
		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			if (mReverse == SolderFace && !Wire) {
				return;
			}
			var x1 = mPosition.X + dx;
			var y1 = mPosition.Y + dy;
			var x2 = mEnd.X + dx;
			var y2 = mEnd.Y + dy;
			if (selected) {
				g.DrawLine(SELECT_COLOR, x1, y1, x2, y2);
			} else {
				if (mReverse == SolderFace) {
					g.DrawLine(mPen, x1, y1, x2, y2);
				} else {
					g.DrawLine(REVERSE, x1, y1, x2, y2);
				}
			}
		}
	}
}
