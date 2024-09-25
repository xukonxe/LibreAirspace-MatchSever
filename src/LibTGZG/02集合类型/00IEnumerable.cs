using System;
using System.Collections.Generic;
using System.Linq;

namespace CMKZ {
    //IEnumerable
    public static partial class LocalStorage {
        public static T Choice<T>(this IEnumerable<T> X) {
            var A = X.ToList_CMKZ();
            if (A.Count == 0) {
                PrintWarning("注意：正在对空集Choice");
                return default;
            }
            return A[Random(0, A.Count)];
        }
        public static T ChoiceWithOut<T>(this IEnumerable<T> X,T Y) {
            var A = X.ToList_CMKZ();
            A.Remove(Y);
            if (A.Count == 0) {
                PrintWarning("注意：正在对空集Choice");
                return default;
            }
            return A[Random(0, A.Count)];
        }
        public static T First<T>(this IEnumerable<T> X) {
            foreach (var i in X) {
                return i;
            }
            PrintWarning("注意：正在对空集First");
            return default;
            //throw new Exception("错误：空集不允许First");
        }
        public static T Find<T>(this IEnumerable<T> X, Func<T, bool> Y) {
            foreach (var i in X) {
                if (Y(i)) {
                    return i;
                }
            }
            if (!X.Any())  PrintWarning("注意：正在对空集Find"); 
            return default;
            //throw new Exception($"错误：找不到指定目标。目前集合：{X.JsonSerialize()}");
        }
        public static int FindIndex<T>(this IEnumerable<T> X, Func<T, bool> Y) {
            var A = 0;
            foreach (var i in X) {
                if (Y(i)) {
                    return A;
                }
                A++;
            }
            return -1;
        }
        public static bool Contains<T>(this IEnumerable<T> X, Func<T, bool> Y) {
            foreach (var i in X) {
                if (Y(i)) {
                    return true;
                }
            }
            return false;
        }
        public static void ForEach<T>(this IEnumerable<T> X, Action<T> Y) {
            foreach(var i in X) Y(i);
        }
        public static CMKZList<T> ToList_CMKZ<T>(this IEnumerable<T> X) {
            var A = new CMKZList<T>();
            foreach (var i in X) {
                A.Add(i);
            }
            return A;
        }
    }
    //Array
    public static partial class LocalStorage {
        public static T[] RemoveAt<T>(this T[] X, int Y) => null;
        public static T[] RemoveAll<T>(this T[] X, Func<T, bool> Y) {
            var index = 0;
            var result = new T[X.Length];
            foreach (var item in X) {
                if (!Y(item)) {
                    result[index++] = item;
                }
            }
            Array.Resize(ref result, index);
            return result;
        }
        public static T[] RemoveFirst<T>(this T[] X) {
            var A = new T[X.Length - 1];
            for (var i = 0; i < X.Length - 1; i++) {
                A[i] = X[i + 1];
            }
            return A;
        }
        public static T2[] ParseTo<T1,T2>(this T1[] X, Func<T1, T2> Y) {
            var A = new T2[X.Length];
            for (var i = 0; i < X.Length; i++) A[i] = Y(X[i]);
            return A;
        }
        public static T[] 克隆<T>(this T[] X) {
            var A = new T[X.Length];
            for (var i = 0; i < X.Length; i++) {
                A[i] = X[i];
            }
            return A;
        }
        public static T Next<T>(this IEnumerable<T> X, T Y) {
            return X.SkipWhile(x => !x.Equals(Y)).Skip(1).FirstOrDefault();
        }
    }
    //其他
    public static partial class LocalStorage {
        public static string[][] Split(this string[] X, string Y) {
            var A = new string[X.Length][];
            for (var i = 0; i < X.Length; i++) {
                A[i] = X[i].Split(Y);
            }
            return A;
        }
        //public static List<T> 获取分页<T>(this List<T> X, int Y) {
        //    if (Y < 1) {
        //        return new List<T>();
        //    }
        //    if (Y > (int)Math.Ceiling((double)X.Count / 100)) {
        //        return new List<T>();
        //    }
        //    var startIndex = (Y - 1) * 100;
        //    var endIndex = startIndex + 99;
        //    if (endIndex >= X.Count) {
        //        endIndex = X.Count - 1;
        //    }
        //    return X.GetRange(startIndex, endIndex - startIndex + 1);
        //}
    }
}