using System;
using System.Collections;
using System.Collections.Generic;
using static CMKZ.LocalStorage;

namespace CMKZ {
    public class Grid<T> : IEnumerable<T> where T : new() {
        public T[,] Data;
        public int Width;
        public int Height;
        public Grid(int X, int Y) {
            Init(X, Y);
        }
        public Grid<T> Init(int X, int Y) {
            Width = X;
            Height = Y;
            Data = new T[X, Y];
            Fill((i, j) => new T());
            return this;
        }
        public T this[int X] {
            get => this[X / Height, X % Height]; 
            set => this[X / Height, X % Height] = value;
        }
        public T this[int X, int Y] {
            get {
                if (X < 0 || X >= Width || Y < 0 || Y >= Height) {
                    throw new Exception($"坐标超出范围 {X},{Y}。范围：{Width},{Height}");
                }
                return Data[X, Y];
            }
            set {
                if (X < 0 || X >= Width || Y < 0 || Y >= Height) {
                    throw new Exception($"坐标超出范围 {X},{Y}。范围：{Width},{Height}");
                }
                Data[X, Y] = value;
            }
        }
        public void Fill(Func<int, int, T> X) {
            for (var i = 0; i < Width; i++) {
                for (var j = 0; j < Height; j++) {
                    Data[i, j] = X(i, j);
                }
            }
        }
        public void FillRandom(Func<int, int, T> X, int Y) { //在随机Y个位置执行X
            var A = new CMKZList<int>();
            for (var i = 0; i < Width; i++) {
                for (var j = 0; j < Height; j++) {
                    A.Add(i * Height + j);
                }
            }
            for (var i = 0; i < Y; i++) {
                var B = Random(0, A.Count);
                var C = A[B] / Height;
                var D = A[B] % Height;
                Data[C, D] = X(C, D);
                A.RemoveAt(B);
            }
        }
        public void ForEach(Action<T> X) => ForEach((i, j, k) => X(k));
        public void ForEach(Action<int, int, T> X) {
            for (var i = 0; i < Width; i++) {
                for (var j = 0; j < Height; j++) {
                    X(i, j, Data[i, j]);
                }
            }
        }
        public CMKZ_Dictionary<坐标类, T> GetRound(int X, int Y) {
            var A = new CMKZ_Dictionary<坐标类, T>();
            for (var i = -1; i <= 1; i++) {
                for (var j = -1; j <= 1; j++) {
                    if (i == 0 && j == 0) {
                        continue;
                    }
                    var B = X + i;
                    var C = Y + j;
                    if (B >= 0 && B < Width && C >= 0 && C < Height) {
                        A.Add(new 坐标类(B, C), Data[B, C]);
                    }
                }
            }
            return A;
        }
        public Number SumRound(int X, int Y, Func<T, Number> Z) {
            var A = new Number();
            foreach (var i in GetRound(X, Y).Values) {
                A += Z(i);
            }
            return A;
        }
        public IEnumerator<T> GetEnumerator() {
            for (var i = 0; i < Width; i++) {
                for (var j = 0; j < Height; j++) {
                    yield return Data[i, j];
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
    public class 坐标类 {
        public int X;
        public int Y;
        public 坐标类(int A, int B) {
            X = A;
            Y = B;
        }
    }
}