using System;
using System.IO;
using System.Linq;

namespace CMKZ {
    public enum FileSortType {
        FileName,
        CreationTime,
        LastWriteTime,
        Size
    }
    public static partial class LocalStorage {
        public static long FileSize(string X) {
            var A = new FileInfo(X);
            return A.Exists ? A.Length : -1;
        }
        public static string[] SortFile(this CMKZList<string> fileNames) {
            return fileNames.Select(fileName => (fileName, File.GetCreationTime(fileName))).OrderBy(file => file.Item2).Select(file => file.Item1).ToArray();
        }
        public static FileSystemInfo[] SortByCreateTime(this FileSystemInfo[] X) {
            Array.Sort(X, (x, y) => y.CreationTime.CompareTo(x.CreationTime));
            return X;
        }
        public static CMKZList<string> 文件检索(string 文件夹名, string 内容) {
            var A = new CMKZList<string>();
            foreach (var i in GetFileList(文件夹名)) {
                foreach (var j in File.ReadAllLines(i)) {
                    if (j.Contains(内容)) {
                        A.Add(i + "：" + j);
                    }
                }
            }
            return A;
        }
        public static string TryFileRead(string X, string Y) {
            if (!FileExists(X)) {
                FileWrite(X, Y);
            }
            return FileRead(X);
        }
        public static T TryFileRead<T>(string X, T Y, bool 加密 = false, bool 全保存 = true) {
            if (!FileExists(X)) {
                FileWrite(X, Y, 加密, 全保存);
            }
            return FileRead<T>(X, 加密, 全保存);
        }
        public static string FileRead(string X) {
            X = X.绝对路径();
            if (File.Exists(X)) {
                return File.ReadAllText(X).Replace("\r", "");
            } else {
                PrintWarning("Read的文件不存在：" + X);
                return "";
            }
        }
        public static T FileRead<T>(string X, bool 加密 = false, bool 全保存 = true) {
            X = X.绝对路径();
            if (File.Exists(X)) {
                if (!加密) {
                    return File.ReadAllText(X).JsonToCS<T>(全保存, 全保存);
                }
                return File.ReadAllBytes(X).Decrypt().BytesToString().JsonToCS<T>(全保存, 全保存);
            } else {
                PrintWarning("Read的文件不存在：" + X);
                return default;
            }
        }
        public static bool FileExists(string X) {
            X = X.绝对路径();
            return File.Exists(X);
        }
    }
}