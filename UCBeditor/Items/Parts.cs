using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Items {
	public class Parts : Item {
		public enum EDisplay {
			INVISIBLE,
			TRANSPARENT,
			VISIBLE,
		}

		public static EDisplay Display { get; set; }

		public readonly string Group;
		public readonly string PackageName;
		public readonly bool IsSMD;

		private ROTATE mRotate;
		private Point mImageOfs;
		private PartsBody.Pin[] mPins;
		private readonly PartsBody mBody = new PartsBody();
		private readonly PartsFoot mFoot = new PartsFoot();
		private readonly Bitmap[] mSolid = new Bitmap[4];
		private readonly Bitmap[] mAlpha = new Bitmap[4];

		public Parts() {
		}

		public Parts(string[] cols) {
			mRotate = (ROTATE)int.Parse(cols[3]);
			mPosition = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			Group = cols[4];
			PackageName = cols[5];
			if (Package.Get(Group, PackageName, out Package package)) {
				Height = package.IsSMD ? -package.Height : package.Height;
				IsSMD = package.IsSMD;
				mFoot.CopyFrom(package.Foot);
				mBody.CopyFrom(package.Body);
				mBody.Get(mRotate, out mImageOfs, out mPins);
				Array.Copy(package.Solid, mSolid, mSolid.Length);
				Array.Copy(package.Alpha, mAlpha, mAlpha.Length);
			} else {
				throw new Exception($"パッケージが見つかりません。\r\n\tグループ:{Group}\r\n\tパッケージ:{PackageName}");
			}
		}

		public Parts(string group, string packageName) {
			Group = group;
			PackageName = packageName;
			if (Package.Get(Group, PackageName, out Package package)) {
				Height = package.IsSMD ? -package.Height : package.Height;
				IsSMD = package.IsSMD;
				mFoot.CopyFrom(package.Foot);
				mBody.CopyFrom(package.Body);
				mBody.Get(mRotate, out mImageOfs, out mPins);
				Array.Copy(package.Solid, mSolid, mSolid.Length);
				Array.Copy(package.Alpha, mAlpha, mAlpha.Length);
			} else {
				throw new Exception($"パッケージが見つかりません。\r\n\tグループ:{Group}\r\n\tパッケージ:{PackageName}");
			}
		}

		public void SetPosition(ROTATE r, Point p) {
			mRotate = r;
			mPosition = p;
			mBody.Get(mRotate, out mImageOfs, out mPins);
		}

		public PointF[] GetFoot(int index, bool round, bool solder) {
			mBody.GetTranslatedPos(mRotate, mPosition, out Point p);
			return mFoot.Get(mRotate, p, index, round, solder);
		}

		public List<PointF[]> GetMarks() {
			return mFoot.GetMarks(mRotate, mPosition);
		}

		public override Item Clone() {
			var ret = new Parts(Group, PackageName);
			ret.SetPosition(mRotate, mPosition);
			return ret;
		}

		public override Point[] GetTerminals() {
			var terms = new Point[mPins.Length];
			for (int i = 0; i < mPins.Length; i++) {
				terms[i].X = mPosition.X + mPins[i].X;
				terms[i].Y = mPosition.Y + mPins[i].Y;
			}
			return terms;
		}

		public override bool IsSelected(Point point) {
			return (!SolderFace ^ IsSMD) && base.IsSelected(point);
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

		public override void DrawDisplay(Graphics g, int dx, int dy, bool selected) {
			if (!selected && Display == EDisplay.INVISIBLE) {
				return;
			}
			var px = mPosition.X + mImageOfs.X + dx;
			var py = mPosition.Y + mImageOfs.Y + dy;
			if (Display == EDisplay.TRANSPARENT) {
				var bmp = selected ? mSolid: mAlpha;
				g.DrawImage(bmp[(int)mRotate], px, py);
			} else if (selected || (SolderFace ^ IsSMD)) {
				g.DrawImage(mAlpha[(int)mRotate], px, py);
			} else {
				g.DrawImage(mSolid[(int)mRotate], px, py);
			}
		}

		public override void DrawPattern(IDrawer g) {
			var marks = GetMarks();
			g.FillColor = Color.Black;
			foreach (var mark in marks) {
				g.FillPolygon(mark);
			}
		}

		public override void DrawSilk(IDrawer g) {
		}
	}
}
