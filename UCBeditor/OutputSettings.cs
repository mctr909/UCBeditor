using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

using Items;
using Pdf;

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
			var size = PageSize.L_H;
			if (rbPrintA4.Checked) {
				size = PageSize.A4_H;
			}
			if (rbPrintA5.Checked) {
				size = PageSize.A5_H;
			}
			if (rbPrintPost.Checked) {
				size = PageSize.POST_H;
			}
			var pdf = new PdfFile();
			{
				var page = new PdfPage(size, 2.54 / Item.GridWidth);
				foreach (var rec in mList) {
					rec.DrawPattern(page);
				}
				pdf.Add(page);
			}
			if (chkResistMask.Checked) {
				var page = new PdfPage(size, 2.54 / Item.GridWidth);
				foreach (var rec in mList) {
					rec.DrawSolderMask(page);
				}
				pdf.Add(page);
			}
			pdf.Save(filePath);
			Close();
		}
	}
}
