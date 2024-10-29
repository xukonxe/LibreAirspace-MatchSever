using System.Collections.Generic;

namespace TGZG.战雷革命游戏服务器 {
	public static class CommunicateConstant {
		private static string[] ms_ReservedPacketType = ["_ID"];

		public static IReadOnlyList<string> GetReservedPacketType() => ms_ReservedPacketType;

		public static class ProtocolType {
			public const string TGZG键值对交换协议 = "TGZG键值对交换";
			public const string TGZG键值对交换协议_TCP = "TGZG键值对交换_TCP";
			public const string 自由空域对局协议 = "自由空域对局协议";
			public const string 自由空域房间列表协议 = "自由空域房间列表协议";
		}

		public static class PacketType {
			[ClientPacket([ProtocolType.TGZG键值对交换协议_TCP])]
			[ServerPacket([ProtocolType.TGZG键值对交换协议_TCP])]
			public const string _版本检测 = "_版本检测";

			[ClientPacket([ProtocolType.TGZG键值对交换协议_TCP])]
			[ServerPacket([ProtocolType.TGZG键值对交换协议_TCP])]
			public const string 测试信息 = "测试信息";

			[ClientPacket([ProtocolType.TGZG键值对交换协议])]
			[ServerPacket([ProtocolType.TGZG键值对交换协议])]
			public const string 数据错误 = "数据错误";

			[ClientPacket([ProtocolType.自由空域对局协议])]
			public const string 验证登录 = "验证登录";

			[ClientPacket([ProtocolType.自由空域对局协议])]
			public const string 更新位置 = "更新位置";

			[ClientPacket([ProtocolType.自由空域对局协议])]
			public const string 发送聊天消息 = "发送聊天消息";

			[ClientPacket([ProtocolType.自由空域对局协议])]
			public const string 重生 = "重生";

			[ClientPacket([ProtocolType.自由空域对局协议])]
			public const string 导弹发射 = "导弹发射";

			[ClientPacket([ProtocolType.自由空域对局协议])]
			public const string 导弹爆炸 = "导弹爆炸";
			
			[ClientPacket([ProtocolType.自由空域对局协议])]
			public const string 损坏 = "损坏";

			[ClientPacket([ProtocolType.自由空域对局协议])]
			public const string 击伤 = "击伤";

			[ServerPacket([ProtocolType.自由空域对局协议])]
			public const string 登录失败 = "登录失败";

			[ServerPacket([ProtocolType.自由空域对局协议])]
			public const string 登录成功 = "登录成功";

			[ServerPacket([ProtocolType.自由空域对局协议])]
			public const string 被击伤 = "被击伤";

			[ServerPacket([ProtocolType.自由空域对局协议])]
			public const string 同步损坏 = "同步损坏";

			[ServerPacket([ProtocolType.自由空域对局协议])]
			public const string 死亡 = "死亡";

			[ServerPacket([ProtocolType.自由空域对局协议])]
			public const string 同步重生 = "同步重生";

			[ServerPacket([ProtocolType.自由空域对局协议])]
			public const string 聊天消息 = "聊天消息";

			[ServerPacket([ProtocolType.自由空域对局协议])]
			public const string 房间内玩家死亡消息 = "房间内玩家死亡消息";

			[ServerPacket([ProtocolType.自由空域对局协议])]
			public const string 同步其他玩家数据 = "同步其他玩家数据";
		}
	}

	[System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class ClientPacketAttribute : System.Attribute 
	{
		public string[] ProtocolName;
		public ClientPacketAttribute(string[] ProtocolName) {
			this.ProtocolName = ProtocolName;
		}
	}

	[System.AttributeUsage(System.AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
	public sealed class ServerPacketAttribute : System.Attribute {
		public string[] ProtocolName;
		public ServerPacketAttribute(string[] ProtocolName) {
			this.ProtocolName = ProtocolName;
		}
	}
}