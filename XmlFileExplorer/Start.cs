using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace XmlFileExplorer
{
    public partial class Start : Form
    {
        private static XmlDocument document = new XmlDocument();
        private static string[,] allRecentFiles;

        public Start()
        {
            InitializeComponent();
        }

        private void Start_Load(object sender, EventArgs e)
        {
            setTheme();

            checkFile();
            document.Load(Settings.OriginalPath);

            loadRecentFiles();

            Methods.setCommonFileExtensions();

            Load_DataGridView();
        }

        private void createRecentFile(string path, string date)
        {
            XmlElement file = document.CreateElement("file");

            file.SetAttribute("path", path);
            file.SetAttribute("date", date);

            document.SelectSingleNode("/recent").AppendChild(file);

            document.Save(Settings.OriginalPath);
        }

        private void loadRecentFiles()
        {
            XmlNodeList nodeList = document.SelectNodes("/recent/file");

            allRecentFiles = new string[nodeList.Count, 2];
            for (int i = 0; i < nodeList.Count; i++)
            {
                allRecentFiles[i, 0] = nodeList[i].Attributes["path"].Value;
                allRecentFiles[i, 1] = nodeList[i].Attributes["date"].Value;
            }
        }

        private void checkFile()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Settings.OriginalPath) + @"\Storage");

            if (!File.Exists(Settings.OriginalPath))
            {
                XmlElement recentElement = document.CreateElement("recent");
                document.AppendChild(recentElement);

                document.Save(Settings.OriginalPath);
            }
        }

        private void Load_DataGridView()
        {
            dataGridView1.Rows.Clear();

            for (int i = 0; i < allRecentFiles.GetLength(0); i++)
            {
                TimeSpan days = DateTime.Now - DateTime.FromFileTime(Int64.Parse(allRecentFiles[i, 1]));

                dataGridView1.Rows.Add(i, Properties.Resources.XmlFileIcon, Path.GetFileName(allRecentFiles[i, 0]), getDateAgo(days.TotalDays), Path.GetDirectoryName(allRecentFiles[i, 0]));
            }

            dataGridView1.ClearSelection();
        }

        private void setTheme()
        {
            dataGridView1.Focus();

            this.BackColor = Settings.Form_BackColor;

            openButton.BackColor = Settings.Button_BackColor;
            openButton.ForeColor = Settings.Button_ForeColor;
            openButton.FlatAppearance.BorderColor = Settings.Button_BorderColor;

            newButton.BackColor = Settings.Button_BackColor;
            newButton.ForeColor = Settings.Button_ForeColor;
            newButton.FlatAppearance.BorderColor = Settings.Button_BorderColor;

            dataGridView1.BackgroundColor = Settings.DataGridView_BackColor;
            dataGridView1.DefaultCellStyle.BackColor = Settings.DataGridView_BackColor;
            dataGridView1.DefaultCellStyle.ForeColor = Settings.DataGridView_ForeColor;
            dataGridView1.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Settings.DataGridView_RowSelectionColor;

            dataGridView1.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            dataGridView1.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;

            dataGridView1.AdvancedCellBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;
            dataGridView1.AdvancedCellBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;

            panel1.BackColor = ColorTranslator.FromHtml("#3E3E3F");

            label1.ForeColor = Settings.Label_ForeColor;
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private string getDateAgo(double totalDays)
        {
            string date = "";

            if (totalDays < 1)
            {
                date = "Today";
            }
            else if (totalDays > 1 && totalDays < 7)
            {
                date = ((int)(totalDays)).ToString() + " Days Ago";
            }
            else if (totalDays > 6 && totalDays < 14)
            {
                date = "1 Week Ago";
            }
            else if (totalDays > 13 && totalDays < 32)
            {
                date = ((int)(totalDays / 7)).ToString() + " Weeks Ago";
            }
            else if (totalDays > 31 && totalDays < 62)
            {
                date = "1 Month Ago";
            }
            else if (totalDays > 61 && totalDays < 365)
            {
                date = ((int)(totalDays / 31)).ToString() + " Months Ago";
            }
            else if (totalDays > 364 && totalDays < 730)
            {
                date = "1 Year Ago";
            }
            else if (totalDays > 729)
            {
                date = ((int)(totalDays / 365)).ToString() + " Years Ago";
            }

            return date;
        }

        private void openExplorer(string path)
        {
            Explorer explorer = new Explorer();
            Explorer.BasePath = path;

            Hide();

            explorer.ShowDialog();

            Close();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            openExplorer(allRecentFiles[dataGridView1.SelectedRows[0].Index, 0]);
        }

        private static int preRow = -1;

        private void dataGridView1_MouseLeave(object sender, EventArgs e)
        {
            if (preRow > -1){
                dataGridView1.Rows[preRow].DefaultCellStyle.BackColor = Settings.DataGridView_BackColor;

                preRow = -1;

                dataGridView1.Cursor = Cursors.Arrow;
            }
        }

        private void dataGridView1_MouseMove(object sender, MouseEventArgs e)
        {
            var hit = dataGridView1.HitTest(e.X, e.Y);

            if (hit.Type == DataGridViewHitTestType.Cell) {
                if (preRow != hit.RowIndex)
                {
                    dataGridView1.Rows[hit.RowIndex].DefaultCellStyle.BackColor = ColorTranslator.FromHtml("#595959");

                    if (preRow > -1)
                    {
                        dataGridView1.Rows[preRow].DefaultCellStyle.BackColor = Settings.DataGridView_BackColor;
                    }
                    preRow = hit.RowIndex;
                }

                dataGridView1.Cursor = Cursors.Hand;
            } else if (hit.Type == DataGridViewHitTestType.None && preRow > -1) {
                dataGridView1.Rows[preRow].DefaultCellStyle.BackColor = Settings.DataGridView_BackColor;
                preRow = -1;

                dataGridView1.Cursor = Cursors.Arrow;
            }
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            var hit = dataGridView1.HitTest(e.X, e.Y);

            if (hit.Type == DataGridViewHitTestType.None)
            {
                dataGridView1.ClearSelection();
            }
        }
    }

    class DBDataGridView : DataGridView
    {
        public DBDataGridView() { DoubleBuffered = true; }
    }
}
