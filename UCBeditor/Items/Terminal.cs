using System.Drawing;
using System.IO;

namespace Items {
	class Terminal : Item {
		public Terminal(string[] cols) {
			mPosition = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			Height = -0.01;
		}

		public Terminal(Point pos) {
			mPosition = pos;
			Height = -0.01;
		}

		public override Item Clone() {
			return new Terminal(mPosition);
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine("TERM\t{0}\t{1}", mPosition.X, mPosition.Y);
		}

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			var x1 = mPosition.X + dx - 5;
			var y1 = mPosition.Y + dy - 5;
			var x2 = mPosition.X + dx - 2;
			var y2 = mPosition.Y + dy - 2;
			if (selected) {
				g.DrawArc(SELECT_COLOR, x1, y1, 10, 10, 0, 360);
				g.DrawArc(SELECT_COLOR, x2, y2, 4, 4, 0, 360);
			} else {
				if (SolderFace) {
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
			page.FillCircle(mPosition, TermLandDiameter * GridScale);
			page.FillColor = Color.White;
			page.FillCircle(mPosition, TermHoleDiameter * GridScale);
		}

		public override void DrawSolderMask(PDF.Page page) {
			page.FillColor = Color.Black;
			page.FillCircle(mPosition, (TermLandDiameter + ResistMaskClearance) * GridScale);
		}
	}
}
