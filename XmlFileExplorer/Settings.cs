using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace XmlFileExplorer
{
    class Settings
    {
        //Base Path
        public static readonly string OriginalPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\FolderInOneFile 2019\RecentFiles.xml";

        //DateTime Format
        public static string DateTime_Format = "dd.MM.yyyy HH:mm";

        //Form Theme
        public static Color Form_BackColor = ColorTranslator.FromHtml("#272728");

        //Explorer Form Theme
        public static Color ExplorerForm_BackColor = ColorTranslator.FromHtml("#232323");
        public static Color ExplorerForm_BackColorDark = ColorTranslator.FromHtml("#1E1E1E");

        //DataGridView Theme
        public static Color DataGridView_BackColor = ColorTranslator.FromHtml("#3E3E3F");
        public static Color DataGridView_ForeColor = ColorTranslator.FromHtml("#E5E5E5");
        public static Color DataGridView_RowSelectionColor = Color.Gray;

        //Button Theme
        public static Color Button_BackColor = ColorTranslator.FromHtml("#4F4F51");
        public static Color Button_ForeColor = Color.White;
        public static Color Button_BorderColor = Color.LightGray;

        //Label Theme
        public static Color Label_ForeColor = Color.White;


        //All directory attributes
        public static readonly string[] Directory_HeadAttributes = new string[] {"name", "size", "modifdate"};

        //All file attributes
        public static readonly string[] File_HeadAttributes = new string[] { "name"};
        public static readonly string[] File_SubAttributes = new string[] { "extension", "text", "modifdate", "size"};

        //Common extensions
        public static Dictionary<string, string> CommonExtensions = new Dictionary<string, string>();
    }
}
