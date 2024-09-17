using Microsoft.CodeAnalysis;
using Newtonsoft.Json;//Json
using System;//Action
using System.Collections;
using System.Collections.Generic;//List
using System.Diagnostics;
using System.IO;//File
using System.Linq;//from XX select XX
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;//Timer
using MathNet.Numerics.Distributions;

namespace CMKZ {
    public static partial class LocalStorage {
        public static readonly Action EmptyAction = () => { };
        public static int StringWidth(this string input) {
            var A = input.Split('\n');
            var B = A.Select(i => i.StringWidthLine());
            return B.Max();
        }
        public static int StringWidthLine(this string input) {
            int width = 0;
            foreach (char c in input) {
                if (IsChinese(c)) {
                    width += 2; // 中文字符算两个宽度
                } else {
                    width += 1; // 其他字符算一个宽度
                }
            }
            return width;
        }
        private static bool IsChinese(char c) {
            return Regex.IsMatch(c.ToString(), @"[\u3000-\u303F\u4E00-\u9FFF\uFF00-\uFFEF]");
        }
        private static Gamma gamma = new(6, 5, new Random(42));
        public static Number 获取下一个随机数() {
            return Math.Round(gamma.Sample(), 2);
        }
        public static void GamaDemo() {
            var A = 生成随机数并统计区间数量(10000, 0, 300, 0.1);
            foreach (var i in A) {
                Print($"[{i.Key.两位小数()}, {(i.Key + 0.1).两位小数()}]: {i.Value}");
            }
        }
        public static CMKZ_Dictionary<double, int> 生成随机数并统计区间数量(int 随机数个数, double 左端点, double 右端点, double 步进) {
            var 统计 = new CMKZ_Dictionary<double, int>();
            // 初始化统计
            for (double i = 左端点; i < 右端点; i += 步进) {
                统计[i] = 0;
            }
            // 生成随机数并统计
            for (int i = 0; i < 随机数个数; i++) {
                var 随机数 = gamma.Sample();
                foreach (var 区间 in 统计) {
                    if (区间.Key <= 随机数 && 随机数 < 区间.Key + 步进) {
                        统计[区间.Key]++;
                        break;
                    }
                }
            }
            return 统计;
        }
        public static T Choice<T>(params T[] X) {
            return X[Random(0, X.Length)];
        }
        public static string 数字变化(this string X,Func<Number,Number> Y) {
            //从X中正则提取出数字，然后用Y变化，再替换回去
            return Regex.Replace(X, @"\d+", i => Y(i.Value.ToInt()).ToString());
        }
    }
    public class 编号类 {
        public static uint? 当前编号;
        public uint 获取编号() => (uint)当前编号++;
        public uint 获取材料编号() => (uint)当前编号--;
        public 编号类(uint 初始值) {
            当前编号 = 初始值;
        }
        public 编号类() {
            if (当前编号 == null) throw new Exception("编号类未初始化");
            当前编号 -= 0x10;
        }
    }
}