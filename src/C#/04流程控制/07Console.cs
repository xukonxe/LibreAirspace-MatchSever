//using System;
//using System.Linq;
//using System.Reflection;
//using static CMKZ.LocalStorage;

//namespace CMKZ {
//    public static partial class LocalStorage {
//        public static Console 控制台 = new();
//        public static Action<I指令> OnCommandExe;
//        public static Action<I指令> BeforeCommandExe;
//        public static Action<I指令> AfterCommandExe;
//        public static bool Invoke(this I指令 X) {
//            BeforeCommandExe?.Invoke(X);
//            Print($"指令 {X} 运行！");
//            bool 成功执行 = false;
//            if (X._Invoke()) {
//                OnCommandExe?.Invoke(X);
//                成功执行 = true;
//            }
//            AfterCommandExe?.Invoke(X);
//            return 成功执行;
//        }
//    }
//    public interface I指令 {
//        bool _Invoke();
//    }
//    public interface I开发者指令 : I指令 {

//    }
//    public class 帮助指令 : I指令 {
//        bool I指令._Invoke() {
//            控制台.帮助();
//            return true;
//        }
//    }
//    public static partial class LocalStorage {
//        public static string Get注释(this I指令 X) {
//            var A = new CMKZList<string>();
//            var B = X.GetType();
//            foreach (var i in B.GetProperties()) {
//                var C = i.GetCustomAttribute<注释Attribute>();
//                if (C != null) {
//                    A.Add(C.内容);
//                }
//                foreach (var j in B.GetInterfaces()) {
//                    var D = j.GetProperty(i.Name);
//                    if (D != null) {
//                        var E = D.GetCustomAttribute<注释Attribute>();
//                        if (E != null) {
//                            A.Add(E.内容);
//                            goto T;
//                        }
//                    }
//                }
//                A.Add(i.PropertyType.BaseString());
//                T:;
//            }
//            return B.Name.Remove("指令") + " " + A.Join(" ");
//        }
//    }
//    public class 注释Attribute : Attribute {
//        public string 内容;
//        public 注释Attribute(string 内容) {
//            this.内容 = 内容;
//        }
//    }
//    public class Console {
//        public CMKZList<I指令> 所有指令 = new();
//        public CMKZ_Dictionary<Type, Func<string, object>> 转化器 = new();
//        public string 帮助文本;
//        public Console() {
//            MainTypes.Where(t => typeof(I指令).IsAssignableFrom(t) && t.IsClass).ForEach(t => 所有指令.Add((I指令)Activator.CreateInstance(t)));
//            帮助文本 = "所有指令：\n" + 所有指令.Where(t => t is not I开发者指令).Select(t => "·" + t.Get注释()).Join("\n");
//            转化器[typeof(short)] = t => Convert.ChangeType(t, typeof(short));
//            转化器[typeof(ushort)] = t => Convert.ChangeType(t, typeof(ushort));
//            转化器[typeof(int)] = t => Convert.ChangeType(t, typeof(int));
//            转化器[typeof(uint)] = t => Convert.ChangeType(t, typeof(uint));
//            转化器[typeof(long)] = t => Convert.ChangeType(t, typeof(long));
//            转化器[typeof(ulong)] = t => Convert.ChangeType(t, typeof(ulong));
//            转化器[typeof(float)] = t => Convert.ChangeType(t, typeof(float));
//            转化器[typeof(double)] = t => Convert.ChangeType(t, typeof(double));
//            转化器[typeof(char)] = t => Convert.ChangeType(t, typeof(char));
//            转化器[typeof(byte)] = t => Convert.ChangeType(t, typeof(byte));
//            转化器[typeof(string)] = t => t;
//        }
//        public I指令 匹配指令(string X) {
//            foreach (var a in 所有指令) {
//                if (a.GetType().Name.Remove("指令") == X) return a;
//            }
//            return null;
//        }
//        public void 匹配参数(string X, I指令 指令) {
//            var 整段指令 = X.Split(' ');
//            var 参数数量 = 整段指令.Length - 1;
//            var 所有字段 = 指令.GetType().GetProperties().Where(t => t.Name.StartsWith("D"));
//            if (参数数量 != 所有字段.Count()) {
//                PrintWarning($"指令【{指令}】的参数数量不对，需要{所有字段.Count()}个参数，实际{参数数量}个", LocalStorage.ConsoleSystem);
//                return;
//            }
//            for (int i = 1; i < 整段指令.Length; i++) {
//                var 当前字段 = 所有字段.Find(t => t.Name == "D" + i);
//                var value = 转化器[当前字段.PropertyType]?.Invoke(整段指令[i]);
//                if (value == null) {
//                    PrintWarning($"参数{当前字段.Name}的类型从string到{当前字段.PropertyType}转化失败，可能是因为并未注册转化器、或者转化器运行结果为null", LocalStorage.ConsoleSystem);
//                    return;
//                }
//                当前字段.SetValue(指令, value);
//            }
//            指令.Invoke();
//        }
//        public void 执行(string X) {
//            PrintSystem($"> {X}");
//            X = X.Trim();
//            if (X == "") {
//                PrintWarning($"指令不可为空");
//                return;
//            }
//            var 分割 = X.Split(' ');
//            var A = 匹配指令(分割[0]);
//            if (A == null) {
//                PrintWarning($"指令引导词 {分割[0]} 不存在", LocalStorage.ConsoleSystem);
//                return;
//            }
//            匹配参数(X, A);
//        }
//        public void 帮助() {
//            PrintSystem(帮助文本);
//        }
//    }
//}