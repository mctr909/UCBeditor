using System.Collections.Generic;
using System.Drawing;

namespace Items {
	public class PartsBody {
		public struct Pin {
			public int X;
			public int Y;
			public string Name;
		}

		private int mOffsetX;
		private int mOffsetY;
		private int mPivotX;
		private int mPivotY;
		private readonly List<Pin> mPinList = new List<Pin>();

		public void CopyFrom(PartsBody src) {
			mOffsetX = src.mOffsetX;
			mOffsetY = src.mOffsetY;
			mPivotX = src.mPivotX;
			mPivotY = src.mPivotY;
			foreach (var pin in src.mPinList) {
				mPinList.Add(pin);
			}
		}

		public void SetOffset(int x, int y) {
			mOffsetX = x;
			mOffsetY = y;
		}

		public void SetPivot(int x, int y) {
			mPivotX = x;
			mPivotY = y;
		}

		public void AddPin(int x, int y, string name) {
			var pin = new Pin
			{
				X = x,
				Y = y,
				Name = name
			};
			mPinList.RemoveAll(v => v.X == pin.X && v.Y == pin.Y);
			mPinList.Add(pin);
		}

		public void Get(ROTATE rotate, out Point imageOfs, out Pin[] pins) {
			var x = mPivotX + mOffsetX;
			var y = mPivotY + mOffsetY;
			switch (rotate) {
			case ROTATE.DEG90:
			case ROTATE.DEG270:
				imageOfs = new Point(-y, -x);
				break;
			case ROTATE.DEG180:
			default:
				imageOfs = new Point(-x, -y);
				break;
			}
			pins = new Pin[mPinList.Count];
			for (int i = 0; i < mPinList.Count; i++) {
				var term = mPinList[i];
				int px, py;
				switch (rotate) {
				case ROTATE.DEG90:
					px = y - term.Y;
					py = term.X - x;
					break;
				case ROTATE.DEG180:
					px = x - term.X;
					py = y - term.Y;
					break;
				case ROTATE.DEG270:
					px = term.Y - y;
					py = x - term.X;
					break;
				default:
					px = term.X - x;
					py = term.Y - y;
					break;
				}
				pins[i].Name = term.Name;
				pins[i].X = px;
				pins[i].Y = py;
			}
		}

		public void GetTranslatedPos(ROTATE rotate, Point p, out Point translatedPos) {
			translatedPos = new Point(p.X, p.Y);
			switch (rotate) {
			case ROTATE.DEG90:
			case ROTATE.DEG270:
				translatedPos.X -= mOffsetY;
				translatedPos.Y -= mOffsetX;
				break;
			case ROTATE.DEG180:
			default:
				translatedPos.X -= mOffsetX;
				translatedPos.Y -= mOffsetY;
				break;
			}
		}
	}
}
