using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using ORLabs.Screens;
using ORLabs.Framework;
using ORLabs.Algorithms;

namespace ORLabs
{
    public partial class GraphIO : Form
    {
        public enum IOTaskType
        {
            Save,
            Load
        }

        public GraphIO()
        {
            InitializeComponent();

            System.Drawing.Image imf = System.Drawing.Image.FromFile(@"Resources\FileIcon.png");
            buttonNodes.BackgroundImage = imf;
            buttonNodes.BackgroundImageLayout = ImageLayout.Stretch;
            buttonEdges.BackgroundImage = imf;
            buttonEdges.BackgroundImageLayout = ImageLayout.Stretch;
            buttonGrid.BackgroundImage = imf;
            buttonGrid.BackgroundImageLayout = ImageLayout.Stretch;

            System.Drawing.Image imF = System.Drawing.Image.FromFile(@"Resources\FolderIcon.png");
            buttonNF.BackgroundImage = imF;
            buttonNF.BackgroundImageLayout = ImageLayout.Stretch;
            buttonEF.BackgroundImage = imF;
            buttonEF.BackgroundImageLayout = ImageLayout.Stretch;
            buttonGF.BackgroundImage = imF;
            buttonGF.BackgroundImageLayout = ImageLayout.Stretch;
        }

        private void addPLine(string s)
        {
            textProgress.AppendText(s + "\r\n");
        }
        private void addPLine(string format, params object[] args)
        {
            textProgress.AppendText(string.Format(format, args) + "\r\n");
        }

        private void buttonNodes_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Node Files (*.nodes)|*.nodes|All Files (*.*)|*.*";
                ofd.ShowDialog();
                if (ofd.FileName != null && File.Exists(ofd.FileName))
                {
                    feNodes.Text = ofd.FileName;
                }
            }
        }
        private void buttonEdges_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Edge Files (*.edges)|*.edges|All Files (*.*)|*.*";
                ofd.ShowDialog();
                if (ofd.FileName != null && File.Exists(ofd.FileName))
                {
                    feEdges.Text = ofd.FileName;
                }
            }
        }
        private void buttonGrid_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Grid Files (*.grid)|*.grid|All Files (*.*)|*.*";
                ofd.ShowDialog();
                if (ofd.FileName != null && File.Exists(ofd.FileName))
                {
                    feGrid.Text = ofd.FileName;
                }
            }
        }

        private void buttonNF_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog ofd = new FolderBrowserDialog())
            {
                ofd.ShowDialog();
                if (ofd.SelectedPath != null && Directory.Exists(ofd.SelectedPath))
                {
                    feNodes.Text = ofd.SelectedPath;
                }
            }
        }
        private void buttonEF_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog ofd = new FolderBrowserDialog())
            {
                ofd.ShowDialog();
                if (ofd.SelectedPath != null && Directory.Exists(ofd.SelectedPath))
                {
                    feEdges.Text = ofd.SelectedPath;
                }
            }
        }
        private void buttonGF_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog ofd = new FolderBrowserDialog())
            {
                ofd.ShowDialog();
                if (ofd.SelectedPath != null && Directory.Exists(ofd.SelectedPath))
                {
                    feGrid.Text = ofd.SelectedPath;
                }
            }
        }

        public void setLoading()
        {
            buttonLoad.Enabled = true;
            buttonSave.Enabled = false;
        }
        private void loadGraph(GraphScreen gs)
        {
            textProgress.Clear();
            progressBar.Value = 0;

            textProgress.AppendText("Checking Node File...     ");
            if (!File.Exists(feNodes.Text))
            {
                addPLine("Failed");
                return;
            }
            addPLine("Success");
            progressBar.Value += 3;

            textProgress.AppendText("Checking Edge File...     ");
            if (!File.Exists(feEdges.Text))
            {
                addPLine("Failed");
                return;
            }
            addPLine("Success");
            progressBar.Value += 3;

            textProgress.AppendText("Checking Grid File...     ");
            if (string.IsNullOrWhiteSpace(feGrid.Text))
            {
                addPLine("None Used");
                feGrid.Text = null;
            }
            else if (!File.Exists(feNodes.Text))
            {
                addPLine("Failed");
                return;
            }
            else
            {
                addPLine("Success");
            }
            progressBar.Value += 3;

            addPLine("Opening File Group");
            ORGraphFile ogf = new ORGraphFile(
                feNodes.Text,
                feEdges.Text,
                feGrid.Text
                );
            progressBar.Value = 50;
            addPLine("Parsing Files");
            try
            {
                ogf.read();
            }
            catch (Exception exc)
            {
                addPLine("Exception:\r\n{0}\r\n{1}", exc.Message, exc.StackTrace);
                progressBar.Value = 100;
            }
            addPLine("Successful Read");
            addPLine("Safe To Close");
            progressBar.Value = 100;
            gs.setGraph(ogf.Graph);
        }
        private void buttonLoad_Click(object sender, EventArgs e)
        {
            buttonLoad.Enabled = false;
            loadGraph(GraphScreen.GIOScreen);
            buttonLoad.Enabled = true;
        }
        
        public void setSaving()
        {
            buttonSave.Enabled = true;
            buttonLoad.Enabled = false;
        }
        private void saveGraph(GraphScreen gs)
        {
            textProgress.Clear();
            progressBar.Value = 0;
            if (gs.Graph == null) { addPLine("No Graph Supplied"); return; }

            FileInfo fi;

            textProgress.AppendText("Checking Node File...     ");
            if (!string.IsNullOrWhiteSpace(feNodes.Text))
            {
                fi = new FileInfo(feNodes.Text);
                if (fi.Exists || fi.Directory.Exists)
                {
                    addPLine("Success");
                }
                else
                {
                    addPLine("Bad Location");
                    return;
                }
            }
            else
            {
                addPLine("Node File Path Needed");
                return;
            }
            progressBar.Value += 3;

            textProgress.AppendText("Checking Edge File...     ");
            if (!string.IsNullOrWhiteSpace(feEdges.Text))
            {
                fi = new FileInfo(feEdges.Text);
                if (fi.Exists || fi.Directory.Exists)
                {
                    addPLine("Success");
                }
                else
                {
                    addPLine("Bad Location");
                    return;
                }
            }
            else
            {
                addPLine("Edge File Path Needed");
                return;
            }
            progressBar.Value += 3;

            textProgress.AppendText("Checking Grid File...     ");
            if (string.IsNullOrWhiteSpace(feGrid.Text))
            {
                addPLine("None Used");
                feGrid.Text = null;
            }
            else
            {
                fi = new FileInfo(feEdges.Text);
                if (fi.Exists || fi.Directory.Exists)
                {
                    addPLine("Success");
                }
                else
                {
                    addPLine("Bad Location");
                    return;
                }
            }
            progressBar.Value += 3;

            addPLine("Opening File Group");
            ORGraphFile ogf = new ORGraphFile(
                feNodes.Text,
                feEdges.Text,
                feGrid.Text,
                gs.Graph
                );
            progressBar.Value = 50;
            addPLine("Parsing Files");
            try
            {
                ogf.write();
            }
            catch (Exception exc)
            {
                addPLine("Exception:\r\n{0}\r\n{1}", exc.Message, exc.StackTrace);
                progressBar.Value = 100;
            }
            addPLine("Successful Write");
            addPLine("Safe To Close");
            progressBar.Value = 100;
        }
        private void buttonSave_Click(object sender, EventArgs e)
        {
            buttonSave.Enabled = false;
            saveGraph(GraphScreen.GIOScreen);
            buttonSave.Enabled = true;
        }
    }
}
