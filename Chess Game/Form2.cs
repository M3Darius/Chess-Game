using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chess_Game
{
	public partial class Form2 : Form
	{
		public Form2()
		{
			InitializeComponent();
			foreach(string s in Form1.PiecesNames)
			{
				comboBox1.Items.Add(s);
				comboBox2.Items.Add(s);
			}

			foreach (string s in Form1.BoardNames)
			{
				comboBox3.Items.Add(s);
			}
			comboBox1.SelectedIndex = 0;
			comboBox2.SelectedIndex = 1;
			comboBox3.SelectedIndex = 0;
		}

		private void button1_Click(object sender, EventArgs e)
		{
			Form1 form = new Form1(comboBox1.SelectedIndex, comboBox2.SelectedIndex, comboBox3.SelectedIndex);
			Console.WriteLine(comboBox1.SelectedIndex + " " + comboBox2.SelectedIndex + " " + comboBox3.SelectedIndex);
			this.Hide();
			form.ShowDialog();
			this.Close();
		}

		Random r = new Random();
		private void button2_Click(object sender, EventArgs e)
		{
			comboBox1.SelectedIndex = r.Next(Form1.PiecesNames.Length);
			comboBox2.SelectedIndex = r.Next(Form1.PiecesNames.Length);
			comboBox3.SelectedIndex = r.Next(Form1.BoardNames.Length);
		}
	}
}
