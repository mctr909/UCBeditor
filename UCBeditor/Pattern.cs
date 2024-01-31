using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace UCB {
	class Pattern : Wire {
		public readonly float Thick = 0.3f;

		public static void Divide(List<Item> list, Item dividerItem) {
			var divTerms = dividerItem.GetTerminals();
			foreach (var divTerm in divTerms) {
				var addList = new List<Item>();
				foreach (var item in list) {
					if (item is Pattern pattern && pattern.OnMiddle(divTerm)) {
						addList.Add(new Pattern(pattern.mPosition, divTerm, pattern.Thick));
						pattern.mPosition = divTerm;
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
							if (patternA.mPosition.Equals(termB)) {
								if (itemB is Pattern pB) {
									if (connectedItemFound) {
										patternB = null;
										break;
									}
									patternB = pB;
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
							if (patternA.mEnd.Equals(termB)) {
								if (itemB is Pattern pB) {
									if (connectedItemFound) {
										patternB = null;
										break;
									}
									patternB = pB;
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
					var ax = patternA.mEnd.X - patternA.mPosition.X;
					var ay = patternA.mEnd.Y - patternA.mPosition.Y;
					var bx = patternB.mEnd.X - patternB.mPosition.X;
					var by = patternB.mEnd.Y - patternB.mPosition.Y;
					var cross = ax * by - ay * bx;
					if (0 == cross && (patternA.Thick == patternB.Thick)) {
						patternB.Removed = true;
						if (patternA.mPosition.Equals(patternB.mPosition)) {
							patternA.mPosition = patternB.mEnd;
						} else if (patternA.mPosition.Equals(patternB.mEnd)) {
							patternA.mPosition = patternB.mPosition;
						} else if (patternA.mEnd.Equals(patternB.mEnd)) {
							patternA.mEnd = patternB.mPosition;
						} else {
							patternA.mEnd = patternB.mEnd;
						}
					}
				}
			}
			list.RemoveAll(p => p.Removed);
		}

		public Pattern(string[] cols) {
			mPosition = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
			mEnd = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
			if (6 <= cols.Length) {
				Thick = float.Parse(cols[5]);
			}
			Height = -0.02;
		}

		public Pattern(Point begin, Point end, double thick) {
			mPosition = begin;
			mEnd = end;
			Height = -0.02;
			Thick = (float)thick;
		}

		public override bool IsSelected(Point point) {
			return Pattern && base.Distance(point) < SNAP;
		}

		public override Item Clone() {
			return new Pattern(mPosition, mEnd, Thick);
		}
		
		public override void Write(StreamWriter sw) {
			sw.WriteLine(
				"TIN\t{0}\t{1}\t{2}\t{3}\t{4}",
				mPosition.X, mPosition.Y,
				mEnd.X, mEnd.Y,
				Thick
			);
		}

		public override void Draw(Graphics g, int dx, int dy, bool selected) {
			var x1 = mPosition.X + dx;
			var y1 = mPosition.Y + dy;
			var x2 = mEnd.X + dx;
			var y2 = mEnd.Y + dy;
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

		public override void DrawPattern(PDF.Page page) {
			var d = Thick * GridScale;
			var sx = mEnd.X - mPosition.X;
			var sy = mEnd.Y - mPosition.Y;
			var th = Math.Atan2(sy, sx) + Math.PI / 2;
			var rx = (float)(Math.Cos(th) * d * 0.5);
			var ry = (float)(Math.Sin(th) * d * 0.5);
			page.FillColor = Color.Black;
			page.FillPolygon(new PointF[] {
				new PointF(mPosition.X + rx, mPosition.Y + ry),
				new PointF(mEnd.X + rx, mEnd.Y + ry),
				new PointF(mEnd.X - rx, mEnd.Y - ry),
				new PointF(mPosition.X - rx, mPosition.Y - ry)
			});
			page.FillCircle(mPosition, d);
			page.FillCircle(mEnd, d);
		}
	}
}
