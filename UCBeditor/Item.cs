﻿using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;
using System.Collections.Generic;

namespace UCBeditor {
	public enum ROTATE {
		NONE,
		DEG90,
		DEG180,
		DEG270
	}

	public abstract class Item {
		protected static readonly Pen SELECT_COLOR = new Pen(Color.Magenta, 2);
		protected static readonly Pen PATTERN = new Pen(Color.FromArgb(147, 147, 147), 1) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		protected static readonly Pen PATTERN_B = new Pen(PATTERN.Color, 9) { StartCap = LineCap.Round, EndCap = LineCap.Round };
		protected static readonly Pen LAND = new Pen(Color.FromArgb(191, 191, 0), 1) { StartCap = LineCap.Round, EndCap = LineCap.Round };

		public const int GridWidth = 16;
		public const float GridScale = GridWidth / 2.54f;

		public static bool Reverse { get; set; }
		public static bool Pattern { get; set; }
		public static bool Wire { get; set; }
		public static bool Parts { get; set; }

		public static Item Construct(string line) {
			var cols = line.Split('\t');
			switch (cols[0]) {
			case "TIN":
				return new Pattern(cols);
			case "WIRE":
			case "WRAP":
				return new Wire(cols);
			case "PARTS":
				return new Parts(cols);
			case "TERM":
			default:
				return new Terminal(cols);
			}
		}

		public Point Begin;
		public Point End;
		public double Height { get; protected set; }
		public bool Removed { get; set; }

		public bool IsSelected(Rectangle selectArea) {
			return selectArea.Contains(Begin) || selectArea.Contains(End);
		}
		public void Draw(Graphics g, bool selected) {
			Draw(g, 0, 0, selected);
		}

		public virtual double Distance(Point point) {
			var apX = point.X - Begin.X;
			var apY = point.Y - Begin.Y;
			return Math.Sqrt(apX * apX + apY * apY);
		}
		public virtual bool IsSelected(Point point) {
			if (GetType() == typeof(Terminal)) {
				return Distance(point) < 8.0;
			}
			if (GetType() == typeof(Parts)) {
				return Distance(point) < 8.0;
			}
			return false;
		}
		public virtual Point[] GetTerminals() { return new Point[] { Begin }; }

		public virtual void DrawPDF(PDF.Page page) { }

		public abstract Item Clone();
		public abstract void Write(StreamWriter sw);
		public abstract void Draw(Graphics g, int dx, int dy, bool selected);
	}

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

