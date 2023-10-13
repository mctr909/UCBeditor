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
        
        public List<Point> Terminals = new List<Point>();

        public enum EDisplay {
            INVISIBLE,
            TRANSPARENT,
            SOLID,
        }

        public static EDisplay Display { get; set; }
        public static string GroupPath { get; private set; }
        public static string SolidPath { get; private set; }
        public static string AlphaPath { get; private set; }

        public static Dictionary<string, Dictionary<string, Package>> List = new Dictionary<string, Dictionary<string, Package>>();

        public static void LoadXML(string dir, string fileName) {
            GroupPath = dir + "group\\";
            SolidPath = dir + "solid\\";
            AlphaPath = dir + "alpha\\";
            List.Clear();
            var xmlPath = dir + fileName;
            if (!File.Exists(xmlPath)) {
                return;
            }
            var xml = XmlReader.Create(xmlPath);
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
                        var groupPath = GroupPath + currentGroup + ".png";
                        if (!File.Exists(groupPath)) {
                            break;
                        }
                        var solidPath = SolidPath + currentGroup + "\\" + currentPackage.Name + ".png";
                        if (!File.Exists(solidPath)) {
                            break;
                        }
                        var alphaPath = AlphaPath + currentGroup + "\\" + currentPackage.Name + ".png";
                        if (!File.Exists(alphaPath)) {
                            break;
                        }
                        var solid = new Bitmap(solidPath);
                        var alpha = new Bitmap(alphaPath);
                        if (solid.Width != alpha.Width || solid.Height != alpha.Height) {
                            break;
                        }
                        if (solid.Width != solid.Height) {
                            break;
                        }
                        currentPackage.Center = solid.Width / 2;
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
