namespace UCB {
	partial class Form1 {
		/// <summary>
		/// 必要なデザイナ変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナで生成されたコード

		/// <summary>
		/// デザイナ サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディタで変更しないでください。
		/// </summary>
		private void InitializeComponent() {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.drawTimer = new System.Windows.Forms.Timer(this.components);
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.pnlBoard = new System.Windows.Forms.Panel();
			this.picBoard = new System.Windows.Forms.PictureBox();
			this.tsBoard = new System.Windows.Forms.ToolStrip();
			this.tsbSelect = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbFront = new System.Windows.Forms.ToolStripButton();
			this.tsbBack = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbPattern = new System.Windows.Forms.ToolStripButton();
			this.tsbPatternThick = new System.Windows.Forms.ToolStripButton();
			this.tsbTerminal = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbWireInvisible = new System.Windows.Forms.ToolStripButton();
			this.tssDispBoard = new System.Windows.Forms.ToolStripSeparator();
			this.tsbWireBlack = new System.Windows.Forms.ToolStripButton();
			this.tsbWireRed = new System.Windows.Forms.ToolStripButton();
			this.tsbWireGreen = new System.Windows.Forms.ToolStripButton();
			this.tsbWireBlue = new System.Windows.Forms.ToolStripButton();
			this.tsbWireMagenta = new System.Windows.Forms.ToolStripButton();
			this.tsbWireYellow = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbPartsSolid = new System.Windows.Forms.ToolStripButton();
			this.tsbPartsTransparent = new System.Windows.Forms.ToolStripButton();
			this.tsbPartsInvisible = new System.Windows.Forms.ToolStripButton();
			this.pnlParts = new System.Windows.Forms.Panel();
			this.tsParts = new System.Windows.Forms.ToolStrip();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.MenuFile = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuFileNew = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuFileOpen = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuFileSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.MenuFileSave = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuFileSaveAs = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.MenuFilePDF = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEdit = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEditSelect = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.MenuEditCut = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEditCopy = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEditPaste = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEditDelete = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEditSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.MenuEditRotL = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuEditRotR = new System.Windows.Forms.ToolStripMenuItem();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.pnlBoard.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picBoard)).BeginInit();
			this.tsBoard.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// drawTimer
			// 
			this.drawTimer.Tick += new System.EventHandler(this.Draw);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 24);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.pnlBoard);
			this.splitContainer1.Panel1.Controls.Add(this.tsBoard);
			this.splitContainer1.Panel1.Resize += new System.EventHandler(this.Panel_Resize);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.pnlParts);
			this.splitContainer1.Panel2.Controls.Add(this.tsParts);
			this.splitContainer1.Panel2.Resize += new System.EventHandler(this.Panel_Resize);
			this.splitContainer1.Size = new System.Drawing.Size(1008, 705);
			this.splitContainer1.SplitterDistance = 800;
			this.splitContainer1.TabIndex = 0;
			// 
			// pnlBoard
			// 
			this.pnlBoard.AutoScroll = true;
			this.pnlBoard.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlBoard.Controls.Add(this.picBoard);
			this.pnlBoard.Location = new System.Drawing.Point(3, 28);
			this.pnlBoard.Name = "pnlBoard";
			this.pnlBoard.Size = new System.Drawing.Size(244, 230);
			this.pnlBoard.TabIndex = 2;
			// 
			// picBoard
			// 
			this.picBoard.Location = new System.Drawing.Point(3, 3);
			this.picBoard.Name = "picBoard";
			this.picBoard.Size = new System.Drawing.Size(100, 50);
			this.picBoard.TabIndex = 1;
			this.picBoard.TabStop = false;
			this.picBoard.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Board_MouseDown);
			this.picBoard.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Board_MouseMove);
			this.picBoard.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Board_MouseUp);
			// 
			// tsBoard
			// 
			this.tsBoard.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbSelect,
            this.toolStripSeparator2,
            this.tsbBack,
            this.tsbFront,
            this.toolStripSeparator3,
            this.tsbPattern,
            this.tsbPatternThick,
            this.tsbTerminal,
            this.toolStripSeparator5,
            this.tsbWireInvisible,
            this.tssDispBoard,
            this.tsbWireBlack,
            this.tsbWireRed,
            this.tsbWireGreen,
            this.tsbWireBlue,
            this.tsbWireMagenta,
            this.tsbWireYellow,
            this.toolStripSeparator1,
            this.tsbPartsSolid,
            this.tsbPartsTransparent,
            this.tsbPartsInvisible});
			this.tsBoard.Location = new System.Drawing.Point(0, 0);
			this.tsBoard.Name = "tsBoard";
			this.tsBoard.Size = new System.Drawing.Size(800, 25);
			this.tsBoard.TabIndex = 0;
			this.tsBoard.Text = "toolStrip1";
			// 
			// tsbSelect
			// 
			this.tsbSelect.Checked = true;
			this.tsbSelect.CheckOnClick = true;
			this.tsbSelect.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbSelect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbSelect.Image = ((System.Drawing.Image)(resources.GetObject("tsbSelect.Image")));
			this.tsbSelect.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbSelect.Name = "tsbSelect";
			this.tsbSelect.Size = new System.Drawing.Size(23, 22);
			this.tsbSelect.Text = "選択";
			this.tsbSelect.Click += new System.EventHandler(this.EditMode_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbFront
			// 
			this.tsbFront.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbFront.Image = ((System.Drawing.Image)(resources.GetObject("tsbFront.Image")));
			this.tsbFront.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbFront.Name = "tsbFront";
			this.tsbFront.Size = new System.Drawing.Size(23, 22);
			this.tsbFront.Text = "正面";
			this.tsbFront.Click += new System.EventHandler(this.DispBoard_Click);
			// 
			// tsbBack
			// 
			this.tsbBack.Checked = true;
			this.tsbBack.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbBack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbBack.Image = ((System.Drawing.Image)(resources.GetObject("tsbBack.Image")));
			this.tsbBack.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbBack.Name = "tsbBack";
			this.tsbBack.Size = new System.Drawing.Size(23, 22);
			this.tsbBack.Text = "銅箔面";
			this.tsbBack.Click += new System.EventHandler(this.DispBoard_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbPattern
			// 
			this.tsbPattern.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbPattern.Image = ((System.Drawing.Image)(resources.GetObject("tsbPattern.Image")));
			this.tsbPattern.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbPattern.Name = "tsbPattern";
			this.tsbPattern.Size = new System.Drawing.Size(23, 22);
			this.tsbPattern.Text = "パターン(細)";
			this.tsbPattern.Click += new System.EventHandler(this.EditMode_Click);
			// 
			// tsbPatternThick
			// 
			this.tsbPatternThick.CheckOnClick = true;
			this.tsbPatternThick.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbPatternThick.Image = ((System.Drawing.Image)(resources.GetObject("tsbPatternThick.Image")));
			this.tsbPatternThick.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbPatternThick.Name = "tsbPatternThick";
			this.tsbPatternThick.Size = new System.Drawing.Size(23, 22);
			this.tsbPatternThick.Text = "パターン(太)";
			this.tsbPatternThick.Click += new System.EventHandler(this.EditMode_Click);
			// 
			// tsbTerminal
			// 
			this.tsbTerminal.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbTerminal.Image = ((System.Drawing.Image)(resources.GetObject("tsbTerminal.Image")));
			this.tsbTerminal.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbTerminal.Name = "tsbTerminal";
			this.tsbTerminal.Size = new System.Drawing.Size(23, 22);
			this.tsbTerminal.Text = "端子";
			this.tsbTerminal.Click += new System.EventHandler(this.EditMode_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbWireInvisible
			// 
			this.tsbWireInvisible.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireInvisible.Image = ((System.Drawing.Image)(resources.GetObject("tsbWireInvisible.Image")));
			this.tsbWireInvisible.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireInvisible.Name = "tsbWireInvisible";
			this.tsbWireInvisible.Size = new System.Drawing.Size(23, 22);
			this.tsbWireInvisible.Text = "導線(非表示)";
			this.tsbWireInvisible.Click += new System.EventHandler(this.tsbWireInvisible_Click);
			// 
			// tssDispBoard
			// 
			this.tssDispBoard.Name = "tssDispBoard";
			this.tssDispBoard.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbWireBlack
			// 
			this.tsbWireBlack.CheckOnClick = true;
			this.tsbWireBlack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireBlack.Image = ((System.Drawing.Image)(resources.GetObject("tsbWireBlack.Image")));
			this.tsbWireBlack.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireBlack.Name = "tsbWireBlack";
			this.tsbWireBlack.Size = new System.Drawing.Size(23, 22);
			this.tsbWireBlack.Text = "導線(黒)";
			this.tsbWireBlack.Click += new System.EventHandler(this.EditMode_Click);
			// 
			// tsbWireRed
			// 
			this.tsbWireRed.CheckOnClick = true;
			this.tsbWireRed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireRed.Image = ((System.Drawing.Image)(resources.GetObject("tsbWireRed.Image")));
			this.tsbWireRed.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireRed.Name = "tsbWireRed";
			this.tsbWireRed.Size = new System.Drawing.Size(23, 22);
			this.tsbWireRed.Text = "導線(赤)";
			this.tsbWireRed.Click += new System.EventHandler(this.EditMode_Click);
			// 
			// tsbWireGreen
			// 
			this.tsbWireGreen.CheckOnClick = true;
			this.tsbWireGreen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireGreen.Image = ((System.Drawing.Image)(resources.GetObject("tsbWireGreen.Image")));
			this.tsbWireGreen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireGreen.Name = "tsbWireGreen";
			this.tsbWireGreen.Size = new System.Drawing.Size(23, 22);
			this.tsbWireGreen.Text = "導線(緑)";
			this.tsbWireGreen.Click += new System.EventHandler(this.EditMode_Click);
			// 
			// tsbWireBlue
			// 
			this.tsbWireBlue.CheckOnClick = true;
			this.tsbWireBlue.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireBlue.Image = ((System.Drawing.Image)(resources.GetObject("tsbWireBlue.Image")));
			this.tsbWireBlue.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireBlue.Name = "tsbWireBlue";
			this.tsbWireBlue.Size = new System.Drawing.Size(23, 22);
			this.tsbWireBlue.Text = "導線(青)";
			this.tsbWireBlue.Click += new System.EventHandler(this.EditMode_Click);
			// 
			// tsbWireMagenta
			// 
			this.tsbWireMagenta.CheckOnClick = true;
			this.tsbWireMagenta.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireMagenta.Image = ((System.Drawing.Image)(resources.GetObject("tsbWireMagenta.Image")));
			this.tsbWireMagenta.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireMagenta.Name = "tsbWireMagenta";
			this.tsbWireMagenta.Size = new System.Drawing.Size(23, 22);
			this.tsbWireMagenta.Text = "導線(紫)";
			this.tsbWireMagenta.Click += new System.EventHandler(this.EditMode_Click);
			// 
			// tsbWireYellow
			// 
			this.tsbWireYellow.CheckOnClick = true;
			this.tsbWireYellow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireYellow.Image = ((System.Drawing.Image)(resources.GetObject("tsbWireYellow.Image")));
			this.tsbWireYellow.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireYellow.Name = "tsbWireYellow";
			this.tsbWireYellow.Size = new System.Drawing.Size(23, 22);
			this.tsbWireYellow.Text = "導線(黄)";
			this.tsbWireYellow.Click += new System.EventHandler(this.EditMode_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbPartsSolid
			// 
			this.tsbPartsSolid.Checked = true;
			this.tsbPartsSolid.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbPartsSolid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbPartsSolid.Image = ((System.Drawing.Image)(resources.GetObject("tsbPartsSolid.Image")));
			this.tsbPartsSolid.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbPartsSolid.Name = "tsbPartsSolid";
			this.tsbPartsSolid.Size = new System.Drawing.Size(23, 22);
			this.tsbPartsSolid.Text = "部品を表示";
			this.tsbPartsSolid.Click += new System.EventHandler(this.DispParts_Click);
			// 
			// tsbPartsTransparent
			// 
			this.tsbPartsTransparent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbPartsTransparent.Image = ((System.Drawing.Image)(resources.GetObject("tsbPartsTransparent.Image")));
			this.tsbPartsTransparent.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbPartsTransparent.Name = "tsbPartsTransparent";
			this.tsbPartsTransparent.Size = new System.Drawing.Size(23, 22);
			this.tsbPartsTransparent.Text = "部品を透過";
			this.tsbPartsTransparent.Click += new System.EventHandler(this.DispParts_Click);
			// 
			// tsbPartsInvisible
			// 
			this.tsbPartsInvisible.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbPartsInvisible.Image = ((System.Drawing.Image)(resources.GetObject("tsbPartsInvisible.Image")));
			this.tsbPartsInvisible.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbPartsInvisible.Name = "tsbPartsInvisible";
			this.tsbPartsInvisible.Size = new System.Drawing.Size(23, 22);
			this.tsbPartsInvisible.Text = "部品を表示しない";
			this.tsbPartsInvisible.Click += new System.EventHandler(this.DispParts_Click);
			// 
			// pnlParts
			// 
			this.pnlParts.AutoScroll = true;
			this.pnlParts.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.pnlParts.Location = new System.Drawing.Point(1, 28);
			this.pnlParts.Name = "pnlParts";
			this.pnlParts.Size = new System.Drawing.Size(244, 230);
			this.pnlParts.TabIndex = 1;
			// 
			// tsParts
			// 
			this.tsParts.Location = new System.Drawing.Point(0, 0);
			this.tsParts.Name = "tsParts";
			this.tsParts.Size = new System.Drawing.Size(204, 25);
			this.tsParts.TabIndex = 0;
			this.tsParts.Text = "toolStrip2";
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuFile,
            this.MenuEdit});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1008, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// MenuFile
			// 
			this.MenuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuFileNew,
            this.MenuFileOpen,
            this.MenuFileSeparator1,
            this.MenuFileSave,
            this.MenuFileSaveAs,
            this.toolStripSeparator4,
            this.MenuFilePDF});
			this.MenuFile.Name = "MenuFile";
			this.MenuFile.Size = new System.Drawing.Size(67, 20);
			this.MenuFile.Text = "ファイル(F)";
			// 
			// MenuFileNew
			// 
			this.MenuFileNew.Name = "MenuFileNew";
			this.MenuFileNew.Size = new System.Drawing.Size(184, 22);
			this.MenuFileNew.Text = "新規作成(N)";
			this.MenuFileNew.Click += new System.EventHandler(this.MenuFileNew_Click);
			// 
			// MenuFileOpen
			// 
			this.MenuFileOpen.Name = "MenuFileOpen";
			this.MenuFileOpen.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.MenuFileOpen.Size = new System.Drawing.Size(184, 22);
			this.MenuFileOpen.Text = "開く(O)";
			this.MenuFileOpen.Click += new System.EventHandler(this.MenuFileOpen_Click);
			// 
			// MenuFileSeparator1
			// 
			this.MenuFileSeparator1.Name = "MenuFileSeparator1";
			this.MenuFileSeparator1.Size = new System.Drawing.Size(181, 6);
			// 
			// MenuFileSave
			// 
			this.MenuFileSave.Name = "MenuFileSave";
			this.MenuFileSave.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.MenuFileSave.Size = new System.Drawing.Size(184, 22);
			this.MenuFileSave.Text = "上書き保存(S)";
			this.MenuFileSave.Click += new System.EventHandler(this.MenuFileSave_Click);
			// 
			// MenuFileSaveAs
			// 
			this.MenuFileSaveAs.Name = "MenuFileSaveAs";
			this.MenuFileSaveAs.Size = new System.Drawing.Size(184, 22);
			this.MenuFileSaveAs.Text = "名前を付けて保存(A)";
			this.MenuFileSaveAs.Click += new System.EventHandler(this.MenuFileSaveAs_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(181, 6);
			// 
			// MenuFilePDF
			// 
			this.MenuFilePDF.Name = "MenuFilePDF";
			this.MenuFilePDF.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.P)));
			this.MenuFilePDF.Size = new System.Drawing.Size(184, 22);
			this.MenuFilePDF.Text = "PDF出力";
			this.MenuFilePDF.Click += new System.EventHandler(this.MenuFilePDF_Click);
			// 
			// MenuEdit
			// 
			this.MenuEdit.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuEditSelect,
            this.toolStripSeparator6,
            this.MenuEditCut,
            this.MenuEditCopy,
            this.MenuEditPaste,
            this.MenuEditDelete,
            this.MenuEditSeparator1,
            this.MenuEditRotL,
            this.MenuEditRotR});
			this.MenuEdit.Name = "MenuEdit";
			this.MenuEdit.Size = new System.Drawing.Size(57, 20);
			this.MenuEdit.Text = "編集(E)";
			// 
			// MenuEditSelect
			// 
			this.MenuEditSelect.Name = "MenuEditSelect";
			this.MenuEditSelect.ShortcutKeyDisplayString = "Esc";
			this.MenuEditSelect.Size = new System.Drawing.Size(186, 22);
			this.MenuEditSelect.Text = "選択(S)";
			this.MenuEditSelect.Click += new System.EventHandler(this.MenuEditSelect_Click);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(183, 6);
			// 
			// MenuEditCut
			// 
			this.MenuEditCut.Name = "MenuEditCut";
			this.MenuEditCut.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.MenuEditCut.Size = new System.Drawing.Size(186, 22);
			this.MenuEditCut.Text = "切り取り(T)";
			this.MenuEditCut.Click += new System.EventHandler(this.MenuEditCut_Click);
			// 
			// MenuEditCopy
			// 
			this.MenuEditCopy.Name = "MenuEditCopy";
			this.MenuEditCopy.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.MenuEditCopy.Size = new System.Drawing.Size(186, 22);
			this.MenuEditCopy.Text = "コピー(C)";
			this.MenuEditCopy.Click += new System.EventHandler(this.MenuEditCopy_Click);
			// 
			// MenuEditPaste
			// 
			this.MenuEditPaste.Name = "MenuEditPaste";
			this.MenuEditPaste.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.MenuEditPaste.Size = new System.Drawing.Size(186, 22);
			this.MenuEditPaste.Text = "貼り付け(P)";
			this.MenuEditPaste.Click += new System.EventHandler(this.MenuEditPaste_Click);
			// 
			// MenuEditDelete
			// 
			this.MenuEditDelete.Name = "MenuEditDelete";
			this.MenuEditDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.MenuEditDelete.Size = new System.Drawing.Size(186, 22);
			this.MenuEditDelete.Text = "削除(D)";
			this.MenuEditDelete.Click += new System.EventHandler(this.MenuEditDelete_Click);
			// 
			// MenuEditSeparator1
			// 
			this.MenuEditSeparator1.Name = "MenuEditSeparator1";
			this.MenuEditSeparator1.Size = new System.Drawing.Size(183, 6);
			// 
			// MenuEditRotL
			// 
			this.MenuEditRotL.Name = "MenuEditRotL";
			this.MenuEditRotL.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left)));
			this.MenuEditRotL.Size = new System.Drawing.Size(186, 22);
			this.MenuEditRotL.Text = "左回転(L)";
			this.MenuEditRotL.Click += new System.EventHandler(this.MenuEditRotL_Click);
			// 
			// MenuEditRotR
			// 
			this.MenuEditRotR.Name = "MenuEditRotR";
			this.MenuEditRotR.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right)));
			this.MenuEditRotR.Size = new System.Drawing.Size(186, 22);
			this.MenuEditRotR.Text = "右回転(R)";
			this.MenuEditRotR.Click += new System.EventHandler(this.MenuEditRotR_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1008, 729);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.menuStrip1);
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "Form1";
			this.Text = "Form1";
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			this.splitContainer1.ResumeLayout(false);
			this.pnlBoard.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.picBoard)).EndInit();
			this.tsBoard.ResumeLayout(false);
			this.tsBoard.PerformLayout();
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Timer drawTimer;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ToolStrip tsBoard;
		private System.Windows.Forms.ToolStrip tsParts;
		private System.Windows.Forms.ToolStripButton tsbSelect;
		private System.Windows.Forms.ToolStripButton tsbWireBlack;
		private System.Windows.Forms.ToolStripButton tsbPatternThick;
		private System.Windows.Forms.ToolStripButton tsbWireRed;
		private System.Windows.Forms.ToolStripButton tsbWireGreen;
		private System.Windows.Forms.ToolStripButton tsbWireBlue;
		private System.Windows.Forms.ToolStripButton tsbWireMagenta;
		private System.Windows.Forms.ToolStripButton tsbWireYellow;
		private System.Windows.Forms.PictureBox picBoard;
		private System.Windows.Forms.Panel pnlBoard;
		private System.Windows.Forms.Panel pnlParts;
		private System.Windows.Forms.ToolStripButton tsbTerminal;
		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem MenuFile;
		private System.Windows.Forms.ToolStripMenuItem MenuFileNew;
		private System.Windows.Forms.ToolStripMenuItem MenuFileOpen;
		private System.Windows.Forms.ToolStripMenuItem MenuFileSave;
		private System.Windows.Forms.ToolStripMenuItem MenuFileSaveAs;
		private System.Windows.Forms.ToolStripSeparator MenuFileSeparator1;
		private System.Windows.Forms.ToolStripMenuItem MenuEdit;
		private System.Windows.Forms.ToolStripMenuItem MenuEditSelect;
		private System.Windows.Forms.ToolStripMenuItem MenuEditDelete;
		private System.Windows.Forms.ToolStripMenuItem MenuEditCut;
		private System.Windows.Forms.ToolStripMenuItem MenuEditCopy;
		private System.Windows.Forms.ToolStripMenuItem MenuEditPaste;
		private System.Windows.Forms.ToolStripMenuItem MenuEditRotL;
		private System.Windows.Forms.ToolStripMenuItem MenuEditRotR;
		private System.Windows.Forms.ToolStripSeparator MenuEditSeparator1;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.ToolStripButton tsbPartsSolid;
		private System.Windows.Forms.ToolStripButton tsbPartsTransparent;
		private System.Windows.Forms.ToolStripButton tsbPartsInvisible;
		private System.Windows.Forms.ToolStripButton tsbFront;
		private System.Windows.Forms.ToolStripButton tsbBack;
		private System.Windows.Forms.ToolStripSeparator tssDispBoard;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
		private System.Windows.Forms.ToolStripButton tsbPattern;
		private System.Windows.Forms.ToolStripButton tsbWireInvisible;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem MenuFilePDF;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
	}
}
