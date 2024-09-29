using System;

/// 此文件需要引用Tencent.Xlua插件的54-v2.2.16版本
namespace TGZG {
	public static partial class 公共空间 {
        public static void Log(this object 消息) {
            Console.WriteLine(消息);
        }
        public static void log(this object 消息) {
            消息.Log();
        }
        public static void logerror(this object message) {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public static void logwarning(this object message) {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}