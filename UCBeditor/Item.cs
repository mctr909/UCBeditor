using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;

namespace UCBeditor {
    public struct Rect {
        public Point A;
        public Point B;
        public Rect(Point a, Point b) {
            A = a;
            B = b;
        }
    }

    abstract class Item {
        public Point Begin;
        public Point End;
        public double Height;

        protected static readonly Pen HoverColor = Pens.Blue;

        public static Item Construct(string line) {
            var cols = line.Split('\t');
            switch (cols[0]) {
            case "TIN":
                return new Tin(cols);
            case "WIRE":
                return new Wire(cols);
            case "PARTS":
                return new Parts(cols);
            case "LAND":
            default:
                return new Land(cols);
            }
        }

        public void Draw(Graphics g, bool reverse, bool selected) {
            Draw(g, 0, 0, reverse, selected);
        }

        public virtual Point[] GetTerminals() { return new Point[0]; }

        public abstract bool IsSelected(Point point);
        public abstract bool IsSelected(Rect rect);
        public abstract double Distance(Point point);
        public abstract void Write(StreamWriter sw);
        public abstract void Draw(Graphics g, int dx, int dy, bool reverse, bool selected);
    }

    class Land : Item {
        public bool IsFoot;

        static readonly Pen COLOR = new Pen(Color.FromArgb(211, 211, 0), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

        public Land(string[] cols) {
            Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
            End = Begin;
            Height = -0.1;
            IsFoot = false;
        }

        public Land(Point pos) {
            Begin = pos;
            End = pos;
            Height = -0.1;
            IsFoot = false;
        }

        public override bool IsSelected(Point point) {
            var sx = Begin.X - point.X;
            var sy = Begin.Y - point.Y;
            return Math.Sqrt(sx * sx + sy * sy) < 6.0;
        }

        public override bool IsSelected(Rect rect) {
            var rectX1 = rect.B.X < rect.A.X ? rect.B.X : rect.A.X;
            var rectX2 = rect.B.X < rect.A.X ? rect.A.X : rect.B.X;
            var rectY1 = rect.B.Y < rect.A.Y ? rect.B.Y : rect.A.Y;
            var rectY2 = rect.B.Y < rect.A.Y ? rect.A.Y : rect.B.Y;
            return (rectX1 <= Begin.X && Begin.X <= rectX2 && rectY1 <= Begin.Y && Begin.Y <= rectY2);
        }

        public override double Distance(Point point) {
            var apX = point.X - Begin.X;
            var apY = point.Y - Begin.Y;
            return Math.Sqrt(apX * apX + apY * apY);
        }

        public override void Write(StreamWriter sw) {
            if (IsFoot) {
                return;
            }
            sw.WriteLine("LAND\t{0}\t{1}", Begin.X, Begin.Y);
        }

        public override void Draw(Graphics g, int dx, int dy, bool reverse, bool selected) {
            var x1 = Begin.X + dx - 4;
            var y1 = Begin.Y + dy - 4;
            var x2 = Begin.X + dx - 2;
            var y2 = Begin.Y + dy - 2;
            if (selected) {
                g.DrawArc(HoverColor, x1, y1, 8, 8, 0, 360);
                g.DrawArc(HoverColor, x2, y2, 4, 4, 0, 360);
            } else {
                if (reverse) {
                    g.FillEllipse(COLOR.Brush, x1, y1, 8, 8);
                    g.FillEllipse(Brushes.White, x2, y2, 4, 4);
                } else {
                    g.FillEllipse(Tin.COLOR.Brush, x1, y1, 8, 8);
                    g.FillEllipse(Brushes.White, x2, y2, 4, 4);
                }
            }
        }
    }