		public override void DrawPDF(PDF.Page page) {
			page.FillColor = Color.Black;
			page.FillCircle(Begin, 1.7 * GridScale);
			page.FillColor = Color.White;
			page.FillCircle(Begin, 0.5 * GridScale);
		}
	}

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
			Parent = parent;
			mIndex = index;
			if (parent is Parts parts) {
				mFoot = parts.GetFoot(index, true);
			}
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
					g.DrawEllipse(PATTERN, x2, y2, 4, 4);
				}
			} else {
				if (Reverse) {
					g.FillPolygon(LAND.Brush, mFoot);
				} else {
					g.FillPolygon(PATTERN.Brush, mFoot);
				}
			}
		}

		public override void DrawPDF(PDF.Page page) {
			if (Parent is Wire) {
				page.FillColor = Color.Black;
				page.FillCircle(Begin, 0.6 * GridScale);
				page.FillColor = Color.White;
				page.FillCircle(Begin, 0.15 * GridScale);
			} else if (null != mFoot) {
				var foot = ((Parts)Parent).GetFoot(mIndex, false);
				page.FillColor = Color.Black;
				page.FillPolygon(foot);
			} else {
				page.FillColor = Color.Black;
				page.FillCircle(Begin, 1.7 * GridScale);
				page.FillColor = Color.White;
				page.FillCircle(Begin, 0.5 * GridScale);
			}
		}
	}

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

		Pen mPen;
		readonly Colors mColor;
		readonly bool mReverse;

		void SetColor() {
			switch (mColor) {
			case Colors.BLACK:
				mPen = BLACK;
				break;
			case Colors.RED:
				mPen = RED;
				break;
			case Colors.GREEN:
				mPen = GREEN;
				break;
			case Colors.BLUE:
				mPen = BLUE;
				break;
			case Colors.MAGENTA:
				mPen = MAGENTA;
				break;
			case Colors.YELLOW:
				mPen = YELLOW;
				break;
			default:
				return;
			}
		}

		PointF NeerPoint(Point point) {
			var abX = End.X - Begin.X;
			var abY = End.Y - Begin.Y;
			var apX = point.X - Begin.X;
			var apY = point.Y - Begin.Y;
			var abL2 = abX * abX + abY * abY;
			if (0.0 == abL2) {
				return Begin;
			}
			var r = (double)(abX * apX + abY * apY) / abL2;
			if (r <= 0.0) {
				return Begin;
			} else if (1.0 <= r) {
				return End;
			} else {
				return new PointF(
					(float)(Begin.X + r * abX),
					(float)(Begin.Y + r * abY)
				);
			}
		}

		protected Wire() { }

		public Wire(string[] cols) {
			mReverse = "WRAP" == cols[0];
			Height = mReverse ? -100 : 100;
			Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			End = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
			mColor = (Colors)Enum.Parse(typeof(Colors), cols[5]);
			SetColor();
		}

		public Wire(Point begin, Point end, Colors color, bool reverse) {
			mReverse = reverse;
			Height = mReverse ? -100 : 100;
			Begin = begin;
			End = end;
			mColor = color;
			SetColor();
		}

		public bool OnMiddle(Point point) {
			var abX = End.X - Begin.X;
			var abY = End.Y - Begin.Y;
			var apX = point.X - Begin.X;
			var apY = point.Y - Begin.Y;
			var abL2 = abX * abX + abY * abY;
			if (0 == abL2) {
				return false;
			}
			var r = (double)(abX * apX + abY * apY) / abL2;
			if (0.0 < r && r < 1.0) {
				var px = apX - r * abX;
				var py = apY - r * abY;
				return 0 == (px * px + py * py);
			} else {
				return false;
			}
		}

		public override Item Clone() {
			return new Wire(Begin, End, mColor, mReverse);
		}

		public override Point[] GetTerminals() {
			return new Point[] { Begin, End };
		}

		public override bool IsSelected(Point point) {
			return Wire && mReverse == Reverse && Distance(point) < 8.0;
		}

		public override double Distance(Point point) {
			var n = NeerPoint(point);
			var sx = point.X - n.X;
			var sy = point.Y - n.Y;
			return Math.Sqrt(sx * sx + sy * sy);
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
				mReverse ? "WRAP" : "WIRE",
				Begin.X, Begin.Y,
				End.X, End.Y,
				mColor
			);
		}

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			if (mReverse == Reverse && !Wire) {
				return;
			}
			var x1 = Begin.X + dx;
			var y1 = Begin.Y + dy;
			var x2 = End.X + dx;
			var y2 = End.Y + dy;
			if (selected) {
				g.DrawLine(SELECT_COLOR, x1, y1, x2, y2);
			} else {
				if (mReverse == Reverse) {
					g.DrawLine(mPen, x1, y1, x2, y2);
				} else {
					g.DrawLine(REVERSE, x1, y1, x2, y2);
				}
			}
		}
	}

	class Pattern : Wire {
		public readonly float Thick = 0.3f;

		public static void Divide(List<Item> list, Item dividerItem) {
			var divTerms = dividerItem.GetTerminals();
			foreach (var divTerm in divTerms) {
				var addList = new List<Item>();
				foreach (var item in list) {
					if (item is Pattern pattern && pattern.OnMiddle(divTerm)) {
						addList.Add(new Pattern(pattern.Begin, divTerm, pattern.Thick));
						pattern.Begin = divTerm;
					}
				}
				list.AddRange(addList);
			}
			if (dividerItem.GetType() == typeof(Wire)) {
				foreach (var divTerm in divTerms) {
					foreach (var item in list) {
						if (item is Pattern pattern && 0 == pattern.Distance(divTerm)) {
							list.Add(new Land(divTerm, dividerItem));
							break;
						}
					}
				}
			}
		}

		public static void Join(List<Item> list) {
			foreach (var itemA in list) {
				if (!(itemA is Pattern patternA) || patternA.Removed) {
					continue;
				}
				Pattern patternB = null;
				{
					var connectedItemFound = false;
					foreach (var itemB in list) {
						if (patternA == itemB || itemB.Removed) {
							continue;
						}
						foreach (var termB in itemB.GetTerminals()) {
							if (patternA.Begin.Equals(termB)) {
								if (itemB is Pattern) {
									if (connectedItemFound) {
										patternB = null;
										break;
									}
									patternB = (Pattern)itemB;
									connectedItemFound = true;
								} else {
									patternB = null;
									connectedItemFound = true;
									break;
								}
							}
						}
						if (connectedItemFound && null == patternB) {
							break;
						}
					}
				}
				if (null == patternB) {
					var connectedItemFound = false;
					foreach (var itemB in list) {
						if (patternA == itemB || itemB.Removed) {
							continue;
						}
						foreach (var termB in itemB.GetTerminals()) {
							if (patternA.End.Equals(termB)) {
								if (itemB is Pattern) {
									if (connectedItemFound) {
										patternB = null;
										break;
									}
									patternB = (Pattern)itemB;
									connectedItemFound = true;
								} else {
									patternB = null;
									connectedItemFound = true;
									break;
								}
							}
						}
						if (connectedItemFound && null == patternB) {
							break;
						}
					}
				}
				if (null != patternB) {
					var ax = patternA.End.X - patternA.Begin.X;
					var ay = patternA.End.Y - patternA.Begin.Y;
					var bx = patternB.End.X - patternB.Begin.X;
					var by = patternB.End.Y - patternB.Begin.Y;
					var cross = ax * by - ay * bx;
					if (0 == cross && (patternA.Thick == patternB.Thick)) {
						patternB.Removed = true;
						if (patternA.Begin.Equals(patternB.Begin)) {
							patternA.Begin = patternB.End;
						} else if (patternA.Begin.Equals(patternB.End)) {
							patternA.Begin = patternB.Begin;
						} else if (patternA.End.Equals(patternB.End)) {
							patternA.End = patternB.Begin;
						} else {
							patternA.End = patternB.End;
						}
					}
				}
			}
			list.RemoveAll(p => p.Removed);
		}

		public Pattern(string[] cols) {
			Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			End = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
			if (6 <= cols.Length) {
				Thick = float.Parse(cols[5]);
			}
			Height = -0.02;
		}

		public Pattern(Point begin, Point end, double thick) {
			Begin = begin;
			End = end;
			Height = -0.02;
			Thick = (float)thick;
		}

		public override Item Clone() {
			return new Pattern(Begin, End, Thick);
		}

		public override bool IsSelected(Point point) {
			return Pattern && Distance(point) < 8.0;
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"TIN\t{0}\t{1}\t{2}\t{3}\t{4}",
				Begin.X, Begin.Y,
				End.X, End.Y,
				Thick
			);
		}

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			var x1 = Begin.X + dx;
			var y1 = Begin.Y + dy;
			var x2 = End.X + dx;
			var y2 = End.Y + dy;
			if (selected) {
				g.DrawLine(SELECT_COLOR, x1, y1, x2, y2);
			} else {
				if (0.5 < Thick) {
					g.DrawLine(PATTERN_B, x1, y1, x2, y2);
				} else {
					g.DrawLine(PATTERN, x1, y1, x2, y2);
				}
			}
		}

		public override void DrawPDF(PDF.Page page) {
			var d = Thick * GridScale;
			var sx = End.X - Begin.X;
			var sy = End.Y - Begin.Y;
			var th = Math.Atan2(sy, sx) + Math.PI / 2;
			var rx = (float)(Math.Cos(th) * d * 0.5);
			var ry = (float)(Math.Sin(th) * d * 0.5);
			page.FillColor = Color.Black;
			page.FillPolygon(new PointF[] {
				new PointF(Begin.X + rx, Begin.Y + ry),
				new PointF(End.X + rx, End.Y + ry),
				new PointF(End.X - rx, End.Y - ry),
				new PointF(Begin.X - rx, Begin.Y - ry)
			});
			page.FillCircle(Begin, d);
			page.FillCircle(End, d);
		}
	}

	class Parts : Item {
		public enum EDisplay {
			INVISIBLE,
			TRANSPARENT,
			SOLID,
		}
		public static EDisplay Display { get; set; }

		public readonly ROTATE Rotate;
		public readonly Point Pivot;
		public readonly string Group;
		public readonly string Name;
		public readonly string PackageName;

		readonly Package mPackage;

		public Parts(string[] cols) {
			Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			End = Begin;
			Rotate = (ROTATE)int.Parse(cols[3]);
			Group = cols[4];
			Name = "";
			PackageName = cols[5];
			if (Package.Find(Group, PackageName)) {
				mPackage = Package.Get(Group, PackageName);
				Pivot = mPackage.BodyImage.Pivot;
				Height = mPackage.IsSMD ? -mPackage.Height : mPackage.Height;
			} else {
				mPackage = null;
				Pivot = new Point();
				Height = 0;
			}
		}

		public Parts(Point pos, ROTATE rotate, string group, string package) {
			Begin = pos;
			End = pos;
			Rotate = rotate;
			Group = group;
			Name = "";
			PackageName = package;
			if (Package.Find(group, package)) {
				mPackage = Package.Get(group, package);
				Pivot = mPackage.BodyImage.Pivot;
				Height = mPackage.IsSMD ? -mPackage.Height : mPackage.Height;
			} else {
				mPackage = null;
				Pivot = new Point();
				Height = 0;
			}
		}

		public PointF[] GetFoot(int index, bool round) {
			return mPackage.FootPrint.Get(Begin, Rotate, index, round);
		}

		public List<PointF[]> GetMarks(bool round) {
			return mPackage.FootPrint.GetMarks(Begin, Rotate, round);
		}

		public override Item Clone() {
			return new Parts(Begin, Rotate, Group, PackageName);
		}

		public override bool IsSelected(Point point) {
			return (!Reverse ^ mPackage.IsSMD) && base.IsSelected(point);
		}

		public override Point[] GetTerminals() {
			if (null == mPackage) {
				return new Point[0];
			}
			var terminals = mPackage.BodyImage.PinList;
			var points = new Point[terminals.Count];
			var ofsX = Pivot.X - 1;
			for (int i = 0; i < terminals.Count; i++) {
				var term = terminals[i];
				points[i] = Begin;
				switch (Rotate) {
				case ROTATE.DEG90:
					points[i].X += ofsX - term.Y;
					points[i].Y += term.X - ofsX;
					break;
				case ROTATE.DEG180:
					points[i].X += ofsX - term.X;
					points[i].Y += ofsX - term.Y;
					break;
				case ROTATE.DEG270:
					points[i].X += term.Y - ofsX;
					points[i].Y += ofsX - term.X;
					break;
				default:
					points[i].X += term.X - ofsX;
					points[i].Y += term.Y - ofsX;
					break;
				}
			}
			return points;
		}

		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"PARTS\t{0}\t{1}\t{2}\t{3}\t{4}",
				Begin.X, Begin.Y,
				(int)Rotate,
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
			Bitmap bmp;
			if (Display == EDisplay.TRANSPARENT) {
				bmp = (selected ? mPackage.Solid : mPackage.Alpha)[(int)Rotate];
			} else if (selected || (Reverse ^ mPackage.IsSMD)) {
				bmp = mPackage.Alpha[(int)Rotate];
			} else {
				bmp = mPackage.Solid[(int)Rotate];
			}
			var x = Begin.X + dx;
			var y = Begin.Y + dy;
			g.DrawImage(bmp, new Point(x - Pivot.X, y - Pivot.Y));
		}

		public override void DrawPDF(PDF.Page page) {
			var marks = GetMarks(false);
			page.FillColor = Color.Black;
			foreach (var mark in marks) {
				page.FillPolygon(mark);
			}
		}
	}
}