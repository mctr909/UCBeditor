using System;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.IO;

namespace UCBeditor {
    struct Item {
        public enum EType {
            LAND,
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
        static readonly Pen LandColor = new Pen(Color.FromArgb(192, 192, 0), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen TIN_W = new Pen(Color.FromArgb(111, 111, 111), 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen TIN_H = new Pen(Color.FromArgb(191, 191, 191), 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
        static readonly Pen TIN_N = new Pen(Color.FromArgb(211, 211, 211), 1.0f) { DashPattern = new float[] { 1, 1 } };

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

        EWire mWireColor;

        public Item(string line) {
            Rotate = RotateFlipType.RotateNoneFlipNone;
            PartsGroup = "";
            PartsName = "";
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
            mWireColor = EWire.BLACK;
        }

        public Item(Point begin, Point end) {
            Type = EType.TIN;
            Begin = begin;
            End = end;
            Rotate = RotateFlipType.RotateNoneFlipNone;
            PartsGroup = "";
            PartsName = "";
            mWireColor = EWire.BLACK;
        }

        public Item(Point begin, Point end, EWire color) {
            Type = EType.WIRE;
            Begin = begin;
            End = end;
            Rotate = RotateFlipType.RotateNoneFlipNone;
            PartsGroup = "";
            PartsName = "";
            mWireColor = color;
        }

        public Item(Point pos, RotateFlipType rot, string group, string name) {
            Type = EType.PARTS;
            Begin = pos;
            End = pos;
            Rotate = rot;
            PartsGroup = group;
            PartsName = name;
            mWireColor = EWire.BLACK;
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
                    g.FillEllipse(TIN_H.Brush, x1, y1, 8, 8);
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
                if (reverse) {
                    g.FillPie(TIN_W.Brush, x1 - 4, y1 - 4, 8, 8, 0, 360);
                    g.FillPie(TIN_W.Brush, x2 - 4, y2 - 4, 8, 8, 0, 360);
                    g.DrawLine(TIN_W, x1, y1, x2, y2);
                    g.DrawLine(TIN_N, x1, y1, x2, y2);
                } else {
                    g.FillPie(TIN_H.Brush, x1 - 4, y1 - 4, 8, 8, 0, 360);
                    g.FillPie(TIN_H.Brush, x2 - 4, y2 - 4, 8, 8, 0, 360);
                    g.DrawLine(TIN_H, x1, y1, x2, y2);
                    g.DrawLine(TIN_N, x1, y1, x2, y2);
                }
            }
        }

        void DrawWire(Graphics g, int dx, int dy, bool reverse, bool selected) {
            var x1 = Begin.X + dx;
            var y1 = Begin.Y + dy;
            var x2 = End.X + dx;
            var y2 = End.Y + dy;
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
    }
}
