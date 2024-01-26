using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace UCBeditor {
	public partial class Form1 : Form {
		readonly Pen BoardColor = new Pen(Color.FromArgb(225, 255, 225), 0.5f);
		readonly Pen BorderColor = new Pen(Color.FromArgb(235, 235, 211), 0.5f);
		readonly Pen GridColor = new Pen(Color.FromArgb(95, 95, 95), 0.5f);
		const int SNAP = Item.GridWidth / 2;

		enum EditMode {
			SELECT,
			PATTERN,
			WIRE,
			TERMINAL,
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

			var size = PDF.PAGE_SIZE.A4_H.Size;
			picBoard.Width = (int)(size.X * Item.GridScale);
			picBoard.Height = (int)(size.Y * Item.GridScale);

			Package.LoadXML(AppDomain.CurrentDomain.BaseDirectory, "packages.xml");
			SetPackageList();
			SetEditMode(tsbSelect);

			tsbPartsSolid.PerformClick();

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

		private void MenuFilePDF_Click(object sender, EventArgs e) {
			saveFileDialog1.Filter = "PDFファイル(*.pdf)|*.pdf";
			saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(Text);
			saveFileDialog1.ShowDialog();
			var filePath = saveFileDialog1.FileName;
			if (string.IsNullOrEmpty(filePath) || !Directory.Exists(Path.GetDirectoryName(filePath))) {
				return;
			}
			var page = new PDF.Page(PDF.PAGE_SIZE.L_H);
			var pdf = new PDF();
			page.Scale = 2.54 / Item.GridWidth;
			foreach (var rec in mList) {
				rec.DrawPDF(page);
			}
			pdf.AddPage(page);
			pdf.Save(filePath);
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

		private void tsbWireInvisible_Click(object sender, EventArgs e) {
			if (mEditMode == EditMode.WIRE) {
				tsbWireInvisible.Checked = false;
				Item.Wire = true;
			} else {
				tsbWireInvisible.Checked = !tsbWireInvisible.Checked;
				Item.Wire = !tsbWireInvisible.Checked;
			}
		}

		private void DispParts_Click(object sender, EventArgs e) {
			tsbPartsInvisible.Checked = false;
			tsbPartsTransparent.Checked = false;
			tsbPartsSolid.Checked = false;
			if (tsbPartsInvisible == sender) {
				tsbPartsInvisible.Checked = true;
				Parts.Display = Parts.EDisplay.INVISIBLE;
			}
			if (tsbPartsTransparent == sender) {
				tsbPartsTransparent.Checked = true;
				Parts.Display = Parts.EDisplay.TRANSPARENT;
			}
			if (tsbPartsSolid == sender) {
				tsbPartsSolid.Checked = true;
				Parts.Display = Parts.EDisplay.SOLID;
			}
			Item.Parts = tsbPartsSolid.Checked || tsbPartsTransparent.Checked;
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
			case EditMode.PATTERN:
				if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
					AddItem(new Pattern(mBeginPos, mEndPos, tsbPatternThick.Checked ? 1.8 : 0.3));
				}
				break;
			case EditMode.WIRE:
				if (mBeginPos.X != mEndPos.X || mBeginPos.Y != mEndPos.Y) {
					AddItem(new Wire(mBeginPos, mEndPos, mWireColor, Item.Reverse));
				}
				break;
			case EditMode.TERMINAL:
				AddItem(new Terminal(mEndPos));
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
			mBeginPos = new Point();
			mEndPos = new Point();
			mSelectArea = new Rectangle();
			mIsDrag = false;
			mClipBoard.Clear();

			tsbSelect.Checked = tsbSelect == button;
			tsbTerminal.Checked = tsbTerminal == button;
			tsbPattern.Checked = tsbPattern == button;
			tsbPatternThick.Checked = tsbPatternThick == button;
			
			tsbWireBlack.Checked = tsbWireBlack == button;
			tsbWireRed.Checked = tsbWireRed == button;
			tsbWireGreen.Checked = tsbWireGreen == button;
			tsbWireBlue.Checked = tsbWireBlue == button;
			tsbWireMagenta.Checked = tsbWireMagenta == button;
			tsbWireYellow.Checked = tsbWireYellow == button;

			if (tsbSelect.Checked) {
				mEditMode = EditMode.SELECT;
			}

			if (tsbTerminal.Checked) {
				mEditMode = EditMode.TERMINAL;
				Item.Pattern = false;
				Item.Wire = false;
				Item.Parts = false;
				return;
			}

			Item.Pattern = tsbPattern.Checked || tsbPatternThick.Checked;
			if (Item.Pattern) {
				mEditMode = EditMode.PATTERN;
				Item.Parts = false;
				return;
			}

			if (tsbWireBlack.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.BLACK;
			}
			if (tsbWireRed.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.RED;
			}
			if (tsbWireGreen.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.GREEN;
			}
			if (tsbWireBlue.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.BLUE;
			}
			if (tsbWireMagenta.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.MAGENTA;
			}
			if (tsbWireYellow.Checked) {
				mEditMode = EditMode.WIRE;
				mWireColor = Wire.Colors.YELLOW;
			}

			Item.Wire = mEditMode == EditMode.WIRE;
			tsbWireInvisible.Checked = !Item.Wire;
			if (Item.Wire) {
				return;
			}

			Item.Parts = tsbPartsSolid.Checked || tsbPartsTransparent.Checked;
			SetEditParts(null);
		}

		void SetEditParts(Package parts) {
			if (null == parts) {
				mSelectedParts = new Package();
			} else {
				mSelectedParts = parts;
			}
			mIsDrag = false;
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
					Item.Parts = true;
				} else {
					panel.BackColor = BoardColor.Color;
					panel.BorderStyle = BorderStyle.None;
				}
			}
			if (null != parts) {
				if (mSelectedParts.IsSMD) {
					tsbBack.PerformClick();
				} else {
					tsbFront.PerformClick();
				}
			}
		}

		void SetBeginPos() {
			switch (mEditMode) {
			case EditMode.SELECT:
			case EditMode.PATTERN:
			case EditMode.WIRE:
				mSelectArea = new Rectangle();
				mIsDrag = true;
				break;
			}
			mBeginPos.X = (int)((double)mMousePos.X / SNAP + 0.5) * SNAP;
			mBeginPos.Y = (int)((double)mMousePos.Y / SNAP + 0.5) * SNAP;
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
			mEndPos.X = ox + (int)((double)(mMousePos.X - ox) / SNAP + 0.5) * SNAP;
			mEndPos.Y = oy + (int)((double)(mMousePos.Y - oy) / SNAP + 0.5) * SNAP;
		}

		void SaveFile(string filePath) {
			try {
				var fs = new FileStream(filePath, FileMode.CreateNew);
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
			Pattern.Join(mList);
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
			gripPos.X = gripPos.X / SNAP * SNAP;
			gripPos.Y = gripPos.Y / SNAP * SNAP;
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
			var ofsX = mEndPos.X / SNAP * SNAP;
			var ofsY = mEndPos.Y / SNAP * SNAP;
			foreach (var rec in mClipBoard) {
				var item = rec.Clone();
				item.Begin.X += ofsX;
				item.Begin.Y += ofsY;
				item.End.X += ofsX;
				item.End.Y += ofsY;
				AddItem(item, false);
			}
			SortItems();
		}

		void AddItem(Item newItem, bool divPattern = true) {
			if (newItem is Pattern) {
				mList.Add(newItem);
				if (divPattern) {
					var checkList = new List<Item>();
					foreach (var item in mList) {
						checkList.Add(item);
					}
					foreach (var item in checkList) {
						Pattern.Divide(mList, item);
					}
					Pattern.Join(mList);
				}
				return;
			}
			if (newItem is Parts parts) {
				var terms = parts.GetTerminals();
				for (int i = 0; i < terms.Length; i++) {
					mList.Add(new Land(terms[i], parts, i));
				}
			}
			if (divPattern) {
				Pattern.Divide(mList, newItem);
			}
			mList.Add(newItem);
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
							SetEditMode(tsbSelect);
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

			for (int x = 0; x < bmp.Width; x += Item.GridWidth * 5) {
				g.DrawLine(BorderColor, x, 0, x, bmp.Height);
			}
			for (int y = 0; y < bmp.Height; y += Item.GridWidth) {
				if (0 == y % (Item.GridWidth * 5)) {
					g.DrawLine(BorderColor, 0, y, bmp.Width, y);
				}
				for (int x = 0; x < bmp.Width; x += Item.GridWidth) {
					g.DrawRectangle(GridColor, x, y, 0.5f, 0.5f);
				}
			}

			var nearestItem = GetNearestItem();
			foreach (var rec in mList) {
				rec.Draw(g, rec == nearestItem || rec.IsSelected(mSelectArea));
			}
			foreach (var rec in mClipBoard) {
				rec.Draw(g,
					mEndPos.X / SNAP * SNAP,
					mEndPos.Y / SNAP * SNAP,
					true
				);
			}

			DrawEditItem(g);

			g.DrawEllipse(Pens.Red, mEndPos.X - 2, mEndPos.Y - 2, 4, 4);

			var pen = new Pen(Color.Gray) {
				DashPattern = new float[] { 4, 2 }
			};
			g.DrawRectangle(pen, 0, 0,
				PDF.PAGE_SIZE.L_H.Size.X * Item.GridScale,
				PDF.PAGE_SIZE.L_H.Size.Y * Item.GridScale
			);
			g.DrawRectangle(pen, 0, 0,
				PDF.PAGE_SIZE.POST_H.Size.X * Item.GridScale,
				PDF.PAGE_SIZE.POST_H.Size.Y * Item.GridScale
			);
			g.DrawRectangle(pen, 0, 0,
				PDF.PAGE_SIZE.A5_H.Size.X * Item.GridScale,
				PDF.PAGE_SIZE.A5_H.Size.Y * Item.GridScale
			);
			g.DrawRectangle(pen, 0, 0,
				PDF.PAGE_SIZE.A4_H.Size.X * Item.GridScale,
				PDF.PAGE_SIZE.A4_H.Size.Y * Item.GridScale
			);

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
			case EditMode.PATTERN:
			case EditMode.WIRE:
				if (mIsDrag) {
					g.DrawLine(Pens.Magenta, mBeginPos, mEndPos);
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