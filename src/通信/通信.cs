using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace TGZG.战雷革命游戏服务器 {
	public static partial class 公共空间 {
		public static Dictionary<int, 玩家游玩数据> 所有玩家 = new();
		public static 休闲模式计分板 休闲计分板 = new();
	}
}