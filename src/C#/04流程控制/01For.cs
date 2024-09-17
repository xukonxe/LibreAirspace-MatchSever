using System;//Action

namespace CMKZ {
    public static partial class LocalStorage {
        /// <summary>
        /// X必须在0到1之间
        /// </summary>
        public static void RandomToDo(this double X, Action Y) {
            if (Random(X)) {
                Y();
            }
        }
        public static void TimesToDo(this int X, Action Y) {
            for (var i = 0; i < X; i++) {
                Y();
            }
        }
        public static void TimesToDo(this int X, Action<int> Y) {
            for (var i = 0; i < X; i++) {
                Y(i);
            }
        }
        public static void 执行X次(int 次数, Action X) {
            for (var i = 0; i < 次数; i++) X();
        }
        public static void 执行X次(int 次数, Action<int> X) {
            for (var i = 0; i < 次数; i++) X.Invoke(i);
        }
        public static void 执行XY次(int X, int Y, Action<int, int> Z) {
            for (var i = 0; i < X; i++) {
                for (var j = 0; j < Y; j++) {
                    Z(i, j);
                }
            }
        }
        /// <summary>
        /// While(() => true).Do(() => { }).With(10);
        /// </summary>
        public static Whiler While(Func<bool> Y) {
            return new Whiler(Y);
        }
    }
    public class Whiler {
        public Func<bool> X;
        public Action Y;
        public int Z;
        public Whiler(Func<bool> X) {
            this.X = X;
        }
        public Whiler Do(Action X) {
            Y = X;
            return this;
        }
        public Whiler With(int X) {
            Z = X;
            Start();
            return this;
        }
        public void Start() {
            while (X()) {
                Y();
                Z--;
                if (Z <= 0) {
                    break;
                }
            }
        }
    }
}