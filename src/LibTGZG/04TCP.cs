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
using System.Linq;

namespace CMKZ {
    public class TcpClient {
        public Action OnConnect;
        public Action<ITcpClientBase> OnDisconnect;
        public Action<string> OnSend;
        public Action<string, TouchSocket.Sockets.TcpClient> OnReceive;
        public Dictionary<string, Action<Dictionary<string, string>>> OnRead = new();
        public string IP = "127.0.0.1:7789";
        public TouchSocket.Sockets.TcpClient Client = new();
        public int ID = 0;
        public Dictionary<string, Action<Dictionary<string, string>>> Success = new();
        public TcpClient() {

        }
        public TcpClient(string X) {
            IP = X;
        }
        public TcpClient(string X, string Y) {
            IP = X;
            OnConnect += () => {
                Send(new() { { "标题", "_版本检测" }, { "版本", Y } }, t => {
                    if (t["版本正确"] == "错误") {
                        Client.Close();
                    }
                });
            };
        }
        public void Stop() {
            Client.Close();
        }
        public bool Start() {
            Client.Connected = (client, e) => OnConnect?.Invoke();
            Client.Disconnected = (client, e) => OnDisconnect?.Invoke(client);
            Client.Received = (client, byteBlock, requestInfo) => {
                lock (Success) {
                    OnReceive?.Invoke(byteBlock.ToString(), client);
                    var A = Encoding.UTF8.GetString(byteBlock.Buffer, 0, byteBlock.Len).JsonToCS<Dictionary<string, string>>(true, true);
                    if (A.ContainsKey("_ID")) {
                        Success[A["_ID"]](A);
                        Success.RemoveKey(A["_ID"]);
                    } else if (OnRead.ContainsKey(A["标题"])) {
                        try {
                            OnRead[A["标题"]](A);
                        } catch (Exception ex) {
                            //输出红色错误信息
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"处理消息[\"{A["标题"]}\"]时遇到错误：" + ex.Message);
                            Console.ResetColor();
                        }
                    }
                }
            };
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
        public void Send(Dictionary<string, string> X, Action<Dictionary<string, string>> Y = null) {
            lock (Success) {
                try {
                    X["_ID"] = ID++.ToString();
                    if (Y != null) Success[X["_ID"]] = Y;
                    var A = X.ToJson(false, false);
                    Client.Send(A);
                    OnSend?.Invoke(A);
                } catch {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Print("TCP发送失败");
                    Console.ResetColor();
                    Success.RemoveKey(X["_ID"]);
                }
            }
        }
        public async Task<Dictionary<string, string>> SendAsync(Dictionary<string, string> X) {
            var tcs = new TaskCompletionSource<Dictionary<string, string>>();
            Send(X, result => {
                tcs.SetResult(result);
            });
            return await tcs.Task;
        }
    }
    public class TcpServer {
        public int Port;
        public Dictionary<string, Func<Dictionary<string, string>, SocketClient, Dictionary<string, string>>> OnRead = new();
        public Action<string, SocketClient> OnReceive;
        public Action<SocketClient> OnConnect;
        public Action<SocketClient> OnDisconnect;
        public string Version;
        public TcpService Server = new TcpService();
        public TcpServer(int port) {
            Port = port;
        }
        public TcpServer(int port, string Y) {
            Port = port;
            Version = Y;
        }
        public void Start() {
            Server.Connected = (client, e) => {
                OnConnect?.Invoke(client);
            };
            Server.Disconnected = (client, e) => {
                OnDisconnect?.Invoke(client);
            };
            Server.Received = (client, byteBlock, requestInfo) => {
                OnReceive?.Invoke(byteBlock.ToString(), client);
                var A = byteBlock.ToString().JsonToCS<Dictionary<string, string>>(false, false);
                var B = OnRead[A["标题"]](A, client);
                if (B != null) {
                    B["_ID"] = A["_ID"];
                    client.Send(B.ToJson(false, false));//返回消息
                }
            };
            OnRead["_版本检测"] = (t, c) => {
                if (t["版本"] == Version) {
                    return new Dictionary<string, string> { { "版本正确", "正确" } };
                } else {
                    return new Dictionary<string, string> { { "版本正确", "错误" } };
                }
            };
            OnRead["测试信息"] = (t, c) => {
                return new() { { "返回", $"您发来的消息是 {t["内容"]}" } };
            };
            Server.Setup(new TouchSocketConfig()
                .SetListenIPHosts(new IPHost[] { new IPHost(Port) })
                .SetDataHandlingAdapter(() => new FixedHeaderPackageAdapter() { FixedHeaderType = FixedHeaderType.Int }))
            .Start();
        }
        public void AllSend(Dictionary<string, string> X) {
            foreach (var i in Server.GetClients()) {
                i.Send(X.ToJson(false));
            }
        }
        public void Send(SocketClient X, Dictionary<string, string> Y) {
            X.Send(Y.ToJson(false));
        }
        public void Send(string ip, Dictionary<string, string> Y) {
            var 客户端 = Server.GetClients().FirstOrDefault(i => i.IP == ip);
            if (客户端 == null || !客户端.Online) return;
            客户端.Send(Y.ToJson(false));
        }
        public async void SendAsync(string ip, Dictionary<string, string> Y) {
            var 客户端 = Server.GetClients().FirstOrDefault(i => i.IP == ip);
            if (客户端 == null || !客户端.Online) return;
            await 客户端.SendAsync(Y.ToJson(false));
        }
    }
}
