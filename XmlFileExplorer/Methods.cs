using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Xml;
using System.IO;

namespace XmlFileExplorer
{
    class Methods
    {
        public static void changeFileAttribute(string path, string attribute, string newValue) {
            int type = typeOfFileAttribute(attribute);

            //if head attribute
            if (type == 0) {
                Explorer.document.SelectSingleNode(path).Attributes[attribute].Value = Methods.stringToHex(newValue);
            }

            //if sub attribute
            if (type == 1) {
                Explorer.document.SelectSingleNode(path + "/" + attribute).InnerText = newValue;
            }
        }

        public static void changeDirectoryAttribute(string path, string attribute, string newValue) {
            Explorer.document.SelectSingleNode(path).Attributes[attribute].Value = Methods.stringToHex(newValue);
        }

        public static int typeOfFileAttribute(string attribute) {
            int type = -1;
            //check if head attribute
            for (int i = 0; i < Settings.File_HeadAttributes.Length; i++){
                if (Settings.File_HeadAttributes[i].Equals(attribute)){
                    type = 0;
                }
            }

            //check if sub attribute
            if (type == -1){
                for (int i = 0; i < Settings.File_SubAttributes.Length; i++){
                    if (Settings.File_SubAttributes[i].Equals(attribute)){
                        type = 1;
                    }
                }
            }

            return type;
        }

        public static XmlElement getFile(string name){
            XmlElement file = Explorer.document.CreateElement("file");

            file.SetAttribute("name", Methods.stringToHex(name));

            XmlElement extensionAtt = Explorer.document.CreateElement("extension");
            extensionAtt.InnerText = Methods.getExtension(name);
            file.AppendChild(extensionAtt);

            XmlElement textAtt = Explorer.document.CreateElement("text");
            textAtt.InnerText = "";
            file.AppendChild(textAtt);

            XmlElement modifdateAtt = Explorer.document.CreateElement("modifdate");
            modifdateAtt.InnerText = DateTime.Now.ToFileTime().ToString();
            file.AppendChild(modifdateAtt);

            XmlElement sizeAtt = Explorer.document.CreateElement("size");
            sizeAtt.InnerText = "0";
            file.AppendChild(sizeAtt);

            return file;
        }

        public static XmlElement getDirectory(string name) {
            XmlElement directory = Explorer.document.CreateElement("directory");

            directory.SetAttribute("name", Methods.stringToHex(name));
            directory.SetAttribute("modifdate", Methods.stringToHex(DateTime.Now.ToFileTime().ToString()));
            directory.SetAttribute("size", Methods.stringToHex("0"));

            return directory;
        }

        public static void PathAdd(string name)
        {
            string[] tempPath = new string[Explorer.Path.Length + 1];

            for (int i = 0; i < Explorer.Path.Length; i++)
            {
                tempPath[i] = Explorer.Path[i];
            }

            tempPath[tempPath.Length - 1] = name;
            Explorer.Path = tempPath;
        }

        public static void PathRemove()
        {
            if (Explorer.Path.Length > 0)
            {
                string[] tempPath = new string[Explorer.Path.Length - 1];

                for (int i = 0; i < tempPath.Length; i++)
                {
                    tempPath[i] = Explorer.Path[i];
                }

                Explorer.Path = tempPath;
            }
        }

        public static string getDateTimeShow(string fileTime) {
            return DateTime.FromFileTime(Int64.Parse(fileTime)).ToString(Settings.DateTime_Format);
        }

        public static string getPathTextBox() {
            string text = "";

            for (int i = 0; i < Explorer.Path.Length; i++) {
                text += Methods.hexToString(Explorer.Path[i]) + ((i == Explorer.Path.Length - 1) ? "" : " > ");
            }

            return text;
        }

        public static string getShowSize(string sizeInBytes) {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = (double)Int32.Parse(sizeInBytes);
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            return String.Format("{0:0.##} {1}", len, sizes[order]);
        }

        public static string getShowExtension(string extension) {
            string text = "";

            text = extension.ToUpper();

            if(string.IsNullOrEmpty(text)){
                text = "File";
            }else {
                if (Settings.CommonExtensions.ContainsKey(text)){
                    text = Settings.CommonExtensions[text] + " File";
                }else {
                    text += " File";
                }
            }

            return text;
        }

        public static string getExtension(string name) {
            string extension = "";
            string[] array = name.Split('.');

            if (array.Length > 1)
            {
                extension = array[1].ToUpper();
            }

            return extension;
        }

        public static string stringToHex(string text) {
            string newText = "";

            byte[] ba = Encoding.Default.GetBytes(text);
            newText = BitConverter.ToString(ba);

            return newText;
        }

        public static string hexToString(string text) {
            string newText = "";

            text = text.Replace("-", "");
            byte[] raw = new byte[text.Length / 2];
            for (int i = 0; i < raw.Length; i++)
            {
                raw[i] = Convert.ToByte(text.Substring(i * 2, 2), 16);
            }

            newText = Encoding.ASCII.GetString(raw);

            return newText;
        }

        public static string getPath(string[] pathList) {
            string path = "/root";

            for (int i = 0; i < pathList.Length; i++){
                path += "/directory[@name='" + pathList[i] + "']";
            }

            return path;
        }

        public static string getDirectoryAttribute(string path, string attribute, bool convertFromHex) {
            string value = "";

            value = Explorer.document.SelectSingleNode(path).Attributes[attribute].Value;

            if (convertFromHex) {
                value = Methods.hexToString(value);
            }

            return value;
        }

        public static string getFileAttribute(string path, string attribute, bool convertFromHex) {
            string value = "";

            int type = typeOfFileAttribute(attribute);

            //get head attribute
            if (type == 0) {
                value = Explorer.document.SelectSingleNode(path).Attributes[attribute].Value;

                if (convertFromHex){
                    value = Methods.hexToString(value);
                }
            }

            //get sub attribute
            if (type == 1) {
                value = Explorer.document.SelectSingleNode(path + "/" + attribute).InnerText;
            }

            return value;
        }

        public static void setCommonFileExtensions() {
            XmlDocument extensionDocument=  new XmlDocument();
            extensionDocument.LoadXml(Properties.Resources.CommonExtensions);

            XmlNodeList allExtensionNodes = extensionDocument.SelectNodes("/root/file");
            for (int i = 0; i < allExtensionNodes.Count; i++) {
                Settings.CommonExtensions.Add(
                    allExtensionNodes[i].Attributes["extension"].Value,
                    allExtensionNodes[i].Attributes["description"].Value
                );
            }
        }
    }
}
