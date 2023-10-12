using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace UCBeditor {
	public partial class Form1 : Form {
        readonly Pen BoardColor = new Pen(Color.FromArgb(239, 255, 239), 0.5f);
        readonly Pen GridMajorColor = new Pen(Color.FromArgb(95, 95, 95), 0.5f);
        readonly Pen GridMinorColor = new Pen(Color.FromArgb(211, 211, 211), 0.5f);
        readonly Pen DragColor = Pens.Blue;

		enum EditMode {
			INVALID,
			SELECT,
			WIRE,
			TIN,
			PARTS,
			LAND
		}

        List<Item> mList = new List<Item>();
        List<Item> mClipBoard = new List<Item>();
		bool mItemHeightDesc = false;
		EditMode mEditMode = EditMode.WIRE;
        Wire.Colors mWireColor = Wire.Colors.BLACK;
		RotateFlipType mCurRotate = RotateFlipType.RotateNoneFlipNone;
		const int BaseGridWidth = 16;
        int mCurGridWidth = BaseGridWidth;

		bool mIsDragItem;
        bool mIsDragPost;
        Point mMousePos = new Point();
		Point mBeginPos = new Point();
		Point mEndPos = new Point();
		Rect mRect = new Rect();

        Package mSelectedParts;

		public Form1() {
			InitializeComponent();

			panelResize();
			picBoard.Width = mCurGridWidth * 80;
			picBoard.Height = mCurGridWidth * 80;

            Package.LoadXML(AppDomain.CurrentDomain.BaseDirectory, "packages.xml");
			setPackageList();
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

		#region MenuberEvent [File]
		private void 新規作成NToolStripMenuItem_Click(object sender, EventArgs e) {
			mList.Clear();
			mClipBoard.Clear();
			Text = "";

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

            tsbCursor.Checked = true;
			selectLine(tsbCursor);

            var fs = new FileStream(filePath, FileMode.Open);
			var sr = new StreamReader(fs);

			mList.Clear();
			while (!sr.EndOfStream) {
				var rec = Item.Construct(sr.ReadLine());
				addItem(rec);
			}
			sr.Close();
			fs.Close();
			sr.Dispose();
			fs.Dispose();

            Text = filePath;
            mBeginPos = new Point();
            mEndPos = new Point();
            mRect = new Rect();
            mIsDragItem = false;
            mIsDragPost = false;
        }

        private void 上書き保存SToolStripMenuItem_Click(object sender, EventArgs e) {
			var filePath = "";
			if (string.IsNullOrEmpty(Text) || !File.Exists(Text)) {
				saveFileDialog1.Filter = "UCBeditorファイル(*.ucb)|*.ucb";
				saveFileDialog1.FileName = "";
				saveFileDialog1.ShowDialog();
				filePath = saveFileDialog1.FileName;
				if (string.IsNullOrEmpty(filePath) || !Directory.Exists(Path.GetDirectoryName(filePath))) {
					return;
				}
                Text = filePath;
			} else {
				filePath = Text;
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
            Text = filePath;
        }
        #endregion

        #region MenuberEvent [Edit]
        private void 選択SToolStripMenuItem_Click(object sender, EventArgs e) {
			tsbCursor.Checked = true;
			selectLine(tsbCursor);
			mClipBoard.Clear();
		}

		private void 切り取りTToolStripMenuItem_Click(object sender, EventArgs e) {
			var temp = new List<Item>();
			var min = new Point(int.MaxValue, int.MaxValue);
			for (var d = 0; d < mList.Count; ++d) {
				var rec = mList[d];
				if (rec.IsSelected(mRect)) {
					mClipBoard.Add(rec);
					int size;
					if (rec is Parts) {
						size = ((Parts)rec).Size;
					} else {
						size = 0;
					}
                    Point begin = new Point(rec.Begin.X - size, rec.Begin.Y - size);
                    Point end = new Point(rec.End.X - size, rec.End.Y - size);
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
					temp.Add(rec);
				}
			}

			for (var i = 0; i < mClipBoard.Count; ++i) {
				var p = mClipBoard[i];
				p.Begin.X -= (min.X / mCurGridWidth) * mCurGridWidth;
				p.Begin.Y -= (min.Y / mCurGridWidth) * mCurGridWidth;
				p.End.X -= (min.X / mCurGridWidth) * mCurGridWidth;
				p.End.Y -= (min.Y / mCurGridWidth) * mCurGridWidth;
				mClipBoard[i] = p;
			}

			mList = temp;
			mRect = new Rect();
		}

		private void コピーCToolStripMenuItem_Click(object sender, EventArgs e) {
			var min = new Point(int.MaxValue, int.MaxValue);

			for (var d = 0; d < mList.Count; ++d) {
                var rec = mList[d];
                if (rec.IsSelected(mRect)) {
					mClipBoard.Add(rec);
					int size;
					if (rec is Parts) {
						size = ((Parts)rec).Size;
                    } else {
						size = 0;
					}
                    var begin = new Point(rec.Begin.X - size, rec.Begin.Y - size);
					var end = new Point(rec.End.X - size, rec.End.Y - size);
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
				p.Begin.X -= (min.X / mCurGridWidth) * mCurGridWidth;
				p.Begin.Y -= (min.Y / mCurGridWidth) * mCurGridWidth;
				p.End.X -= (min.X / mCurGridWidth) * mCurGridWidth;
				p.End.Y -= (min.Y / mCurGridWidth) * mCurGridWidth;
				mClipBoard[i] = p;
			}

			mRect = new Rect();
		}

		private void 貼り付けPToolStripMenuItem_Click(object sender, EventArgs e) {
			foreach (var p in mClipBoard) {
				var rec = p;
				rec.Begin.X += mEndPos.X;
				rec.Begin.Y += mEndPos.Y;
				rec.End.X += mEndPos.X;
				rec.End.Y += mEndPos.Y;
				addItem(rec);
			}
		}

		private void 削除DToolStripMenuItem_Click(object sender, EventArgs e) {
            deleteItems();
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
			setEndPos();
		}
        #endregion

        #region ToolStripButton [Mode]
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

        private void tsbTin_Click(object sender, EventArgs e) {
            selectLine(tsbTin);
        }
        #endregion

        #region ToolStripButton [Display]
        private void tsbSolid_Click(object sender, EventArgs e) {
            tsbSolid.Checked = true;
            tsbTransparent.Checked = false;
            tsbNothing.Checked = false;
        }

        private void tsbTransparent_Click(object sender, EventArgs e) {
            tsbSolid.Checked = false;
            tsbTransparent.Checked = true;
            tsbNothing.Checked = false;
        }

        private void tsbNothing_Click(object sender, EventArgs e) {
            tsbSolid.Checked = false;
            tsbTransparent.Checked = false;
            tsbNothing.Checked = true;
        }

        private void tsbFront_Click(object sender, EventArgs e) {
            tsbFront.Checked = true;
            tsbBack.Checked = false;
			mItemHeightDesc = false;
			sortItem();
        }

        private void tsbBack_Click(object sender, EventArgs e) {
            tsbBack.Checked = true;
            tsbFront.Checked = false;
            mItemHeightDesc = true;
            sortItem();
        }

        private void tscGridWidth_SelectedIndexChanged(object sender, EventArgs e) {
            switch (tscGridWidth.SelectedIndex) {
            case 0:
                mCurGridWidth = BaseGridWidth;
                break;
            case 1:
                mCurGridWidth = BaseGridWidth / 2;
                break;
            case 2:
                mCurGridWidth = BaseGridWidth / 4;
                break;
            }
        }
        #endregion

        #region MouseEvent
        private void picBoard_MouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left) {
                mMousePos = picBoard.PointToClient(Cursor.Position);
                mBeginPos.X = (int)((double)mMousePos.X / mCurGridWidth + 0.5) * mCurGridWidth;
                mBeginPos.Y = (int)((double)mMousePos.Y / mCurGridWidth + 0.5) * mCurGridWidth;

				switch (mEditMode) {
				case EditMode.SELECT:
				case EditMode.WIRE:
				case EditMode.TIN: {
                    double mostNear = double.MaxValue;
                    Item mostNearItem;
                    foreach (var item in mList) {
						var dist = item.Distance(mMousePos);
						if (dist < mostNear) {
							mostNear = dist;
							mostNearItem = item;
						}
					}
					mRect = new Rect();
					mIsDragItem = true;
					break;
				}
				}
            }

			if (e.Button == MouseButtons.Right) {
				deleteItems();
			}
        }

        private void picBoard_MouseMove(object sender, MouseEventArgs e) {
            setEndPos();
        }

        private void picBoard_MouseUp(object sender, MouseEventArgs e) {
            if (e.Button != MouseButtons.Left) {
                return;
            }

            mIsDragItem = false;
            mIsDragPost = false;

            switch (mEditMode) {
            case EditMode.SELECT:
                mRect = new Rect();
                mRect.A = mBeginPos;
                mRect.B = mEndPos;
                break;
            case EditMode.WIRE:
                if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
                    addItem(new Wire(mBeginPos, mEndPos, mWireColor));
                }
                break;
            case EditMode.TIN:
                if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
                    addItem(new Tin(mBeginPos, mEndPos));
                }
                break;
            case EditMode.LAND:
                addItem(new Land(mEndPos));
                break;
            case EditMode.PARTS: {
				var rec = new Parts(
					mEndPos, mCurRotate,
					mSelectedParts.Group,
					mSelectedParts.Name
				);
				if (Package.Find(mSelectedParts.Group, mSelectedParts.Name)) {
					var p = Package.Get(mSelectedParts.Group, mSelectedParts.Name);
					rec.Height = p.IsSMD ? -p.Height : p.Height;
				}
				addItem(rec);
				break;
			}
            }

            foreach (var p in mClipBoard) {
                var rec = p;
                rec.Begin.X += mEndPos.X;
                rec.Begin.Y += mEndPos.Y;
                rec.End.X += mEndPos.X;
                rec.End.Y += mEndPos.Y;
                addItem(rec);
            }
            mClipBoard.Clear();
        }
        #endregion

        private void timer1_Tick(object sender, EventArgs e) {
			var bmp = new Bitmap(picBoard.Width, picBoard.Height);
			var g = Graphics.FromImage(bmp);
			var lineOfs = mCurGridWidth / 2;

            g.FillRectangle(BoardColor.Brush, 0, 0, bmp.Width, bmp.Height);

			for (int y = 0; y < bmp.Height; y += mCurGridWidth) {
				for (int x = 0; x < bmp.Width; x += mCurGridWidth) {
                    if (0 != x % BaseGridWidth || 0 != y % BaseGridWidth) {
						g.DrawRectangle(GridMinorColor, x, y, 0.5f, 0.5f);
					} else {
						g.DrawRectangle(GridMajorColor, x, y, 0.5f, 0.5f);
					}
				}
			}

			drawList(g);
			drawClipBoard(g);
			drawCur(g);

			g.DrawEllipse(DragColor, mEndPos.X - 3, mEndPos.Y - 3, 6, 6);

			picBoard.Image = bmp;
		}

		void selectLine(ToolStripButton btn) {
			tsbCursor.Checked = tsbCursor == btn;
			tsbLand.Checked = tsbLand == btn;
            tsbTin.Checked = tsbTin == btn;
            if (tsbCursor.Checked) {
                mEditMode = EditMode.SELECT;
            }
            if (tsbLand.Checked) {
                mEditMode = EditMode.LAND;
            }
            if (tsbTin.Checked) {
                mEditMode = EditMode.TIN;
            }

            tsbWireBlack.Checked = tsbWireBlack == btn;
			tsbWireRed.Checked = tsbWireRed == btn;
			tsbWireBlue.Checked = tsbWireBlue == btn;
			tsbWireGreen.Checked = tsbWireGreen == btn;
			tsbWireYellow.Checked = tsbWireYellow == btn;
            if (tsbWireBlack.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.BLACK;
			}
			if (tsbWireRed.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.RED;
			}
			if (tsbWireBlue.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.BLUE;
			}
			if (tsbWireGreen.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.GREEN;
			}
			if (tsbWireYellow.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.YELLOW;
			}

			selectItem(new Package());
		}

		void selectItem(Package parts) {
            mIsDragItem = false;
            mIsDragPost = false;
            mSelectedParts = parts;
            mRect = new Rect();
			foreach (var ctrl in pnlParts.Controls) {
				if (!(ctrl is Panel)) {
					continue;
				}
				var panel = (Panel)ctrl;
				if (panel.Name == mSelectedParts.Name) {
					panel.BackColor = SystemColors.ButtonHighlight;
					panel.BorderStyle = BorderStyle.FixedSingle;
                    mEditMode = EditMode.PARTS;
				} else {
					panel.BackColor = BoardColor.Color;
					panel.BorderStyle = BorderStyle.None;
				}
			}
		}

		void setEndPos() {
			mMousePos = picBoard.PointToClient(Cursor.Position);
			int ox, oy;
			switch (mCurRotate) {
			case RotateFlipType.Rotate90FlipNone:
			case RotateFlipType.Rotate270FlipNone:
				ox = mSelectedParts.Offset.X;
				oy = mSelectedParts.Offset.Y;
				break;
			default:
				ox = mSelectedParts.Offset.Y;
				oy = mSelectedParts.Offset.X;
				break;
			}
			int snap;
			switch (mEditMode) {
			case EditMode.LAND:
			case EditMode.TIN:
			case EditMode.PARTS:
				snap = 16;
				break;
			default:
				snap = mCurGridWidth;
				break;
			}
			mEndPos.X = ox + (int)((double)(mMousePos.X - ox) / snap + 0.5) * snap;
			mEndPos.Y = oy + (int)((double)(mMousePos.Y - oy) / snap + 0.5) * snap;
		}

		void saveFile(string filePath) {
			try {
				var fs = new FileStream(filePath, FileMode.Create);
				var sw = new StreamWriter(fs);
				foreach (var rec in mList) {
					rec.Write(sw);
				}
				sw.Close();
				fs.Close();
				sw.Dispose();
				fs.Dispose();
			} catch (Exception ex) {
				MessageBox.Show(ex.ToString());
			}
		}

		void deleteItems() {
            var temp = new List<Item>();
            var deleteTermList = new List<Point>();
            foreach (var item in mList) {
                if (item.IsSelected(mRect) || item.IsSelected(mMousePos)) {
                    var terms = item.GetTerminals();
                    foreach (var term in terms) {
                        if (!deleteTermList.Contains(term)) {
                            deleteTermList.Add(term);
                        }
                    }
					if (item is Land && !((Land)item).IsFoot) {
						deleteTermList.Add(item.Begin);
					}
                } else {
					if (!(item is Land)) {
						temp.Add(item);
					}
                }
            }
			foreach (var item in mList) {
				if (!(item is Land) || deleteTermList.Contains(item.Begin)) {
					continue;
				}
                temp.Add(item);
            }
            mRect = new Rect();
            mList = temp;
			sortItem();
        }

        void addItem(Item newItem) {
			mList.Add(newItem);
			var terms = newItem.GetTerminals();
			foreach (var term in terms) {
				if (0 < term.X % BaseGridWidth || 0 < term.Y % BaseGridWidth) {
					continue;
				}
				var land = new Land(term);
				land.IsFoot = true;
				mList.Add(land);
			}
			sortItem();
		}

		void sortItem() {
            if (mItemHeightDesc) {
				mList.Sort((a, b) => {
					double aHeight;
					if (a is Tin) {
						aHeight = 0;
                    } else {
						aHeight = a.Height;
					}
                    double bHeight;
                    if (b is Tin) {
                        bHeight = 0;
                    } else {
                        bHeight = b.Height;
                    }
                    var diff = bHeight - aHeight;
					return 0 == diff ? 0 : diff < 0 ? -1 : 1;
				});
			} else {
				mList.Sort((a, b) => {
					var diff = a.Height - b.Height;
					return 0 == diff ? 0 : diff < 0 ? -1 : 1;
				});
			}
		}

		void drawList(Graphics g) {
			foreach (var item in mList) {
				if (item is Parts) {
					var p = (Parts)item;
					if (tsbNothing.Checked) {
						continue;
					}
					if (!Package.Find(p.Group, p.Name)) {
						continue;
					}
					var package = Package.Get(p.Group, p.Name);
					var filePath = p.Group + "\\" + p.Name + ".png";
					var selected = p.IsSelected(mMousePos) || p.IsSelected(mRect);
					if (tsbTransparent.Checked || (tsbBack.Checked ^ package.IsSMD) || selected) {
						filePath = Package.AlphaPath + filePath;
					} else {
						filePath = Package.SolidPath + filePath;
					}
					var temp = new Bitmap(filePath);
					temp.RotateFlip(p.Rotate);
					g.DrawImage(temp, new Point(p.Begin.X - p.Size, p.Begin.Y - p.Size));
					if (selected) {
						g.DrawArc(Pens.Red, p.Begin.X - 3, p.Begin.Y - 3, 6, 6, 0, 360);
					}
				} else {
					item.Draw(g, tsbBack.Checked, item.IsSelected(mMousePos) || item.IsSelected(mRect));
				}
			}
		}

		void drawClipBoard(Graphics g) {
            foreach (var d in mClipBoard) {
                if (d is Parts) {
                    continue;
                }
				d.Draw(g, mEndPos.X, mEndPos.Y, false, true);
            }
			foreach (var d in mClipBoard) {
				if (!(d is Parts)) {
					continue;
				}
				var parts = (Parts)d;
				var filePath = Package.AlphaPath + parts.Group + "\\" + parts.Name + ".png";
				var b = new Point(parts.Begin.X + mEndPos.X, parts.Begin.Y + mEndPos.Y);
				var temp = new Bitmap(filePath);
				temp.RotateFlip(parts.Rotate);
				g.DrawImage(temp, new Point(b.X - parts.Size, b.Y - parts.Size));
			}
		}

		void drawCur(Graphics g) {
			switch (mEditMode) {
			case EditMode.SELECT:
				if (mIsDragItem) {
					var x = mBeginPos.X < mEndPos.X ? mBeginPos.X : mEndPos.X;
					var y = mBeginPos.Y < mEndPos.Y ? mBeginPos.Y : mEndPos.Y;
					g.DrawRectangle(
						Pens.Cyan,
						x, y,
						Math.Abs(mEndPos.X - mBeginPos.X),
						Math.Abs(mEndPos.Y - mBeginPos.Y)
					);
				}
				break;

			case EditMode.WIRE:
            case EditMode.TIN:
                if (mIsDragItem) {
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
				var filePath = Package.AlphaPath + mSelectedParts.Group + "\\" + mSelectedParts.Name + ".png";
				var temp = new Bitmap(filePath);
				temp.RotateFlip(mCurRotate);
				g.DrawImage(temp, new Point(
					mEndPos.X - mSelectedParts.Size,
					mEndPos.Y - mSelectedParts.Size
                ));
				break;
			}
		}

		void setPackageList() {
			pnlParts.BackColor = BoardColor.Color;
            foreach (var group in Package.List) {
				var tsb = new ToolStripButton();
				tsb.Name = group.Key;
				tsb.Image = new Bitmap(Package.GroupPath + group.Key + ".png");
				tsb.Click += new EventHandler((object sender, EventArgs e) => {
					for (var j = 0; j < tsParts.Items.Count; ++j) {
						if (tsParts.Items[j] is ToolStripButton) {
							var item = (ToolStripButton)tsParts.Items[j];
							item.Checked = false;
						}
					}
					tsb.Checked = true;
					var currentY = 0;
					pnlParts.Controls.Clear();
					foreach (var package in group.Value.Values) {
						var label = new Label();
						label.Text = package.Name;
						label.TextAlign = ContentAlignment.BottomLeft;
						label.Height = 16;
						label.Top = currentY;
						label.Left = 8;
						pnlParts.Controls.Add(label);

						var bmp = new Bitmap(Package.SolidPath + package.Group + "\\" + package.Name + ".png");
						var picture = new PictureBox();
						picture.Image = bmp;
						picture.Top = 2;
						picture.Left = 2;
                        picture.Width = bmp.Width;
						picture.Height = bmp.Height;
						picture.MouseDown += new MouseEventHandler((s, ev) => {
							selectItem(package);
						});

						var panel = new Panel();
						panel.Name = package.Name;
						panel.Controls.Add(picture);
                        panel.BackColor = Color.Transparent;
                        panel.Width = picture.Width + 6;
						panel.Height = picture.Height + 6;
						panel.Left = 8;
						panel.Top = currentY + label.Height;
						pnlParts.Controls.Add(panel);

						currentY += panel.Height + label.Height + 6;
					}
				});
				tsParts.Items.Add(tsb);
			}
		}
	}
}