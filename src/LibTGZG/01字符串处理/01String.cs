using System;
using System.Collections.Generic;
using System.Linq;

namespace CMKZ {
    //String
    public static partial class LocalStorage {
        public static string Join(this string[] X, string Y = ",") => string.Join(Y, X);
        public static string ToHex(this string input) => input.Aggregate(string.Empty, (current, c) => current + ((int)c).ToString("X2"));
        public static string Remove(this string X, string Y) => X.Replace(Y, "");
        public static string TrimStart(this string X, string Y) {
            if (X.StartsWith(Y)) {
                X = X[Y.Length..];
            }
            return X;
        }
        public static string 省略(this string input) {
            if (input.Length > 100) {
                var firstFiveChars = input[..50];
                var lastFiveChars = input.Substring(input.Length - 50, 50);
                return $"{firstFiveChars}...{lastFiveChars}";
            } else {
                return input;
            }
        }
        public static string Join(this int[] X, char Y) {
            return X.ToString(t => t.ToString(), Y);
        }
        public static string Join(this IEnumerable<string> X, char Y) {
            return string.Join(Y, X);
        }
        public static string Join(this IEnumerable<string> X, string Y) {
            return string.Join(Y, X);
        }
        public static string ToString<T>(this IEnumerable<T> X, Func<T, string> Y, char Z = ' ') {
            var A = "";
            foreach (var B in X) A += Y(B) + Z;
            return A.TrimEnd(Z);
        }
        public static string ToString<T>(this IEnumerable<T> X, Func<int,T, string> Y, char Z = ' ') {
            var A = "";
            int i = 0;
            foreach (var B in X) {
                A += Y(i, B) + Z;
                i++;
            }
            return A.TrimEnd(Z);
        }
        public static bool IsNullOrEmpty(this string str) {
            return string.IsNullOrEmpty(str);
        }
    }
}