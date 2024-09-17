using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using static TGZG.公共空间;
using TGZG;
using CMKZ;
using static CMKZ.LocalStorage;

namespace TGZG {
    public abstract class 网络信道类 {
        public TcpClient 游戏端;
        public string 服务端IP;
        public string 版本;
        public bool IsConnected => 游戏端 != null && 游戏端.Client.Online;
        //事件
        /// <summary>
        /// 连接成功时触发
        /// </summary>
        public event Action OnConnect;
        /// <summary>
        /// 连接失败时触发
        /// </summary>
        public event Action OnConnecFail;
        /// <summary>
        /// 断开连接时触发
        /// </summary>
        public event Action OnDisconnect;
        public 网络信道类(string IP_Port, string version) {
            服务端IP = IP_Port;
            版本 = version;
        }
        /// <summary>
        /// 尝试连接，成功触发OnConnect事件，失败触发OnConnecFail事件
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void 连接() {
            if (服务端IP == null) {
                throw new Exception("服务端IP不能为空");
            }
            断开();
            游戏端 = new TcpClient(服务端IP, 版本);
            游戏端.OnConnect += () => {
                OnConnect?.Invoke();
            };
            游戏端.OnDisconnect += (e) => {
                OnDisconnect?.Invoke();
            };
            游戏端.OnReceive += (t, C) => {
                var A = t.JsonToCS<Dictionary<string, string>>(false, false);
                if (A.ContainsKey("错误")) {

                }
            };
            if (!游戏端.Start()) {
                OnConnecFail?.Invoke();
            }
        }
        public void 断开() {
            游戏端?.Stop();
            游戏端 = null;
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
    }
}