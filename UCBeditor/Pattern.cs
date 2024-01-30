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
							if (patternA.End.Equals(termB)) {
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

		public override void DrawPattern(PDF.Page page) {
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
}
