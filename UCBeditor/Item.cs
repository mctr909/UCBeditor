using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;

namespace UCBeditor {
    struct Rect {
        public Point A;
        public Point B;
        public Rect(Point a, Point b) {
            A = a;
            B = b;
        }
    }

    struct Item {
        public enum EType {
            LAND,
            FOOT,
            TIN,
            WIRE,
            PARTS
        }
        public enum EWire {
            BLACK,
            BLUE,
            RED,
            GREEN,
            YELLOW
        }

        static readonly Pen HoverColor = Pens.Blue;
        static readonly Pen LandColor = new Pen(Color.FromArgb(211, 211, 0), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen TIN_W = new Pen(Color.FromArgb(191, 191, 191), 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen TIN_N = new Pen(Color.FromArgb(231, 231, 231), 1.0f) { DashPattern = new float[] { 1, 1 } };

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

        public EType Type;
        public Point Begin;
        public Point End;
        public RotateFlipType Rotate;
        public string PartsGroup;
        public string PartsName;
        public double Height;

        EWire mWireColor;

        public Item(string line) {
            Rotate = RotateFlipType.RotateNoneFlipNone;
            PartsGroup = "";
            PartsName = "";
            Height = 0;
            mWireColor = EWire.BLACK;
            var cols = line.Split('\t');
            switch (cols[0]) {
            case "TIN":
                Type = EType.TIN;
                Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
                End = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
                break;
            case "WIRE":
                Type = EType.WIRE;
                Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
                End = new Point(int.Parse(cols[3]), int.Parse(cols[4]));
                mWireColor = (EWire)Enum.Parse(typeof(EWire), cols[5]);
                Height = 100;
                break;
            case "PARTS":
                Type = EType.PARTS;
                Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
                End = Begin;
                Rotate = (RotateFlipType)int.Parse(cols[3]);
                PartsGroup = cols[4];
                PartsName = cols[5];
                break;
            case "LAND":
            default:
                Type = EType.LAND;
                Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
                End = Begin;
                Height = 0.1;
                break;
            }
        }

        public Item(Point pos) {
            Type = EType.LAND;
            Begin = pos;
            End = pos;
            Rotate = RotateFlipType.RotateNoneFlipNone;
            PartsGroup = "";
            PartsName = "";
            Height = 0.1;
            mWireColor = EWire.BLACK;
        }

        public Item(Point begin, Point end) {
            Type = EType.TIN;
            Begin = begin;
            End = end;
            Rotate = RotateFlipType.RotateNoneFlipNone;
            PartsGroup = "";
            PartsName = "";
            Height = 0;
            mWireColor = EWire.BLACK;
        }

        public Item(Point begin, Point end, EWire color) {
            Type = EType.WIRE;
            Begin = begin;
            End = end;
            Rotate = RotateFlipType.RotateNoneFlipNone;
            PartsGroup = "";
            PartsName = "";
            Height = 100;
            mWireColor = color;
        }

        public Item(Point pos, RotateFlipType rot, string group, string name) {
            Type = EType.PARTS;
            Begin = pos;
            End = pos;
            Rotate = rot;
            PartsGroup = group;
            PartsName = name;
            Height = 0;
            mWireColor = EWire.BLACK;
        }

        public bool isSelected(Point point) {
            Point p;
            switch (Type) {
            case EType.WIRE:
            case EType.TIN:
                p = nearPointOnLine(point);
                break;
            default:
                p = Begin;
                break;
            }
            var sx = p.X - point.X;
            var sy = p.Y - point.Y;
            return Math.Sqrt(sx * sx + sy * sy) < 6.0;
        }

        public bool isSelected(Rect rect) {
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

        public void Write(StreamWriter sw) {
            switch (Type) {
            case EType.LAND:
                sw.WriteLine(
                    "{0}\t{1}\t{2}",
                    Type,
                    Begin.X, Begin.Y
                );
                break;
            case EType.TIN:
                sw.WriteLine(
                    "{0}\t{1}\t{2}\t{3}\t{4}",
                    Type,
                    Begin.X, Begin.Y,
                    End.X, End.Y
                );
                break;
            case EType.WIRE:
                sw.WriteLine(
                    "{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                    Type,
                    Begin.X, Begin.Y,
                    End.X, End.Y,
                    mWireColor
                );
                break;
            case EType.PARTS:
                sw.WriteLine(
                    "{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
                    Type,
                    Begin.X, Begin.Y,
                    (int)Rotate,
                    PartsGroup,
                    PartsName
                );
                break;
            }
        }

        public void Draw(Graphics g, bool reverse, bool selected) {
            Draw(g, 0, 0, reverse, selected);
        }

        public void Draw(Graphics g, int dx, int dy, bool reverse, bool selected) {
            switch (Type) {
            case EType.LAND:
                DrawLand(g, dx, dy, reverse, selected);
                break;
            case EType.TIN:
                DrawTin(g, dx, dy, reverse, selected);
                break;
            case EType.WIRE:
                DrawWire(g, dx, dy, reverse, selected);
                break;
            }
        }

        void DrawLand(Graphics g, int dx, int dy, bool reverse, bool selected) {
            var x1 = Begin.X + dx - 4;
            var y1 = Begin.Y + dy - 4;
            var x2 = Begin.X + dx - 2;
            var y2 = Begin.Y + dy - 2;
            if (selected) {
                g.DrawArc(HoverColor, x1, y1, 8, 8, 0, 360);
                g.DrawArc(HoverColor, x2, y2, 4, 4, 0, 360);
            } else {
                if (reverse) {
                    g.FillEllipse(LandColor.Brush, x1, y1, 8, 8);
                    g.FillEllipse(Brushes.White, x2, y2, 4, 4);
                } else {
                    g.FillEllipse(TIN_W.Brush, x1, y1, 8, 8);
                    g.FillEllipse(Brushes.White, x2, y2, 4, 4);
                }
            }
        }

        void DrawTin(Graphics g, int dx, int dy, bool reverse, bool selected) {
            var x1 = Begin.X + dx;
            var y1 = Begin.Y + dy;
            var x2 = End.X + dx;
            var y2 = End.Y + dy;
            if (selected) {
                g.DrawLine(HoverColor, x1, y1, x2, y2);
            } else {
                g.DrawLine(TIN_W, x1, y1, x2, y2);
                g.DrawLine(TIN_N, x1, y1, x2, y2);
            }
        }

        void DrawWire(Graphics g, int dx, int dy, bool reverse, bool selected) {
            var x1 = Begin.X + dx;
            var y1 = Begin.Y + dy;
            var x2 = End.X + dx;
            var y2 = End.Y + dy;
            var nx = (double)x2 - x1;
            var ny = (double)y2 - y1;
            var n_len = Math.Sqrt(nx * nx + ny * ny);
            nx /= n_len;
            ny /= n_len;

            x1 = (int)(x1 + nx * 2);
            y1 = (int)(y1 + ny * 2);
            x2 = (int)(x2 - nx * 2);
            y2 = (int)(y2 - ny * 2);

            if (selected) {
                g.DrawLine(HoverColor, x1, y1, x2, y2);
            } else {
                if (reverse) {
                    switch (mWireColor) {
                    case EWire.BLACK:
                        g.DrawLine(BLACK, x1, y1, x2, y2); break;
                    case EWire.BLUE:
                        g.DrawLine(BLUE, x1, y1, x2, y2); break;
                    case EWire.RED:
                        g.DrawLine(RED, x1, y1, x2, y2); break;
                    case EWire.GREEN:
                        g.DrawLine(GREEN, x1, y1, x2, y2); break;
                    case EWire.YELLOW:
                        g.DrawLine(YELLOW, x1, y1, x2, y2); break;
                    }
                } else {
                    switch (mWireColor) {
                    case EWire.BLACK:
                        g.DrawLine(BBLACK, x1, y1, x2, y2); break;
                    case EWire.BLUE:
                        g.DrawLine(BBLUE, x1, y1, x2, y2); break;
                    case EWire.RED:
                        g.DrawLine(BRED, x1, y1, x2, y2); break;
                    case EWire.GREEN:
                        g.DrawLine(BGREEN, x1, y1, x2, y2); break;
                    case EWire.YELLOW:
                        g.DrawLine(BYELLOW, x1, y1, x2, y2); break;
                    }
                }
            }
        }

        Point nearPointOnLine(Point point) {
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
}
