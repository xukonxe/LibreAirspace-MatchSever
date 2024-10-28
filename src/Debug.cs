using System;

/// 此文件需要引用Tencent.Xlua插件的54-v2.2.16版本
namespace TGZG {
	public static partial class 公共空间 {
		public static int LineCount = 0;
		public static void Log(this object 消息) {
			ClearCheck();
			Console.WriteLine(消息);
			LineCount++;
		}
		public static void ClearCheck() {
			if (LineCount > 1000) {
				Console.Clear();
				LineCount = 0;
				"========消息已清理========".log();
			}
		}
		public static void log(this object 消息) => 消息.Log();
		public static void logerror(this object message) {
			ClearCheck();
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(message);
			Console.ResetColor();
			LineCount++;
		}
		public static void logwarning(this object message) {
			ClearCheck();
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(message);
			Console.ResetColor();
			LineCount++;
		}
	}
}