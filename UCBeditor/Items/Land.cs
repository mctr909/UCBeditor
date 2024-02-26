using System.Drawing;
using System.IO;

namespace Items {
	class Land : Item {
		public readonly Item Parent;
		readonly int mIndex;
		readonly PointF[] mFoot;

		public Land(Point pos, Item parent) {
			mPosition = pos;
			if (parent is Wire) {
				Height = -0.015;
			} else {
				Height = -0.01;
			}
			Parent = parent;
		}

		public Land(Point pos, Item parent, int index) {
			mPosition = pos;
			Height = -0.01;
			if (parent is Parts parts) {
				mFoot = parts.GetFoot(index, true, false);
			}
			Parent = parent;
			mIndex = index;
		}

		public override Item Clone() { return null; }

		public override void Write(StreamWriter sw) { }

		public override void DrawDisplay(Graphics g, int dx, int dy, bool selected) {
			if (Parent is Wire) {
				var r = 2;
				var d = r * 2;
				var px = mPosition.X + dx - r;
				var py = mPosition.Y + dy - r;
				g.FillEllipse(Brushes.White, px, py, d, d);
				g.DrawEllipse(Pens.Black, px, py, d, d);
			} else if (null == mFoot) {
				if (SolderFace) {
					var x1 = mPosition.X + dx - 5;
					var y1 = mPosition.Y + dy - 5;
					var x2 = mPosition.X + dx - 2;
					var y2 = mPosition.Y + dy - 2;
					g.FillEllipse(LAND.Brush, x1, y1, 10, 10);
					g.FillEllipse(Brushes.White, x2, y2, 4, 4);
				} else {
					var x = mPosition.X + dx - 2;
					var y = mPosition.Y + dy - 2;
					g.FillEllipse(Brushes.White, x, y, 4, 4);
					g.DrawEllipse(Pens.Gray, x, y, 4, 4);
				}
			} else {
				if (SolderFace) {
					g.FillPolygon(LAND.Brush, mFoot);
				} else {
					g.FillPolygon(PATTERN.Brush, mFoot);
				}
			}
		}

		public override void DrawPattern(IDrawer g) {
			if (Parent is Wire) {
				g.FillColor = Color.Black;
				g.FillCircle(mPosition, WireLandDiameter * GridScale);
				g.FillColor = Color.White;
				g.FillCircle(mPosition, WireHoleDiameter * GridScale);
			} else if (mFoot != null && Parent is Parts parts) {
				var foot = parts.GetFoot(mIndex, false, false);
				g.FillColor = Color.Black;
				g.FillPolygon(foot);
			} else {
				g.FillColor = Color.Black;
				g.FillCircle(mPosition, TermLandDiameter * GridScale);
				g.FillColor = Color.White;
				g.FillCircle(mPosition, TermHoleDiameter * GridScale);
			}
		}

		public override void DrawSolderMask(IDrawer g) {
			if (Parent is Wire) {
				g.FillColor = Color.Black;
				g.FillCircle(mPosition, (WireLandDiameter + ResistMaskClearance) * GridScale);
			} else if (mFoot != null && Parent is Parts parts) {
				var foot = parts.GetFoot(mIndex, false, true);
				g.FillColor = Color.Black;
				g.FillPolygon(foot);
			} else {
				g.FillColor = Color.Black;
				g.FillCircle(mPosition, (TermLandDiameter + ResistMaskClearance) * GridScale);
			}
		}
	}
}
