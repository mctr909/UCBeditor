using System;
using System.Collections.Generic;
using System.Drawing;

namespace UCBeditor {
    public class Package {
        public string Group;
        public string Name;
        public bool IsSMD;
        public bool Enable;
        public double Height;
        public Point Offset;
        public int Size;
        public List<Point> Terminals = new List<Point>();

        public static Dictionary<string, Dictionary<string, Package>> List = new Dictionary<string, Dictionary<string, Package>>();

        public static void Add(Package newPackage) {
            if (!List.ContainsKey(newPackage.Group)) {
                List.Add(newPackage.Group, new Dictionary<string, Package>());
            }
            List[newPackage.Group].Add(newPackage.Name, newPackage);
        }
        public static bool Find(string group, string name) {
            return List.ContainsKey(group) && List[group].ContainsKey(name);
        }
        public static Package Get(string group, string name) {
            return List[group][name];
        }
    }
}
