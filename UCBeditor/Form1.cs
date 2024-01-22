using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace UCBeditor {
	public partial class Form1 : Form {
		readonly Pen BoardColor = new Pen(Color.FromArgb(235, 255, 235), 0.5f);
		readonly Pen BorderColor = new Pen(Color.FromArgb(235, 235, 211), 0.5f);
		readonly Pen GridColor = new Pen(Color.FromArgb(95, 95, 95), 0.5f);
		const int GridWidth = 16;

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
		Wire.Colors mWireColor = Wire.Colors.BLACK;
		ROTATE mRotate = ROTATE.NONE;
		Package mSelectedParts;

		bool mIsDrag;
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
					MenuEditSelect.PerformClick();
					break;
				}
			});

			Panel_Resize();
			picBoard.Width = GridWidth * 80;
			picBoard.Height = GridWidth * 80;

			Package.LoadXML(AppDomain.CurrentDomain.BaseDirectory, "packages.xml");
			SetPackageList();
			SetEditMode(tsbSelect);

			tsbSolid.PerformClick();

			drawTimer.Interval = 50;
			drawTimer.Enabled = true;
			drawTimer.Start();
		}

		private void Panel_Resize(object sender = null, EventArgs e = null) {
			pnlBoard.Width = splitContainer1.Panel1.Width - 4;
			pnlBoard.Height = splitContainer1.Panel1.Height - tsBoard.Height - 6;
			pnlParts.Width = splitContainer1.Panel2.Width - 4;
			pnlParts.Height = splitContainer1.Panel2.Height - tsParts.Height - 6;
		}

		#region MenuberEvent [File]
		private void MenuFileNew_Click(object sender, EventArgs e) {
			mList.Clear();
			SetEditMode(tsbSelect);
			Text = "";
		}

		private void MenuFileOpen_Click(object sender, EventArgs e) {
			openFileDialog1.Filter = "UCBeditorファイル(*.ucb)|*.ucb";
			openFileDialog1.FileName = "";
			openFileDialog1.ShowDialog();
			var filePath = openFileDialog1.FileName;
			if (!File.Exists(filePath)) {
				return;
			}

			SetEditMode(tsbSelect);

			var fs = new FileStream(filePath, FileMode.Open);
			var sr = new StreamReader(fs);

			mList.Clear();
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
		}

		private void MenuFileSave_Click(object sender, EventArgs e) {
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

		private void MenuFileSaveAs_Click(object sender, EventArgs e) {
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
		private void MenuEditSelect_Click(object sender, EventArgs e) {
			SetEditMode(tsbSelect);
		}

		private void MenuEditCut_Click(object sender, EventArgs e) {
			CopyItems(true);
		}

		private void MenuEditCopy_Click(object sender, EventArgs e) {
			CopyItems();
		}

		private void MenuEditPaste_Click(object sender, EventArgs e) {
			PasteItems();
		}

		private void MenuEditDelete_Click(object sender, EventArgs e) {
			DeleteItems();
		}

		private void MenuEditRotL_Click(object sender, EventArgs e) {
			switch (mRotate) {
			case ROTATE.NONE:
				mRotate = ROTATE.DEG270;
				break;
			case ROTATE.DEG270:
				mRotate = ROTATE.DEG180;
				break;
			case ROTATE.DEG180:
				mRotate = ROTATE.DEG90;
				break;
			case ROTATE.DEG90:
				mRotate = ROTATE.NONE;
				break;
			}
			SetPos();
		}

		private void MenuEditRotR_Click(object sender, EventArgs e) {
			switch (mRotate) {
			case ROTATE.NONE:
				mRotate = ROTATE.DEG90;
				break;
			case ROTATE.DEG90:
				mRotate = ROTATE.DEG180;
				break;
			case ROTATE.DEG180:
				mRotate = ROTATE.DEG270;
				break;
			case ROTATE.DEG270:
				mRotate = ROTATE.NONE;
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
				Parts.Display = Parts.EDisplay.INVISIBLE;
			}
			if (tsbTransparent == sender) {
				tsbTransparent.Checked = true;
				Parts.Display = Parts.EDisplay.TRANSPARENT;
			}
			if (tsbSolid == sender) {
				tsbSolid.Checked = true;
				Parts.Display = Parts.EDisplay.SOLID;
			}
		}

		private void DispBoard_Click(object sender, EventArgs e) {
			tsbBack.Checked = false;
			tsbFront.Checked = false;
			if (tsbFront == sender) {
				tsbFront.Checked = true;
				Item.Reverse = false;
			}
			if (tsbBack == sender) {
				tsbBack.Checked = true;
				Item.Reverse = true;
			}
			if (mEditMode == EditMode.WIRE || mEditMode == EditMode.WLAP) {
				mEditMode = Item.Reverse ? EditMode.WLAP : EditMode.WIRE;
			}
			SortItems();
		}
		#endregion

		#region MouseEvent
		private void Board_MouseDown(object sender, MouseEventArgs e) {
			if (e.Button == MouseButtons.Left) {
				SetBeginPos();
			}
			if (e.Button == MouseButtons.Right) {
				DeleteItems();
			}
		}

		private void Board_MouseUp(object sender, MouseEventArgs e) {
			if (e.Button != MouseButtons.Left) {
				return;
			}

			mIsDrag = false;

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
					AddItem(new Pattern(mBeginPos, mEndPos));
				}
				break;
			case EditMode.WIRE:
				if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
					AddItem(new Wire(mBeginPos, mEndPos, mWireColor));
				}
				break;
			case EditMode.WLAP:
				if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
					AddItem(new Wrap(mBeginPos, mEndPos, mWireColor));
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
			mMousePos = picBoard.PointToClient(Cursor.Position);
			SetPos();
		}
		#endregion

		void SetEditMode(ToolStripButton button) {
			tsbSelect.Checked = tsbSelect == button;
			tsbTerminal.Checked = tsbTerminal == button;
			tsbPattern.Checked = tsbPattern == button;
			if (tsbSelect.Checked) {
				mEditMode = EditMode.SELECT;
			}
			if (tsbTerminal.Checked) {
				mEditMode = EditMode.TERMINAL;
			}
			if (tsbPattern.Checked) {
				mEditMode = EditMode.TIN;
			}
			Pattern.Enable = tsbPattern.Checked;

			tsbWireBlack.Checked = tsbWireBlack == button;
			tsbWireRed.Checked = tsbWireRed == button;
			tsbWireGreen.Checked = tsbWireGreen == button;
			tsbWireBlue.Checked = tsbWireBlue == button;
			tsbWireMagenta.Checked = tsbWireMagenta == button;
			tsbWireYellow.Checked = tsbWireYellow == button;
			var wireType = Item.Reverse ? EditMode.WLAP : EditMode.WIRE;
			if (tsbWireBlack.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.BLACK;
			}
			if (tsbWireRed.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.RED;
			}
			if (tsbWireGreen.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.GREEN;
			}
			if (tsbWireBlue.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.BLUE;
			}
			if (tsbWireMagenta.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.MAGENTA;
			}
			if (tsbWireYellow.Checked) {
				mEditMode = wireType;
				mWireColor = Wire.Colors.YELLOW;
			}

			mBeginPos = new Point();
			mEndPos = new Point();
			mSelectArea = new Rectangle();
			mIsDrag = false;
			mClipBoard.Clear();
			SetEditParts(new Package());
		}

		void SetEditParts(Package parts) {
			mIsDrag = false;
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

		void SetBeginPos() {
			switch (mEditMode) {
			case EditMode.SELECT:
			case EditMode.WIRE:
			case EditMode.WLAP:
			case EditMode.TIN:
				mSelectArea = new Rectangle();
				mIsDrag = true;
				break;
			}
			int snap;
			switch (mEditMode) {
			case EditMode.SELECT:
			case EditMode.WLAP:
				snap = GridWidth / 2;
				break;
			default:
				snap = GridWidth;
				break;
			}
			mBeginPos.X = (int)((double)mMousePos.X / snap + 0.5) * snap;
			mBeginPos.Y = (int)((double)mMousePos.Y / snap + 0.5) * snap;
		}

		void SetPos() {
			int ox, oy;
			switch (mRotate) {
			case ROTATE.DEG90:
			case ROTATE.DEG270:
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
				snap = GridWidth / 2;
				break;
			default:
				snap = GridWidth;
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

		Item GetNearestItem() {
			var nearestDist = double.MaxValue;
			Item nearestItem = null;
			foreach (var rec in mList) {
				if (rec is Land) {
					continue;
				}
				var dist = rec.Distance(mMousePos);
				if (dist < nearestDist && rec.IsSelected(mMousePos)) {
					nearestDist = dist;
					nearestItem = rec;
				}
			}
			return nearestItem;
		}

		void DeleteItems() {
			var nearestItem = GetNearestItem();
			var deleteList = new List<Item>();
			var temp = new List<Item>();
			foreach (var rec in mList) {
				if (rec is Land) {
					continue;
				}
				if (rec == nearestItem) {
					continue;
				}
				if (rec.IsSelected(mSelectArea)) {
					deleteList.Add(rec);
				} else {
					temp.Add(rec);
				}
			}
			if (null != nearestItem) {
				deleteList.Add(nearestItem);
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
			gripPos.X = gripPos.X / GridWidth * GridWidth;
			gripPos.Y = gripPos.Y / GridWidth * GridWidth;
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
			var ofsX = mEndPos.X / GridWidth * GridWidth;
			var ofsY = mEndPos.Y / GridWidth * GridWidth;
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
				if (newItem is Parts parts) {
					if (parts.HasFoot) {
						mList.Add(new Land(term, newItem, parts.GetDispFoot(term)));
					} else {
						if (0 == term.X % GridWidth && 0 == term.Y % GridWidth) {
							mList.Add(new Land(term, newItem));
						}
					}
				}
			}
		}

		void SortItems() {
			if (Item.Reverse) {
				mList.Sort((a, b) => {
					var aHeight = (a.GetType() == typeof(Pattern)) ? 0 : a.Height;
					var bHeight = (b.GetType() == typeof(Pattern)) ? 0 : b.Height;
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

		void Draw(object sender, EventArgs e) {
			var bmp = new Bitmap(picBoard.Width, picBoard.Height);
			var g = Graphics.FromImage(bmp);

			g.FillRectangle(BoardColor.Brush, 0, 0, bmp.Width, bmp.Height);

			for (int x = 0; x < bmp.Width; x += GridWidth * 5) {
				g.DrawLine(BorderColor, x, 0, x, bmp.Height);
			}
			for (int y = 0; y < bmp.Height; y += GridWidth) {
				if (0 == y % (GridWidth * 5)) {
					g.DrawLine(BorderColor, 0, y, bmp.Width, y);
				}
				for (int x = 0; x < bmp.Width; x += GridWidth) {
					g.DrawRectangle(GridColor, x, y, 0.5f, 0.5f);
				}
			}

			var nearestItem = GetNearestItem();
			foreach (var rec in mList) {
				rec.Draw(g, rec == nearestItem || rec.IsSelected(mSelectArea));
			}
			foreach (var rec in mClipBoard) {
				rec.Draw(g,
					mEndPos.X / GridWidth * GridWidth,
					mEndPos.Y / GridWidth * GridWidth,
					true
				);
			}

			DrawEditItem(g);

			g.DrawEllipse(Pens.Red, mEndPos.X - 2, mEndPos.Y - 2, 4, 4);
			picBoard.Image = bmp;
		}

		void DrawEditItem(Graphics g) {
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
				if (mIsDrag) {
					g.DrawLine(Pens.Turquoise, mBeginPos, mEndPos);
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
	}
}