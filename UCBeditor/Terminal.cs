using System.Drawing;
using System.IO;

namespace UCB {
	class Terminal : Item {
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
					g.FillEllipse(LAND.Brush, x1, y1, 10, 10);
					g.FillEllipse(Brushes.White, x2, y2, 4, 4);
				} else {
					g.FillEllipse(Brushes.White, x2, y2, 4, 4);
					g.DrawEllipse(Pens.Black, x2, y2, 4, 4);
				}
			}
		}

		public override void DrawPattern(PDF.Page page) {
			page.FillColor = Color.Black;
			page.FillCircle(Begin, TermLandDiameter * GridScale);
			page.FillColor = Color.White;
			page.FillCircle(Begin, TermHoleDiameter * GridScale);
		}

		public override void DrawSolderMask(PDF.Page page) {
			page.FillColor = Color.Black;
			page.FillCircle(Begin, (TermLandDiameter + ResistMaskClearance) * GridScale);
		}
	}
}
