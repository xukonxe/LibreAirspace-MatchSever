using System.Threading.Tasks;
using static CMKZ.LocalStorage;
using static TGZG.战雷革命游戏服务器.公共空间;

namespace TGZG.战雷革命游戏服务器 {
	// 自由空域房间管理协议的客户端实现。
	public class 房间管理信道类 : 网络信道类 {
		public 房间管理信道类(string ip, string 版本) : base(ip, 版本) {
			OnDisconnect += () => {
				"房间信道断开".log();
			};
			OnConnect += () => {
				$"房间信道已连接{房间管理信道.服务端IP}".log();
			};
		}
		public async void 房间数据更新() {
			var 消息 = await 游戏端.SendAsync(new() {
				{ "标题","房间数据更新" },
				{ "数据", 房间数据.ToJson(格式美化: false) }
			});
			if (消息.ContainsKey("失败")) {
				Print("更新失败：" + 消息["失败"]);
				return;
			}
			if (消息.ContainsKey("成功")) {
				return;
			}
			Print("收到服务器消息，但内容异常。错误码6678");
		}
		public async void 发送注册房间() {
			var 消息 = await 游戏端.SendAsync(new() {
				{ "标题","注册" },
				{ "数据", 房间数据.ToJson(格式美化: false) }
			});
			if (消息.ContainsKey("失败")) {
				Print("注册失败：" + 消息["失败"]);
				return;
			}
			if (消息.ContainsKey("成功")) {
				Print(消息["成功"]);
				return;
			}
			Print("收到服务器消息，但内容异常。错误码6677");
		}
		public async void 玩家数据更新(string 玩家昵称, 玩家计分数据 统计数据) {
			await 游戏端.SendAsync(new() {
				{ "标题","玩家数据上传" },
				{ "账号", 玩家昵称 },
				{ "数据", 统计数据.ToJson(格式美化: false) }
			});
		}
		public async Task<(bool 成功, string 内容)> 验证登录(string 账号, string 密码) {
			var 消息 = await 游戏端.SendAsync(new() {
				{ "标题","验证登录" },
				{ "账号", 账号 },
				{ "密码", 密码 }
			});
			if (消息.ContainsKey("失败")) {
				return (false, 消息["失败"]);
			}
			if (消息.ContainsKey("成功")) {
				return (true, 消息["成功"]);
			}
			return (false, "登录服务器验证异常，请联系管理员。错误码6676");
		}
	}
}