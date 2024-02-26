using System;
using System.Collections.Generic;
using System.Drawing;

namespace Items {
	public class PartsFoot {
		private struct Pin {
			public double X;
			public double Y;
			public string Link;
		}

		private struct Polygon {
			public double Margin;
			public PointF[] Vertex;
		}

		private double mOffsetX;
		private double mOffsetY;
		private readonly List<Pin> mPinList = new List<Pin>();
		private readonly List<Pin> mMarkList = new List<Pin>();
		private readonly Dictionary<string, Polygon> mPolygonList = new Dictionary<string, Polygon>();

		public void CopyFrom(PartsFoot src) {
			mOffsetX = src.mOffsetX;
			mOffsetY = src.mOffsetY;
			foreach (var pin in src.mPinList) {
				mPinList.Add(pin);
			}
			foreach (var mark in src.mMarkList) {
				mMarkList.Add(mark);
			}
			foreach (var poly in src.mPolygonList) {
				mPolygonList.Add(poly.Key, poly.Value);
			}
		}

		public void SetOffset(double x, double y) {
			mOffsetX = x;
			mOffsetY = y;
		}

		public void AddPin(double x, double y, string link) {
			var pin = new Pin
			{
				X = x,
				Y = y,
				Link = link
			};
			mPinList.RemoveAll(v => v.X == pin.X && v.Y == pin.Y && v.Link == pin.Link);
			mPinList.Add(pin);
		}

		public void AddMark(double x, double y, string link) {
			var pin = new Pin
			{
				X = x,
				Y = y,
				Link = link
			};
			mMarkList.RemoveAll(v => v.X == pin.X && v.Y == pin.Y && v.Link == pin.Link);
			mMarkList.Add(pin);
		}

		public void AddPolygon(string name, double margin, params PointF[] vertex) {
			mPolygonList.Remove(name);
			mPolygonList.Add(name, new Polygon()
			{
				Margin = margin,
				Vertex = vertex
			});
		}

		public PointF[] Get(ROTATE rotate, PointF p, int index, bool round, bool solder) {
			if (mPinList.Count <= index) {
				return null;
			}
			double rotX, rotY;
			switch (rotate) {
			case ROTATE.DEG90:
				rotX = 0;
				rotY = Item.GridScale;
				break;
			case ROTATE.DEG180:
				rotX = -Item.GridScale;
				rotY = 0;
				break;
			case ROTATE.DEG270:
				rotX = 0;
				rotY = -Item.GridScale;
				break;
			case ROTATE.NONE:
			default:
				rotX = Item.GridScale;
				rotY = 0;
				break;
			}
			var pin = mPinList[index];
			var poly = mPolygonList[pin.Link];
			var verts = new PointF[poly.Vertex.Length];
			double margin;
			if (solder) {
				margin = poly.Margin;
			} else {
				margin = 0;
			}
			for (var i = 0; i < verts.Length; i++) {
				var v = poly.Vertex[i];
				var vx = (double)v.X;
				var vy = (double)v.Y;
				var r = Math.Sqrt(vx * vx + vy * vy) + margin;
				var th = Math.Atan2(vy, vx);
				vx = Math.Cos(th) * r;
				vy = Math.Sin(th) * r;
				vx += pin.X - mOffsetX;
				vy += pin.Y + mOffsetY;
				var rx = p.X + vx * rotX - vy * rotY;
				var ry = p.Y + vy * rotX + vx * rotY;
				if (round) {
					rx = (int)(rx + Math.Sign(rx) * 0.5);
					ry = (int)(ry + Math.Sign(ry) * 0.5);
				}
				verts[i] = new PointF((float)rx, (float)ry);
			}
			return verts;
		}

		public List<PointF[]> GetMarks(ROTATE rotate, Point p) {
			double rotX, rotY;
			switch (rotate) {
			case ROTATE.DEG90:
				rotX = 0;
				rotY = Item.GridScale;
				break;
			case ROTATE.DEG180:
				rotX = -Item.GridScale;
				rotY = 0;
				break;
			case ROTATE.DEG270:
				rotX = 0;
				rotY = -Item.GridScale;
				break;
			case ROTATE.NONE:
			default:
				rotX = Item.GridScale;
				rotY = 0;
				break;
			}
			var ret = new List<PointF[]>();
			foreach (var mark in mMarkList) {
				var poly = mPolygonList[mark.Link];
				var verts = new PointF[poly.Vertex.Length];
				for (var i = 0; i < verts.Length; i++) {
					var v = poly.Vertex[i];
					var vx = (double)v.X;
					var vy = (double)v.Y;
					vx += mark.X - mOffsetX;
					vy += mark.Y + mOffsetY;
					var rx = p.X + vx * rotX - vy * rotY;
					var ry = p.Y + vy * rotX + vx * rotY;
					verts[i] = new PointF((float)rx, (float)ry);
				}
				ret.Add(verts);
			}
			return ret;
		}
	}
}
