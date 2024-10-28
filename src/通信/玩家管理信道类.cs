using System;
using System.Collections.Generic;
using System.Linq;
using static CMKZ.LocalStorage;
using static TGZG.战雷革命游戏服务器.公共空间;
using PktTypTab = TGZG.战雷革命游戏服务器.CommunicateConstant.PacketType;

namespace TGZG.战雷革命游戏服务器 {
	public class 玩家管理信道类 : 网络信道服务器类_KCP {
        public 玩家管理信道类(ushort 端口, string 版本) : base(端口, 版本) {
			//处理客户端发来的消息
			OnConnected += (客户端ID) => {

			};
			OnDisconnected += (客户端ID) => {
				if (所有玩家.ContainsKey(客户端ID)) {
					var 玩家数据 = 所有玩家[客户端ID];
					所有玩家.Remove(客户端ID);
					$"玩家 {玩家数据.u.n} 已下线".log();
					广播系统消息("系统消息", $"玩家 {玩家数据.u.n} 已下线");
					lock (房间数据) {
						房间数据.人数 = 所有玩家.Count;
					}
					lock (休闲计分板) {
						房间管理信道.玩家数据更新(玩家数据.u.n, 休闲计分板.最终转化(玩家数据.u.n));
					}
					房间管理信道.房间数据更新();
				}
			};
			OnUpdate += () => {
				foreach (var 客户端 in server.connections.Keys) {
					if (所有玩家.ContainsKey(客户端)) {
						更新世界数据(客户端);
					}
				}
			};
		}

