using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;

namespace UCBeditor {
    public class Package {
        public string Group { get; private set; }
        public string Name { get; private set; }
        public bool IsSMD { get; private set; }
        public double Height { get; private set; }
        public Point Offset { get; private set; }
        public int Center { get; private set; }
        public Bitmap[] Solid { get; private set; } = new Bitmap[4];
        public Bitmap[] Alpha { get; private set; } = new Bitmap[4];

        public List<Point> Terminals = new List<Point>();
        public Foot FootPrint = null;

        public static string GroupPath { get; private set; }
        public static Dictionary<string, Dictionary<string, Package>> List = new Dictionary<string, Dictionary<string, Package>>();

        public class Foot {
            public class Pin {
                public double X = 0.0;
                public double Y = 0.0;
                public string Polygon = "";
            }
            public Dictionary<string, PointF[]> Polygon = new Dictionary<string, PointF[]>();
            public List<Pin> Pins = new List<Pin>();
            public PointF[][] Get(Point pos, ROTATE rotate) {
                double rx, ry;
                switch (rotate) {
                case ROTATE.DEG90:
                    rx = 0;
                    ry = 1;
                    break;
                case ROTATE.DEG180:
                    rx = -1;
                    ry = 0;
                    break;
                case ROTATE.DEG270:
                    rx = 0;
                    ry = -1;
                    break;
                case ROTATE.NONE:
                default:
                    rx = 1;
                    ry = 0;
                    break;
                }
                rx *= Form1.GridWidth / 2.54;
                ry *= Form1.GridWidth / 2.54;
                var ret = new List<PointF[]>();
                foreach (var pin in Pins) {
                    var poly = Polygon[pin.Polygon];
                    var points = new PointF[poly.Length];
                    for (var i = 0; i < poly.Length; i++) {
                        var p = poly[i];
                        points[i] = new PointF(
                            (float)(pos.X + p.X * rx - p.Y * ry),
                            (float)(pos.Y + p.Y * rx + p.X * ry)
                        );
                    }
                    ret.Add(points);
                }
                return ret.ToArray();
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
            var currentGroup = "";
            var currentPackage = new Package();
            while (xml.Read()) {
                switch (xml.NodeType) {
                case XmlNodeType.Element:
                    switch (xml.Name) {
                    case "group":
                        currentGroup = xml.GetAttribute("name").ToUpper();
                        break;
                    case "item":
                        currentPackage = new Package() {
                            Group = currentGroup,
                            Name = xml.GetAttribute("name"),
                            Height = double.Parse(xml.GetAttribute("height")),
                            IsSMD = xml.GetAttribute("type") == "smd"
                        };
                        break;
                    case "offset":
                        currentPackage.Offset = new Point(
                            int.Parse(xml.GetAttribute("x")),
                            int.Parse(xml.GetAttribute("y"))
                        );
                        break;
                    case "terminal":
                        currentPackage.Terminals.Add(new Point(
                            int.Parse(xml.GetAttribute("x")),
                            int.Parse(xml.GetAttribute("y")))
                        );
                        break;
                    case "foot":
                        currentPackage.FootPrint = new Foot();
                        break;
                    case "rect":
                        if (null != currentPackage.FootPrint) {
                            var w = float.Parse(xml.GetAttribute("width")) * 0.5f;
                            var h = float.Parse(xml.GetAttribute("height")) * 0.5f;
                            var n = xml.GetAttribute("name");
                            n = null == n ? "" : n;
                            currentPackage.FootPrint.Polygon.Add(n, new PointF[] {
                                new PointF(w, h), new PointF(-w, h),
                                new PointF(-w, -h), new PointF(w, -h)
                            });
                        }
                        break;
                    case "pin":
                        if (null != currentPackage.FootPrint) {
                            var x = double.Parse(xml.GetAttribute("x"));
                            var y = double.Parse(xml.GetAttribute("y"));
                            var n = xml.GetAttribute("link");
                            n = null == n ? "" : n;
                            currentPackage.FootPrint.Pins.Add(new Foot.Pin() {
                                X = x,
                                Y = y,
                                Polygon = n
                            });
                        }
                        break;
                    default:
                        break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    switch (xml.Name) {
                    case "foot":
                        break;
                    case "item": {
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
                        if (solid.Width != solid.Height) {
                            break;
                        }
                        currentPackage.Center = solid.Width / 2;
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
                        break;
                    }
                    }
                    break;
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
