using System;
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

        public enum EDisplay {
            INVISIBLE,
            TRANSPARENT,
            SOLID,
        }

        public static EDisplay Display { get; set; }
        public static bool Reverse { get; set; }
        public static string GroupPath { get; private set; }
        public static Dictionary<string, Dictionary<string, Package>> List = new Dictionary<string, Dictionary<string, Package>>();

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
                    default:
                        break;
                    }
                    break;
                case XmlNodeType.EndElement:
                    switch (xml.Name) {
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
