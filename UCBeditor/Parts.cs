using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace UCB {
	class Parts : Item {
		public enum EDisplay {
			INVISIBLE,
			TRANSPARENT,
			SOLID,
		}
		public static EDisplay Display { get; set; }

		public readonly string Group;
		public readonly string Name;
		public readonly string PackageName;

		readonly ROTATE mRotate;
		readonly Package mPackage;
		Point mImagePos;
		Point[] mTermPos;

		void SetPos() {
			var pivot = mPackage.BodyImage.Pivot;
			var offset = mPackage.BodyImage.Offset;
			switch (mRotate) {
			case ROTATE.DEG90:
			case ROTATE.DEG270:
				mImagePos = new Point(-(int)(offset.Y + pivot.Y), -(int)(offset.X + pivot.X));
				break;
			case ROTATE.DEG180:
			default:
				mImagePos = new Point(-(int)(offset.X + pivot.X), -(int)(offset.Y + pivot.Y));
				break;
			}
			var terminals = mPackage.BodyImage.PinList;
			mTermPos = new Point[terminals.Count];
			for (int i = 0; i < terminals.Count; i++) {
				var term = terminals[i];
				var p = mImagePos;
				switch (mRotate) {
				case ROTATE.DEG90:
				case ROTATE.DEG270:
					p.X += term.Y;
					p.Y += term.X;
					break;
				case ROTATE.DEG180:
				default:
					p.X += term.X;
					p.Y += term.Y;
					break;
				}
				mTermPos[i] = p;
			}
		}

		public Parts(string[] cols) {
			mPosition = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			mRotate = (ROTATE)int.Parse(cols[3]);
			Group = cols[4];
			PackageName = cols[5];
			Name = "";
			if (Package.Find(Group, PackageName)) {
				mPackage = Package.Get(Group, PackageName);
				Height = mPackage.IsSMD ? -mPackage.Height : mPackage.Height;
			} else {
				mPackage = null;
				Height = 0;
			}
			SetPos();
		}

		public Parts(Point pos, ROTATE rotate, string group, string package) {
			mPosition = pos;
			mRotate = rotate;
			Group = group;
			PackageName = package;
			Name = "";
			if (Package.Find(group, package)) {
				mPackage = Package.Get(group, package);
				Height = mPackage.IsSMD ? -mPackage.Height : mPackage.Height;
			} else {
				mPackage = null;
				Height = 0;
			}
			SetPos();
		}

		public PointF[] GetFoot(int index, bool round, bool solder) {
			var offset = mPackage.BodyImage.Offset;
			var pos = mPosition;
			switch (mRotate) {
			case ROTATE.DEG90:
			case ROTATE.DEG270:
				pos.X -= offset.Y;
				pos.Y -= offset.X;
				break;
			case ROTATE.DEG180:
			default:
				pos.X -= offset.X;
				pos.Y -= offset.Y;
				break;
			}
			return mPackage.FootPrint.Get(pos, mRotate, index, round, solder);
		}

		public List<PointF[]> GetMarks(bool round) {
			return mPackage.FootPrint.GetMarks(mPosition, mRotate, round);
		}

		public override Item Clone() {
			return new Parts(mPosition, mRotate, Group, PackageName);
		}

		public override Point[] GetTerminals() {
			var terms = new Point[mTermPos.Length];
			for (int i = 0; i < terms.Length; i++) {
				terms[i] = mTermPos[i];
				terms[i].X += mPosition.X;
				terms[i].Y += mPosition.Y;
			}
			return terms;
		}

		public override bool IsSelected(Point point) {
			return (!SolderFace ^ mPackage.IsSMD) && base.IsSelected(point);
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"PARTS\t{0}\t{1}\t{2}\t{3}\t{4}",
				mPosition.X, mPosition.Y,
				(int)mRotate,
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
			var px = mPosition.X + mImagePos.X + dx;
			var py = mPosition.Y + mImagePos.Y + dy;
			if (Display == EDisplay.TRANSPARENT) {
				var bmp = selected ? mPackage.Solid: mPackage.Alpha;
				g.DrawImage(bmp[(int)mRotate], px, py);
			} else if (selected || (SolderFace ^ mPackage.IsSMD)) {
				g.DrawImage(mPackage.Alpha[(int)mRotate], px, py);
			} else {
				g.DrawImage(mPackage.Solid[(int)mRotate], px, py);
			}
		}

		public override void DrawPattern(PDF.Page page) {
			var marks = GetMarks(false);
			page.FillColor = Color.Black;
			foreach (var mark in marks) {
				page.FillPolygon(mark);
			}
		}
	}
}
