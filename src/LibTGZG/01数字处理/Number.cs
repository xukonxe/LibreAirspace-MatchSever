using System;
using System.Collections.Generic;

namespace CMKZ {
    public static partial class LocalStorage {
        public static double Sum(this IEnumerable<Number> X) {
            var A = 0.0;
            foreach (var B in X) {
                A += B;
            }
            return A;
        }
    }
    public struct Number : INumber {
        private double Data;
        //重载加减乘除 大于小于等于不等 隐式到int flota
        public static Number operator +(Number X, Number Y) => new() { Data = X.Data + Y.Data };
        public static Number operator -(Number X, Number Y) => new() { Data = X.Data - Y.Data };
        public static Number operator *(Number X, Number Y) => new() { Data = X.Data * Y.Data };
        public static Number operator /(Number X, Number Y) => new() { Data = X.Data / Y.Data };
        public static bool operator >(Number X, Number Y) => X.Data > Y.Data;
        public static bool operator <(Number X, Number Y) => X.Data < Y.Data;
        public static bool operator >=(Number X, Number Y) => X.Data >= Y.Data;
        public static bool operator <=(Number X, Number Y) => X.Data <= Y.Data;
        public static bool operator ==(Number X, Number Y) => X.Data == Y.Data;
        public static bool operator !=(Number X, Number Y) => X.Data != Y.Data;
        public static implicit operator int(Number X) => (int)X.Data;
        public static implicit operator float(Number X) => (float)X.Data;
        public static implicit operator double(Number X) => X.Data;
        public static implicit operator int(Number? X) => (int)X.Value.Data;
        public static implicit operator float(Number? X) => (float)X.Value.Data;
        public static implicit operator double(Number? X) => X.Value.Data;
        public static implicit operator string(Number X) => X.ToString();
        public static implicit operator Number(int X) => new() { Data = X };
        public static implicit operator Number(int? X) => new() { Data = (int)X };
        public static implicit operator Number(float X) => new() { Data = X };
        public static implicit operator Number(float? X) => new() { Data = (float)X };
        public static implicit operator Number(double X) => new() { Data = X };
        public static implicit operator Number(double? X) => new() { Data = (double)X };
        public static implicit operator Number(string X) => new() { Data = double.Parse(X) };
        public static implicit operator Number?(int X) => new() { Data = X };
        public static implicit operator Number?(int? X) => new() { Data = (int)X };
        public static implicit operator Number?(float X) => new() { Data = X };
        public static implicit operator Number?(float? X) => new() { Data = (float)X };
        public static implicit operator Number?(double X) => new() { Data = X };
        public static implicit operator Number?(double? X) => new() { Data = (double)X };
        public static implicit operator Number?(string X) => new() { Data = double.Parse(X) };
        public override bool Equals(object obj) {
            return obj is Number number && Data == number.Data;
        }
        public override int GetHashCode() {
            return Data.GetHashCode();
        }
        public static bool 限一 = false;//最小为1
        public static NumberMode 模式;
        public override string ToString() {
            var A = 限一 ? Math.Max(Data, 1) : Data;
            return 模式 switch {
                NumberMode.两位小数 => A.两位小数(),
                NumberMode.向上取整 => A.取整().ToString(),
                NumberMode.向下取整 => A.向下取整().ToString(),
                NumberMode.万亿两位 => A.To万单位(),
                NumberMode.万亿上整 => A.To万单位(false),
                NumberMode.万亿下整 => A.To万单位(false),
                NumberMode.两位有效 => A.X位有效(),
                NumberMode.三位有效 => A.X位有效(3),
                NumberMode.万亿三位 => A.万亿三位(),
                _ => A.ToString(),
            };
        }
        public bool IsZero() => Data == 0;
        public Number 乘方(double X) {
            return new() { Data = Math.Pow(Data, X) };
        }
    }
    public enum NumberMode {
        两位小数,
        两位有效,
        三位有效,
        向上取整,
        向下取整,
        万亿两位,
        万亿三位,
        万亿上整,
        万亿下整,
    }
}