namespace UCB {
	partial class OutputSettings {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.rbPrintPost = new System.Windows.Forms.RadioButton();
			this.rbPrintL = new System.Windows.Forms.RadioButton();
			this.rbPrintA5 = new System.Windows.Forms.RadioButton();
			this.rbPrintA4 = new System.Windows.Forms.RadioButton();
			this.chkReverse = new System.Windows.Forms.CheckBox();
			this.chkResistMask = new System.Windows.Forms.CheckBox();
			this.chkDrill = new System.Windows.Forms.CheckBox();
			this.btnOutput = new System.Windows.Forms.Button();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.rbPrintPost);
			this.groupBox1.Controls.Add(this.rbPrintL);
			this.groupBox1.Controls.Add(this.rbPrintA5);
			this.groupBox1.Controls.Add(this.rbPrintA4);
			this.groupBox1.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.groupBox1.Location = new System.Drawing.Point(8, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(216, 48);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "用紙";
			// 
			// rbPrintPost
			// 
			this.rbPrintPost.AutoSize = true;
			this.rbPrintPost.Checked = true;
			this.rbPrintPost.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.rbPrintPost.Location = new System.Drawing.Point(100, 17);
			this.rbPrintPost.Name = "rbPrintPost";
			this.rbPrintPost.Size = new System.Drawing.Size(62, 22);
			this.rbPrintPost.TabIndex = 3;
			this.rbPrintPost.TabStop = true;
			this.rbPrintPost.Text = "はがき";
			this.rbPrintPost.UseVisualStyleBackColor = true;
			// 
			// rbPrintL
			// 
			this.rbPrintL.AutoSize = true;
			this.rbPrintL.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.rbPrintL.Location = new System.Drawing.Point(168, 17);
			this.rbPrintL.Name = "rbPrintL";
			this.rbPrintL.Size = new System.Drawing.Size(45, 22);
			this.rbPrintL.TabIndex = 2;
			this.rbPrintL.Text = "L判";
			this.rbPrintL.UseVisualStyleBackColor = true;
			// 
			// rbPrintA5
			// 
			this.rbPrintA5.AutoSize = true;
			this.rbPrintA5.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.rbPrintA5.Location = new System.Drawing.Point(53, 17);
			this.rbPrintA5.Name = "rbPrintA5";
			this.rbPrintA5.Size = new System.Drawing.Size(41, 22);
			this.rbPrintA5.TabIndex = 1;
			this.rbPrintA5.Text = "A5";
			this.rbPrintA5.UseVisualStyleBackColor = true;
			// 
			// rbPrintA4
			// 
			this.rbPrintA4.AutoSize = true;
			this.rbPrintA4.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.rbPrintA4.Location = new System.Drawing.Point(6, 17);
			this.rbPrintA4.Name = "rbPrintA4";
			this.rbPrintA4.Size = new System.Drawing.Size(41, 22);
			this.rbPrintA4.TabIndex = 0;
			this.rbPrintA4.Text = "A4";
			this.rbPrintA4.UseVisualStyleBackColor = true;
			// 
			// chkReverse
			// 
			this.chkReverse.AutoSize = true;
			this.chkReverse.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.chkReverse.Location = new System.Drawing.Point(8, 62);
			this.chkReverse.Name = "chkReverse";
			this.chkReverse.Size = new System.Drawing.Size(123, 22);
			this.chkReverse.TabIndex = 1;
			this.chkReverse.Text = "パターン左右反転";
			this.chkReverse.UseVisualStyleBackColor = true;
			// 
			// chkResistMask
			// 
			this.chkResistMask.AutoSize = true;
			this.chkResistMask.Checked = true;
			this.chkResistMask.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkResistMask.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.chkResistMask.Location = new System.Drawing.Point(8, 90);
			this.chkResistMask.Name = "chkResistMask";
			this.chkResistMask.Size = new System.Drawing.Size(123, 22);
			this.chkResistMask.TabIndex = 2;
			this.chkResistMask.Text = "ソルダーレジスト";
			this.chkResistMask.UseVisualStyleBackColor = true;
			// 
			// chkDrill
			// 
			this.chkDrill.AutoSize = true;
			this.chkDrill.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.chkDrill.Location = new System.Drawing.Point(8, 118);
			this.chkDrill.Name = "chkDrill";
			this.chkDrill.Size = new System.Drawing.Size(99, 22);
			this.chkDrill.TabIndex = 3;
			this.chkDrill.Text = "ドリルデータ";
			this.chkDrill.UseVisualStyleBackColor = true;
			// 
			// btnOutput
			// 
			this.btnOutput.Font = new System.Drawing.Font("メイリオ", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
			this.btnOutput.Location = new System.Drawing.Point(165, 109);
			this.btnOutput.Name = "btnOutput";
			this.btnOutput.Size = new System.Drawing.Size(59, 29);
			this.btnOutput.TabIndex = 4;
			this.btnOutput.Text = "出力";
			this.btnOutput.UseVisualStyleBackColor = true;
			this.btnOutput.Click += new System.EventHandler(this.btnOutput_Click);
			// 
			// OutputSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(230, 144);
			this.Controls.Add(this.btnOutput);
			this.Controls.Add(this.chkDrill);
			this.Controls.Add(this.chkResistMask);
			this.Controls.Add(this.chkReverse);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OutputSettings";
			this.Text = "出力設定";
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton rbPrintL;
		private System.Windows.Forms.RadioButton rbPrintA5;
		private System.Windows.Forms.RadioButton rbPrintA4;
		private System.Windows.Forms.RadioButton rbPrintPost;
		private System.Windows.Forms.CheckBox chkReverse;
		private System.Windows.Forms.CheckBox chkResistMask;
		private System.Windows.Forms.CheckBox chkDrill;
		private System.Windows.Forms.Button btnOutput;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
	}
}