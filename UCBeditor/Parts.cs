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
			var ofsX = (int)(pivot.X + offset.X);
			var ofsY = (int)(pivot.Y + offset.Y);
			switch (mRotate) {
			case ROTATE.DEG90:
			case ROTATE.DEG270:
				mImagePos = new Point(-ofsY, -ofsX);
				break;
			case ROTATE.DEG180:
			default:
				mImagePos = new Point(-ofsX, -ofsY);
				break;
			}
			var terminals = mPackage.BodyImage.PinList;
			mTermPos = new Point[terminals.Count];
			for (int i = 0; i < terminals.Count; i++) {
				var term = terminals[i];
				var p = new Point();
				switch (mRotate) {
				case ROTATE.DEG90:
					p.X = ofsY - term.Y;
					p.Y = term.X - ofsX;
					break;
				case ROTATE.DEG180:
					p.X = ofsX - term.X;
					p.Y = ofsY - term.Y;
					break;
				case ROTATE.DEG270:
					p.X = term.Y - ofsY;
					p.Y = ofsX - term.X;
					break;
				default:
					p.X = term.X - ofsX;
					p.Y = term.Y - ofsY;
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
