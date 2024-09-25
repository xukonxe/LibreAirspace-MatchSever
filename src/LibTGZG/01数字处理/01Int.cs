using System;

namespace CMKZ {
    public interface INumber {
        public bool IsZero();
    }
    public static partial class LocalStorage {
        public static string To16进制(this uint A) {
            return A.ToString("X");
        }
        public static bool IsZero<T>(this T X) {
            if (X == null) {
                return false;
            }
            Type A = typeof(T);
            if (A.IsGenericType && A.GetGenericTypeDefinition() == typeof(Nullable<>)) {
                if (X.Equals(null)) {
                    return false;
                }
                A = Nullable.GetUnderlyingType(A);
            }
            if (A == typeof(byte) ||
                A == typeof(sbyte) ||
                A == typeof(short) ||
                A == typeof(ushort) ||
                A == typeof(int) ||
                A == typeof(uint) ||
                A == typeof(long) ||
                A == typeof(ulong) ||
                A == typeof(float) ||
                A == typeof(double) ||
                A == typeof(decimal)) {
                return Convert.ToDouble(X) == 0;
            }
            if (X is INumber number) {
                return number.IsZero();
            }
            if(X is string B) {
                return B is "" or "0";
            }
            return false;
        }
        public static int[] ToIntList(this string X) => X.Split(" ").ParseTo(t => t.ToInt());
        public static bool IsInt(this string X) => int.TryParse(X, out _);
        public static void Add(this int[] X, int[] Y) {
            for (var i = 0; i < X.Length; i++) {
                X[i] += Y[i];
            }
        }
        public static int[] Multiply(this int[] X, int[] Y) {
            for (var i = 0; i < X.Length; i++) {
                X[i] *= Y[i];
            }
            return X;
        }
        public static int[] AddToNew(this int[] X, int[] Y) {
            var A = new int[X.Length];
            for (var i = 0; i < X.Length; i++) {
                A[i] = X[i] + Y[i];
            }
            return A;
        }
        public static double[] AddToNew(this double[] X, double[] Y) {
            var A = new double[X.Length];
            for (var i = 0; i < X.Length; i++) {
                A[i] = X[i] + Y[i];
            }
            return A;
        }
        public static bool BiggerThan(this int[] X, int[] Y) {
            for (var i = 0; i < X.Length; i++) {
                if (X[i] < Y[i]) {
                    return false;
                }
            }
            return true;
        }
        public static bool NotBiggerThan(this int[] X, int[] Y) {
            for (var i = 0; i < X.Length; i++) {
                if (X[i] > Y[i]) {
                    return false;
                }
            }
            return true;
        }
        public static bool OneBiggerThan(this int[] X, int[] Y) {
            for (var i = 0; i < X.Length; i++) {
                if (X[i] > Y[i]) {
                    return true;
                }
            }
            return false;
        }
        public static bool OneBiggerThan(this double[] X, double[] Y) {
            for (var i = 0; i < X.Length; i++) {
                if (X[i] > Y[i]) {
                    return true;
                }
            }
            return false;
        }
        public static int IntAdd(this string X) => X == "" ? 1 : int.Parse(X) + 1;
        public static int ToInt(this string X) {
            X = X.Replace("_", "");
            //去掉小数点之后的部分
            if (X.Contains(".")) {
                X = X.Split(".")[0];
            }
            return int.Parse(X);
        }
        public static double ToDouble(this string X) {
            X = X.Replace("_", "");
            return double.Parse(X);
        }
        public static long ToLong(this string X) {
            X = X.Replace("_", "");
            return long.Parse(X);
        }
        public static float ToFloat(this double X) {
            return (float)X;
        }
        public static float 百分化(this float X) => X / (X + 100);
        public static double 百分化(this long X) => X / (X + 100d);
        public static string 一位小数(this float X) {
            if (X.ToString("0.0") == "0.0") return "0";
            return X.ToString("0.0").TrimEnd('0').TrimEnd('.');
        }
        public static string 一位小数(this double X) {
            if (X.ToString("0.0") == "0.0") return "0";
            return X.ToString("0.0").TrimEnd('0').TrimEnd('.');
        }
        public static string 整数(this float X) {
            return Math.Round(X, 0).ToString();
        }
        public static float ToFloat(this string X) {
            X = X.Replace("_", "");
            return float.Parse(X);
        }
        public static float Exp(this float X, float Y) {
            return (float)Math.Pow(X, Y);
        }
        public static double Pow(double X, int Y) {
            return X.乘方(Y);
        }
        /// <summary>
        /// 向上取整
        /// </summary>
        public static long 取整(this double X) {
            return (long)Math.Ceiling(X);
        }
        /// <summary>
        /// 向下取整
        /// </summary>
        public static long 向下取整(this double X) {
            return (long)Math.Floor(X);
        }
        public static string 两位小数(this double X) {
            if (X.ToString("0.00") == "0.00") return "0";
            return X.ToString("0.00").TrimEnd('0').TrimEnd('.');
        }
        public static string X位有效(this double X, int 位数 = 2) {
            if (X == 0) return "0";
            double scale = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(X))) - (位数 - 1));
            double roundedValue = Math.Round(X / scale) * scale;
            // 动态生成格式化字符串
            string formatString = "0." + new string('#', 位数 - 1);
            return roundedValue.ToString(formatString);
        }
        public static string 两位小数(this double X, bool 两位) {
            return 两位 ? X.两位小数() : X.取整().ToString();
        }
        public static string 万亿三位(this double X) {
            string A = X < 0 ? "-" : "";
            X = Math.Abs(X);
            if (X is < 10000) {
                A += X.X位有效(3);
            } else if (X is >= 10000 and < 10000_0000) {
                A += (X / 10000).X位有效(3) + "万";
            } else if (X is >= 10000_0000 and < 10000_0000_0000) {
                A += (X / 10000_0000).X位有效(3) + "亿";
            } else {
                A += (X / 10000_0000_0000).X位有效(3) + "万亿";
            }
            return A;
        }
        public static string To万单位(this double X, bool 两位 = true) {
            string A = X < 0 ? "-" : "";
            X = Math.Abs(X);
            if (X is >= 10000 and < 10000_0000) {
                A += (X / 10000).两位小数(两位) + "万";
            } else if (X is >= 10000_0000 and < 10000_0000_0000) {
                A += (X / 10000_0000).两位小数(两位) + "亿";
            } else if (X >= 10000_0000_0000) {
                A += (X / 10000_0000_0000).两位小数(两位) + "万亿";
            } else {
                A += X.两位小数(两位);
            }
            return A;
        }
        public static string To万单位(this long number) {
            return To万单位((double)number);
        }
        //从小数点左侧开始，每四位插入一个下划线
        public static string To下划线划分(this double X) {
            string A = X < 0 ? "-" : "";
            X = Math.Abs(X);
            string B = X.两位小数();
            string[] C = B.Split('.');
            string D = C[0];
            for (int i = D.Length - 4; i > 0; i -= 4) {
                D = D.Insert(i, "_");
            }
            return A + D + (C.Length == 2 ? "." + C[1] : "");
        }
        public static double 乘方(this double X, double Y) {
            return Math.Pow(X, Y);
        }
        public static double 乘方(this int X, double Y) {
            return Math.Pow(X, Y);
        }
        public static double 百分化(this double X) {
            if (X <= 0) {
                return 0;
            }
            return X / (X + 100);
        }
        /// <summary>
        /// Y为1表示X乘以50%到200%，Y为2表示X乘以33%到300%
        /// </summary>
        public static double 波动(this double X, double Y) {
            return X * Random(1 / (1 + Y), 1 + Y);
        }
        public static double 定值波动(this double X, double Y) {
            return X + Random(-Y, Y);
        }
    }
}