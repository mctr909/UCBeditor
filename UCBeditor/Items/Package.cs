using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace Items {
	internal class Package {
		public static string Path { get; private set; }
		public static readonly Dictionary<string, Dictionary<string, Package>> List = new Dictionary<string, Dictionary<string, Package>>();

		public string Group { get; private set; }
		public string Name { get; private set; }
		public double Height { get; private set; }
		public bool IsSMD { get; private set; }

		public readonly PartsBody Body = new PartsBody();
		public readonly PartsFoot Foot = new PartsFoot();
		public readonly Bitmap[] Solid = new Bitmap[4];
		public readonly Bitmap[] Alpha = new Bitmap[4];

		public Bitmap GetPreviewImage() {
			return Solid[0];
		}

		public static void Load(string dir, string fileName) {
			Path = $"{dir}group\\";
			List.Clear();
			var xmlPath = dir + fileName;
			if (!File.Exists(xmlPath)) {
				return;
			}
			var xml = XmlReader.Create(xmlPath);
			var dirSolid = $"{dir}solid\\";
			var dirAlpha = $"{dir}alpha\\";

			var currentGroup = "";
			Package currentPackage = null;
			bool enableBody = false;
			bool enableFoot = false;
			string errorMessage = "";
			while (xml.Read()) {
				switch (xml.NodeType) {
				case XmlNodeType.Element: {
					switch (xml.Name) {
					case "group":
						currentGroup = xml.GetAttribute("name").ToUpper();
						break;
					case "item":
						currentPackage = new Package
						{
							Group = currentGroup,
							Name = xml.GetAttribute("name") ?? "",
							IsSMD = (xml.GetAttribute("type") ?? "").ToUpper() == "SMD",
							Height = double.Parse(xml.GetAttribute("height"))
						};
						break;
					case "body":
						enableBody = currentPackage != null;
						break;
					case "foot":
						enableFoot = currentPackage != null;
						break;

					case "pivot":
						if (enableBody) {
							var x = int.Parse(xml.GetAttribute("x"));
							var y = int.Parse(xml.GetAttribute("y"));
							currentPackage.Body.SetPivot(x, y);
						}
						break;
					case "offset":
						if (enableBody) {
							var x = int.Parse(xml.GetAttribute("x"));
							var y = int.Parse(xml.GetAttribute("y"));
							currentPackage.Body.SetOffset(x, y);
						}
						if (enableFoot) {
							var x = double.Parse(xml.GetAttribute("x"));
							var y = double.Parse(xml.GetAttribute("y"));
							currentPackage.Foot.SetOffset(x, y);
						}
						break;
					case "pin":
						if (enableBody) {
							var x = int.Parse(xml.GetAttribute("x"));
							var y = int.Parse(xml.GetAttribute("y"));
							var name = (xml.GetAttribute("name") ?? "").ToUpper();
							currentPackage.Body.AddPin(x, y, name);
						}
						if (enableFoot) {
							var x = double.Parse(xml.GetAttribute("x"));
							var y = double.Parse(xml.GetAttribute("y"));
							var link = (xml.GetAttribute("link") ?? "").ToUpper();
							currentPackage.Foot.AddPin(x, y, link);
						}
						break;
					case "mark":
						if (enableFoot) {
							var x = double.Parse(xml.GetAttribute("x"));
							var y = double.Parse(xml.GetAttribute("y"));
							var link = (xml.GetAttribute("link") ?? "").ToUpper();
							currentPackage.Foot.AddMark(x, y, link);
						}
						break;

					case "rect":
						if (enableFoot) {
							var w = float.Parse(xml.GetAttribute("width")) * 0.5f;
							var h = float.Parse(xml.GetAttribute("height")) * 0.5f;
							var name = (xml.GetAttribute("name") ?? "").ToUpper();
							var margin = double.Parse(xml.GetAttribute("margin") ?? "0");
							currentPackage.Foot.AddPolygon(
								name,
								margin,
								new PointF(w, h),
								new PointF(-w, h),
								new PointF(-w, -h),
								new PointF(w, -h)
							);
						}
						break;
					case "circle":
						if (enableFoot) {
							var r = float.Parse(xml.GetAttribute("diameter")) * 0.5f;
							var vert = new PointF[24];
							for (int i = 0; i < vert.Length; i++) {
								var th = 2 * Math.PI * (i + 0.5) / vert.Length;
								vert[i] = new PointF(
									(float)(r * Math.Cos(th)),
									(float)(r * Math.Sin(th))
								);
							}
							var name = (xml.GetAttribute("name") ?? "").ToUpper();
							var margin = double.Parse(xml.GetAttribute("margin") ?? "0");
							currentPackage.Foot.AddPolygon(name, margin, vert);
						}
						break;
					case "polygon":
						if (enableFoot) {
							var inner = xml.ReadInnerXml().Replace("\t", "");
							var vert = new List<PointF>();
							foreach (var line in inner.Split('\n')) {
								if ("" == line) {
									continue;
								}
								var cols = line.Split(' ');
								var x = float.Parse(cols[0]);
								var y = float.Parse(cols[1]);
								vert.Add(new PointF(x, y));
							}
							var name = (xml.GetAttribute("name") ?? "").ToUpper();
							var margin = double.Parse(xml.GetAttribute("margin") ?? "0");
							currentPackage.Foot.AddPolygon(name, margin, vert.ToArray());
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
						currentGroup = "";
						break;
					case "item": {
						var pathGroup = $"{Path}{currentPackage.Group}.png";
						var pathSolid = $"{dirSolid}{currentPackage.Group}\\{currentPackage.Name}.png";
						var pathAlpha = $"{dirAlpha}{currentPackage.Group}\\{currentPackage.Name}.png";
						var notFoundGroup = !File.Exists(pathGroup);
						var notFoundSolid = !File.Exists(pathSolid);
						var notFoundAlpha = !File.Exists(pathAlpha);
						if (notFoundGroup) {
							errorMessage += $"グループアイコンが見つかりません。\r\n\t{pathGroup}\r\n";
						}
						if (notFoundSolid) {
							errorMessage += $"パーツ画像が見つかりません。\r\n\t{pathSolid}\r\n";
						}
						if (notFoundAlpha) {
							errorMessage += $"パーツ画像(透過)が見つかりません。\r\n\t{pathAlpha}\r\n";
						}
						if (notFoundGroup || notFoundSolid || notFoundAlpha) {
							break;
						}
						var bmpSolid = new Bitmap(pathSolid);
						var bmpAlpha = new Bitmap(pathAlpha);
						if (bmpSolid.Width != bmpAlpha.Width || bmpSolid.Height != bmpAlpha.Height) {
							errorMessage += $"パーツ画像のサイズが一致しません。\r\n\t{pathSolid}\r\n\t{pathAlpha}\r\n";
							break;
						}
						var solid = currentPackage.Solid;
						solid[0] = (Bitmap)bmpSolid.Clone();
						solid[1] = (Bitmap)bmpSolid.Clone();
						solid[2] = (Bitmap)bmpSolid.Clone();
						solid[3] = (Bitmap)bmpSolid.Clone();
						solid[1].RotateFlip(RotateFlipType.Rotate90FlipNone);
						solid[2].RotateFlip(RotateFlipType.Rotate180FlipNone);
						solid[3].RotateFlip(RotateFlipType.Rotate270FlipNone);
						var alpha = currentPackage.Alpha;
						alpha[0] = (Bitmap)bmpAlpha.Clone();
						alpha[1] = (Bitmap)bmpAlpha.Clone();
						alpha[2] = (Bitmap)bmpAlpha.Clone();
						alpha[3] = (Bitmap)bmpAlpha.Clone();
						alpha[1].RotateFlip(RotateFlipType.Rotate90FlipNone);
						alpha[2].RotateFlip(RotateFlipType.Rotate180FlipNone);
						alpha[3].RotateFlip(RotateFlipType.Rotate270FlipNone);
						if (!List.ContainsKey(currentPackage.Group)) {
							List.Add(currentPackage.Group, new Dictionary<string, Package>());
						}
						List[currentPackage.Group].Add(currentPackage.Name, currentPackage);
						currentPackage = null;
						break;
					}
					case "body":
						enableBody = false;
						break;
					case "foot":
						enableFoot = false;
						break;
					}
					break;
				}
				default:
					break;
				}
			}
			if (!string.IsNullOrEmpty(errorMessage)) {
				throw new Exception(errorMessage);
			}
		}

		public static bool Get(string group, string name, out Package package) {
			if (List.ContainsKey(group) && List[group].ContainsKey(name)) {
				package = List[group][name];
				return true;
			} else {
				package = null;
				return false;
			}
		}
	}
}