		protected override void RegisterPacketHandler() {
			base.RegisterPacketHandler();
			//处理客户端发来的消息
			this.m_PacketHandlerRegistry.RegisterPacketType(PktTypTab.更新位置);
			this.m_PacketHandlerRegistry.RegisterPacketHandler(PktTypTab.更新位置, "自由空域对局服务器__处理客户端更新位置",
				(客户端ID, t, addArg) => {
					var 数据 = t["数据"].JsonToCS<玩家游玩数据>(false);
					var 老数据 = 所有玩家[客户端ID];
					lock (所有玩家) {
						所有玩家[客户端ID] = 数据;
					}
					lock (休闲计分板) {
						休闲计分板.玩家位置更新(所有玩家[客户端ID].u.n, 老数据.p, 数据.p);
					}
				});
			this.m_PacketHandlerRegistry.RegisterPacketType(PktTypTab.发送聊天消息);
			this.m_PacketHandlerRegistry.RegisterPacketHandler(PktTypTab.发送聊天消息, "自由空域对局服务器__处理客户端发送聊天消息",
				(客户端ID, t, addArg) => {
					var 发送者 = t["发送者"];
					var 内容 = t["内容"];
					var 广播内容 = 内容;
					lock (休闲计分板) {
						休闲计分板.玩家聊天(发送者);
					}
					广播玩家消息(客户端ID, 广播内容, 队伍.无);
				});
			this.m_PacketHandlerRegistry.RegisterPacketType(PktTypTab.导弹发射);
			this.m_PacketHandlerRegistry.RegisterPacketHandler(PktTypTab.导弹发射, "自由空域对局服务器__处理客户端导弹发射",
				(客户端ID, t, addArg) => {
					var 数据 = t["数据"].JsonToCS<导弹飞行数据>(false);
					广播导弹发射消息(所有玩家[客户端ID], 数据);
				});
			this.m_PacketHandlerRegistry.RegisterPacketType(PktTypTab.导弹爆炸);
			this.m_PacketHandlerRegistry.RegisterPacketHandler(PktTypTab.导弹爆炸, "自由空域对局服务器__处理客户端导弹爆炸",
				(客户端ID, t, addArg) => {
					var 数据 = t["数据"].JsonToCS<导弹飞行数据>(false);
					广播导弹爆炸消息(所有玩家[客户端ID], 数据);
				});
			//发来此消息者，表示自己已被损坏。
			this.m_PacketHandlerRegistry.RegisterPacketType(PktTypTab.损坏);
			this.m_PacketHandlerRegistry.RegisterPacketHandler(PktTypTab.损坏, "自由空域对局服务器__处理客户端损坏",
				(客户端ID, t, addArg) => {
					string 攻击者 = t["攻击者"];
					部位 数据 = Enum.Parse<部位>(t["数据"]);
					//收到损坏信息时，找到此玩家，并广播给其他玩家
					var 玩家客户端信息 = 所有玩家.FirstOrDefault(t => t.Key == 客户端ID);
					if (玩家客户端信息.Key == default) return;
					var 玩家名称 = 玩家客户端信息.Value.u.n;
					if (数据 is 部位.身) {
						发送死亡信息(客户端ID);
						广播死亡消息(玩家名称);
						lock (休闲计分板) {
							休闲计分板.玩家死亡(玩家名称);
							休闲计分板.玩家击杀(攻击者);
						}
						发送击杀提示(攻击者, 玩家名称);
						广播系统消息("系统消息", $"{攻击者} 杀死了 {玩家名称}");
					}
				});
			//发来此消息者，表示成功攻击其他玩家，执行响应操作
			this.m_PacketHandlerRegistry.RegisterPacketType(PktTypTab.击伤);
			this.m_PacketHandlerRegistry.RegisterPacketHandler(PktTypTab.击伤, "自由空域对局服务器__处理客户端击伤",
				(客户端ID, t, addArg) => {
					击伤信息 数据 = t["数据"].JsonToCS<击伤信息>();
					//收到击伤信息时，找到被击伤的玩家，向它发送击伤消息，并广播给其他玩家
					var 被攻击玩家客户端信息 = 所有玩家.FirstOrDefault(t => t.Value.u.n == 数据.被攻者);
					if (被攻击玩家客户端信息.Key == default) return;
					var 被攻击玩家客户端ID = 被攻击玩家客户端信息.Key;
					var 被攻击玩家名称 = 被攻击玩家客户端信息.Value.u.n;
					var 攻击者客户端信息 = 所有玩家.FirstOrDefault(t => t.Key == 客户端ID);
					if (攻击者客户端信息.Key == default) return;
					var 攻击者名称 = 攻击者客户端信息.Value.u.n;
					lock (休闲计分板) {
						休闲计分板.玩家击伤(攻击者名称);
					}

					发送击伤消息(被攻击玩家客户端ID, 数据);
					广播系统消息("系统消息", $"{攻击者名称} 击中了 {被攻击玩家名称},对 {数据.伤害.ToString()} 造成 {数据.部位.ToString()} 点伤害");
					//发送击伤消息(客户端ID, 数据);
					//广播系统消息("系统消息", $"您 击中了 您自己,对 {数据.bp.ToString()} 造成 {数据.dm} 点伤害");
				});
			this.m_PacketHandlerRegistry.RegisterPacketType(PktTypTab.验证登录);
			this.m_PacketHandlerRegistry.RegisterPacketHandler(PktTypTab.验证登录, "自由空域对局服务器__处理客户端验证登录",
				(客户端ID, t, addArg) => {
					//向房间管理服务器发送验证登录请求，等待返回验证结果
					var 账号 = t["账号"];
					var 密码 = t["密码"];
					var 验证结果 = 房间管理信道.验证登录(账号, 密码).Result;
					if (验证结果.成功) {
						lock (房间数据) {
							所有玩家.Add(客户端ID, new() {
								u = new() { n = 账号 },
								p = 玩家世界数据.初始化(),
							});
							房间数据.人数 = 所有玩家.Count;
						}
						lock (休闲计分板) {
							休闲计分板.添加玩家(账号);
						}
						房间管理信道.房间数据更新();
						$"玩家 {账号} 进入服务器".log();
						广播系统消息("系统消息", $"玩家 {账号} 已登录");

						Send(客户端ID,
							[("标题", "登录验证结果"),
							("状态", "成功"),
							("消息", 验证结果.内容)]);
					} else
						Send(客户端ID,
							[("标题", "登录验证结果"),
							("状态", "失败"),
							("消息", 验证结果.内容)]);
				});
		}

