using System;//Action
using System.Linq;
using System.Security.Cryptography;
using static CMKZ.LocalStorage;

namespace CMKZ {
    public static partial class LocalStorage {
        public static int RandomSeed = 42;
        public static Random _Randomer;
        public static Random Randomer {
            get {
                if (_Randomer == null) {
                    _Randomer = new Random(RandomSeed);
                }
                return _Randomer;
            }
        }
        public static void SetRandomSeed(int X = 0) {
            if (X == 0) {
                X = DateTime.Now.Millisecond;
            }
            RandomSeed = X;
            _Randomer = new Random(RandomSeed);
        }
        public static int GetHash(int X) {
            //哈希运算：即混沌运算。把一个数变成另一个数。输入信息的一点微小变化，都会导致输出的巨大变化，变化的毫无规律、但又确切唯一。
            byte[] dataBytes = BitConverter.GetBytes(X);
            byte[] hashBytes = SHA256.Create().ComputeHash(dataBytes);
            return int.Parse(BitConverter.ToString(hashBytes).Replace("-", string.Empty).Substring(0, 8));
        }
        public static int RandomBySeed(this int 标准值, params int[] 种子) {
            //要求：【标准值、小种子、种子】确定唯一结果。结果接近标准值，接近程度与小种子线性无关、与种子线性无关、与标准值线性无关。
            int A = 0;
            foreach (var i in 种子) {
                A += GetHash(i);
            }
            return Math.Max(1, 标准值 * Math.Max(2, (A + GetHash(标准值)) % 100) / 20);
        }
        ///<summary>0到1的随机数</summary>
        public static float RandomFloat() {
            return Random(0.0f, 1.0f);
        }
        public static Guid RandomGuid() {
            return Guid.NewGuid();
        }
        public static bool Random(double X) {
            return Random() < X;
        }
        public static void Random(double X,Action Y) {
            if (Random(X)) {
                Y();
            }
        }
        public static float Random(float X, float Y) {
            return (float)(Randomer.NextDouble() * (Y - X) + X);
        }
        public static double Random() {
            return Randomer.NextDouble();
        }
        public static double Random(double X, double Y) {
            return Randomer.NextDouble() * (Y - X) + X;
        }
        /// <summary>
        /// 有左无右
        /// </summary>
        public static int Random(int X, int Y) {
            return Randomer.Next(X, Y);
        }
        public static int RightRandom(int X, int Y) {
            if (Y - X < 2) {
                PrintWarning("随机数差距小于2");
                return Y;
            }
            var A = Randomer.Next(X, Y);
            if (A < (Y + X) / 2) {
                A = RightRandom(X, Y);
            }
            return A;
        }
        public static T 权重开箱<T>(this CMKZ_Dictionary<T, double> X) {
            return new 权重箱子<T>(X).开箱();
        }
        public static T 权重开箱<T>(this System.Collections.Generic.Dictionary<T, double> X) {
            return new 权重箱子<T>(X).开箱();
        }
        public static T 权重开箱<T>(this CMKZList<T> X) where T : I权重 {
            return new 权重箱子<T>(X.ToDictionary(i => i, i => i.权重)).开箱();
        }
        public static T 期望开箱<T>(this CMKZ_Dictionary<T, double> X, double 期望) {
            var 期望箱子 = new 期望箱子<T>();
            foreach (var i in X) {
                期望箱子[i.Key] = i.Value;
            }
            期望箱子.期望 = 期望;
            return 期望箱子.开箱();
        }
        /// <summary>
        /// 左可取，右不可
        /// </summary>
        public static CMKZList<int> Range(int A, int B) {
            var C = new CMKZList<int>();
            for (int i = A; i < B; i++) {
                C.Add(i);
            }
            return C;
        }
    }
    public interface I权重 {
        public double 权重 { get; }
    }
    /// <summary>
    /// 权重和开出来的概率正相关。
    /// </summary>
    public class 权重箱子 : CMKZ_Dictionary<string, double> {
        public 权重箱子() { }
        public 权重箱子(CMKZ_Dictionary<string, double> X) {
            foreach (var i in X) {
                this[i.Key] = i.Value;
            }
        }
        //public void Add(I道具设定 X) {
        //    this[X.名称] = X.价格;
        //}
        public string 开箱() {
            if (Count == 0) throw new Exception("开箱错误");
            double A = Random(0, 1f) * Values.Sum();
            double B = 0f;
            foreach (var i in this) {
                B += i.Value;
                if (A < B) {
                    return i.Key;
                }
            }
            throw new Exception("开箱错误");
        }
    }
    public class 权重箱子<T> : CMKZ_Dictionary<T, double> {
        public 权重箱子() { }
        public 权重箱子(CMKZ_Dictionary<T, double> X) {
            foreach (var i in X) {
                this[i.Key] = i.Value;
            }
        }
        public T 开箱() {
            if (Count == 0) throw new Exception("开箱错误");
            double A = Random(0, 1f) * Values.Sum();
            double B = 0f;
            foreach (var i in this) {
                B += i.Value;
                if (A < B) {
                    return i.Key;
                }
            }
            throw new Exception("开箱错误");
        }
    }
    /// <summary>
    /// 有更大可能开出期望价格的物品。
    /// </summary>
    public class 期望箱子<T> : CMKZ_Dictionary<T, double> {
        public double 期望;
        public T 开箱() {
            if (Count == 0) throw new Exception("开箱错误");
            double A = Values.Sum();
            var B = this.ToDictionary(kvp => kvp.Key, kvp => kvp.Value / A);
            double C = Random(0, 1f);
            double D = 0f;
            foreach (var i in B) {
                D += i.Value;
                if (C <= D) {
                    return i.Key;
                }
            }
            throw new Exception("开箱错误");
        }
    }
}