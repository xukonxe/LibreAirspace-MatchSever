using System;
using System.Reflection;

namespace CMKZ {
    //Int
    public static partial class LocalStorage {
        public static bool IsClass(this Type type) => type != typeof(object) && Type.GetTypeCode(type) == TypeCode.Object;
        public static bool IsEnumerableType(this Type type) => type.GetInterface("IEnumerable") != null;
        public static T CreateObject<T>(params object[] X) {
            if (typeof(T).GetConstructor(Type.EmptyTypes) == null) {
                return default;
            } else if (typeof(T).IsClass()) {
                return (T)Activator.CreateInstance(typeof(T), X);
            }
            return default;
        }
        public static object 创建实例(this Type X) {
            if (X == null) {
                throw new Exception("不可创建null实例");
            }
            return Activator.CreateInstance(X);
        }
        public static T 创建实例<T>(this Type X) {
            if (X == null) {
                throw new Exception("不可创建null实例");
            }
            //T是抽象的，X是具体的、继承自T
            if (!X.IsSubclassOf(typeof(T))) {
                throw new Exception($"创建实例错误：{X.Name} 不是 {typeof(T).Name}");
            }
            return (T)Activator.CreateInstance(X);
        }
        public static bool 继承自(this Type X, Type Y) {
            return X.IsSubclassOf(Y) || Y.IsAssignableFrom(X);
        }
        public static bool 继承自<T>(this Type X) {
            var Y = typeof(T);
            return X.IsSubclassOf(Y) || Y.IsAssignableFrom(X);
        }
        public static bool IsInstanceOfGenericType(this object instance, Type genericType) {
            //if (instance.GetType().IsGenericType) {
            //    return instance.GetType().GetGenericTypeDefinition() == genericType;
            //}
            //return false;
            Type instanceType = instance.GetType();
            return genericType.IsAssignableFrom(instanceType) && instanceType.IsGenericType;
        }
        public static string BaseString(this Type X) {
            if (X == typeof(int) || X == typeof(float) || X == typeof(double) || X == typeof(long)) {
                return "数字";
            } else if (X == typeof(string)) {
                return "文本";
            } else {
                return X.Name;
            }
        }
        public static T GetValue<T>(this PropertyInfo X, object Y) {
            return (T)X.GetValue(Y);
        }
        public static T GetFieldValue<T>(this object X, string Y) {
            if (X == null) {
                throw new ArgumentNullException(nameof(X));
            }
            var A = X.GetType().GetField(Y, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (A != null) {
                return (T)A.GetValue(X);
            }
            var B = X.GetType().GetProperty(Y, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (B != null) {
                return (T)B.GetValue(X);
            }
            throw new Exception($"{X.GetType()} 中不存在字段或属性 {Y}");
        }
        public static object GetFieldValue(this object X, string Y) {
            if (X == null) {
                throw new ArgumentNullException(nameof(X));
            }
            var A = X.GetType().GetField(Y, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (A != null) {
                return A.GetValue(X);
            }
            var B = X.GetType().GetProperty(Y, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (B != null) {
                return B.GetValue(X);
            }
            throw new Exception($"{X.GetType()} 中不存在字段或属性 {Y}");
        }
        public static void SetFieldValue<T>(this object X, string Y, T Z) {
            if (X == null) {
                throw new ArgumentNullException(nameof(X));
            }
            var A = X.GetType().GetField(Y, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (A != null) {
                A.SetValue(X, Z);
                return;
            }
            var B = X.GetType().GetProperty(Y, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (B != null) {
                B.SetValue(X, Z);
                return;
            }
            throw new Exception($"{X.GetType()} 中不存在字段或属性 {Y}");
        }
    }
}