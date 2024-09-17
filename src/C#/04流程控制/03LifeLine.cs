using System;//Action
using System.Linq;//from XX select XX
using System.Reflection;
using static CMKZ.LocalStorage;

namespace CMKZ {
    public enum 初始化优先级 {
        暂时废弃,
        PlayerPrefs,
        无依赖,//遍历设定
        第二级,//基于设定，再遍历得到二级设定
    }
    /// <summary>
    /// 标记为此特性的函数会在游戏启动时自动执行
    /// </summary>
    public class 初始化函数Attribute : Attribute {
        public 初始化优先级 优先级;
        public 初始化函数Attribute(初始化优先级 X = 初始化优先级.无依赖) {
            优先级 = X;
        }
    }
    public static partial class LocalStorage {
        //Tobo：项目生命周期（启动初始化、关闭、暂停、恢复）
        public static Type[] _MainTypes;
        /// <summary>
        /// 包含类内的类
        /// </summary>
        public static Type[] MainTypes => _MainTypes ??= Assembly.GetExecutingAssembly().GetTypes().Where(i => i.Namespace?.StartsWith(nameof(CMKZ)) == true).ToArray();
        public static Type[] _MainTypesNotAbstract;
        /// <summary>
        /// 包含类内的类
        /// </summary>
        public static Type[] MainTypesNotAbstract => _MainTypesNotAbstract ??= MainTypes.Where(i => !i.IsAbstract).ToArray();
        public static void Init() {
            //var A = from i in MainTypes
            //        from t in i.GetMethods()
            //        let a = t.GetCustomAttribute<初始化函数Attribute>()
            //        where a != null
            //        select (t, a);
            var A = new CMKZList<(MethodInfo 函数, 初始化函数Attribute 特性)>();
            foreach (var i in MainTypes) {
                foreach (var j in i.GetMethods()) {
                    var B = j.GetCustomAttribute<初始化函数Attribute>();
                    if (B != null) {
                        A.Add((j, B));
                    }
                }
            }

            A.Where(t => t.特性.优先级 == 初始化优先级.PlayerPrefs).ForEach(t => t.函数.Invoke(null, null));
            A.Where(t => t.特性.优先级 == 初始化优先级.无依赖).ForEach(t => t.函数.Invoke(null, null));
            A.Where(t => t.特性.优先级 == 初始化优先级.第二级).ForEach(t => t.函数.Invoke(null, null));
            //Print(A.Count());//17
        }
    }
}