using System;
using System.Collections.Concurrent;
using System.Text;
using TouchSocket.Core;
using TouchSocket.Sockets;
using CMKZ;
using static CMKZ.LocalStorage;
using static TGZG.公共空间;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using TGZG.Net;
using static TGZG.Net.PacketHandlerRegistry;
using PktTypTab = TGZG.战雷革命游戏服务器.CommunicateConstant.PacketType;

namespace TGZG.Net {
	public class TcpClient {
        public Action OnConnect;
        public Action<ITcpClientBase> OnDisconnect;
        public Action<string> OnSend;
        public Action<string, TouchSocket.Sockets.TcpClient> OnReceive;

        public 数据包处理器注册表_Tcp客户端 m_PacketHandlerRegistry = new();
		protected HashSet<string> m_CachedPacketTypeList = null;
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Should always lock this variable before accessing this variable.
		/// </remarks>
		public Dictionary</* RPC请求的标识符 */string, /* 回调函数 */Action<Dictionary<string, string>>> RpcResponseHandler = new();

		public string IP = "127.0.0.1:7789";
        public TouchSocket.Sockets.TcpClient Client = new();
        public int ID = 0;

		public string protocolVersion;
		public bool IsConnected => this.Client.Online;
        public TcpClient(string ip, string protocolVersion) {
            IP = ip;
            OnConnect += () => {
                Send(new() { { "标题", "_版本检测" }, { "版本", protocolVersion } }, t => {
                    if (t["版本正确"] == "错误") {
                        Client.Close();
                    }
                });
            };
			Client.Connected = (client, e) => OnConnect?.Invoke();
			Client.Disconnected = (client, e) => OnDisconnect?.Invoke(client);
			Client.Received = (client, byteBlock, requestInfo) => {
				lock (RpcResponseHandler) {
					OnReceive?.Invoke(byteBlock.ToString(), client);
					Dictionary<string, string> message = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len).JsonToCS<Dictionary<string, string>>(true, true);
					if (message.ContainsKey("_ID")) {
						RpcResponseHandler[message["_ID"]](message);
						RpcResponseHandler.RemoveKey(message["_ID"]);
					} else if (message.ContainsKey("标题")) {
						string _messageType = message["标题"];
						try {
							foreach (var _handler in this.m_PacketHandlerRegistry.GetPacketHandlers(_messageType)) {
								_handler.callback(0/* not used*/, message, this);
							}
						} catch (Exception ex) {
							//输出红色错误信息
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine($"处理消息[\"{_messageType}\"]时遇到错误：" + ex.Message);
							Console.ResetColor();
						}
					}
				}
			};
			OnReceive = (t, C) => {
				var A = t.JsonToCS<Dictionary<string, string>>(false, false);
				if (A.ContainsKey("错误")) {

				}
			};
		}

        public void Disconnect() {
            Client.Close();
        }

        public bool Connect() {
			Disconnect();
			Client.Setup(new TouchSocketConfig()
                .SetRemoteIPHost(new IPHost(IP))
                .UsePlugin()
                //.ConfigurePlugins(a => a.UseReconnection(5, true, 1000))
                .SetBufferLength(1024 * 64)
                .SetDataHandlingAdapter(() => new FixedHeaderPackageAdapter() { FixedHeaderType = FixedHeaderType.Int }));
            try {
                Client.Connect();
            } catch (Exception e) {
                Console.ForegroundColor = ConsoleColor.Red;
                $"TCP连接失败：{e.Message}".log();
                Console.ForegroundColor = ConsoleColor.White;
                return false;
			}
			if (IP == null) {
				throw new Exception("服务端IP不能为空");
			}
			return true;
        }
        public async Task<string> GetPing() {
            var 服务器发送时间 = await SendAsync(new() { { "标题", "_GetPing" } });
            var 当前时间 = DateTime.Now;
            return (当前时间 - 服务器发送时间["_服务器发送时间"].JsonToCS<DateTime>(false, false)).TotalMilliseconds.ToString() + "ms";
        }
        public async Task<string> Get丢包() {
            return "123";
        }
        public async Task<string> Get传输速度() {
            return "123";
		}
		public double Ping(string IP) {
			Ping ping = new Ping();
			PingReply reply = ping.Send(IP);
			// 检查回复状态
			if (reply.Status == IPStatus.Success) {
				return reply.RoundtripTime;
			} else {
				("Ping失败。状态: " + reply.Status).log();
				return -1;
			}
		}
		public void Send(Dictionary<string, string> message, Action<Dictionary<string, string>> Rpc返回结果回调函数 = null) {
            lock (RpcResponseHandler) {
                try {
                    message["_ID"] = ID++.ToString();
                    if (Rpc返回结果回调函数 != null) RpcResponseHandler[message["_ID"]] = Rpc返回结果回调函数;
                    var A = message.ToJson(false, false);
                    Client.Send(A);
                    OnSend?.Invoke(A);
                } catch {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Print("TCP发送失败");
                    Console.ResetColor();
                    RpcResponseHandler.RemoveKey(message["_ID"]);
                }
            }
        }
        public async Task<Dictionary<string, string>> SendAsync(Dictionary<string, string> X) {
            var tcs = new TaskCompletionSource<Dictionary<string, string>>();
            Send(X, tcs.SetResult);
            return await tcs.Task;
        }
    }
}
