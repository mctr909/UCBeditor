using System.Drawing;
using System.IO;

namespace UCB {
	class Land : Item {
		public readonly Item Parent;
		readonly int mIndex;
		readonly PointF[] mFoot;

		public Land(Point pos, Item parent) {
			Begin = pos;
			End = pos;
			if (parent is Wire) {
				Height = -0.005;
			} else {
				Height = -0.01;
			}
			Parent = parent;
		}

		public Land(Point pos, Item parent, int index) {
			Begin = pos;
			End = pos;
			if (parent is Wire) {
				Height = -0.005;
			} else {
				Height = -0.01;
			}
			if (parent is Parts parts) {
				mFoot = parts.GetFoot(index, true, false);
			}
			Parent = parent;
			mIndex = index;
		}

		public override Item Clone() { return null; }

		public override void Write(StreamWriter sw) { }

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			if (Parent is Wire) {
				var r = 2;
				var d = r * 2;
				var px = Begin.X + dx - r;
				var py = Begin.Y + dy - r;
				g.FillEllipse(Brushes.White, px, py, d, d);
				g.DrawEllipse(Pens.Black, px, py, d, d);
			} else if (null == mFoot) {
				var x1 = Begin.X + dx - 5;
				var y1 = Begin.Y + dy - 5;
				var x2 = Begin.X + dx - 2;
				var y2 = Begin.Y + dy - 2;
				if (Reverse) {
					g.FillEllipse(LAND.Brush, x1, y1, 10, 10);
					g.FillEllipse(Brushes.White, x2, y2, 4, 4);
				} else {
					g.FillEllipse(Brushes.White, x2, y2, 4, 4);
					g.DrawEllipse(Pens.Gray, x2, y2, 4, 4);
				}
			} else {
				if (Reverse) {
					g.FillPolygon(LAND.Brush, mFoot);
				} else {
					g.FillPolygon(PATTERN.Brush, mFoot);
				}
			}
		}

		public override void DrawPattern(PDF.Page page) {
			if (Parent is Wire) {
				page.FillColor = Color.Black;
				page.FillCircle(Begin, WireLandDiameter * GridScale);
				page.FillColor = Color.White;
				page.FillCircle(Begin, WireHoleDiameter * GridScale);
			} else if (mFoot != null && Parent is Parts parts) {
				var foot = parts.GetFoot(mIndex, false, false);
				page.FillColor = Color.Black;
				page.FillPolygon(foot);
			} else {
				page.FillColor = Color.Black;
				page.FillCircle(Begin, TermLandDiameter * GridScale);
				page.FillColor = Color.White;
				page.FillCircle(Begin, TermHoleDiameter * GridScale);
			}
		}

		public override void DrawSolderMask(PDF.Page page) {
			if (Parent is Wire) {
				page.FillColor = Color.Black;
				page.FillCircle(Begin, (WireLandDiameter + ResistMaskClearance) * GridScale);
			} else if (mFoot != null && Parent is Parts parts) {
				var foot = parts.GetFoot(mIndex, false, true);
				page.FillColor = Color.Black;
				page.FillPolygon(foot);
			} else {
				page.FillColor = Color.Black;
				page.FillCircle(Begin, (TermLandDiameter + ResistMaskClearance) * GridScale);
			}
		}
	}
}
