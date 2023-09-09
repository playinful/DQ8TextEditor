using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace DQ8TextEditor
{
    public partial class Form1 : Form
    {
        private MessageFile ActiveFile = new MessageFile();

        public Form1()
        {
            InitializeComponent();
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                tryOpenFile(args[1]);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Dragon Quest VIII Message File (*.binE, *.binF, *.binG, *.binI, *.binS)|*.binE;*.binF;*.binG;*.binI;*.binS";
            openFileDialog.ShowDialog();

            tryOpenFile(openFileDialog.FileName);
        }
        private void tryOpenFile (string filename)
        {
            //try to load that file
            ActiveFile.Load(filename);
            ActiveFile.ShowDetails(stringListBox, textBoxMain);
            EnableDisableControls();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ActiveFile.Save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.DefaultExt = ".binE";
            saveFileDialog.Filter = "Dragon Quest VIII Message File (*.binE, *.binF, *.binG, *.binI, *.binS)|*.binE;*.binF;*.binG;*.binI;*.binS";
            saveFileDialog.ShowDialog();

            ActiveFile.Save(saveFileDialog.FileName);
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool succ = ActiveFile.Close();
            if (succ)
            {
                textBoxMain.Text = "";
                textBoxMain.Enabled = false;
                stringListBox.Items.Clear();
            }
            EnableDisableControls();
        }

        private void stringListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ActiveFile != null && ActiveFile.Ready())
            {
                textBoxMain.Text = ActiveFile.Strings[stringListBox.SelectedIndex];
            }
        }

        private void textBoxMain_TextChanged(object sender, EventArgs e)
        {
            ActiveFile.EditString(stringListBox.SelectedIndex, textBoxMain.Text);
        }

        private void EnableDisableControls()
        {
            bool isReady = ActiveFile.Ready();

            saveToolStripMenuItem.Enabled = isReady;
            saveAsToolStripMenuItem.Enabled = isReady;
            closeToolStripMenuItem.Enabled = isReady;
        }
        private void DragOverEvent(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }
        private void DragDropEvent(object sender, DragEventArgs e)
        {
            string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Any())
            {
                tryOpenFile(files[0]);
            }
        }

    }
}
