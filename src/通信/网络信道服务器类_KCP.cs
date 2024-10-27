using kcp2k;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static CMKZ.LocalStorage;
using PktTypTab = TGZG.战雷革命游戏服务器.CommunicateConstant.PacketType;

namespace TGZG.战雷革命游戏服务器 {
	public partial class 网络信道服务器类_KCP {
        protected int Tick = 128;
        protected KcpServer server;
        protected ushort port;
        protected string 版本;
        protected event Action<int> OnConnected;
        protected event Action<int, ArraySegment<byte>, KcpChannel> OnData;
        protected event Action<int> OnDisconnected;
        protected event Action<int, ErrorCode, string> OnError;

        protected event Action OnUpdate;

		protected 数据包处理器注册表 m_PacketHandlerRegistry = new();

		protected HashSet<string> m_CachedPacketTypeList = null;

		public 网络信道服务器类_KCP(int Port, string 版本) {
            port = (ushort)Port;
            this.版本 = 版本;
            kcp2k.Log.Info = Console.WriteLine;
            kcp2k.Log.Warning = Console.WriteLine;
            kcp2k.Log.Error = Console.WriteLine;

            KcpConfig config = new KcpConfig(
                NoDelay: true,
                DualMode: false,
                Interval: 1,
                Timeout: 5000,
                SendWindowSize: Kcp.WND_SND * 1000,
                ReceiveWindowSize: Kcp.WND_RCV * 1000,
                CongestionWindow: false,
                MaxRetransmits: Kcp.DEADLINK * 2
            );
            server = new KcpServer(
                (cId) => OnConnected?.Invoke(cId),
                (cId, message, channel) => OnData?.Invoke(cId, message, channel),
                (cId) => OnDisconnected?.Invoke(cId),
                (cId, error, reason) => OnError?.Invoke(cId, error, reason),
                config: config
            );
            OnConnected = (cId) => { Log.Info($"[KCP] OnServerConnected({cId})"); };
			//OnData = (cId, message, channel) => Log.Info($"[KCP] OnServerDataReceived({cId}, {BitConverter.ToString(message.Array, message.Offset, message.Count)} @ {channel})");
			OnDisconnected = (cId) => {
			};
            OnError = (cId, error, reason) => Log.Error($"[KCP] OnServerError({cId}, {error}, {reason}");

			this.RegisterPacketHandler();

            OnData += (客户端ID, 数据, 频道) => {
                //处理空数据包
                if (数据.Array == null) {
                    Send(客户端ID, ("标题", PktTypTab.数据错误), ("内容", "数据内容为空"));
                    return;
                }
                //转码数据包
                string 数据json = Encoding.UTF8.GetString(数据.Array, 数据.Offset, 数据.Count);
                var 解析后 = 数据json.JsonToCS<Dictionary<string, string>>();
                //处理无标题数据包
                if (!解析后.ContainsKey("标题")) {
                    Send(客户端ID, ("标题", PktTypTab.数据错误), ("内容", "数据不含标题"));
                    return;
                }
                var 标题 = 解析后["标题"];
                //处理异常标题数据包
                if (!this.m_CachedPacketTypeList.Contains(标题)) {
                    Send(客户端ID, ("标题", PktTypTab.数据错误), ("内容", $"未知数据标题，请检查游戏版本。当前服务器版本：{版本}。发来的标题：{标题}"));
                    return;
                }
                //进入数据处理流程并返回对应消息
                foreach ((string handlerId, PacketHandlerRegistry.PacketHandlerDelegate callback) _handler in this.m_PacketHandlerRegistry.GetPacketHandlers(标题))
                {
					_handler.callback(客户端ID, 解析后, this);
                }
            };
        }
        public void Start(int Tick) {
            if (Tick <= 8) {
                throw new ArgumentException($"Tick 必须大于 8。当前为：{Tick}");
            }
            this.Tick = Tick;
            server.Start(port);
            Log.Info($"[KCP] 玩家管理信道已启动，端口：{port}");
            计时器.Start();
        }
        public void Send(int 客户端ID, Dictionary<string, string> 数据) {
            server.Send(客户端ID, 数据.ToJson(格式美化: false).StringToBytes(), KcpChannel.Reliable);
        }
		public void Send(int 客户端ID, params (string, string)[] 数据) {
            var 返回数据 = new Dictionary<string, string>();
            foreach (var 内容 in 数据) {
                返回数据[内容.Item1] = 内容.Item2;
            }
            server.Send(客户端ID, 返回数据.ToJson(格式美化: false).StringToBytes(), KcpChannel.Reliable);
        }
		public void SendAll(params (string, string)[] 数据) {
            SendAll(null, 数据);
        }
		public void SendAll(int? 排除客户端, params (string, string)[] 数据) {
            var 返回数据 = new Dictionary<string, string>();
            foreach (var 内容 in 数据) {
                返回数据[内容.Item1] = 内容.Item2;
            }
            IEnumerable<int> 返回的客户端;
            if (排除客户端 != null) {
                返回的客户端 = server.connections.Keys.Except(new int[] { (int)排除客户端 });
            } else {
                返回的客户端 = server.connections.Keys;
            }
            foreach (var 客户端 in 返回的客户端) {
                server.Send(客户端, 返回数据.ToJson(格式美化: false).StringToBytes(), KcpChannel.Reliable);
            }
        }

        protected Stopwatch 计时器 = new();
        public void Update() {
            server.Tick();

            if (计时器.ElapsedMilliseconds > 1000 / Tick) {
                OnUpdate?.Invoke();
                计时器.Restart();
            }
        }

		/// <summary>
		/// 仅在初始化类时调用此方法。
		/// </summary>
		protected virtual void RegisterPacketHandler() {
			this.m_CachedPacketTypeList = new HashSet<string>(this.m_PacketHandlerRegistry.GetAllRegisteredPacketType());
			this.m_PacketHandlerRegistry.OnPacketTypeRegistryUpdated += delegate {
					this.m_CachedPacketTypeList = new HashSet<string>(this.m_PacketHandlerRegistry.GetAllRegisteredPacketType());
				};
			this.m_PacketHandlerRegistry.RegisterPacketType(PktTypTab.数据错误);
			this.m_PacketHandlerRegistry.RegisterPacketHandler(PktTypTab.数据错误, "战雷革命游戏服务器__写日志", 
				(_clientId, _mesgTab, _addArg) => {
					Log.Info($"[KCP] 收到错误标题数据：{_mesgTab.ToJson(格式美化: false)}");
				});
		}
	}
}