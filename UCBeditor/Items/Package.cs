﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

using static Items.Package.Foot;

namespace Items {
	public class Package {
		public string Group { get; private set; }
		public string Name { get; private set; }
		public bool IsSMD { get; private set; }
		public double Height { get; private set; }
		public Bitmap[] Solid { get; private set; } = new Bitmap[4];
		public Bitmap[] Alpha { get; private set; } = new Bitmap[4];

		public Image BodyImage = new Image();
		public Foot FootPrint = new Foot();

		public static string GroupPath { get; private set; }
		public static Dictionary<string, Dictionary<string, Package>> List = new Dictionary<string, Dictionary<string, Package>>();

		public class Image {
			public List<Point> PinList = new List<Point>();
			public Point Offset = new Point();
			public PointF Pivot = new PointF();
		}
		public class Foot {
			public class Pin {
				public double X = 0.0;
				public double Y = 0.0;
				public string Polygon = "";
			}
			public class Polygon {
				public PointF[] Points = new PointF[0];
				public double SolderClearance = 1.0;
			}
			public Dictionary<string, Polygon> PolygonList = new Dictionary<string, Polygon>();
			public List<Pin> PinList = new List<Pin>();
			public List<Pin> MarkList = new List<Pin>();
			public PointF Offset = new PointF();

			public PointF[] Get(PointF pos, ROTATE rotate, int index, bool round, bool solder) {
				if (PinList.Count <= index) {
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
				var pin = PinList[index];
				var poly = PolygonList[pin.Polygon];
				var points = new PointF[poly.Points.Length];
				double clearance;
				if (solder) {
					clearance = poly.SolderClearance;
				} else {
					clearance = 1;
				}
				for (var i = 0; i < points.Length; i++) {
					var p = poly.Points[i];
					var px = p.X;
					var py = p.Y;
					var r = Math.Sqrt(px * px + py * py) * clearance;
					var th = Math.Atan2(py, px);
					px = (float)(Math.Cos(th) * r);
					py = (float)(Math.Sin(th) * r);
					px += (float)pin.X - Offset.X;
					py += (float)pin.Y + Offset.Y;
					if (round) {
						points[i] = new PointF(
							(int)(pos.X + px * rotX - py * rotY + 0.5),
							(int)(pos.Y + py * rotX + px * rotY + 0.5)
						);
					} else {
						points[i] = new PointF(
							(float)(pos.X + px * rotX - py * rotY),
							(float)(pos.Y + py * rotX + px * rotY)
						);
					}
				}
				return points;
			}

			public List<PointF[]> GetMarks(Point pos, ROTATE rotate, bool round) {
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
				foreach (var mark in MarkList) {
					var poly = PolygonList[mark.Polygon];
					var points = new PointF[poly.Points.Length];
					for (var i = 0; i < points.Length; i++) {
						var p = poly.Points[i];
						p.X += (float)mark.X - Offset.X;
						p.Y += (float)mark.Y + Offset.Y;
						if (round) {
							points[i] = new PointF(
								(int)(pos.X + p.X * rotX - p.Y * rotY + 0.5),
								(int)(pos.Y + p.Y * rotX + p.X * rotY + 0.5)
							);
						} else {
							points[i] = new PointF(
								(float)(pos.X + p.X * rotX - p.Y * rotY),
								(float)(pos.Y + p.Y * rotX + p.X * rotY)
							);
						}
					}
					ret.Add(points);
				}
				return ret;
			}
		}

