using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace UCBeditor {
	public partial class Form1 : Form {
        readonly Pen GridMajorColor = new Pen(Color.FromArgb(95, 95, 95), 0.5f);
        readonly Pen GridMinorColor = new Pen(Color.FromArgb(211, 211, 211), 0.5f);
        readonly Pen DragColor = Pens.Blue;
		readonly string ElementPath = AppDomain.CurrentDomain.BaseDirectory + "element\\";

		enum EditMode {
			INVALID,
			SELECT,
			WIRE,
			TIN,
			PARTS,
			LAND
		}

		class PartsInfo {
			public string Group;
			public string Name;
			public bool IsSMD;
			public bool Enable;
			public double Height;
			public Point Offset;
			public int Size;
			public List<Point> Terminals = new List<Point>();
		}

		Dictionary<string, Dictionary<string, PartsInfo>> mPartsList = new Dictionary<string, Dictionary<string, PartsInfo>>();
        List<Item> mList = new List<Item>();
        List<Item> mClipBoard = new List<Item>();
		EditMode mEditMode = EditMode.WIRE;
        Item.EWire mWireColor = Item.EWire.BLACK;
		RotateFlipType mCurRotate = RotateFlipType.RotateNoneFlipNone;
		const int BaseGridWidth = 16;
        int mCurGridWidth = BaseGridWidth;

		bool mIsDrag;
		Point mMousePos = new Point();
		Point mBeginPos = new Point();
		Point mEndPos = new Point();
		Rect mRect = new Rect();

        PartsInfo mSelectedParts;

		public Form1() {
			InitializeComponent();

			panelResize();
			picBoard.Width = mCurGridWidth * 80;
			picBoard.Height = mCurGridWidth * 80;

			loadPartsXML(ElementPath + "elements.xml");
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
				var rec = new Item(sr.ReadLine());
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
            mIsDrag = false;
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
				if (rec.isSelected(mRect)) {
					mClipBoard.Add(rec);
					int size;
					if (rec.Type == Item.EType.PARTS) {
						var item = mPartsList[rec.PartsGroup][rec.PartsName];
						size = item.Size;
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
                if (rec.isSelected(mRect)) {
					mClipBoard.Add(rec);
					int size;
					if (rec.Type == Item.EType.PARTS) {
                        var item = mPartsList[rec.PartsGroup][rec.PartsName];
						size = item.Size;
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
			var temp = new List<Item>();
			for (var d = mList.Count - 1; 0 <= d; --d) {
				if (!mList[d].isSelected(mRect)) {
					temp.Add(mList[d]);
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
        }

        private void tsbBack_Click(object sender, EventArgs e) {
            tsbBack.Checked = true;
            tsbFront.Checked = false;
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
                case EditMode.TIN:
                    mRect = new Rect();
                    mIsDrag = true;
                    break;
                }
            }

            if (e.Button == MouseButtons.Right) {
                var temp = new List<Item>();
                for (var d = 0; d < mList.Count; ++d) {
                    if (!mList[d].isSelected(mMousePos)) {
                        temp.Add(mList[d]);
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

            switch (mEditMode) {
            case EditMode.SELECT:
                mRect = new Rect();
                mRect.A = mBeginPos;
                mRect.B = mEndPos;
                mIsDrag = false;
                break;
            case EditMode.WIRE:
                if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
                    addItem(new Item(mBeginPos, mEndPos, mWireColor));
                }
                mIsDrag = false;
                break;
            case EditMode.TIN:
                if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
                    addItem(new Item(mBeginPos, mEndPos));
                }
                mIsDrag = false;
                break;
            case EditMode.LAND:
                addItem(new Item(mEndPos));
                break;
            case EditMode.PARTS: {
                var rec = new Item(
                    mEndPos, mCurRotate,
                    mSelectedParts.Group,
                    mSelectedParts.Name
                );
                if (mPartsList.ContainsKey(mSelectedParts.Group) &&
                    mPartsList[mSelectedParts.Group].ContainsKey(mSelectedParts.Name)) {
                    var item = mPartsList[mSelectedParts.Group][mSelectedParts.Name];
                    rec.Height = item.Height;
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

			g.FillRectangle(Pens.White.Brush, 0, 0, bmp.Width, bmp.Height);

			for (var y = 0; y < bmp.Height; y += mCurGridWidth) {
				for (var x = 0; x < bmp.Width; x += mCurGridWidth) {
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
				mWireColor = Item.EWire.BLACK;
			}
			if (tsbWireRed.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Item.EWire.RED;
			}
			if (tsbWireBlue.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Item.EWire.BLUE;
			}
			if (tsbWireGreen.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Item.EWire.GREEN;
			}
			if (tsbWireYellow.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Item.EWire.YELLOW;
			}

			selectParts(new PartsInfo());
		}

		void selectParts(PartsInfo parts) {
			mSelectedParts = parts;
            mRect = new Rect();
			foreach (var ctrl in pnlParts.Controls) {
				if (!(ctrl is Panel)) {
					continue;
				}
				var panel = (Panel)ctrl;
				if (panel.Name == mSelectedParts.Name) {
					panel.BackColor = SystemColors.ButtonShadow;
					panel.BorderStyle = BorderStyle.FixedSingle;
					mIsDrag = false;
					mEditMode = EditMode.PARTS;
				} else {
					panel.BackColor = SystemColors.ButtonFace;
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

		void addItem(Item newItem) {
            var tmp = new List<Item>();
            var idx = 0;
            for (; idx < mList.Count; idx++) {
                var p = mList[idx];
                double listHeight;
                if (p.Type == Item.EType.PARTS &&
                    mPartsList.ContainsKey(p.PartsGroup) &&
                    mPartsList[p.PartsGroup].ContainsKey(p.PartsName)) {
                    var item = mPartsList[p.PartsGroup][p.PartsName];
                    listHeight = item.IsSMD ? -p.Height : p.Height;
                } else {
                    listHeight = p.Height;
                }
                if (newItem.Height < listHeight) {
                    break;
                }
                tmp.Add(p);
            }
            tmp.Add(newItem);
            for (; idx < mList.Count; idx++) {
                tmp.Add(mList[idx]);
            }
            mList.Clear();
            mList.AddRange(tmp);
        }

		void drawList(Graphics g) {
			foreach (var d in mList) {
				if (Item.EType.PARTS == d.Type) {
					if (tsbNothing.Checked) {
						continue;
					}
					if (!mPartsList.ContainsKey(d.PartsGroup) || !mPartsList[d.PartsGroup].ContainsKey(d.PartsName)) {
						continue;
					}
					var item = mPartsList[d.PartsGroup][d.PartsName];
					var filePath = d.PartsGroup + "\\" + d.PartsName + ".png";
					var selected = d.isSelected(mMousePos) || d.isSelected(mRect);
                    if (tsbTransparent.Checked || (tsbBack.Checked ^ item.IsSMD) || selected) {
						filePath = ElementPath + "alpha\\" + filePath;
					} else {
						filePath = ElementPath + "solid\\" + filePath;
					}
					var temp = new Bitmap(filePath);
					temp.RotateFlip(d.Rotate);
					g.DrawImage(temp, new Point(d.Begin.X - item.Size, d.Begin.Y - item.Size));
					if (selected) {
						g.DrawArc(Pens.Red, d.Begin.X - 3, d.Begin.Y - 3, 6, 6, 0, 360);
					}
				} else {
                    d.Draw(g, tsbBack.Checked, d.isSelected(mMousePos) || d.isSelected(mRect));
                }
			}
		}

		void drawClipBoard(Graphics g) {
            foreach (var d in mClipBoard) {
                if (Item.EType.PARTS == d.Type) {
                    continue;
                }
				d.Draw(g, mEndPos.X, mEndPos.Y, false, true);
            }
			foreach (var d in mClipBoard) {
				if (Item.EType.PARTS != d.Type) {
					continue;
				}
				var filePath = ElementPath + "alpha\\" + d.PartsGroup + "\\" + d.PartsName + ".png";
				var b = new Point(d.Begin.X + mEndPos.X, d.Begin.Y + mEndPos.Y);
				var temp = new Bitmap(filePath);
				temp.RotateFlip(d.Rotate);
				var item = mPartsList[d.PartsGroup][d.PartsName];
				g.DrawImage(temp, new Point(b.X - item.Size, b.Y - item.Size));
			}
		}

		void drawCur(Graphics g) {
			switch (mEditMode) {
			case EditMode.SELECT:
				if (mIsDrag) {
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
				var filePath = ElementPath + "alpha\\"
					+ mSelectedParts.Group + "\\"
					+ mSelectedParts.Name + ".png";
				var temp = new Bitmap(filePath);
				temp.RotateFlip(mCurRotate);
				g.DrawImage(temp, new Point(
					mEndPos.X - mSelectedParts.Size,
					mEndPos.Y - mSelectedParts.Size
                ));
				break;
			}
		}

		void loadPartsXML(string xmlPath) {
			mPartsList.Clear();
			if (!File.Exists(xmlPath)) {
				return;
			}
			var xml = XmlReader.Create(xmlPath);
			var currentGroup = "";
			var currentParts = new PartsInfo();
			while (xml.Read()) {
				switch (xml.NodeType) {
				case XmlNodeType.Element:
					switch (xml.Name) {
					case "group":
						currentGroup = xml.GetAttribute("name").ToUpper();
						break;
					case "item":
						currentParts = new PartsInfo();
						currentParts.Group = currentGroup;
						currentParts.Name = xml.GetAttribute("name");
						currentParts.IsSMD = xml.GetAttribute("type") == "smd";
						currentParts.Height = double.Parse(xml.GetAttribute("height"));
						break;
					case "offset":
						currentParts.Offset = new Point(
							int.Parse(xml.GetAttribute("x")),
							int.Parse(xml.GetAttribute("y"))
						);
						break;
					case "terminal":
						currentParts.Terminals.Add(new Point(
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
					case "item":
						if (!mPartsList.ContainsKey(currentGroup)) {
							mPartsList.Add(currentGroup, new Dictionary<string, PartsInfo>());
						}
						mPartsList[currentGroup].Add(currentParts.Name, currentParts);
						break;
					}
					break;
				default:
					break;
				}
			}
		}

		void setPartsList() {
			foreach (var group in mPartsList) {
				var groupIcon = ElementPath + "group\\" + group.Key + ".png";
				if (!File.Exists(groupIcon)) {
					continue;
				}
				var solidDir = ElementPath + "solid\\" + group.Key;
				var alphaDir = ElementPath + "alpha\\" + group.Key;
				foreach (var item in group.Value) {
					var solidPath = solidDir + "\\" + item.Key + ".png";
					if (!File.Exists(solidPath)) {
						continue;
					}
					var alphaPath = alphaDir + "\\" + item.Key + ".png";
					if (!File.Exists(alphaPath)) {
						continue;
					}
					var solid = new Bitmap(solidPath);
					var alpha = new Bitmap(alphaPath);
					if (solid.Width != alpha.Width || solid.Height != alpha.Height) {
						continue;
					}
					if (solid.Width != solid.Height) {
						continue;
					}
					item.Value.Enable = true;
					item.Value.Size = solid.Width / 2;
				}
			}
			foreach (var group in mPartsList) {
				var tsb = new ToolStripButton();
				tsb.Name = group.Key;
				tsb.Image = new Bitmap(ElementPath + "group\\" + group.Key + ".png");
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
					foreach (var parts in group.Value.Values) {
						if (!parts.Enable) {
							continue;
						}

						var label = new Label();
						label.Text = parts.Name;
						label.TextAlign = ContentAlignment.BottomLeft;
						label.Height = 16;
						label.Top = currentY;
						label.Left = 8;
						pnlParts.Controls.Add(label);

						var bmp = new Bitmap(ElementPath + "solid\\" + parts.Group + "\\" + parts.Name + ".png");
						var picture = new PictureBox();
						picture.Image = bmp;
						picture.Top = 2;
						picture.Left = 2;
						picture.Width = bmp.Width;
						picture.Height = bmp.Height;
						picture.MouseDown += new MouseEventHandler((s, ev) => {
							selectParts(parts);
						});

						var panel = new Panel();
						panel.Name = parts.Name;
						panel.Controls.Add(picture);
						panel.BackColor = SystemColors.ButtonFace;
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