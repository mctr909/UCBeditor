using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace UCBeditor {
	public partial class Form1 : Form {
		readonly Pen BoardColor = new Pen(Color.FromArgb(255, 255, 235), 0.5f);
		readonly Pen BorderColor = new Pen(Color.FromArgb(235, 235, 211), 0.5f);
		readonly Pen GridColor = new Pen(Color.FromArgb(95, 95, 95), 0.5f);
		const int BaseGridWidth = 16;

		enum EditMode {
			SELECT,
			TERMINAL,
			TIN,
			WIRE,
			WLAP,
			PARTS
		}

		List<Item> mList = new List<Item>();
		List<Item> mClipBoard = new List<Item>();
		EditMode mEditMode = EditMode.WIRE;
		Tin.Colors mWireColor = Tin.Colors.BLACK;
		Parts.ROTATE mRotate = Parts.ROTATE.NONE;
		Package mSelectedParts;

		bool mIsDragItem;
		Point mMousePos = new Point();
		Point mBeginPos = new Point();
		Point mEndPos = new Point();
		Rectangle mSelectArea = new Rectangle();

		public Form1() {
			InitializeComponent();

			KeyPreview = true;
			KeyUp += new KeyEventHandler((sender, args) => {
				switch (args.KeyCode) {
				case Keys.Escape:
					選択SToolStripMenuItem.PerformClick();
					break;
				}
			});

			PanelResize();
			picBoard.Width = BaseGridWidth * 80;
			picBoard.Height = BaseGridWidth * 80;

			Package.LoadXML(AppDomain.CurrentDomain.BaseDirectory, "packages.xml");
			SetPackageList();
			SetEditMode(tsbSelect);

			tsbSolid.PerformClick();

			timer1.Interval = 50;
			timer1.Enabled = true;
			timer1.Start();
		}

		#region resize
		private void splitContainer1_Panel1_Resize(object sender, EventArgs e) {
			PanelResize();
		}

		private void splitContainer1_Panel2_Resize(object sender, EventArgs e) {
			PanelResize();
		}

		private void PanelResize() {
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

			tsbSelect.Checked = true;
			SetEditMode(tsbSelect);
		}

		private void 開くOToolStripMenuItem_Click(object sender, EventArgs e) {
			openFileDialog1.Filter = "UCBeditorファイル(*.ucb)|*.ucb";
			openFileDialog1.FileName = "";
			openFileDialog1.ShowDialog();
			var filePath = openFileDialog1.FileName;
			if (!File.Exists(filePath)) {
				return;
			}

			tsbSelect.Checked = true;
			SetEditMode(tsbSelect);

			var fs = new FileStream(filePath, FileMode.Open);
			var sr = new StreamReader(fs);

			mList.Clear();
			mClipBoard.Clear();
			while (!sr.EndOfStream) {
				var rec = Item.Construct(sr.ReadLine());
				AddItem(rec);
			}
			SortItems();
			sr.Close();
			fs.Close();
			sr.Dispose();
			fs.Dispose();

			Text = filePath;
			mBeginPos = new Point();
			mEndPos = new Point();
			mSelectArea = new Rectangle();
			mIsDragItem = false;
		}

		private void 上書き保存SToolStripMenuItem_Click(object sender, EventArgs e) {
			string filePath;
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

			SaveFile(filePath);
		}

		private void 名前を付けて保存AToolStripMenuItem_Click(object sender, EventArgs e) {
			saveFileDialog1.Filter = "UCBeditorファイル(*.ucb)|*.ucb";
			saveFileDialog1.FileName = "";
			saveFileDialog1.ShowDialog();
			var filePath = saveFileDialog1.FileName;
			if (string.IsNullOrEmpty(filePath) || !Directory.Exists(Path.GetDirectoryName(filePath))) {
				return;
			}
			SaveFile(filePath);
			Text = filePath;
		}
		#endregion

		#region MenuberEvent [Edit]
		private void 選択SToolStripMenuItem_Click(object sender, EventArgs e) {
			mSelectArea = new Rectangle();
			mClipBoard.Clear();
			SetEditMode(tsbSelect);
		}

		private void 切り取りTToolStripMenuItem_Click(object sender, EventArgs e) {
			CopyItems(true);
		}

		private void コピーCToolStripMenuItem_Click(object sender, EventArgs e) {
			CopyItems();
		}

		private void 貼り付けPToolStripMenuItem_Click(object sender, EventArgs e) {
			PasteItems();
		}

		private void 削除DToolStripMenuItem_Click(object sender, EventArgs e) {
			DeleteItems();
		}

		private void 左回転LToolStripMenuItem_Click(object sender, EventArgs e) {
			switch (mRotate) {
			case Parts.ROTATE.NONE:
				mRotate = Parts.ROTATE.DEG90;
				break;
			case Parts.ROTATE.DEG90:
				mRotate = Parts.ROTATE.DEG180;
				break;
			case Parts.ROTATE.DEG180:
				mRotate = Parts.ROTATE.DEG270;
				break;
			case Parts.ROTATE.DEG270:
				mRotate = Parts.ROTATE.NONE;
				break;
			}
			SetPos();
		}

		private void 右回転RToolStripMenuItem_Click(object sender, EventArgs e) {
			switch (mRotate) {
			case Parts.ROTATE.NONE:
				mRotate = Parts.ROTATE.DEG270;
				break;
			case Parts.ROTATE.DEG270:
				mRotate = Parts.ROTATE.DEG180;
				break;
			case Parts.ROTATE.DEG180:
				mRotate = Parts.ROTATE.DEG90;
				break;
			case Parts.ROTATE.DEG90:
				mRotate = Parts.ROTATE.NONE;
				break;
			}
			SetPos();
		}
		#endregion

		#region ToolStripEvent
		private void EditMode_Click(object sender, EventArgs e) {
			SetEditMode((ToolStripButton)sender);
		}

		private void DispParts_Click(object sender, EventArgs e) {
			tsbInvisible.Checked = false;
			tsbTransparent.Checked = false;
			tsbSolid.Checked = false;
			if (tsbInvisible == sender) {
				tsbInvisible.Checked = true;
				Package.Display = Package.EDisplay.INVISIBLE;
			}
			if (tsbTransparent == sender) {
				tsbTransparent.Checked = true;
				Package.Display = Package.EDisplay.TRANSPARENT;
			}
			if (tsbSolid == sender) {
				tsbSolid.Checked = true;
				Package.Display = Package.EDisplay.SOLID;
			}
		}

		private void DispBoard_Click(object sender, EventArgs e) {
			tsbBack.Checked = false;
			tsbFront.Checked = false;
            if (tsbFront == sender) {
                tsbFront.Checked = true;
				Package.Reverse = false;
            }
            if (tsbBack == sender) {
                tsbBack.Checked = true;
                Package.Reverse = true;
            }
			if (mEditMode == EditMode.WIRE || mEditMode == EditMode.WLAP) {
				mEditMode = Package.Reverse ? EditMode.WLAP : EditMode.WIRE;
            }
            SortItems();
		}

		private void GridWidth_SelectedIndexChanged(object sender, EventArgs e) {
		}
		#endregion

		#region MouseEvent
		private void Board_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				mMousePos = picBoard.PointToClient(Cursor.Position);
				int snap;
				switch (mEditMode) {
				case EditMode.SELECT:
				case EditMode.WLAP:
					snap = BaseGridWidth / 2;
					break;
				default:
					snap = BaseGridWidth;
					break;
				}
				mBeginPos.X = (int)((double)mMousePos.X / snap + 0.5) * snap;
				mBeginPos.Y = (int)((double)mMousePos.Y / snap + 0.5) * snap;

				switch (mEditMode) {
				case EditMode.SELECT:
				case EditMode.WIRE:
				case EditMode.WLAP:
				case EditMode.TIN: {
					double mostNear = double.MaxValue;
					Item mostNearItem;
					foreach (var rec in mList) {
						var dist = rec.Distance(mMousePos);
						if (dist < mostNear) {
							mostNear = dist;
							mostNearItem = rec;
						}
					}
					mSelectArea = new Rectangle();
					mIsDragItem = true;
					break;
				}
				}
			}

			if (e.Button == MouseButtons.Right) {
				DeleteItems();
			}
		}

		private void Board_MouseUp(object sender, MouseEventArgs e) {
			if (e.Button != MouseButtons.Left) {
				return;
			}

			mIsDragItem = false;

			switch (mEditMode) {
			case EditMode.SELECT:
				mSelectArea.Location = new Point(
					Math.Min(mBeginPos.X, mEndPos.X),
					Math.Min(mBeginPos.Y, mEndPos.Y)
				);
				mSelectArea.Size = new Size(
					Math.Abs(mEndPos.X - mBeginPos.X) + 1,
					Math.Abs(mEndPos.Y - mBeginPos.Y) + 1
				);
				break;
			case EditMode.TERMINAL:
				AddItem(new Terminal(mEndPos));
				break;
			case EditMode.TIN:
				if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
					AddItem(new Tin(mBeginPos, mEndPos));
				}
				break;
			case EditMode.WIRE:
				if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
					AddItem(new Wire(mBeginPos, mEndPos, mWireColor));
				}
				break;
            case EditMode.WLAP:
                if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
                    AddItem(new Wlap(mBeginPos, mEndPos, mWireColor));
                }
                break;
            case EditMode.PARTS:
				AddItem(new Parts(
					mEndPos, mRotate,
					mSelectedParts.Group,
					mSelectedParts.Name
				));
				break;
			}

			PasteItems();
		}

		private void Board_MouseMove(object sender, MouseEventArgs e) {
			SetPos();
		}
		#endregion

		private void timer1_Tick(object sender, EventArgs e) {
			var bmp = new Bitmap(picBoard.Width, picBoard.Height);
			var g = Graphics.FromImage(bmp);

			g.FillRectangle(BoardColor.Brush, 0, 0, bmp.Width, bmp.Height);

			for (int x = 0; x < bmp.Width; x += BaseGridWidth * 5) {
				g.DrawLine(BorderColor, x, 0, x, bmp.Height);
			}
			for (int y = 0; y < bmp.Height; y += BaseGridWidth) {
				if (0 == y % (BaseGridWidth * 5)) {
					g.DrawLine(BorderColor, 0, y, bmp.Width, y);
				}
				for (int x = 0; x < bmp.Width; x += BaseGridWidth) {
					g.DrawRectangle(GridColor, x, y, 0.5f, 0.5f);
				}
			}

			foreach (var rec in mList) {
				rec.Draw(g, rec.IsSelected(mMousePos) || rec.IsSelected(mSelectArea));
			}
			foreach (var rec in mClipBoard) {
				rec.Draw(g,
					mEndPos.X / BaseGridWidth * BaseGridWidth,
					mEndPos.Y / BaseGridWidth * BaseGridWidth,
					true
				);
			}

			DrawEditItem(g);

			g.DrawEllipse(Item.HoverColor, mEndPos.X - 2, mEndPos.Y - 2, 4, 4);
			picBoard.Image = bmp;
		}

		void SetEditMode(ToolStripButton btn) {
			tsbSelect.Checked = tsbSelect == btn;
			tsbTerminal.Checked = tsbTerminal == btn;
			tsbTin.Checked = tsbTin == btn;
			if (tsbSelect.Checked) {
				mEditMode = EditMode.SELECT;
			}
			if (tsbTerminal.Checked) {
				mEditMode = EditMode.TERMINAL;
			}
			if (tsbTin.Checked) {
				mEditMode = EditMode.TIN;
			}

			tsbWireBlack.Checked = tsbWireBlack == btn;
			tsbWireRed.Checked = tsbWireRed == btn;
			tsbWireBlue.Checked = tsbWireBlue == btn;
			tsbWireGreen.Checked = tsbWireGreen == btn;
			tsbWireYellow.Checked = tsbWireYellow == btn;
			var wireType = Package.Reverse ? EditMode.WLAP : EditMode.WIRE;
			if (tsbWireBlack.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.BLACK;
			}
			if (tsbWireRed.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.RED;
			}
			if (tsbWireBlue.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.BLUE;
			}
			if (tsbWireGreen.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.GREEN;
			}
			if (tsbWireYellow.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.YELLOW;
			}

			SetEditParts(new Package());
		}

		void SetEditParts(Package parts) {
			mIsDragItem = false;
			mSelectedParts = parts;
			mSelectArea = new Rectangle();
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

		void SetPos() {
			mMousePos = picBoard.PointToClient(Cursor.Position);
			int ox, oy;
			switch (mRotate) {
			case Parts.ROTATE.DEG90:
			case Parts.ROTATE.DEG270:
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
			case EditMode.SELECT:
			case EditMode.WLAP:
				snap = BaseGridWidth / 2;
				break;
			default:
				snap = BaseGridWidth;
				break;
			}
			mEndPos.X = ox + (int)((double)(mMousePos.X - ox) / snap + 0.5) * snap;
			mEndPos.Y = oy + (int)((double)(mMousePos.Y - oy) / snap + 0.5) * snap;
		}

		void SaveFile(string filePath) {
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

		void DeleteItems() {
			var temp = new List<Item>();
			var deleteList = new List<Item>();
			foreach (var rec in mList) {
				if (rec is Land) {
					continue;
				}
				if (rec.IsSelected(mSelectArea) || rec.IsSelected(mMousePos)) {
					if (!deleteList.Contains(rec)) {
						deleteList.Add(rec);
					}
				} else {
					temp.Add(rec);
				}
			}
			foreach (var rec in mList) {
				if (rec is Land land) {
					if (deleteList.Contains(land.Parent)) {
						continue;
					}
					temp.Add(rec);
				}
			}
			mSelectArea = new Rectangle();
			mList = temp;
			SortItems();
		}

		void CopyItems(bool enableCut = false) {
			var temp = new List<Item>();
			var gripPos = new Point(int.MaxValue, int.MaxValue);
			foreach (var rec in mList) {
				if (rec.IsSelected(mSelectArea)) {
					if (rec is Land) {
						continue;
					}
					mClipBoard.Add(enableCut ? rec : rec.Clone());
					int center;
					if (rec is Parts parts) {
						center = parts.Center;
					} else {
						center = 0;
					}
					var begin = new Point(rec.Begin.X - center, rec.Begin.Y - center);
					var end = new Point(rec.End.X - center, rec.End.Y - center);
					if (begin.X < gripPos.X) {
						gripPos.X = begin.X;
					}
					if (end.X < gripPos.X) {
						gripPos.X = end.X;
					}
					if (begin.Y < gripPos.Y) {
						gripPos.Y = begin.Y;
					}
					if (end.Y < gripPos.Y) {
						gripPos.Y = end.Y;
					}
				} else if (enableCut) {
					temp.Add(rec);
				}
			}
			gripPos.X = gripPos.X / BaseGridWidth * BaseGridWidth;
			gripPos.Y = gripPos.Y / BaseGridWidth * BaseGridWidth;
			for (var i = 0; i < mClipBoard.Count; ++i) {
				var p = mClipBoard[i];
				p.Begin.X -= gripPos.X;
				p.Begin.Y -= gripPos.Y;
				p.End.X -= gripPos.X;
				p.End.Y -= gripPos.Y;
			}
			if (enableCut) {
				mList = temp;
			}
			mSelectArea = new Rectangle();
		}

		void PasteItems() {
			var ofsX = mEndPos.X / BaseGridWidth * BaseGridWidth;
			var ofsY = mEndPos.Y / BaseGridWidth * BaseGridWidth;
			foreach (var rec in mClipBoard) {
				var item = rec.Clone();
				item.Begin.X += ofsX;
				item.Begin.Y += ofsY;
				item.End.X += ofsX;
				item.End.Y += ofsY;
				AddItem(item);
			}
            SortItems();
        }

		void AddItem(Item newItem) {
			mList.Add(newItem);
			var terms = newItem.GetTerminals();
			foreach (var term in terms) {
				if (0 < term.X % BaseGridWidth || 0 < term.Y % BaseGridWidth) {
					continue;
				}
				mList.Add(new Land(term, newItem));
			}
		}

		void SortItems() {
			if (Package.Reverse) {
				mList.Sort((a, b) => {
					var aHeight = (a.GetType() == typeof(Tin)) ? 0 : a.Height;
					var bHeight = (b.GetType() == typeof(Tin)) ? 0 : b.Height;
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

		void DrawEditItem(Graphics g) {
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

			case EditMode.TERMINAL:
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
			case EditMode.TIN:
			case EditMode.WIRE:
            case EditMode.WLAP:
                if (mIsDragItem) {
					g.DrawLine(Item.HoverColor, mBeginPos, mEndPos);
				}
				break;
			case EditMode.PARTS:
				var bmp = mSelectedParts.Alpha[(int)mRotate];
				g.DrawImage(bmp, new Point(
					mEndPos.X - mSelectedParts.Center,
					mEndPos.Y - mSelectedParts.Center
				));
				break;
			}
		}

		void SetPackageList() {
			pnlParts.BackColor = BoardColor.Color;
			foreach (var group in Package.List) {
				var tsb = new ToolStripButton() {
					Name = group.Key,
					Image = new Bitmap(Package.GroupPath + group.Key + ".png")
				};
				tsb.Click += new EventHandler((object sender, EventArgs e) => {
					for (var j = 0; j < tsParts.Items.Count; ++j) {
						if (tsParts.Items[j] is ToolStripButton item) {
							item.Checked = false;
						}
					}
					tsb.Checked = true;
					var currentY = 0;
					pnlParts.Controls.Clear();
					foreach (var package in group.Value.Values) {
						var label = new Label() {
							Text = package.Name,
							TextAlign = ContentAlignment.BottomLeft,
							Height = 16,
							Top = currentY,
							Left = 2
						};
						pnlParts.Controls.Add(label);

						var picture = new PictureBox() {
							Image = package.Solid[0],
							Top = 2,
							Left = 2,
							Width = package.Solid[0].Width,
							Height = package.Solid[0].Height
						};
						picture.MouseDown += new MouseEventHandler((s, ev) => {
							SetEditParts(package);
						});

						var panel = new Panel() {
							Name = package.Name,
							BackColor = Color.Transparent,
							Width = picture.Width + 6,
							Height = picture.Height + 6,
							Left = 2,
							Top = currentY + label.Height
						};

						panel.Controls.Add(picture);
						pnlParts.Controls.Add(panel);

						currentY += panel.Height + label.Height + 6;
					}
				});
				tsParts.Items.Add(tsb);
			}
			if (1 <= tsParts.Items.Count) {
				tsParts.Items[0].PerformClick();
			}
		}
	}
}