		public static void LoadXML(string dir, string fileName) {
			GroupPath = dir + "group\\";
			List.Clear();
			var xmlPath = dir + fileName;
			if (!File.Exists(xmlPath)) {
				return;
			}
			var xml = XmlReader.Create(xmlPath);
			var dirSolid = dir + "solid\\";
			var dirAlpha = dir + "alpha\\";

			string currentGroup = null;
			Package currentPackage = null;
			Image currentBodyImage = null;
			Foot currentFootPrint = null;

			while (xml.Read()) {
				switch (xml.NodeType) {
				case XmlNodeType.Element: {
					switch (xml.Name) {
					case "group":
						currentGroup = xml.GetAttribute("name").ToUpper();
						break;
					case "item":
						if (null != currentGroup) {
							currentPackage = new Package() {
								Group = currentGroup,
								Name = xml.GetAttribute("name"),
								Height = double.Parse(xml.GetAttribute("height")),
								IsSMD = xml.GetAttribute("type") == "smd"
							};
						}
						break;
					case "image":
						if (null != currentPackage) {
							currentBodyImage = new Image();
						}
						break;
					case "foot":
						if (null != currentPackage) {
							currentFootPrint = new Foot();
						}
						break;

					case "pivot":
						if (null != currentBodyImage) {
							currentBodyImage.Pivot = new PointF(
								float.Parse(xml.GetAttribute("x")),
								float.Parse(xml.GetAttribute("y"))
							);
						}
						break;
					case "offset":
						if (null != currentBodyImage) {
							currentBodyImage.Offset = new Point(
								int.Parse(xml.GetAttribute("x")),
								int.Parse(xml.GetAttribute("y"))
							);
						}
						if (null != currentFootPrint) {
							currentFootPrint.Offset = new PointF(
								float.Parse(xml.GetAttribute("x")),
								float.Parse(xml.GetAttribute("y"))
							);
						}
						break;
					case "rect":
						if (null != currentFootPrint) {
							var w = float.Parse(xml.GetAttribute("width")) * 0.5f;
							var h = float.Parse(xml.GetAttribute("height")) * 0.5f;
							var name = xml.GetAttribute("name");
							name = name ?? "";
							var poly = new Polygon() {
								Points = new PointF[] {
									new PointF(w, h), new PointF(-w, h),
									new PointF(-w, -h), new PointF(w, -h),
								}
							};
							var str = xml.GetAttribute("solder_clearance");
							if (null != str) {
								poly.SolderClearance = double.Parse(str);
							}
							currentFootPrint.PolygonList.Add(name, poly);
						}
						break;
					case "circle":
						if (null != currentFootPrint) {
							var r = float.Parse(xml.GetAttribute("diameter")) * 0.5f;
							var name = xml.GetAttribute("name");
							name = name ?? "";
							var p = new PointF[24];
							for (int i = 0; i < p.Length; i++) {
								var th = 2 * Math.PI * (i + 0.5) / p.Length;
								p[i] = new PointF(
									(float)(r * Math.Cos(th)),
									(float)(r * Math.Sin(th))
								);
							}
							var poly = new Polygon() { Points = p };
							var str = xml.GetAttribute("solder_clearance");
							if (null != str) {
								poly.SolderClearance = double.Parse(str);
							}
							currentFootPrint.PolygonList.Add(name, poly);
						}
						break;
					case "polygon":
						if (null != currentFootPrint) {
							var name = xml.GetAttribute("name");
							name = name ?? "";
							var inner = xml.ReadInnerXml().Replace("\t", "");
							var p = new List<PointF>();
							foreach (var line in inner.Split('\n')) {
								if ("" == line) {
									continue;
								}
								var cols = line.Split(' ');
								var x = float.Parse(cols[0]);
								var y = float.Parse(cols[1]);
								p.Add(new PointF(x, y));
							}
							var poly = new Polygon() { Points = p.ToArray() };
							var str = xml.GetAttribute("solder_clearance");
							if (null != str) {
								poly.SolderClearance = double.Parse(str);
							}
							currentFootPrint.PolygonList.Add(name, poly);
						}
						break;
					case "pin":
						if (null != currentBodyImage) {
							currentBodyImage.PinList.Add(new Point(
								int.Parse(xml.GetAttribute("x")),
								int.Parse(xml.GetAttribute("y")))
							);
						}
						if (null != currentFootPrint) {
							var x = double.Parse(xml.GetAttribute("x"));
							var y = double.Parse(xml.GetAttribute("y"));
							var link = xml.GetAttribute("link");
							link = link ?? "";
							var pin = new Pin() {
								X = x,
								Y = y,
								Polygon = link
							};
							currentFootPrint.PinList.Add(pin);
						}
						break;
					case "mark":
						if (null != currentFootPrint) {
							var x = double.Parse(xml.GetAttribute("x"));
							var y = double.Parse(xml.GetAttribute("y"));
							var link = xml.GetAttribute("link");
							link = link ?? "";
							currentFootPrint.MarkList.Add(new Pin() {
								X = x,
								Y = y,
								Polygon = link
							});
						}
						break;
					default:
						break;
					}
					break;
				}
				case XmlNodeType.EndElement: {
					switch (xml.Name) {
					case "group":
						currentGroup = null;
						break;
					case "item": {
						if (null == currentGroup) {
							break;
						}
						var pathGroup = GroupPath + currentGroup + ".png";
						if (!File.Exists(pathGroup)) {
							break;
						}
						var pathSolid = dirSolid + currentGroup + "\\" + currentPackage.Name + ".png";
						if (!File.Exists(pathSolid)) {
							break;
						}
						var pathAlpha = dirAlpha + currentGroup + "\\" + currentPackage.Name + ".png";
						if (!File.Exists(pathAlpha)) {
							break;
						}
						var solid = new Bitmap(pathSolid);
						var alpha = new Bitmap(pathAlpha);
						if (solid.Width != alpha.Width || solid.Height != alpha.Height) {
							break;
						}
						for (int i = 0; i < currentPackage.BodyImage.PinList.Count; i++) {
							var p = currentPackage.BodyImage.PinList[i];
							p.Y = solid.Height - p.Y;
						}
						currentPackage.Solid[0] = (Bitmap)solid.Clone();
						currentPackage.Solid[1] = (Bitmap)solid.Clone();
						currentPackage.Solid[2] = (Bitmap)solid.Clone();
						currentPackage.Solid[3] = (Bitmap)solid.Clone();
						currentPackage.Solid[1].RotateFlip(RotateFlipType.Rotate90FlipNone);
						currentPackage.Solid[2].RotateFlip(RotateFlipType.Rotate180FlipNone);
						currentPackage.Solid[3].RotateFlip(RotateFlipType.Rotate270FlipNone);
						currentPackage.Alpha[0] = (Bitmap)alpha.Clone();
						currentPackage.Alpha[1] = (Bitmap)alpha.Clone();
						currentPackage.Alpha[2] = (Bitmap)alpha.Clone();
						currentPackage.Alpha[3] = (Bitmap)alpha.Clone();
						currentPackage.Alpha[1].RotateFlip(RotateFlipType.Rotate90FlipNone);
						currentPackage.Alpha[2].RotateFlip(RotateFlipType.Rotate180FlipNone);
						currentPackage.Alpha[3].RotateFlip(RotateFlipType.Rotate270FlipNone);
						if (!List.ContainsKey(currentPackage.Group)) {
							List.Add(currentPackage.Group, new Dictionary<string, Package>());
						}
						List[currentPackage.Group].Add(currentPackage.Name, currentPackage);
						currentPackage = null;
						break;
					}
					case "image":
						if (null != currentPackage) {
							currentPackage.BodyImage = currentBodyImage;
							currentBodyImage = null;
						}
						break;
					case "foot":
						if (null != currentPackage) {
							currentPackage.FootPrint = currentFootPrint;
							currentFootPrint = null;
						}
						break;
					}
					break;
				}
				default:
					break;
				}
			}
		}
		public static bool Find(string group, string name) {
			return List.ContainsKey(group) && List[group].ContainsKey(name);
		}
		public static Package Get(string group, string name) {
			return List[group][name];
		}
	}
}
