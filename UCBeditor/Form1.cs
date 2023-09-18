using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace UCBeditor {
    public partial class Form1 : Form {
        readonly Pen GridColor = new Pen(Color.Black, 0.5f);
        readonly Pen DragColor = Pens.Blue;
        readonly Pen HoverColor = Pens.Blue;
		readonly Pen LandColor = new Pen(Color.FromArgb(192, 192, 0), 1.0f);

        enum RecordType {
            INVALID,
            CURSOL,
            LINE,
            PARTS,
            LAND
        }

        enum ColorType {
            BLACK,
            WHITE,
            BLUE,
            RED,
            GREEN,
            YELLOW
        }

        struct Record {
            public RecordType Type;
            public ColorType Color;
            public RotateFlipType Rotate;
            public Point Begin;
            public Point End;
            public Point Offset;
            public string Parts;

            public Pen LineColor {
                get {
                    switch (Color) {
                    case ColorType.BLACK:
                        return new Pen(System.Drawing.Color.FromArgb(47, 47, 47), 2.0f);
                    case ColorType.WHITE:
                        return new Pen(System.Drawing.Color.FromArgb(207, 207, 207), 2.0f);
                    case ColorType.BLUE:
                        return new Pen(System.Drawing.Color.FromArgb(0, 0, 191), 2.0f);
                    case ColorType.RED:
                        return new Pen(System.Drawing.Color.FromArgb(191, 0, 0), 2.0f);
                    case ColorType.GREEN:
                        return new Pen(System.Drawing.Color.FromArgb(0, 127, 0), 2.0f);
                    case ColorType.YELLOW:
                        return new Pen(System.Drawing.Color.FromArgb(191, 191, 0), 2.0f);
                    default:
                        return new Pen(System.Drawing.Color.FromArgb(47, 47, 47), 2.0f);
                    }
                }
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
        RecordType mCurType = RecordType.LINE;
        ColorType mCurColor = ColorType.BLACK;
        RotateFlipType mCurRotate = RotateFlipType.Rotate270FlipXY;
		int mCurGridWidth = 12;

        bool mIsBoardDrag;
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

        private void tsbLineBlack_Click(object sender, EventArgs e) {
            selectLine(tsbLineBlack);
        }

        private void tsbLineWhite_Click(object sender, EventArgs e) {
            selectLine(tsbLineWhite);
        }

        private void tsbLineRed_Click(object sender, EventArgs e) {
            selectLine(tsbLineRed);
        }

        private void tsbLineBlue_Click(object sender, EventArgs e) {
            selectLine(tsbLineBlue);
        }

        private void tsbLineGreen_Click(object sender, EventArgs e) {
            selectLine(tsbLineGreen);
        }

        private void tsbLineYellow_Click(object sender, EventArgs e) {
            selectLine(tsbLineYellow);
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

                switch (mCurType) {
                case RecordType.CURSOL:
                case RecordType.LINE:
					mRect = new Rect();
                    mIsBoardDrag = true;
                    break;
                }
            }

            if (e.Button == MouseButtons.Right) {
                var temp = new Dictionary<int, Record>();
                var idx = 0;
				for (var d = 0; d < mList.Count; ++d) {
					if (!isOnLine(mList[d], mEndPos)) {
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

            switch (mCurType) {
            case RecordType.CURSOL:
				mRect = new Rect();
				mRect.A = mBeginPos;
				mRect.B = mEndPos;
				mIsBoardDrag = false;
                break;

			case RecordType.LAND:
				rec.Type = mCurType;
				rec.Color = ColorType.BLACK;
				rec.Rotate = RotateFlipType.RotateNoneFlipXY;
				rec.Begin = mEndPos;
				rec.End = mEndPos;
				rec.Offset = new Point();
				rec.Parts = "";
				mList.Add(mList.Count, rec);
				break;

			case RecordType.PARTS:
				rec.Type = mCurType;
				rec.Color = ColorType.BLACK;
				rec.Rotate = mCurRotate;
				rec.Begin = mEndPos;
				rec.End = mEndPos;
				rec.Offset = mCurOfs;
				rec.Parts = mSelectedPartsPath;
				mList.Add(mList.Count, rec);
				break;

            case RecordType.LINE:
                rec.Type = mCurType;
                rec.Color = mCurColor;
                rec.Rotate = RotateFlipType.RotateNoneFlipXY;
                rec.Begin = mBeginPos;
                rec.End = mEndPos;
                rec.Offset = new Point();
                rec.Parts = "";
				mList.Add(mList.Count, rec);
                mIsBoardDrag = false;
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

			var rec = new Record();
			mList.Clear();
			while (!sr.EndOfStream) {
				var cols = sr.ReadLine().Split('\t');
				switch (cols[0]) {
				case "LAND":
					rec = new Record();
					rec.Type = RecordType.LAND;
					rec.Begin = new Point(int.Parse(cols[1]), int.Parse(cols[2]));
					rec.End = rec.Begin;
					rec.Offset = new Point();
					rec.Parts = "";
					mList.Add(mList.Count, rec);
					break;
				case "PARTS":
					rec = new Record();
					rec.Type = RecordType.PARTS;
					rec.Rotate = (RotateFlipType)int.Parse(cols[1]);
					rec.Begin = new Point(int.Parse(cols[2]), int.Parse(cols[3]));
					rec.End = rec.Begin;
					rec.Offset = new Point(int.Parse(cols[4]), int.Parse(cols[5]));
					rec.Parts = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, cols[6]);
					mList.Add(mList.Count, rec);
					break;
				case "LINE":
					rec = new Record();
					rec.Type = RecordType.LINE;
					rec.Color = (ColorType)int.Parse(cols[1]);
					rec.Begin = new Point(int.Parse(cols[2]), int.Parse(cols[3]));
					rec.End = new Point(int.Parse(cols[4]), int.Parse(cols[5]));
					rec.Offset = new Point();
					rec.Parts = "";
					mList.Add(mList.Count, rec);
					break;
				}
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
			}
			else {
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
				}
				else {
					temp.Add(temp.Count, mList[d]);
				}
			}

			for (var i=0; i < mClipBoard.Count; ++i) {
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

			for (var i=0; i < mClipBoard.Count; ++i) {
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

			for (var y=0; y < bmp.Height; y += mCurGridWidth) {
				for (var x=0; x < bmp.Width; x += mCurGridWidth) {
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
            tsbLineBlack.Checked = tsbLineBlack == btn;
            tsbLineWhite.Checked = tsbLineWhite == btn;
            tsbLineRed.Checked = tsbLineRed == btn;
            tsbLineBlue.Checked = tsbLineBlue == btn;
            tsbLineGreen.Checked = tsbLineGreen == btn;
            tsbLineYellow.Checked = tsbLineYellow == btn;

            if (tsbCursor.Checked) {
                mCurType = RecordType.CURSOL;
            }
            if (tsbLand.Checked) {
                mCurType = RecordType.LAND;
                mCurColor = ColorType.BLACK;
            }
            if (tsbLineBlack.Checked) {
                mCurType = RecordType.LINE;
                mCurColor = ColorType.BLACK;
            }
            if (tsbLineWhite.Checked) {
                mCurType = RecordType.LINE;
                mCurColor = ColorType.WHITE;
            }
            if (tsbLineRed.Checked) {
                mCurType = RecordType.LINE;
                mCurColor = ColorType.RED;
            }
            if (tsbLineBlue.Checked) {
                mCurType = RecordType.LINE;
                mCurColor = ColorType.BLUE;
            }
            if (tsbLineGreen.Checked) {
                mCurType = RecordType.LINE;
                mCurColor = ColorType.GREEN;
            }
            if (tsbLineYellow.Checked) {
                mCurType = RecordType.LINE;
                mCurColor = ColorType.YELLOW;
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

						mIsBoardDrag = false;
						mCurType = RecordType.PARTS;
					}
					else {
						panel.BackColor = SystemColors.ButtonFace;
						panel.BorderStyle = BorderStyle.None;
					}
				}
			}
		}

        private void drawToolPanel(string type) {
            var dir = System.AppDomain.CurrentDomain.BaseDirectory + "icon\\";
			var paths = Directory.GetFiles(dir + type);

            var curTop = 0;
            pnlParts.Controls.Clear();

            for (var i=0; i < paths.Length; ++i) {
                var picture = new PictureBox();
                var bmp = new Bitmap(paths[i]);
                picture.Image = bmp;
				picture.Top = 2;
				picture.Left = 2;
				picture.Width = bmp.Width;
				picture.Height = bmp.Height;

				var lbl = new Label();
				lbl.Text = Path.GetFileNameWithoutExtension(paths[i]).Split('_')[0];
				lbl.Height = 16;
				lbl.Top = curTop + picture.Height + lbl.Height;
				lbl.Left = 8;

                var panel = new Panel();
                panel.Name = paths[i];
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
			var dir = System.AppDomain.CurrentDomain.BaseDirectory + "icon\\";
			var files = Directory.GetFiles(dir);

			foreach (var filePath in files) {
				var tsb = new ToolStripButton();
				tsb.Image = new Bitmap(filePath);
				tsb.Name = Path.GetFileNameWithoutExtension(filePath);
				tsb.Click += new EventHandler((object sender, EventArgs e) => {
					for (var j=0; j < tsParts.Items.Count; ++j) {
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
			mBeginPos = picBoard.PointToClient(System.Windows.Forms.Cursor.Position);
			mBeginPos.X = ((mBeginPos.X + mCurGridWidth / 2) / mCurGridWidth) * mCurGridWidth;
			mBeginPos.Y = ((mBeginPos.Y + mCurGridWidth / 2) / mCurGridWidth) * mCurGridWidth;
		}

		private void setEndPos() {
			mEndPos = picBoard.PointToClient(System.Windows.Forms.Cursor.Position);

			if (0 < mSelectedPartsPos.X || 0 < mSelectedPartsPos.Y) {
				switch (mCurRotate) {
				case RotateFlipType.RotateNoneFlipXY:
					mEndPos.X = (mEndPos.X / mCurGridWidth) * mCurGridWidth + mSelectedPartsPos.X;
					mEndPos.Y = (mEndPos.Y / mCurGridWidth) * mCurGridWidth + mSelectedPartsPos.Y;
					break;
				case RotateFlipType.Rotate90FlipXY:
					mEndPos.X = (mEndPos.X / mCurGridWidth) * mCurGridWidth + mSelectedPartsPos.Y;
					mEndPos.Y = (mEndPos.Y / mCurGridWidth) * mCurGridWidth + mSelectedPartsPos.X;
					break;
				case RotateFlipType.Rotate180FlipXY:
					mEndPos.X = (mEndPos.X / mCurGridWidth) * mCurGridWidth + mSelectedPartsPos.X;
					mEndPos.Y = (mEndPos.Y / mCurGridWidth) * mCurGridWidth + mSelectedPartsPos.Y;
					break;
				case RotateFlipType.Rotate270FlipXY:
					mEndPos.X = (mEndPos.X / mCurGridWidth) * mCurGridWidth + mSelectedPartsPos.Y;
					mEndPos.Y = (mEndPos.Y / mCurGridWidth) * mCurGridWidth + mSelectedPartsPos.X;
					break;
				}
			}
			else {
				mEndPos.X = ((mEndPos.X + mCurGridWidth / 2) / mCurGridWidth) * mCurGridWidth;
				mEndPos.Y = ((mEndPos.Y + mCurGridWidth / 2) / mCurGridWidth) * mCurGridWidth;
			}
		}

		private void saveFile(string filePath) {
			var fs = new FileStream(filePath, FileMode.Create);
			var sw = new StreamWriter(fs);

			foreach (var rec in mList.Values) {
				switch (rec.Type) {
				case RecordType.LAND:
					sw.WriteLine(
						"{0}\t{1}\t{2}",
						rec.Type,
						rec.Begin.X,
						rec.Begin.Y
					);
					break;
				case RecordType.PARTS:
					sw.WriteLine(
						"{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
						rec.Type,
						(int)rec.Rotate,
						rec.Begin.X,
						rec.Begin.Y,
						rec.Offset.X,
						rec.Offset.Y,
						rec.Parts.Replace(System.AppDomain.CurrentDomain.BaseDirectory, "")
					);
					break;
				case RecordType.LINE:
					sw.WriteLine(
						"{0}\t{1}\t{2}\t{3}\t{4}\t{5}",
						rec.Type,
						(int)rec.Color,
						rec.Begin.X,
						rec.Begin.Y,
						rec.End.X,
						rec.End.Y
					);
					break;
				}
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
            }
            else if (1.0 <= r) {
                return record.End;
            }
            else {
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
				if (RecordType.LINE == d.Type) {
					if (isOnLine(d, mEndPos) || isOnLine(d, mRect)) {
						g.DrawLine(HoverColor, d.Begin, d.End);
					}
					else {
						g.DrawLine(d.LineColor, d.Begin, d.End);
					}
				}
			}
			foreach (var d in mList.Values) {
				if (RecordType.LAND == d.Type) {
					g.FillEllipse(
						LandColor.Brush,
						d.Begin.X - 4, d.Begin.Y - 4,
						8, 8
					);
					g.FillEllipse(
						Brushes.White,
						d.Begin.X - 2, d.Begin.Y - 2,
						4, 4
					);
				}
			}
			foreach (var d in mList.Values) {
				if (RecordType.PARTS == d.Type) {
					var filePath = d.Parts.Replace(System.AppDomain.CurrentDomain.BaseDirectory + "icon", "");
					if (tsbReverse.Checked || isOnLine(d, mRect)) {
						filePath = System.AppDomain.CurrentDomain.BaseDirectory + "icon\\alpha" + filePath;
					}
					else {
						filePath = System.AppDomain.CurrentDomain.BaseDirectory + "icon" + filePath;
					}

					var temp = new Bitmap(filePath);
					temp.RotateFlip(d.Rotate);
					g.DrawImage(temp, new Point(
						d.Begin.X - d.Offset.X,
						d.Begin.Y - d.Offset.Y
					));
				}
			}
		}

		private void drawClipBoard(Graphics g) {
			foreach (var d in mClipBoard.Values) {
				if (RecordType.LINE == d.Type) {
					var b = new Point(d.Begin.X + mEndPos.X, d.Begin.Y + mEndPos.Y);
					var e = new Point(d.End.X + mEndPos.X, d.End.Y + mEndPos.Y);
					g.DrawLine(HoverColor, b, e);
				}
			}
			foreach (var d in mClipBoard.Values) {
				if (RecordType.LAND == d.Type) {
					var b = new Point(d.Begin.X + mEndPos.X, d.Begin.Y + mEndPos.Y);
					g.FillEllipse(
						LandColor.Brush,
						b.X - 4, b.Y - 4,
						8, 8
					);
					g.FillEllipse(
						Brushes.White,
						b.X - 2, b.Y - 2,
						4, 4
					);
				}
			}
			foreach (var d in mClipBoard.Values) {
				if (RecordType.PARTS == d.Type) {
					var filePath = d.Parts.Replace(System.AppDomain.CurrentDomain.BaseDirectory + "icon", "");
					filePath = System.AppDomain.CurrentDomain.BaseDirectory + "icon\\alpha" + filePath;
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
			switch (mCurType) {
			case RecordType.CURSOL:
				if (mIsBoardDrag) {
					var x = mBeginPos.X < mEndPos.X ? mBeginPos.X : mEndPos.X;
					var y = mBeginPos.Y < mEndPos.Y ? mBeginPos.Y : mEndPos.Y;
					g.DrawRectangle(
						HoverColor,
						x, y,
						(int)Math.Abs(mEndPos.X - mBeginPos.X),
						(int)Math.Abs(mEndPos.Y - mBeginPos.Y)
					);
				}
				else {
					var x = mRect.A.X < mRect.B.X ? mRect.A.X : mRect.B.X;
					var y = mRect.A.Y < mRect.B.Y ? mRect.A.Y : mRect.B.Y;
					g.DrawRectangle(
						HoverColor,
						x, y,
						(int)Math.Abs(mRect.B.X - mRect.A.X),
						(int)Math.Abs(mRect.B.Y - mRect.A.Y)
					);
				}
				break;

			case RecordType.LINE:
				if (mIsBoardDrag) {
					g.DrawLine(DragColor, mBeginPos, mEndPos);
				}
				break;

			case RecordType.LAND:
				g.FillEllipse(
					LandColor.Brush,
					mEndPos.X - 4, mEndPos.Y - 4,
					8, 8
				);
				g.FillEllipse(
					Brushes.White,
					mEndPos.X - 2, mEndPos.Y - 2,
					4, 4
				);
				break;

			case RecordType.PARTS:
				var filePath = mSelectedPartsPath.Replace(System.AppDomain.CurrentDomain.BaseDirectory + "icon", "");
				filePath = System.AppDomain.CurrentDomain.BaseDirectory + "icon\\alpha" + filePath;

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