using System;//Action

namespace CMKZ {
    public static partial class LocalStorage {
        public static bool IsAllTrue(this Func<bool> X) {
            foreach (Func<bool> i in X.GetInvocationList()) {
                if (!i()) {
                    return false;
                }
            }
            return true;
        }
        public static bool TimesForAllTrue(this int X, Func<int, bool> Y) {
            for (var i = 0; i < X; i++) {
                if (!Y(i)) {
                    return false;
                }
            }
            return true;
        }
    }
}