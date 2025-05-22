using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace XmlFileExplorer
{
    public partial class Explorer : Form
    {
        public static string BasePath = "";
        public static XmlDocument document = new XmlDocument();

        public static Dictionary<int, string[]> AllFiles = new Dictionary<int, string[]>();//index -  new string[]{0 - type, 1 - full path}
        public static string[] Path = { };

        public static List<string> VisitedDirectories = new List<string>();
        public static int IndexOfVisitedDirectories = -1;

        public static List<string> SelectedItems = new List<string> { string.Empty };
        public static int CurrentDirectoryLevel = 0;

        public Explorer()
        {
            InitializeComponent();
        }

        private void Explorer_Load(object sender, EventArgs e)
        {
            setTheme();

            document.Load(BasePath);
            loadFiles();

            load_DataGridView();

            timer1.Start();
        }

        private string getData(int col) {
            return dataGridView1.Rows[dataGridView1.SelectedRows[0].Index].Cells[col].Value.ToString();
        }

        private void load_DataGridView() {
            //Add Rows
            dataGridView1.Rows.Clear();
            for (int i = 0; i < AllFiles.Keys.Count; i++) {
                int type = Int32.Parse(AllFiles[i][0]);

                dataGridView1.Rows.Add(
                    i,
                    type == 0 ? Properties.Resources.Directory_Empty : Properties.Resources.File_NoExtension,
                    type == 0 ? Methods.getDirectoryAttribute((AllFiles[i][1]), "name", true) : Methods.getFileAttribute(AllFiles[i][1], "name", true),
                    Methods.getDateTimeShow(type == 0 ? Methods.getDirectoryAttribute(AllFiles[i][1], "modifdate", true) : Methods.getFileAttribute(AllFiles[i][1], "modifdate", true)),
                    type == 0 ? "Folder" : Methods.getShowExtension(Methods.getFileAttribute(AllFiles[i][1], "extension", true)),
                    type == 0 ? Methods.getShowSize(Methods.getDirectoryAttribute(AllFiles[i][1], "size", true)) : Methods.getShowSize(Methods.getFileAttribute(AllFiles[i][1], "size", true))
                );
                dataGridView1.Rows[i].ReadOnly = true;
            }

            //Select
            dataGridView1.ClearSelection();
            if (!string.IsNullOrEmpty(SelectedItems[CurrentDirectoryLevel])) {
                selectRowBasedOnItemPath(SelectedItems[CurrentDirectoryLevel]);
            }

            //Miscellaneous
            pathTextBox.Text = Methods.getPathTextBox();
            itemCountLabel.Text = AllFiles.Count + " Items";
        }

        private void loadFiles() {
            XmlNodeList xmlDirectories = document.SelectNodes(Methods.getPath(Path) + "/directory");
            XmlNodeList xmlFiles = document.SelectNodes(Methods.getPath(Path) + "/file");

            AllFiles.Clear();

            //directories
            for (int i = 0; i < xmlDirectories.Count; i++) {
                AllFiles.Add(i, new string[] { "0", Methods.getPath(Path) + "/directory[@name='" + xmlDirectories[i].Attributes["name"].Value + "']" });
            }

            //directories
            for (int i = 0; i < xmlFiles.Count; i++)
            {
                AllFiles.Add((i + xmlDirectories.Count), new string[] { "1", Methods.getPath(Path) + "/file[@name='" + xmlFiles[i].Attributes["name"].Value + "']" });
            }
        }

        private void setTheme() {
            this.BackColor = Settings.ExplorerForm_BackColor;

            tableLayoutPanel2.BackColor = Settings.ExplorerForm_BackColorDark;

            panel3.BackColor = ColorTranslator.FromHtml("#353535");
            itemCountLabel.ForeColor = Settings.DataGridView_ForeColor;

            //Button
            leftArrowButton.BackColor = Settings.ExplorerForm_BackColorDark;
            leftArrowButton.ForeColor = Settings.Button_ForeColor;
            leftArrowButton.FlatAppearance.BorderColor = Settings.ExplorerForm_BackColorDark;

            rightArrowButton.BackColor = Settings.ExplorerForm_BackColorDark;
            rightArrowButton.ForeColor = Settings.Button_BackColor;
            rightArrowButton.FlatAppearance.BorderColor = Settings.ExplorerForm_BackColorDark;

            //TextBox
            pathTextBox.BackColor = Settings.ExplorerForm_BackColorDark;
            pathTextBox.ForeColor = Settings.DataGridView_ForeColor;
            pathTextBox.BorderStyle = BorderStyle.FixedSingle;

            searchTextBox.BackColor = Settings.ExplorerForm_BackColorDark;
            searchTextBox.ForeColor = Color.Orange;
            searchTextBox.BorderStyle = BorderStyle.FixedSingle;

            //DatagGridView
            dataGridView1.BackgroundColor = Settings.ExplorerForm_BackColor;
            dataGridView1.DefaultCellStyle.BackColor = Settings.ExplorerForm_BackColor;
            dataGridView1.DefaultCellStyle.ForeColor = Settings.DataGridView_ForeColor;
            dataGridView1.DefaultCellStyle.SelectionBackColor = Settings.DataGridView_RowSelectionColor;

            dataGridView1.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            dataGridView1.AdvancedCellBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            dataGridView1.AdvancedCellBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;

            dataGridView1.AdvancedCellBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;
            dataGridView1.AdvancedCellBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Settings.ExplorerForm_BackColor;
            dataGridView1.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dataGridView1.ColumnHeadersDefaultCellStyle.Padding = new Padding(3, 3, 3, 3);
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.LightGray;

            dataGridView1.EnableHeadersVisualStyles = false;

            //ContextMenuStrip
            ToolStripMenuItem[] toolStripMenus = {openToolStripMenuItem, copyToolStripMenuItem, paseToolStripMenuItem,
                                                  deleteToolStripMenuItem, moveToolStripMenuItem, newToolStripMenuItem,
                                                  folderToolStripMenuItem, fileToolStripMenuItem, renameToolStripMenuItem};

            for (int i = 0; i < toolStripMenus.Length; i++) {
                toolStripMenus[i].BackColor = Settings.ExplorerForm_BackColor;
                toolStripMenus[i].ForeColor = Settings.DataGridView_ForeColor;
            }
        }

        private void searchItems(string name) {
            for (int i = 0; i < dataGridView1.RowCount; i++) {
                if (!dataGridView1.Rows[i].Cells[2].Value.ToString().ToLower().Contains(name.ToLower()) && !string.IsNullOrEmpty(name))
                {
                    dataGridView1.Rows[i].Visible = false;
                } else {
                    dataGridView1.Rows[i].Visible = true;
                }
            }
        }

        private void selectRowBasedOnItemPath(string itemPath) {
            for (int i = 0; i < dataGridView1.RowCount; i++) {
                if (AllFiles[i][1].Equals(itemPath))
                {
                    dataGridView1.Rows[i].Selected = true;
                    i = dataGridView1.RowCount;
                }
            }
        }

        private void selectRowBasedOnValue(int col, string value) {
            for (int i = 0; i < dataGridView1.RowCount; i++) {
                if (dataGridView1.Rows[i].Cells[col].Value.ToString().Equals(value)) {
                    dataGridView1.Rows[i].Selected = true;
                    i = dataGridView1.RowCount;
                }
            }
        }

        private void OpenItem(string SelectedFile) {
            if (AllFiles[Int32.Parse(getData(0))][0] == "0")
            { //if directory
                Methods.PathAdd(Methods.getDirectoryAttribute(SelectedFile, "name", false));

                //visited directories
                IndexOfVisitedDirectories++;
                if (VisitedDirectories.Count == IndexOfVisitedDirectories)
                {//check if to add new
                    VisitedDirectories.Add(Methods.getDirectoryAttribute(SelectedFile, "name", false));
                }
                else if (!VisitedDirectories[IndexOfVisitedDirectories].Equals(Methods.getDirectoryAttribute(SelectedFile, "name", false)))
                {//if not the same
                    VisitedDirectories.RemoveRange(IndexOfVisitedDirectories, VisitedDirectories.Count - IndexOfVisitedDirectories);
                    VisitedDirectories.Add(Methods.getDirectoryAttribute(SelectedFile, "name", false));
                }

                //selected items
                if (SelectedItems.Count - 1 == CurrentDirectoryLevel)
                {//check if to add new
                    SelectedItems.Add(string.Empty);
                }
                else if (!SelectedItems[CurrentDirectoryLevel + 1].Equals(SelectedFile))
                {//check if doesn't equal
                    SelectedItems.RemoveRange(CurrentDirectoryLevel + 1, SelectedItems.Count - (CurrentDirectoryLevel + 1));
                    SelectedItems.Add(string.Empty);
                }
                CurrentDirectoryLevel++;

                loadFiles();

                load_DataGridView();
            }
            else if (AllFiles[Int32.Parse(getData(0))][0] == "1")
            { //if file
                Process p = new Process();
                p.StartInfo.FileName = @"D:\User\Darbvisma\TextReader.exe";
                p.StartInfo.Arguments = BasePath + "\n" + AllFiles[Int32.Parse(getData(0))][1];
                p.Start();
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0) {
                string SelectedFile = AllFiles[Int32.Parse(getData(0))][1];

                OpenItem(SelectedFile);
            }
        }

        private void dataGridView1_MouseClick(object sender, MouseEventArgs e)
        {
            var hit = dataGridView1.HitTest(e.X, e.Y);

            if (hit.Type == DataGridViewHitTestType.None)
            {
                dataGridView1.ClearSelection();
                SelectedItems[CurrentDirectoryLevel] = "";
            }
            else if(hit.Type == DataGridViewHitTestType.Cell){
                if (dataGridView1.SelectedRows.Count > 0) {
                    SelectedItems[CurrentDirectoryLevel] = AllFiles[Int32.Parse(getData(0))][1];
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Left Arrow Button
            if (Path.Length > 0){
                leftArrowButton.Enabled = true;
            }
            else {
                leftArrowButton.Enabled = false;
            }

            //Right Arrow Button
            if (VisitedDirectories.Count > 0 && IndexOfVisitedDirectories + 1 < VisitedDirectories.Count)
            {
                rightArrowButton.Enabled = true;
            }
            else {
                rightArrowButton.Enabled = false;
            }

            //Context Menu Strip
            if (!string.IsNullOrEmpty(SelectedItems[CurrentDirectoryLevel]))
            {
                openToolStripMenuItem.Visible = true;
                copyToolStripMenuItem.Visible = true;
                deleteToolStripMenuItem.Visible = true;
                moveToolStripMenuItem.Visible = true;
                renameToolStripMenuItem.Visible = true;
            }
            else {
                openToolStripMenuItem.Visible = false;
                copyToolStripMenuItem.Visible = false;
                deleteToolStripMenuItem.Visible = false;
                moveToolStripMenuItem.Visible = false;
                renameToolStripMenuItem.Visible = false;
            }
        }

        private void leftArrowButton_Click(object sender, EventArgs e)
        {
            Methods.PathRemove();

            IndexOfVisitedDirectories--;
            CurrentDirectoryLevel--;

            loadFiles();
            load_DataGridView();
        }

        private void rightArrowButton_Click(object sender, EventArgs e)
        {
            IndexOfVisitedDirectories++;
            CurrentDirectoryLevel++;

            Methods.PathAdd(VisitedDirectories[IndexOfVisitedDirectories]);

            loadFiles();
            load_DataGridView();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenItem(AllFiles[Int32.Parse(getData(0))][1]);
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //dataGridView1.Rows[dataGridView1.SelectedRows[0].Index].Cells[2].ReadOnly = false;
            //dataGridView1.CurrentCell = dataGridView1.Rows[dataGridView1.SelectedRows[0].Index].Cells[2];
            //dataGridView1.BeginEdit(true);

            TextBoxInput input = new TextBoxInput();
            TextBoxInput.FormTitle = "Rename " + (AllFiles[Int32.Parse(getData(0))][0] == "0" ? "folder" : "file");

            if (AllFiles[Int32.Parse(getData(0))][0] == "0") {
                TextBoxInput.TextBoxText = Methods.getDirectoryAttribute(AllFiles[Int32.Parse(getData(0))][1], "name", true);
            } else if (AllFiles[Int32.Parse(getData(0))][0] == "1") {
                TextBoxInput.TextBoxText = Methods.getFileAttribute(AllFiles[Int32.Parse(getData(0))][1], "name", true);
            }

            input.ShowDialog();

            if (!string.IsNullOrEmpty(TextBoxInput.TextBoxText))
            {
                if (AllFiles[Int32.Parse(getData(0))][0] == "0")
                {
                    Methods.changeDirectoryAttribute(AllFiles[Int32.Parse(getData(0))][1], "name", TextBoxInput.TextBoxText);
                }
                else if (AllFiles[Int32.Parse(getData(0))][0] == "1")
                {
                    Methods.changeFileAttribute(AllFiles[Int32.Parse(getData(0))][1], "extension", Methods.getExtension(TextBoxInput.TextBoxText));
                    Methods.changeFileAttribute(AllFiles[Int32.Parse(getData(0))][1], "name", TextBoxInput.TextBoxText);
                }

                document.Save(BasePath);

                loadFiles();
                load_DataGridView();
            }
        }

        private void searchTextBox_TextChanged_1(object sender, EventArgs e)
        {
            searchItems(searchTextBox.Text);
        }

        private void folderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextBoxInput input = new TextBoxInput();
            TextBoxInput.FormTitle = "New Folder";
            TextBoxInput.TextBoxText = "";

            input.ShowDialog();

            if (!string.IsNullOrEmpty(TextBoxInput.TextBoxText)) {
                XmlElement directory = Methods.getDirectory(TextBoxInput.TextBoxText);

                document.SelectSingleNode(Methods.getPath(Path)).AppendChild(directory);
                document.Save(BasePath);

                SelectedItems[CurrentDirectoryLevel] = Methods.getPath(Path) + "/directory[@name='" + Methods.stringToHex(TextBoxInput.TextBoxText) + "']";

                loadFiles();
                load_DataGridView();
            }
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TextBoxInput input = new TextBoxInput();
            TextBoxInput.FormTitle = "New File";
            TextBoxInput.TextBoxText = "";

            input.ShowDialog();

            if (!string.IsNullOrEmpty(TextBoxInput.TextBoxText))
            {
                XmlElement file = Methods.getFile(TextBoxInput.TextBoxText);

                document.SelectSingleNode(Methods.getPath(Path)).AppendChild(file);
                document.Save(BasePath);

                SelectedItems[CurrentDirectoryLevel] = Methods.getPath(Path) + "/file[@name='" + Methods.stringToHex(TextBoxInput.TextBoxText) + "']";

                loadFiles();
                load_DataGridView();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            document.SelectSingleNode(Methods.getPath(Path)).RemoveChild(document.SelectSingleNode(AllFiles[Int32.Parse(getData(0))][1]));

            document.Save(BasePath);

            loadFiles();
            load_DataGridView();
        }
    }
}