		public void 更新计分板((计分板数据 蓝队数据, 计分板数据 红队数据) 计分板数据) {
			SendAll(
				("标题", "更新计分板"),
				("蓝队数据", 计分板数据.蓝队数据.ToJson(格式美化: false)),
				("红队数据", 计分板数据.红队数据.ToJson(格式美化: false))
			);
		}
		public void 发送击伤消息(int 客户端ID, 击伤信息 数据) {
			Send(客户端ID,
				("标题", "被击伤"),
				("数据", 数据.ToJson())
				);
		}
		//public void 同步损坏(string 损坏玩家名, List<部位> 数据) {
		//    SendAll(
		//        ("标题", "同步损坏"),
		//        ("玩家", 损坏玩家名),
		//        ("数据", 数据.ToJson())
		//        );
		//}
		public void 发送击杀提示(string 攻击者, string 被攻击者) {
			var 攻击者ID = 所有玩家.FirstOrDefault(t => t.Value.u.n == 攻击者).Key;
			if (攻击者ID == default) return;
			Send(攻击者ID,
				("标题", "击杀提示"),
				("被击杀者", 被攻击者)
				);
		}
		public void 发送死亡信息(int 客户端ID) {
			var 玩家数据 = 所有玩家[客户端ID];
			Send(客户端ID,
				("标题", "死亡")
				);
		}
		//public void 广播重生消息(玩家游玩数据 玩家数据, int 排除客户端ID) {
		//    SendAll(排除客户端ID,
		//        ("标题", "同步重生"),
		//        ("玩家", 玩家数据.ToJson()));
		//}
		public void 广播系统消息(string 发送者, string 消息内容) {
			SendAll(
				("标题", "聊天消息"),
				("内容", 消息内容),
				("队伍", 队伍.系统.ToString()),
				("发送者", 发送者));
		}
		public void 广播死亡消息(string 玩家名称) {
			SendAll(
				("标题", "房间内玩家死亡消息"),
				("玩家", 玩家名称));
		}
		public void 广播玩家消息(int 发送者ID, string 消息内容, 队伍 队伍) {
			var 发送者 = 所有玩家[发送者ID].u.n;
			SendAll(
				("标题", "聊天消息"),
				("内容", 消息内容),
				("队伍", 队伍.ToString()),
				("发送者", 发送者)
				);
		}
		public void 广播导弹发射消息(玩家游玩数据 玩家数据, 导弹飞行数据 导弹数据) {
			var 玩家客户端ID = 所有玩家.FirstOrDefault(t => t.Value.Equals(玩家数据)).Key;
			SendAll(玩家客户端ID,
				("标题", "导弹发射"),
				("玩家", 玩家数据.ToJson(格式美化: false)),
				("导弹数据", 导弹数据.ToJson(格式美化: false)));
		}
		public void 广播导弹爆炸消息(玩家游玩数据 玩家数据, 导弹飞行数据 导弹数据) {
			var 玩家客户端ID = 所有玩家.FirstOrDefault(t => t.Value.Equals(玩家数据)).Key;
			SendAll(玩家客户端ID,
				("标题", "导弹爆炸"),
				("玩家", 玩家数据.ToJson(格式美化: false)),
				("导弹数据", 导弹数据.ToJson(格式美化: false)));
		}
		public void 更新世界数据(int 客户端ID) {
			var 所有其他玩家数据 = 获取其他玩家数据(客户端ID);
			if (所有其他玩家数据.Count == 0) return;
			Send(客户端ID,
				("标题", "同步其他玩家数据"),
				("其他玩家", 所有其他玩家数据.Select(t => t).Where(t => t.u.tp != default).ToJson(格式美化: false)));
		}
		public List<玩家游玩数据> 获取其他玩家数据(int 请求者ID) {
			lock (所有玩家) {
				return 所有玩家.Where(t => t.Key != 请求者ID).Select(t => t.Value).ToList();
			}
		}
	}
}