    class Tin : Item {
        public static readonly Pen COLOR = new Pen(Color.FromArgb(191, 191, 191), 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

        protected Tin() { }

        public Tin(string[] cols) {
            Height = -0.2;
            Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
            End = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
        }

        public Tin(Point begin, Point end) {
            Height = -0.2;
            Begin = begin;
            End = end;
        }

        public override bool IsSelected(Point point) {
            var p = NearPointOnLine(point);
            var sx = p.X - point.X;
            var sy = p.Y - point.Y;
            return Math.Sqrt(sx * sx + sy * sy) < 6.0;
        }

        public override bool IsSelected(Rect rect) {
            var rectX1 = rect.B.X < rect.A.X ? rect.B.X : rect.A.X;
            var rectX2 = rect.B.X < rect.A.X ? rect.A.X : rect.B.X;
            var rectY1 = rect.B.Y < rect.A.Y ? rect.B.Y : rect.A.Y;
            var rectY2 = rect.B.Y < rect.A.Y ? rect.A.Y : rect.B.Y;
            var rx1 = End.X < Begin.X ? End.X : Begin.X;
            var rx2 = End.X < Begin.X ? Begin.X : End.X;
            var ry1 = End.Y < Begin.Y ? End.Y : Begin.Y;
            var ry2 = End.Y < Begin.Y ? Begin.Y : End.Y;
            return (rectX1 <= rx1 && rx1 <= rectX2 && rectY1 <= ry1 && ry1 <= rectY2 &&
                rectX1 <= rx2 && rx2 <= rectX2 && rectY1 <= ry2 && ry2 <= rectY2);
        }

        public override double Distance(Point point) {
            var abX = End.X - Begin.X;
            var abY = End.Y - Begin.Y;
            var apX = point.X - Begin.X;
            var apY = point.Y - Begin.Y;
            var abL2 = abX * abX + abY * abY;
            if (0.0 == abL2) {
                return Math.Sqrt(apX * apX + apY * apY);
            }
            var r = (double)(abX * apX + abY * apY) / abL2;
            if (r <= 0.0) {
                return Math.Sqrt(apX * apX + apY * apY);
            } else if (1.0 <= r) {
                var bpX = point.X - End.X;
                var bpY = point.X - End.X;
                return Math.Sqrt(bpX * bpX + bpY * bpY);
            } else {
                var x = Begin.X + r * abX;
                var y = Begin.Y + r * abY;
                return Math.Sqrt(x * x + y * y);
            }
        }

        public override void Write(StreamWriter sw) {
            sw.WriteLine(
                "TIN\t{0}\t{1}\t{2}\t{3}",
                Begin.X, Begin.Y,
                End.X, End.Y
            );
        }

        public override void Draw(Graphics g, int dx, int dy, bool reverse, bool selected) {
            var x1 = Begin.X + dx;
            var y1 = Begin.Y + dy;
            var x2 = End.X + dx;
            var y2 = End.Y + dy;
            if (selected) {
                g.DrawLine(HoverColor, x1, y1, x2, y2);
            } else {
                g.DrawLine(COLOR, x1, y1, x2, y2);
            }
        }

        Point NearPointOnLine(Point point) {
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
                return new Point((int)(Begin.X + r * abX), (int)(Begin.Y + r * abY));
            }
        }
    }

    class Wire : Tin {
        public enum Colors {
            BLACK,
            BLUE,
            RED,
            GREEN,
            YELLOW
        }

        Colors mWireColor;

