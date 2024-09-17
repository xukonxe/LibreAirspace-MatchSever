using System.Collections;
using System.Collections.Generic;
using static TGZG.公共空间;
using System.Diagnostics;

/// 此文件需要引用Tencent.Xlua插件的54-v2.2.16版本
namespace TGZG {
    public static partial class 公共空间 {
        public static void Log(this object 消息) {
            Console.WriteLine(消息);
        }
        public static void log(this object 消息) {
            消息.Log();
        }
    }
}