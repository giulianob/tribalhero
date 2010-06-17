using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Simulator {
	public partial class GenericInputBox : Form {
		public string Value {
			get {
				return txtValue.Text;
			}
		}

		public GenericInputBox(string iTitle, string iPrompt, string iDefaultValue) {
			InitializeComponent();
			Text = iTitle;
			label1.Text = iPrompt;
			txtValue.Text = iDefaultValue;
		}

		private void btnCancel_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.Cancel;
		}

		private void btnOK_Click(object sender, EventArgs e) {
			DialogResult = DialogResult.OK;
		}

		private void InputBox_Shown(object sender, EventArgs e) {
			txtValue.Focus();
		}

		private void txtValue_KeyPress(object sender, KeyPressEventArgs e) {
			if(e.KeyChar == (char)Keys.Enter) {
				btnOK.PerformClick();
			} else if(e.KeyChar == (char)Keys.Escape) {
				btnCancel.PerformClick();
			}
		}
	}
}