        static readonly Pen BLACK = new Pen(Color.FromArgb(71, 71, 71), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen BLUE = new Pen(Color.FromArgb(63, 63, 221), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen RED = new Pen(Color.FromArgb(211, 63, 63), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen GREEN = new Pen(Color.FromArgb(47, 167, 47), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen YELLOW = new Pen(Color.FromArgb(191, 191, 0), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen BBLACK = new Pen(BLACK.Color, 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen BBLUE = new Pen(BLUE.Color, 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen BRED = new Pen(RED.Color, 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen BGREEN = new Pen(GREEN.Color, 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen BYELLOW = new Pen(YELLOW.Color, 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };

        public Wire(string[] cols) {
            Height = 100;
            Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
            End = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
            mWireColor = (Colors)Enum.Parse(typeof(Colors), cols[5]);
        }

        public Wire(Point begin, Point end, Colors color) {
            Height = 100;
            Begin = begin;
            End = end;
            mWireColor = color;
        }

        public override void Write(StreamWriter sw) {
            sw.WriteLine(
                "WIRE\t{0}\t{1}\t{2}\t{3}\t{4}",
                Begin.X, Begin.Y,
                End.X, End.Y,
                mWireColor
            );
        }

        public override void Draw(Graphics g, int dx, int dy, bool reverse, bool selected) {
            var x1 = Begin.X + dx;
            var y1 = Begin.Y + dy;
            var x2 = End.X + dx;
            var y2 = End.Y + dy;
            if (selected) {
                g.DrawLine(HoverColor, x1, y1, x2, y2);
            } else {
                if (reverse) {
                    switch (mWireColor) {
                    case Colors.BLACK:
                        g.DrawLine(BLACK, x1, y1, x2, y2); break;
                    case Colors.BLUE:
                        g.DrawLine(BLUE, x1, y1, x2, y2); break;
                    case Colors.RED:
                        g.DrawLine(RED, x1, y1, x2, y2); break;
                    case Colors.GREEN:
                        g.DrawLine(GREEN, x1, y1, x2, y2); break;
                    case Colors.YELLOW:
                        g.DrawLine(YELLOW, x1, y1, x2, y2); break;
                    }
                } else {
                    switch (mWireColor) {
                    case Colors.BLACK:
                        g.DrawLine(BBLACK, x1, y1, x2, y2); break;
                    case Colors.BLUE:
                        g.DrawLine(BBLUE, x1, y1, x2, y2); break;
                    case Colors.RED:
                        g.DrawLine(BRED, x1, y1, x2, y2); break;
                    case Colors.GREEN:
                        g.DrawLine(BGREEN, x1, y1, x2, y2); break;
                    case Colors.YELLOW:
                        g.DrawLine(BYELLOW, x1, y1, x2, y2); break;
                    }
                }
            }
        }
    }

    class Parts : Item {
        public RotateFlipType Rotate;
        public string Group;
        public string Name;
        public int Size;

        public Parts(string[] cols) {
            Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
            End = Begin;
            Rotate = (RotateFlipType)int.Parse(cols[3]);
            Group = cols[4];
            Name = cols[5];
            if (Package.Find(Group, Name)) {
                var p = Package.Get(Group, Name);
                Size = p.Size;
                Height = p.IsSMD ? -p.Height : p.Height;
            } else {
                Size = 0;
                Height = 0;
            }
        }

        public Parts(Point pos, RotateFlipType rot, string group, string name) {
            Begin = pos;
            End = pos;
            Rotate = rot;
            Group = group;
            Name = name;
            if (Package.Find(group, name)) {
                var p = Package.Get(group, name);
                Size = p.Size;
                Height = p.IsSMD ? -p.Height : p.Height;
            } else {
                Size = 0;
                Height = 0;
            }
        }

        public override bool IsSelected(Point point) {
            var sx = Begin.X - point.X;
            var sy = Begin.Y - point.Y;
            return Math.Sqrt(sx * sx + sy * sy) < 6.0;
        }

        public override bool IsSelected(Rect rect) {
            var rectX1 = rect.B.X < rect.A.X ? rect.B.X : rect.A.X;
            var rectX2 = rect.B.X < rect.A.X ? rect.A.X : rect.B.X;
            var rectY1 = rect.B.Y < rect.A.Y ? rect.B.Y : rect.A.Y;
            var rectY2 = rect.B.Y < rect.A.Y ? rect.A.Y : rect.B.Y;
            return (rectX1 <= Begin.X && Begin.X <= rectX2 && rectY1 <= Begin.Y && Begin.Y <= rectY2);
        }

        public override double Distance(Point point) {
            var apX = point.X - Begin.X;
            var apY = point.Y - Begin.Y;
            return Math.Sqrt(apX * apX + apY * apY);
        }

        public override Point[] GetTerminals() {
            if (!Package.Find(Group, Name)) {
                return new Point[0];
            }
            var terminals = Package.Get(Group, Name).Terminals;
            var points = new Point[terminals.Count];
            for (int i = 0; i < terminals.Count; i++) {
                var term = terminals[i];
                points[i] = Begin;
                switch (Rotate) {
                case RotateFlipType.Rotate90FlipNone:
                    points[i].X += Size - term.Y - 1;
                    points[i].Y += term.X - Size + 1;
                    break;
                case RotateFlipType.Rotate180FlipNone:
                    points[i].X += Size - term.X - 1;
                    points[i].Y += Size - term.Y - 1;
                    break;
                case RotateFlipType.Rotate270FlipNone:
                    points[i].X += term.Y - Size + 1;
                    points[i].Y += Size - term.X - 1;
                    break;
                default:
                    points[i].X += term.X - Size + 1;
                    points[i].Y += term.Y - Size + 1;
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
                Name
            );
        }

        public override void Draw(Graphics g, int dx, int dy, bool reverse, bool selected) {
        }
    }
}
