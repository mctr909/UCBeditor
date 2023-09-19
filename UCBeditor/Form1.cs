using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace UCBeditor {
	public partial class Form1 : Form {
        readonly Pen GridColor = new Pen(Color.Black, 0.5f);
		readonly Pen DragColor = Pens.Blue;
		readonly Pen HoverColor = Pens.Blue;
		readonly string ElementPath = AppDomain.CurrentDomain.BaseDirectory + "element\\";

		enum EditMode {
			INVALID,
			SELECT,
			WIRE,
			PARTS,
			LAND
		}

		enum RecordType {
			WIRE,
			PARTS,
			LAND
		}

		enum WireColor {
			BLACK,
			BLUE,
			RED,
			GREEN,
			YELLOW,
			TIN
		}

		struct Record {
			static readonly Pen LandColor = new Pen(Color.FromArgb(192, 192, 0), 1.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
			static readonly Pen BLACK = new Pen(Color.FromArgb(71, 71, 71), 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
			static readonly Pen BLUE = new Pen(Color.FromArgb(63, 63, 221), 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
			static readonly Pen RED = new Pen(Color.FromArgb(211, 63, 63), 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
			static readonly Pen GREEN = new Pen(Color.FromArgb(47, 167, 47), 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
			static readonly Pen YELLOW = new Pen(Color.FromArgb(191, 191, 0), 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            static readonly Pen TIN_W = new Pen(Color.FromArgb(111, 111, 111), 3.0f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            static readonly Pen TIN_N = new Pen(Color.FromArgb(191, 191, 191), 1.0f) { DashPattern = new float[] { 1, 1 } };

            public RecordType Type;
			public Point Begin;
			public Point End;
			public Point Offset;

			public RotateFlipType Rotate;
			public string Parts;

			WireColor mWireColor;

			public void DrawWire(Graphics g) {
                switch (mWireColor) {
                case WireColor.BLACK:
                    g.DrawLine(BLACK, Begin, End); break;
                case WireColor.BLUE:
                    g.DrawLine(BLUE, Begin, End); break;
                case WireColor.RED:
                    g.DrawLine(RED, Begin, End); break;
                case WireColor.GREEN:
                    g.DrawLine(GREEN, Begin, End); break;
                case WireColor.YELLOW:
                    g.DrawLine(YELLOW, Begin, End); break;
                case WireColor.TIN:
					g.FillPie(TIN_W.Brush, Begin.X - 4, Begin.Y - 4, 8, 8, 0, 360);
                    g.FillPie(TIN_W.Brush, End.X - 4, End.Y - 4, 8, 8, 0, 360);
                    g.DrawLine(TIN_W, Begin, End);
                    g.DrawLine(TIN_N, Begin, End);
                    break;
                default:
                    g.DrawLine(Pens.Purple, Begin, End); break;
                }
            }

			public void DrawLand(Graphics g, bool reverse) {
                DrawLand(g, Begin, reverse);
            }

            public void DrawLand(Graphics g, Point pos, bool reverse) {
                var x1 = pos.X - 4;
                var y1 = pos.Y - 4;
                var x2 = pos.X - 2;
                var y2 = pos.Y - 2;
                if (reverse) {
                    g.FillEllipse(LandColor.Brush, x1, y1, 8, 8);
                    g.FillEllipse(Brushes.White, x2, y2, 4, 4);
                } else {
                    g.FillEllipse(TIN_W.Brush, x1, y1, 8, 8);
                    g.FillEllipse(Brushes.White, x2, y2, 4, 4);
                }
            }

            public void Load(string line) {
				var cols = line.Split('\t');
				switch (cols[0]) {
				case "WIRE":
					SetWire(
						new Point(int.Parse(cols[1]), int.Parse(cols[2])),
						new Point(int.Parse(cols[3]), int.Parse(cols[4])),
						(WireColor)Enum.Parse(typeof(WireColor), cols[5])
					);
					break;
				case "LAND":
					SetLand(new Point(int.Parse(cols[1]), int.Parse(cols[2])));
					break;
				case "PARTS":
					SetParts(
						new Point(int.Parse(cols[1]), int.Parse(cols[2])),
						new Point(int.Parse(cols[3]), int.Parse(cols[4])),
						(RotateFlipType)int.Parse(cols[5]),
						cols[6]
					);
					break;
				}
			}

			public void Write(StreamWriter sw) {
				switch (Type) {
				case RecordType.WIRE:
					sw.WriteLine(
						"{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
						Type,
						Begin.X,
						Begin.Y,
						End.X,
						End.Y,
						mWireColor
					);
					break;
				case RecordType.LAND:
					sw.WriteLine(
						"{0}\t{1}\t{2}",
						Type,
						Begin.X,
						Begin.Y
					);
					break;
				case RecordType.PARTS:
					sw.WriteLine(
						"{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
						Type,
						Begin.X,
						Begin.Y,
						Offset.X,
						Offset.Y,
						(int)Rotate,
						Parts
					);
					break;
				}
			}

			public void SetWire(Point begin, Point end, WireColor color) {
				Type = RecordType.WIRE;
				Begin = begin;
				End = end;
				mWireColor = color;
			}

			public void SetLand(Point pos) {
				Type = RecordType.LAND;
				Begin = pos;
				End = pos;
			}

			public void SetParts(Point pos, Point ofs, RotateFlipType rot, string path) {
				Type = RecordType.PARTS;
				Begin = pos;
				End = pos;
				Offset = ofs;
				Rotate = rot;
				Parts = path;
			}
		}

		struct Rect {
			public Point A;
			public Point B;
			public Rect(Point a, Point b) {
				A = a;
				B = b;
			}
		}

		string mFilePath = "";

		Dictionary<int, Record> mList = new Dictionary<int, Record>();
		Dictionary<int, Record> mClipBoard = new Dictionary<int, Record>();
		EditMode mEditMode = EditMode.WIRE;
		WireColor mWireColor = WireColor.BLACK;
		RotateFlipType mCurRotate = RotateFlipType.Rotate270FlipXY;
		int mCurGridWidth = 12;

		bool mIsDrag;
		Point mMousePos = new Point();
		Point mBeginPos = new Point();
		Point mEndPos = new Point();
		Rect mRect = new Rect();

		string mSelectedPartsPath;
		Point mSelectedPartsOfs;
		Point mSelectedPartsPos;
		Point mSelectedPartsSize;
		Point mCurOfs;

		public Form1() {
			InitializeComponent();

			panelResize();
			picBoard.Width = 12 * 80;
			picBoard.Height = 12 * 80;

			setPartsList();
			selectLine(tsbCursor);

			tscGridWidth.SelectedIndex = 0;

			timer1.Interval = 50;
			timer1.Enabled = true;
			timer1.Start();
		}

		#region resize
		private void splitContainer1_Panel1_Resize(object sender, EventArgs e) {
			panelResize();
		}

		private void splitContainer1_Panel2_Resize(object sender, EventArgs e) {
			panelResize();
		}

		private void panelResize() {
			pnlBoard.Width = splitContainer1.Panel1.Width - 4;
			pnlBoard.Height = splitContainer1.Panel1.Height - tsBoard.Height - 6;
			pnlParts.Width = splitContainer1.Panel2.Width - 4;
			pnlParts.Height = splitContainer1.Panel2.Height - tsParts.Height - 6;
		}
		#endregion

		#region ToolStripButton
		private void tsbCursor_Click(object sender, EventArgs e) {
			selectLine(tsbCursor);
		}

		private void tsbLand_Click(object sender, EventArgs e) {
			selectLine(tsbLand);
		}

		private void tsbWireBlack_Click(object sender, EventArgs e) {
			selectLine(tsbWireBlack);
		}

		private void tsbWireRed_Click(object sender, EventArgs e) {
			selectLine(tsbWireRed);
		}

		private void tsbWireBlue_Click(object sender, EventArgs e) {
			selectLine(tsbWireBlue);
		}

		private void tsbWireGreen_Click(object sender, EventArgs e) {
			selectLine(tsbWireGreen);
		}

		private void tsbWireYellow_Click(object sender, EventArgs e) {
			selectLine(tsbWireYellow);
		}

        private void tsbWireTin_Click(object sender, EventArgs e) {
            selectLine(tsbWireTin);
        }

        private void tsbFront_Click(object sender, EventArgs e) {
			tsbReverse.Checked = false;
			tsbFront.Checked = true;
		}

		private void tsbReverse_Click(object sender, EventArgs e) {
			tsbFront.Checked = false;
			tsbReverse.Checked = true;
		}

		private void tscGridWidth_SelectedIndexChanged(object sender, EventArgs e) {
			switch (tscGridWidth.SelectedIndex) {
			case 0:
				mCurGridWidth = 12;
				break;
			case 1:
				mCurGridWidth = 6;
				break;
			case 2:
				mCurGridWidth = 3;
				break;
			}
		}
		#endregion

		#region MouseEvent
		private void picBoard_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				setBeginPos();

				switch (mEditMode) {
				case EditMode.SELECT:
				case EditMode.WIRE:
					mRect = new Rect();
					mIsDrag = true;
					break;
				}
			}

			if (e.Button == MouseButtons.Right) {
				var temp = new Dictionary<int, Record>();
				var idx = 0;
				for (var d = 0; d < mList.Count; ++d) {
					if (!isOnLine(mList[d], mMousePos)) {
						temp.Add(idx, mList[d]);
						++idx;
					}
				}
				mList = temp;
			}
		}

		private void picBoard_MouseMove(object sender, MouseEventArgs e) {
			setEndPos();
		}

		private void picBoard_MouseUp(object sender, MouseEventArgs e) {
			if (e.Button != MouseButtons.Left) {
				return;
			}

			var rec = new Record();

			switch (mEditMode) {
			case EditMode.SELECT:
				mRect = new Rect();
				mRect.A = mBeginPos;
				mRect.B = mEndPos;
				mIsDrag = false;
				break;

			case EditMode.WIRE:
				rec.SetWire(mBeginPos, mEndPos, mWireColor);
				mList.Add(mList.Count, rec);
				mIsDrag = false;
				break;
			case EditMode.LAND:
				rec.SetLand(mEndPos);
				mList.Add(mList.Count, rec);
				break;
			case EditMode.PARTS:
				rec.SetParts(mEndPos, mCurOfs, mCurRotate, mSelectedPartsPath);
				mList.Add(mList.Count, rec);
				break;
			}

			foreach (var p in mClipBoard) {
				rec = p.Value;
				rec.Begin.X += mEndPos.X;
				rec.Begin.Y += mEndPos.Y;
				rec.End.X += mEndPos.X;
				rec.End.Y += mEndPos.Y;
				mList.Add(mList.Count, rec);
			}
			mClipBoard.Clear();
		}
		#endregion

		#region MenuberEvent
		private void 新規作成NToolStripMenuItem_Click(object sender, EventArgs e) {
			mList.Clear();
			mClipBoard.Clear();
			mFilePath = "";

			tsbCursor.Checked = true;
			selectLine(tsbCursor);
		}

		private void 開くOToolStripMenuItem_Click(object sender, EventArgs e) {
			openFileDialog1.Filter = "UCBeditorファイル(*.ucb)|*.ucb";
			openFileDialog1.FileName = "";
			openFileDialog1.ShowDialog();
			var filePath = openFileDialog1.FileName;
			if (!File.Exists(filePath)) {
				return;
			}

			mFilePath = filePath;
			mBeginPos = new Point();
			mEndPos = new Point();
			mRect = new Rect();

			tsbCursor.Checked = true;
			selectLine(tsbCursor);

			var fs = new FileStream(filePath, FileMode.Open);
			var sr = new StreamReader(fs);

			mList.Clear();
			while (!sr.EndOfStream) {
				var rec = new Record();
				rec.Load(sr.ReadLine());
				mList.Add(mList.Count, rec);
			}
			sr.Close();
			fs.Close();
			sr.Dispose();
			fs.Dispose();
		}

		private void 上書き保存SToolStripMenuItem_Click(object sender, EventArgs e) {
			var filePath = "";

			if (string.IsNullOrEmpty(mFilePath) || !File.Exists(mFilePath)) {
				saveFileDialog1.Filter = "UCBeditorファイル(*.ucb)|*.ucb";
				saveFileDialog1.FileName = "";
				saveFileDialog1.ShowDialog();
				filePath = saveFileDialog1.FileName;
				if (string.IsNullOrEmpty(filePath) || !Directory.Exists(Path.GetDirectoryName(filePath))) {
					return;
				}
				mFilePath = filePath;
			} else {
				filePath = mFilePath;
			}

			saveFile(filePath);
		}

		private void 名前を付けて保存AToolStripMenuItem_Click(object sender, EventArgs e) {
			saveFileDialog1.Filter = "UCBeditorファイル(*.ucb)|*.ucb";
			saveFileDialog1.FileName = "";
			saveFileDialog1.ShowDialog();
			var filePath = saveFileDialog1.FileName;
			if (string.IsNullOrEmpty(filePath) || !Directory.Exists(Path.GetDirectoryName(filePath))) {
				return;
			}

			saveFile(filePath);
		}

		private void 選択SToolStripMenuItem_Click(object sender, EventArgs e) {
			tsbCursor.Checked = true;
			selectLine(tsbCursor);
			mClipBoard.Clear();
		}

		private void 切り取りTToolStripMenuItem_Click(object sender, EventArgs e) {
			var temp = new Dictionary<int, Record>();
			var min = new Point(int.MaxValue, int.MaxValue);
			for (var d = 0; d < mList.Count; ++d) {
				if (isOnLine(mList[d], mRect)) {
					mClipBoard.Add(mClipBoard.Count, mList[d]);

					var begin = new Point(
						mList[d].Begin.X - mList[d].Offset.X,
						mList[d].Begin.Y - mList[d].Offset.Y
					);
					var end = new Point(
						mList[d].End.X - mList[d].Offset.X,
						mList[d].End.Y - mList[d].Offset.Y
					);
					if (begin.X < min.X) {
						min.X = begin.X;
					}
					if (end.X < min.X) {
						min.X = end.X;
					}
					if (begin.Y < min.Y) {
						min.Y = begin.Y;
					}
					if (end.Y < min.Y) {
						min.Y = end.Y;
					}
				} else {
					temp.Add(temp.Count, mList[d]);
				}
			}

			for (var i = 0; i < mClipBoard.Count; ++i) {
				var p = mClipBoard[i];
				p.Begin.X -= (min.X / 12) * 12;
				p.Begin.Y -= (min.Y / 12) * 12;
				p.End.X -= (min.X / 12) * 12;
				p.End.Y -= (min.Y / 12) * 12;
				mClipBoard[i] = p;
			}

			mList = temp;
			mRect = new Rect();
		}

		private void コピーCToolStripMenuItem_Click(object sender, EventArgs e) {
			var min = new Point(int.MaxValue, int.MaxValue);

			for (var d = 0; d < mList.Count; ++d) {
				if (isOnLine(mList[d], mRect)) {
					mClipBoard.Add(mClipBoard.Count, mList[d]);

					var begin = new Point(
						mList[d].Begin.X - mList[d].Offset.X,
						mList[d].Begin.Y - mList[d].Offset.Y
					);
					var end = new Point(
						mList[d].End.X - mList[d].Offset.X,
						mList[d].End.Y - mList[d].Offset.Y
					);
					if (begin.X < min.X) {
						min.X = begin.X;
					}
					if (end.X < min.X) {
						min.X = end.X;
					}
					if (begin.Y < min.Y) {
						min.Y = begin.Y;
					}
					if (end.Y < min.Y) {
						min.Y = end.Y;
					}
				}
			}

			for (var i = 0; i < mClipBoard.Count; ++i) {
				var p = mClipBoard[i];
				p.Begin.X -= (min.X / 12) * 12;
				p.Begin.Y -= (min.Y / 12) * 12;
				p.End.X -= (min.X / 12) * 12;
				p.End.Y -= (min.Y / 12) * 12;
				mClipBoard[i] = p;
			}

			mRect = new Rect();
		}

		private void 貼り付けPToolStripMenuItem_Click(object sender, EventArgs e) {
			foreach (var p in mClipBoard) {
				var rec = p.Value;
				rec.Begin.X += mEndPos.X;
				rec.Begin.Y += mEndPos.Y;
				rec.End.X += mEndPos.X;
				rec.End.Y += mEndPos.Y;
				mList.Add(mList.Count, rec);
			}
		}

		private void 削除DToolStripMenuItem_Click(object sender, EventArgs e) {
			var temp = new Dictionary<int, Record>();
			for (var d = 0; d < mList.Count; ++d) {
				if (!isOnLine(mList[d], mRect)) {
					temp.Add(temp.Count, mList[d]);
				}
			}
			mList = temp;
			mRect = new Rect();
		}

		private void 左回転LToolStripMenuItem_Click(object sender, EventArgs e) {
			switch (mCurRotate) {
			case RotateFlipType.RotateNoneFlipXY:
				mCurRotate = RotateFlipType.Rotate90FlipXY;
				break;
			case RotateFlipType.Rotate90FlipXY:
				mCurRotate = RotateFlipType.Rotate180FlipXY;
				break;
			case RotateFlipType.Rotate180FlipXY:
				mCurRotate = RotateFlipType.Rotate270FlipXY;
				break;
			case RotateFlipType.Rotate270FlipXY:
				mCurRotate = RotateFlipType.RotateNoneFlipXY;
				break;
			}

			setPartsOfs();
			setEndPos();
		}

		private void 右回転RToolStripMenuItem_Click(object sender, EventArgs e) {
			switch (mCurRotate) {
			case RotateFlipType.RotateNoneFlipXY:
				mCurRotate = RotateFlipType.Rotate270FlipXY;
				break;
			case RotateFlipType.Rotate270FlipXY:
				mCurRotate = RotateFlipType.Rotate180FlipXY;
				break;
			case RotateFlipType.Rotate180FlipXY:
				mCurRotate = RotateFlipType.Rotate90FlipXY;
				break;
			case RotateFlipType.Rotate90FlipXY:
				mCurRotate = RotateFlipType.RotateNoneFlipXY;
				break;
			}

			setPartsOfs();
			setEndPos();
		}

		#endregion

		private void timer1_Tick(object sender, EventArgs e) {
			var bmp = new Bitmap(picBoard.Width, picBoard.Height);
			var g = Graphics.FromImage(bmp);

			g.FillRectangle(Pens.White.Brush, 0, 0, bmp.Width, bmp.Height);

			for (var y = 0; y < bmp.Height; y += mCurGridWidth) {
				for (var x = 0; x < bmp.Width; x += mCurGridWidth) {
					g.DrawRectangle(GridColor, x, y, 0.5f, 0.5f);
				}
			}

			drawList(g);
			drawClipBoard(g);
			drawCur(g);

			g.DrawEllipse(DragColor, mEndPos.X - 3, mEndPos.Y - 3, 6, 6);

			picBoard.Image = bmp;
		}

		private void selectLine(ToolStripButton btn) {
			tsbCursor.Checked = tsbCursor == btn;
			tsbLand.Checked = tsbLand == btn;
			tsbWireBlack.Checked = tsbWireBlack == btn;
			tsbWireRed.Checked = tsbWireRed == btn;
			tsbWireBlue.Checked = tsbWireBlue == btn;
			tsbWireGreen.Checked = tsbWireGreen == btn;
			tsbWireYellow.Checked = tsbWireYellow == btn;
            tsbWireTin.Checked = tsbWireTin == btn;

            if (tsbCursor.Checked) {
				mEditMode = EditMode.SELECT;
			}
			if (tsbLand.Checked) {
				mEditMode = EditMode.LAND;
				mWireColor = WireColor.BLACK;
			}
			if (tsbWireBlack.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = WireColor.BLACK;
			}
			if (tsbWireRed.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = WireColor.RED;
			}
			if (tsbWireBlue.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = WireColor.BLUE;
			}
			if (tsbWireGreen.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = WireColor.GREEN;
			}
			if (tsbWireYellow.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = WireColor.YELLOW;
			}
            if (tsbWireTin.Checked) {
                mEditMode = EditMode.WIRE;
                mWireColor = WireColor.TIN;
            }

            mSelectedPartsPath = "";
			selectPartsList();
		}

		private void selectPartsList() {
			mSelectedPartsOfs = new Point();
			mSelectedPartsPos = new Point();
			mSelectedPartsSize = new Point();
			mCurOfs = new Point();
			mRect = new Rect();

			foreach (var ctrl in pnlParts.Controls) {
				if (ctrl.GetType().Name == "Panel") {
					var panel = (Panel)ctrl;
					if (panel.Name == mSelectedPartsPath) {
						panel.BackColor = SystemColors.ButtonShadow;
						panel.BorderStyle = BorderStyle.Fixed3D;
						var fname = Path.GetFileNameWithoutExtension(mSelectedPartsPath).Split('_');
						mSelectedPartsOfs = new Point(int.Parse(fname[1]), int.Parse(fname[2]));
						mSelectedPartsPos = new Point(int.Parse(fname[3]), int.Parse(fname[4]));
						var image = (PictureBox)panel.Controls[0];
						mSelectedPartsSize = new Point(image.Width, image.Height);
						setPartsOfs();

						mIsDrag = false;
						mEditMode = EditMode.PARTS;
					} else {
						panel.BackColor = SystemColors.ButtonFace;
						panel.BorderStyle = BorderStyle.None;
					}
				}
			}
		}

		private void drawToolPanel(string type) {
			var curTop = 0;
			pnlParts.Controls.Clear();

			var dir = ElementPath + "solid\\" + type;
			if (!Directory.Exists(dir)) {
				return;
			}
			var paths = Directory.GetFiles(dir);

			for (var i = 0; i < paths.Length; ++i) {
				var picture = new PictureBox();
				var bmp = new Bitmap(paths[i]);
				var path = paths[i].Replace(ElementPath, "").Replace("solid\\", "");

				picture.Image = bmp;
				picture.Top = 2;
				picture.Left = 2;
				picture.Width = bmp.Width;
				picture.Height = bmp.Height;

				var lbl = new Label();
				lbl.Text = Path.GetFileNameWithoutExtension(path).Split('_')[0];
				lbl.Height = 16;
				lbl.Top = curTop + picture.Height + lbl.Height;
				lbl.Left = 8;

				var panel = new Panel();
				panel.Name = path;
				panel.Controls.Add(picture);
				panel.BackColor = SystemColors.ButtonFace;
				panel.Width = bmp.Width + 8;
				panel.Height = bmp.Height + 8;
				panel.Left = 8;
				panel.Top = curTop + 8;

				picture.MouseDown += new MouseEventHandler((object sender, MouseEventArgs e) => {
					mSelectedPartsPath = panel.Name;
					selectPartsList();
				});

				pnlParts.Controls.Add(panel);
				pnlParts.Controls.Add(lbl);

				curTop += panel.Height + 8 + 18;
			}
		}

		private void setPartsList() {
			var files = Directory.GetFiles(ElementPath);
			foreach (var filePath in files) {
				var tsb = new ToolStripButton();
				tsb.Image = new Bitmap(filePath);
				tsb.Name = Path.GetFileNameWithoutExtension(filePath);
				tsb.Click += new EventHandler((object sender, EventArgs e) => {
					for (var j = 0; j < tsParts.Items.Count; ++j) {
						if ("ToolStripButton" == tsParts.Items[j].GetType().Name) {
							var item = (ToolStripButton)tsParts.Items[j];
							item.Checked = false;
						}
					}
					tsb.Checked = true;
					drawToolPanel(tsb.Name);
				});
				tsParts.Items.Add(tsb);
			}
		}

		private void setPartsOfs() {
			switch (mCurRotate) {
			case RotateFlipType.RotateNoneFlipXY:
				mCurOfs.X = mSelectedPartsSize.X - mSelectedPartsOfs.X;
				mCurOfs.Y = mSelectedPartsSize.Y - mSelectedPartsOfs.Y;
				break;
			case RotateFlipType.Rotate90FlipXY:
				mCurOfs.X = mSelectedPartsSize.Y - mSelectedPartsOfs.Y;
				mCurOfs.Y = mSelectedPartsSize.X - mSelectedPartsOfs.X;
				break;
			case RotateFlipType.Rotate180FlipXY:
				mCurOfs.X = mSelectedPartsOfs.X;
				mCurOfs.Y = mSelectedPartsSize.Y - mSelectedPartsOfs.Y;
				break;
			case RotateFlipType.Rotate270FlipXY:
				mCurOfs.X = mSelectedPartsSize.Y - mSelectedPartsOfs.Y;
				mCurOfs.Y = mSelectedPartsOfs.X;
				break;
			}
		}

		private void setBeginPos() {
            mMousePos = picBoard.PointToClient(Cursor.Position);
			mBeginPos.X = (int)((double)mMousePos.X / mCurGridWidth + 0.5) * mCurGridWidth;
			mBeginPos.Y = (int)((double)mMousePos.Y / mCurGridWidth + 0.5) * mCurGridWidth;
		}

		private void setEndPos() {
            mMousePos = picBoard.PointToClient(Cursor.Position);
			var gx = (int)((double)mMousePos.X / mCurGridWidth + 0.5) * mCurGridWidth;
			var gy = (int)((double)mMousePos.Y / mCurGridWidth + 0.5) * mCurGridWidth;
			if (0 < mSelectedPartsPos.X || 0 < mSelectedPartsPos.Y) {
                switch (mCurRotate) {
				case RotateFlipType.RotateNoneFlipXY:
				case RotateFlipType.Rotate180FlipXY:
					mEndPos.X = gx + mSelectedPartsPos.X;
					mEndPos.Y = gy + mSelectedPartsPos.Y;
					break;
				case RotateFlipType.Rotate90FlipXY:
				case RotateFlipType.Rotate270FlipXY:
					mEndPos.X = gx + mSelectedPartsPos.Y;
					mEndPos.Y = gy + mSelectedPartsPos.X;
					break;
				}
			} else {
				mEndPos.X = gx;
				mEndPos.Y = gy;
			}
		}

		private void saveFile(string filePath) {
			var fs = new FileStream(filePath, FileMode.Create);
			var sw = new StreamWriter(fs);
			foreach (var rec in mList.Values) {
				rec.Write(sw);
			}
			sw.Close();
			fs.Close();
			sw.Dispose();
			fs.Dispose();
		}

		private Point nearPointOnLine(Record record, Point point) {
			var abX = record.End.X - record.Begin.X;
			var abY = record.End.Y - record.Begin.Y;
			var apX = point.X - record.Begin.X;
			var apY = point.Y - record.Begin.Y;

			var abL2 = abX * abX + abY * abY;

			if (0.0 == abL2) {
				return record.Begin;
			}

			var r = (double)(abX * apX + abY * apY) / abL2;
			if (r <= 0.0) {
				return record.Begin;
			} else if (1.0 <= r) {
				return record.End;
			} else {
				return new Point((int)(record.Begin.X + r * abX), (int)(record.Begin.Y + r * abY));
			}
		}

		private bool isOnLine(Record record, Point point) {
			var p = nearPointOnLine(record, point);
			var sx = p.X - point.X;
			var sy = p.Y - point.Y;
			return (Math.Sqrt(sx * sx + sy * sy) < 6.0);
		}

		private bool isOnLine(Record record, Rect rect) {
			var rectX1 = rect.B.X < rect.A.X ? rect.B.X : rect.A.X;
			var rectX2 = rect.B.X < rect.A.X ? rect.A.X : rect.B.X;
			var rectY1 = rect.B.Y < rect.A.Y ? rect.B.Y : rect.A.Y;
			var rectY2 = rect.B.Y < rect.A.Y ? rect.A.Y : rect.B.Y;
			var rx1 = record.End.X < record.Begin.X ? record.End.X : record.Begin.X;
			var rx2 = record.End.X < record.Begin.X ? record.Begin.X : record.End.X;
			var ry1 = record.End.Y < record.Begin.Y ? record.End.Y : record.Begin.Y;
			var ry2 = record.End.Y < record.Begin.Y ? record.Begin.Y : record.End.Y;

			return (rectX1 <= rx1 && rx1 <= rectX2 && rectY1 <= ry1 && ry1 <= rectY2 &&
				rectX1 <= rx2 && rx2 <= rectX2 && rectY1 <= ry2 && ry2 <= rectY2);
		}

		private void drawList(Graphics g) {
			foreach (var d in mList.Values) {
				if (RecordType.WIRE != d.Type) {
					continue;
				}
				if (isOnLine(d, mMousePos) || isOnLine(d, mRect)) {
					g.DrawLine(HoverColor, d.Begin, d.End);
				} else {
					d.DrawWire(g);
				}
			}
			foreach (var d in mList.Values) {
				if (RecordType.LAND != d.Type) {
					continue;
				}
				if (isOnLine(d, mMousePos) || isOnLine(d, mRect)) {
                    var x1 = d.Begin.X - 4;
                    var y1 = d.Begin.Y - 4;
                    var x2 = d.Begin.X - 2;
                    var y2 = d.Begin.Y - 2;
                    g.DrawArc(HoverColor, x1, y1, 8, 8, 0, 360);
					g.DrawArc(HoverColor, x2, y2, 4, 4, 0, 360);
				} else {
					d.DrawLand(g, tsbReverse.Checked);
				}
			}
			foreach (var d in mList.Values) {
				if (RecordType.PARTS != d.Type) {
					continue;
				}
				var filePath = d.Parts;
				if (tsbReverse.Checked || isOnLine(d, mRect)) {
					filePath = ElementPath + "alpha\\" + filePath;
				} else {
					filePath = ElementPath + "solid\\" + filePath;
				}
				var temp = new Bitmap(filePath);
				temp.RotateFlip(d.Rotate);
				g.DrawImage(temp, new Point(
					d.Begin.X - d.Offset.X,
					d.Begin.Y - d.Offset.Y
				));
			}
		}

		private void drawClipBoard(Graphics g) {
			foreach (var d in mClipBoard.Values) {
				if (RecordType.WIRE == d.Type) {
					var b = new Point(d.Begin.X + mEndPos.X, d.Begin.Y + mEndPos.Y);
					var e = new Point(d.End.X + mEndPos.X, d.End.Y + mEndPos.Y);
					g.DrawLine(HoverColor, b, e);
				}
			}
			foreach (var d in mClipBoard.Values) {
				if (RecordType.LAND == d.Type) {
					var b = new Point(d.Begin.X + mEndPos.X, d.Begin.Y + mEndPos.Y);
                    var x1 = b.X - 4;
                    var y1 = b.Y - 4;
                    var x2 = b.X - 2;
                    var y2 = b.Y - 2;
                    g.DrawArc(HoverColor, x1, y1, 8, 8, 0, 360);
                    g.DrawArc(HoverColor, x2, y2, 4, 4, 0, 360);
                }
			}
			foreach (var d in mClipBoard.Values) {
				if (RecordType.PARTS == d.Type) {
					var filePath = ElementPath + "alpha\\" + d.Parts;
					var b = new Point(d.Begin.X + mEndPos.X, d.Begin.Y + mEndPos.Y);
					var temp = new Bitmap(filePath);
					temp.RotateFlip(d.Rotate);
					g.DrawImage(temp, new Point(
						b.X - d.Offset.X,
						b.Y - d.Offset.Y
					));
				}
			}
		}

		private void drawCur(Graphics g) {
			switch (mEditMode) {
			case EditMode.SELECT:
				if (mIsDrag) {
					var x = mBeginPos.X < mEndPos.X ? mBeginPos.X : mEndPos.X;
					var y = mBeginPos.Y < mEndPos.Y ? mBeginPos.Y : mEndPos.Y;
					g.DrawRectangle(
						HoverColor,
						x, y,
						Math.Abs(mEndPos.X - mBeginPos.X),
						Math.Abs(mEndPos.Y - mBeginPos.Y)
					);
				}
				break;

			case EditMode.WIRE:
				if (mIsDrag) {
					g.DrawLine(DragColor, mBeginPos, mEndPos);
				}
				break;

			case EditMode.LAND:
				g.DrawArc(
					Pens.Gray,
					mEndPos.X - 4, mEndPos.Y - 4,
					8, 8,
					0, 360
				);
				g.FillEllipse(
					Brushes.White,
					mEndPos.X - 2, mEndPos.Y - 2,
					4, 4
				);
				break;

			case EditMode.PARTS:
				var filePath = ElementPath + "alpha\\" + mSelectedPartsPath;
				var temp = new Bitmap(filePath);
				temp.RotateFlip(mCurRotate);
				g.DrawImage(temp, new Point(
					mEndPos.X - mCurOfs.X,
					mEndPos.Y - mCurOfs.Y
				));
				break;
			}
		}
	}
}