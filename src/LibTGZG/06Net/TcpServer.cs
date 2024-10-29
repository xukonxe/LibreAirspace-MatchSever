using System;
using System.Collections.Generic;
using TouchSocket.Core;
using TouchSocket.Sockets;
using System.Linq;
using PktTypTab = TGZG.战雷革命游戏服务器.CommunicateConstant.PacketType;
using CMKZ;

namespace TGZG.Net {
	public class TcpServer {
        public int Port;
		protected 数据包处理器注册表_Tcp服务器 m_PacketHandlerRegistry = new();
		protected HashSet<string> m_CachedPacketTypeList = null;
		public Action<string, SocketClient> OnReceive;
        public Action<SocketClient> OnConnect;
        public Action<SocketClient> OnDisconnect;
        public string Version;
        public TcpService Server = new TcpService();

        public TcpServer(int port, string protocolVersion) {
            Port = port;
            Version = protocolVersion;

			Server.Connected = (client, e) => {
				OnConnect?.Invoke(client);
			};
			Server.Disconnected = (client, e) => {
				OnDisconnect?.Invoke(client);
			};
			Server.Received = (client, byteBlock, requestInfo) => {
				OnReceive?.Invoke(byteBlock.ToString(), client);
				Dictionary<string, string> 解析后 = byteBlock.ToString().JsonToCS<Dictionary<string, string>>(false);
				try {
					string 标题 = 解析后["标题"];
					//在此处调用数据包包类型特定的回调函数
					//this.m_PacketHandlerRegistry.GetPacketHandlers
					foreach ((string handlerId, TGZG.Net.PacketHandlerRegistry.PacketHandlerDelegate<SocketClient,object> callback) _handler in this.m_PacketHandlerRegistry.GetPacketHandlers(标题)) {
						_handler.callback(client, 解析后, this);
					}
				} catch (Exception ex) {
					//输出红色错误信息
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine($"处理消息[\"{解析后["标题"]}\"]时遇到错误：" + ex.Message);
					Console.ResetColor();
				}
				/*
				if (B != null) {
					B["_ID"] = A["_ID"];
					client.Send(B.ToJson(false));//返回消息
				}
				*/
			};

			this.RegisterPacketHandler();
		}
        public void Start() {
            Server.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { new IPHost(Port) })
                .SetDataHandlingAdapter(() => new FixedHeaderPackageAdapter() { FixedHeaderType = FixedHeaderType.Int }))
            .Start();
        }

		/// <summary>
		/// 仅在初始化类时调用此方法。
		/// </summary>
		protected virtual void RegisterPacketHandler() {
			this.m_CachedPacketTypeList = new HashSet<string>(this.m_PacketHandlerRegistry.GetAllRegisteredPacketType());
			this.m_PacketHandlerRegistry.OnPacketTypeRegistryUpdated += delegate {
				this.m_CachedPacketTypeList = new HashSet<string>(this.m_PacketHandlerRegistry.GetAllRegisteredPacketType());
			};
			this.m_PacketHandlerRegistry.RegisterPacketType(PktTypTab._版本检测, ignoreReserved: true);
			this.m_PacketHandlerRegistry.RegisterPacketHandler(PktTypTab._版本检测, "自由空域房间列表服务器__处理检测协议版本",
				(c, t, addArg) => {
					if (t["版本"] == Version) {
						Send(c, new Dictionary<string, string> { { "版本正确", "正确" } });
					} else {
						Send(c, new Dictionary<string, string> { { "版本正确", "错误" } });
					}
				});
			this.m_PacketHandlerRegistry.RegisterPacketType(PktTypTab.测试信息);
			this.m_PacketHandlerRegistry.RegisterPacketHandler(PktTypTab.测试信息, "自由空域房间列表服务器__处理客户端测试消息",
				(c, t, addArg) => {
					SendRpcReturnValue(c, new Dictionary<string, string>() { { "返回", $"您发来的消息是 {t["内容"]}" } }, t);
				});
		}

		public void AllSend(Dictionary<string, string> X) {
            foreach (var i in Server.GetClients()) {
                i.Send(X.ToJson(false));
            }
        }
        public void Send(SocketClient client, Dictionary<string, string> sendToClient) {
            client.Send(sendToClient.ToJson(false));
        }
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// <para>NOTE: this method will modify <paramref name="sendToClient"/>.</para>
		/// <para>The method invoking this method usually will return immediately after this method returns.</para>
		/// </remarks>
		/// <param name="client"></param>
		/// <param name="sendToClient"></param>
		/// <param name="dataFromClient"></param>
		public void SendRpcReturnValue(SocketClient client, Dictionary<string, string> sendToClient, Dictionary<string, string> dataFromClient) {
			sendToClient["_ID"] = dataFromClient["_ID"];
			this.Send(client, sendToClient);
		}
        public async void SendAsync(string ip, Dictionary<string, string> Y) {
            var 客户端 = Server.GetClients().FirstOrDefault(i => i.IP == ip);
            if (客户端 == null || !客户端.Online) return;
            await 客户端.SendAsync(Y.ToJson(false));
        }
    }
}
