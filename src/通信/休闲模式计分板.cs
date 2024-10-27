using System;
using System.Collections.Generic;
using System.Linq;
using static CMKZ.LocalStorage;
using static TGZG.战雷革命游戏服务器.公共空间;

namespace TGZG.战雷革命游戏服务器 {
	public class 休闲模式计分板 {
		public Dictionary<string, 玩家计分数据> 所有玩家KDA = new();
		public Action<休闲模式计分板> On计分板更新;
		public void 添加玩家(string 名称) {
			所有玩家KDA[名称] = new 玩家计分数据();
			On计分板更新?.Invoke(this);
		}
		public void 删除玩家(string 名称) {
			所有玩家KDA.Remove(名称);
			On计分板更新?.Invoke(this);
		}
		public 玩家计分数据 最终转化(string 名称) {
			if (!验证存在性(名称)) return new 玩家计分数据();
			var 玩家数据 = 所有玩家KDA[名称];
			玩家数据.最终计算();
			return 玩家数据;
		}

		public void 玩家位置更新(string 名称, 玩家世界数据 老位置, 玩家世界数据 新位置) {
			if (!验证存在性(名称)) return;
			//计算高度变化，如果高度变化大于0，则认为是爬高
			var 高度差 = 新位置.p[1] - 老位置.p[1];
			if (高度差 > 0) {
				所有玩家KDA[名称].爬高高度累计 += 高度差;
			}
		}
		public void 玩家击伤(string 名称) {
			if (!验证存在性(名称)) return;
			所有玩家KDA[名称].子弹命中次数++;
			On计分板更新?.Invoke(this);
		}
		public void 玩家击杀(string 名称) {
			if (!验证存在性(名称)) return;
			所有玩家KDA[名称].击杀数++;
			On计分板更新?.Invoke(this);
		}
		public void 玩家死亡(string 名称) {
			if (!验证存在性(名称)) return;
			所有玩家KDA[名称].死亡数++;
			On计分板更新?.Invoke(this);
		}
		public void 玩家聊天(string 名称) {
			if (!验证存在性(名称)) return;
			所有玩家KDA[名称].消息发送总数++;
		}
		bool 验证存在性(string 名称) {
			return 所有玩家KDA.ContainsKey(名称);
		}

		public (计分板数据, 计分板数据) To计分板数据() {
			var 蓝队计分板数据 = new 计分板数据();
			var 红队计分板数据 = new 计分板数据();
			//添加列：玩家名、击杀、死亡、命中;
			蓝队计分板数据.初始化列定义("玩家名", "击杀", "死亡", "命中");
			红队计分板数据.初始化列定义("玩家名", "击杀", "死亡", "命中");
			//添加行：玩家名、击杀、死亡、命中;
			var 玩家 =
			所有玩家KDA
				.Select(t => new string[]{
					t.Key,
					t.Value.击杀数.ToString(),
					t.Value.死亡数.ToString(),
					t.Value.子弹命中次数.ToString()
				});

			玩家
				.Where(t => {
					if (所有玩家.Contains(s => s.Value.u.n == t[0])) return false;
					var 此玩家 = 所有玩家.FirstOrDefault(s => s.Value.u.n == t[0]);
					return 此玩家.Value.tm is 队伍.蓝;
				})
				.ForEach(s => 蓝队计分板数据.添加行(s));

			玩家
				.Where(t => {
					if (所有玩家.Contains(s => s.Value.u.n == t[0])) return false;
					var 此玩家 = 所有玩家.FirstOrDefault(s => s.Value.u.n == t[0]);
					return 此玩家.Value.tm is 队伍.红;
				})
				.ForEach(s => 红队计分板数据.添加行(s));

			return (蓝队计分板数据, 红队计分板数据);
		}
	}
}