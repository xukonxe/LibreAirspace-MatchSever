using System.Text.RegularExpressions;

namespace CMKZ {
    public static partial class LocalStorage {
        public static string RemoveEmptyLines(string input) => Regex.Replace(input, @"^\s*(?:\r\n?|\n)", "", RegexOptions.Multiline);// 让^表示行的开始
        public static string 注释清除(this string X) {
            X = X.不可见清除();
            X = Regex.Replace(X, @"^\s*\n", string.Empty, RegexOptions.Multiline);//删除空行
            X = Regex.Replace(X, @"\/\/.*?$|/\*[\w\W\n]*?\*/", string.Empty, RegexOptions.Multiline);//删除注释
            X = Regex.Replace(X, @" +", " ");
            X = Regex.Replace(X, @"^\s", string.Empty);
            X = Regex.Replace(X, @"\s$", string.Empty);
            return X;
        }
        public static string 不可见清除(this string X) {
            return Regex.Replace(X, @"[\u200B\r]", string.Empty);
        }
        public static CMKZ_Dictionary<string, float> ToDictionary(this string X) {
            X = X.Replace("*", "+");
            var A = X.Replace("-", "+-").Split(" ");
            var B = new CMKZ_Dictionary<string, float>();
            foreach (var i in A) {
                var C = i.Split("+");
                B[C[0]] = C[1].ToFloat();
            }
            return B;
        }
    }
}