using System;
using System.IO;
using System.Text.RegularExpressions;

namespace CMKZ {
    public static partial class LocalStorage {
        public static T FileWrite<T>(string path, T Y, bool 加密 = false, bool 全保存 = true) {
            path = path.绝对路径();
            var A = Path.GetDirectoryName(path);
            if (!Directory.Exists(A)) Directory.CreateDirectory(A);
            if (Y is string B) {
                File.WriteAllText(path, B);
            } else if (加密) {
                File.WriteAllBytes(path, Y.ToJson(全保存, 全保存).StringToBytes().Encrypt());
            } else {
                File.WriteAllText(path, Y.ToJson(全保存, 全保存));
            }
            return Y;
        }
        public static void FileAppend(string X, string Y) {
            FileWrite(X, FileRead(X) + Y);
        }
        public static void TextAppend(string URL, string X) => FileWrite(URL, FileRead(URL) + X + "\n");
        public static void FileRemove(string X) {
            X = X.绝对路径();
            if (File.Exists(X)) {
                File.Delete(X);
            }
        }
        public static void FileRename(string X, string Y) {
            X = X.绝对路径();
            Y = Y.绝对路径();
            if (File.Exists(X)) {
                File.Move(X, Y);
            }
        }
        public static StreamWriter GetWriter(string X) {
            X = X.绝对路径();
            var A = Path.GetDirectoryName(X);
            if (!Directory.Exists(A)) Directory.CreateDirectory(A);
            var B = File.AppendText(X);
            B.AutoFlush = true;
            return B;
        }
        public static string[] GetFiles(string X) {
            ShouldDirExist(X);
            return Directory.GetFiles(X.绝对路径());
        }
        public static void ShouldDirExist(string X) {
            X = X.绝对路径();
            if (!Directory.Exists(X)) Directory.CreateDirectory(X);
        }
        public static void 修改工具(string 文件名) {
            //首先，对日期批量替换
            //然后，删除空行
            //然后，引用内容替换

            //读取桌面中的此文件
            Print(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + 文件名 + ".txt");
            var A = FileRead(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + 文件名 + ".txt");
            A = Regex.Replace(A, @"(忘忧草 我父母删了我之前的qq) (.*)\r\n", "[$2]\n$1: ");
            A = Regex.Replace(A, @"(执悲丶今厄) (.*)\r\n", "[$2]\n$1: ");
            A = Regex.Replace(A, @"\r\n\r\n", "\n");
            A = Regex.Replace(A, @"\[ \]\n", "引用: ");
            FileWrite(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + 文件名 + ".txt", A);
        }
    }
}