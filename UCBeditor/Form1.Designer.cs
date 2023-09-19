namespace UCBeditor
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.pnlBoard = new System.Windows.Forms.Panel();
			this.picBoard = new System.Windows.Forms.PictureBox();
			this.tsBoard = new System.Windows.Forms.ToolStrip();
			this.tsbCursor = new System.Windows.Forms.ToolStripButton();
			this.tsbLand = new System.Windows.Forms.ToolStripButton();
			this.tsbWireBlack = new System.Windows.Forms.ToolStripButton();
			this.tsbWireTin = new System.Windows.Forms.ToolStripButton();
			this.tsbWireRed = new System.Windows.Forms.ToolStripButton();
			this.tsbWireBlue = new System.Windows.Forms.ToolStripButton();
			this.tsbWireGreen = new System.Windows.Forms.ToolStripButton();
			this.tsbWireYellow = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.tsbFront = new System.Windows.Forms.ToolStripButton();
			this.tsbReverse = new System.Windows.Forms.ToolStripButton();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.tscGridWidth = new System.Windows.Forms.ToolStripComboBox();
			this.pnlParts = new System.Windows.Forms.Panel();
			this.tsParts = new System.Windows.Forms.ToolStrip();
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.ファイルFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.新規作成NToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.開くOToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.上書き保存SToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.名前を付けて保存AToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.編集EToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.左回転LToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.右回転RToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.切り取りTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.コピーCToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.貼り付けPToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.削除DToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.選択SToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.pnlBoard.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.picBoard)).BeginInit();
			this.tsBoard.SuspendLayout();
			this.menuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// timer1
			// 
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
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
			this.splitContainer1.Panel1.Resize += new System.EventHandler(this.splitContainer1_Panel1_Resize);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.pnlParts);
			this.splitContainer1.Panel2.Controls.Add(this.tsParts);
			this.splitContainer1.Panel2.Resize += new System.EventHandler(this.splitContainer1_Panel2_Resize);
			this.splitContainer1.Size = new System.Drawing.Size(1008, 705);
			this.splitContainer1.SplitterDistance = 737;
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
			this.picBoard.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picBoard_MouseMove);
			this.picBoard.MouseDown += new System.Windows.Forms.MouseEventHandler(this.picBoard_MouseDown);
			this.picBoard.MouseUp += new System.Windows.Forms.MouseEventHandler(this.picBoard_MouseUp);
			// 
			// tsBoard
			// 
			this.tsBoard.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsbCursor,
            this.tsbLand,
            this.tsbWireBlack,
            this.tsbWireRed,
            this.tsbWireBlue,
            this.tsbWireGreen,
            this.tsbWireYellow,
			this.tsbWireTin,
            this.toolStripSeparator8,
            this.toolStripSeparator4,
            this.tsbFront,
            this.tsbReverse,
            this.toolStripSeparator9,
            this.toolStripSeparator5,
            this.tscGridWidth});
			this.tsBoard.Location = new System.Drawing.Point(0, 0);
			this.tsBoard.Name = "tsBoard";
			this.tsBoard.Size = new System.Drawing.Size(737, 25);
			this.tsBoard.TabIndex = 0;
			this.tsBoard.Text = "toolStrip1";
			// 
			// tsbCursor
			// 
			this.tsbCursor.Checked = true;
			this.tsbCursor.CheckOnClick = true;
			this.tsbCursor.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbCursor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbCursor.Image = ((System.Drawing.Image)(resources.GetObject("tsbCursor.Image")));
			this.tsbCursor.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbCursor.Name = "tsbCursor";
			this.tsbCursor.Size = new System.Drawing.Size(23, 22);
			this.tsbCursor.Text = "選択";
			this.tsbCursor.Click += new System.EventHandler(this.tsbCursor_Click);
			// 
			// tsbLand
			// 
			this.tsbLand.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbLand.Image = ((System.Drawing.Image)(resources.GetObject("tsbLand.Image")));
			this.tsbLand.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbLand.Name = "tsbLand";
			this.tsbLand.Size = new System.Drawing.Size(23, 22);
			this.tsbLand.Text = "ランド";
			this.tsbLand.Click += new System.EventHandler(this.tsbLand_Click);
			// 
			// tsbWireBlack
			// 
			this.tsbWireBlack.CheckOnClick = true;
			this.tsbWireBlack.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireBlack.Image = ((System.Drawing.Image)(resources.GetObject("tsbLineBlack.Image")));
			this.tsbWireBlack.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireBlack.Name = "tsbWireBlack";
			this.tsbWireBlack.Size = new System.Drawing.Size(23, 22);
			this.tsbWireBlack.Text = "導線(黒)";
			this.tsbWireBlack.Click += new System.EventHandler(this.tsbWireBlack_Click);
			// 
			// tsbWireRed
			// 
			this.tsbWireRed.CheckOnClick = true;
			this.tsbWireRed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireRed.Image = ((System.Drawing.Image)(resources.GetObject("tsbLineRed.Image")));
			this.tsbWireRed.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireRed.Name = "tsbWireRed";
			this.tsbWireRed.Size = new System.Drawing.Size(23, 22);
			this.tsbWireRed.Text = "導線(赤)";
			this.tsbWireRed.Click += new System.EventHandler(this.tsbWireRed_Click);
			// 
			// tsbWireBlue
			// 
			this.tsbWireBlue.CheckOnClick = true;
			this.tsbWireBlue.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireBlue.Image = ((System.Drawing.Image)(resources.GetObject("tsbLineBlue.Image")));
			this.tsbWireBlue.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireBlue.Name = "tsbWireBlue";
			this.tsbWireBlue.Size = new System.Drawing.Size(23, 22);
			this.tsbWireBlue.Text = "導線(青)";
			this.tsbWireBlue.Click += new System.EventHandler(this.tsbWireBlue_Click);
			// 
			// tsbWireGreen
			// 
			this.tsbWireGreen.CheckOnClick = true;
			this.tsbWireGreen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireGreen.Image = ((System.Drawing.Image)(resources.GetObject("tsbLineGreen.Image")));
			this.tsbWireGreen.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireGreen.Name = "tsbWireGreen";
			this.tsbWireGreen.Size = new System.Drawing.Size(23, 22);
			this.tsbWireGreen.Text = "導線(緑)";
			this.tsbWireGreen.Click += new System.EventHandler(this.tsbWireGreen_Click);
            // 
            // tsbWireYellow
            // 
            this.tsbWireYellow.CheckOnClick = true;
			this.tsbWireYellow.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbWireYellow.Image = ((System.Drawing.Image)(resources.GetObject("tsbLineYellow.Image")));
			this.tsbWireYellow.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbWireYellow.Name = "tsbWireYellow";
			this.tsbWireYellow.Size = new System.Drawing.Size(23, 22);
			this.tsbWireYellow.Text = "導線(黄)";
			this.tsbWireYellow.Click += new System.EventHandler(this.tsbWireYellow_Click);
            // 
            // tsbWireTin
            // 
            this.tsbWireTin.CheckOnClick = true;
            this.tsbWireTin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tsbWireTin.Image = ((System.Drawing.Image)(resources.GetObject("tsbLineWhite.Image")));
            this.tsbWireTin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsbWireTin.Name = "tsbWireTin";
            this.tsbWireTin.Size = new System.Drawing.Size(23, 22);
            this.tsbWireTin.Text = "すずメッキ線";
            this.tsbWireTin.Click += new System.EventHandler(this.tsbWireTin_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
			// 
			// tsbFront
			// 
			this.tsbFront.Checked = true;
			this.tsbFront.CheckState = System.Windows.Forms.CheckState.Checked;
			this.tsbFront.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbFront.Image = ((System.Drawing.Image)(resources.GetObject("tsbFront.Image")));
			this.tsbFront.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbFront.Name = "tsbFront";
			this.tsbFront.Size = new System.Drawing.Size(23, 22);
			this.tsbFront.Text = "部品不透過";
			this.tsbFront.Click += new System.EventHandler(this.tsbFront_Click);
			// 
			// tsbReverse
			// 
			this.tsbReverse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.tsbReverse.Image = ((System.Drawing.Image)(resources.GetObject("tsbReverse.Image")));
			this.tsbReverse.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.tsbReverse.Name = "tsbReverse";
			this.tsbReverse.Size = new System.Drawing.Size(23, 22);
			this.tsbReverse.Text = "部品透過";
			this.tsbReverse.Click += new System.EventHandler(this.tsbReverse_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
			// 
			// tscGridWidth
			// 
			this.tscGridWidth.Items.AddRange(new object[] {
            "2.54mm",
            "1.27mm"});
			this.tscGridWidth.Name = "tscGridWidth";
			this.tscGridWidth.Size = new System.Drawing.Size(80, 25);
			this.tscGridWidth.SelectedIndexChanged += new System.EventHandler(this.tscGridWidth_SelectedIndexChanged);
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
			this.tsParts.Size = new System.Drawing.Size(267, 25);
			this.tsParts.TabIndex = 0;
			this.tsParts.Text = "toolStrip2";
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ファイルFToolStripMenuItem,
            this.編集EToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(1008, 24);
			this.menuStrip1.TabIndex = 1;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// ファイルFToolStripMenuItem
			// 
			this.ファイルFToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.新規作成NToolStripMenuItem,
            this.toolStripSeparator6,
            this.開くOToolStripMenuItem,
            this.toolStripSeparator1,
            this.上書き保存SToolStripMenuItem,
            this.名前を付けて保存AToolStripMenuItem,
            this.toolStripSeparator2});
			this.ファイルFToolStripMenuItem.Name = "ファイルFToolStripMenuItem";
			this.ファイルFToolStripMenuItem.Size = new System.Drawing.Size(67, 20);
			this.ファイルFToolStripMenuItem.Text = "ファイル(F)";
			// 
			// 新規作成NToolStripMenuItem
			// 
			this.新規作成NToolStripMenuItem.Name = "新規作成NToolStripMenuItem";
			this.新規作成NToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.新規作成NToolStripMenuItem.Text = "新規作成(N)";
			this.新規作成NToolStripMenuItem.Click += new System.EventHandler(this.新規作成NToolStripMenuItem_Click);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(181, 6);
			// 
			// 開くOToolStripMenuItem
			// 
			this.開くOToolStripMenuItem.Name = "開くOToolStripMenuItem";
			this.開くOToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
			this.開くOToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.開くOToolStripMenuItem.Text = "開く(O)";
			this.開くOToolStripMenuItem.Click += new System.EventHandler(this.開くOToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(181, 6);
			// 
			// 上書き保存SToolStripMenuItem
			// 
			this.上書き保存SToolStripMenuItem.Name = "上書き保存SToolStripMenuItem";
			this.上書き保存SToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
			this.上書き保存SToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.上書き保存SToolStripMenuItem.Text = "上書き保存(S)";
			this.上書き保存SToolStripMenuItem.Click += new System.EventHandler(this.上書き保存SToolStripMenuItem_Click);
			// 
			// 名前を付けて保存AToolStripMenuItem
			// 
			this.名前を付けて保存AToolStripMenuItem.Name = "名前を付けて保存AToolStripMenuItem";
			this.名前を付けて保存AToolStripMenuItem.Size = new System.Drawing.Size(184, 22);
			this.名前を付けて保存AToolStripMenuItem.Text = "名前を付けて保存(A)";
			this.名前を付けて保存AToolStripMenuItem.Click += new System.EventHandler(this.名前を付けて保存AToolStripMenuItem_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(181, 6);
			// 
			// 編集EToolStripMenuItem
			// 
			this.編集EToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.選択SToolStripMenuItem,
            this.toolStripSeparator7,
            this.切り取りTToolStripMenuItem,
            this.コピーCToolStripMenuItem,
            this.貼り付けPToolStripMenuItem,
            this.削除DToolStripMenuItem,
            this.toolStripSeparator3,
            this.左回転LToolStripMenuItem,
            this.右回転RToolStripMenuItem});
			this.編集EToolStripMenuItem.Name = "編集EToolStripMenuItem";
			this.編集EToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
			this.編集EToolStripMenuItem.Text = "編集(E)";
			// 
			// 左回転LToolStripMenuItem
			// 
			this.左回転LToolStripMenuItem.Name = "左回転LToolStripMenuItem";
			this.左回転LToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left)));
			this.左回転LToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.左回転LToolStripMenuItem.Text = "左回転(L)";
			this.左回転LToolStripMenuItem.Click += new System.EventHandler(this.左回転LToolStripMenuItem_Click);
			// 
			// 右回転RToolStripMenuItem
			// 
			this.右回転RToolStripMenuItem.Name = "右回転RToolStripMenuItem";
			this.右回転RToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right)));
			this.右回転RToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.右回転RToolStripMenuItem.Text = "右回転(R)";
			this.右回転RToolStripMenuItem.Click += new System.EventHandler(this.右回転RToolStripMenuItem_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(183, 6);
			// 
			// 切り取りTToolStripMenuItem
			// 
			this.切り取りTToolStripMenuItem.Name = "切り取りTToolStripMenuItem";
			this.切り取りTToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.切り取りTToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.切り取りTToolStripMenuItem.Text = "切り取り(T)";
			this.切り取りTToolStripMenuItem.Click += new System.EventHandler(this.切り取りTToolStripMenuItem_Click);
			// 
			// コピーCToolStripMenuItem
			// 
			this.コピーCToolStripMenuItem.Name = "コピーCToolStripMenuItem";
			this.コピーCToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.コピーCToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.コピーCToolStripMenuItem.Text = "コピー(C)";
			this.コピーCToolStripMenuItem.Click += new System.EventHandler(this.コピーCToolStripMenuItem_Click);
			// 
			// 貼り付けPToolStripMenuItem
			// 
			this.貼り付けPToolStripMenuItem.Name = "貼り付けPToolStripMenuItem";
			this.貼り付けPToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.貼り付けPToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.貼り付けPToolStripMenuItem.Text = "貼り付け(P)";
			this.貼り付けPToolStripMenuItem.Click += new System.EventHandler(this.貼り付けPToolStripMenuItem_Click);
			// 
			// 削除DToolStripMenuItem
			// 
			this.削除DToolStripMenuItem.Name = "削除DToolStripMenuItem";
			this.削除DToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.削除DToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.削除DToolStripMenuItem.Text = "削除(D)";
			this.削除DToolStripMenuItem.Click += new System.EventHandler(this.削除DToolStripMenuItem_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// 選択SToolStripMenuItem
			// 
			this.選択SToolStripMenuItem.Name = "選択SToolStripMenuItem";
			this.選択SToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Space)));
			this.選択SToolStripMenuItem.Size = new System.Drawing.Size(186, 22);
			this.選択SToolStripMenuItem.Text = "選択(S)";
			this.選択SToolStripMenuItem.Click += new System.EventHandler(this.選択SToolStripMenuItem_Click);
			// 
			// toolStripSeparator7
			// 
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(183, 6);
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
			// 
			// toolStripSeparator9
			// 
			this.toolStripSeparator9.Name = "toolStripSeparator9";
			this.toolStripSeparator9.Size = new System.Drawing.Size(6, 25);
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

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ToolStrip tsBoard;
        private System.Windows.Forms.ToolStrip tsParts;
        private System.Windows.Forms.ToolStripButton tsbCursor;
        private System.Windows.Forms.ToolStripButton tsbWireBlack;
        private System.Windows.Forms.ToolStripButton tsbWireTin;
        private System.Windows.Forms.ToolStripButton tsbWireRed;
        private System.Windows.Forms.ToolStripButton tsbWireBlue;
        private System.Windows.Forms.ToolStripButton tsbWireGreen;
        private System.Windows.Forms.ToolStripButton tsbWireYellow;
        private System.Windows.Forms.PictureBox picBoard;
        private System.Windows.Forms.Panel pnlBoard;
		private System.Windows.Forms.Panel pnlParts;
        private System.Windows.Forms.ToolStripButton tsbLand;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem ファイルFToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 開くOToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem 上書き保存SToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 名前を付けて保存AToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem 編集EToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 左回転LToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem 右回転RToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripComboBox tscGridWidth;
		private System.Windows.Forms.ToolStripMenuItem 切り取りTToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem コピーCToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 貼り付けPToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 削除DToolStripMenuItem;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripButton tsbFront;
		private System.Windows.Forms.ToolStripButton tsbReverse;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem 新規作成NToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem 選択SToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
    }
}

