using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace UCB {
	public partial class OutputSettings : Form {
		List<Item> mList;
		string mPath;

		public OutputSettings(List<Item> items, string path) {
			InitializeComponent();
			mList = items;
			mPath = path;
		}

		private void btnOutput_Click(object sender, EventArgs e) {
			saveFileDialog1.Filter = "PDFファイル(*.pdf)|*.pdf";
			saveFileDialog1.FileName = Path.GetFileNameWithoutExtension(mPath);
			saveFileDialog1.ShowDialog();
			var filePath = saveFileDialog1.FileName;
			if (string.IsNullOrEmpty(filePath) || !Directory.Exists(Path.GetDirectoryName(filePath))) {
				return;
			}
			PDF.PAGE_SIZE size = PDF.PAGE_SIZE.L_H;
			if (rbPrintA4.Checked) {
				size = PDF.PAGE_SIZE.A4_H;
			}
			if (rbPrintA5.Checked) {
				size = PDF.PAGE_SIZE.A5_H;
			}
			if (rbPrintPost.Checked) {
				size = PDF.PAGE_SIZE.POST_H;
			}
			var pdf = new PDF();
			{
				var page = new PDF.Page(size);
				page.Scale = 2.54 / Item.GridWidth;
				foreach (var rec in mList) {
					rec.DrawPattern(page);
				}
				pdf.AddPage(page);
			}
			if (chkResistMask.Checked) {
				var page = new PDF.Page(size);
				page.Scale = 2.54 / Item.GridWidth;
				foreach (var rec in mList) {
					rec.DrawSolderMask(page);
				}
				pdf.AddPage(page);
			}
			pdf.Save(filePath);
			Close();
		}
	}
}
