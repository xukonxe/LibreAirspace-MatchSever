using kcp2k;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CMKZ.LocalStorage;
using static TGZG.战雷革命游戏服务器.公共空间;

namespace TGZG.战雷革命游戏服务器 {
    public static partial class 公共空间 {
        public static Dictionary<int, 玩家游玩数据> 所有玩家 = new();
        public static 休闲模式计分板 休闲计分板 = new();
    }
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
    public class 玩家管理信道类 : 网络信道服务器类_KCP {
        public 玩家管理信道类(ushort 端口, string 版本) : base(端口, 版本) {
            //处理客户端发来的消息
            OnRead["更新位置"] = (客户端ID, t) => {
                var 数据 = t["数据"].JsonToCS<玩家游玩数据>(false);
                var 老数据 = 所有玩家[客户端ID];
                lock (所有玩家) {
                    所有玩家[客户端ID] = 数据;
                }
                lock (休闲计分板) {
                    休闲计分板.玩家位置更新(所有玩家[客户端ID].u.n, 老数据.p, 数据.p);
                }
                return null;
            };
            OnRead["发送聊天消息"] = (客户端ID, t) => {
                var 发送者 = t["发送者"];
                var 内容 = t["内容"];
                var 广播内容 = 内容;
                lock (休闲计分板) {
                    休闲计分板.玩家聊天(发送者);
                }
                广播玩家消息(客户端ID, 广播内容, 队伍.无);
                return null;
            };
            OnRead["重生"] = (客户端ID, t) => {
                广播重生消息(所有玩家[客户端ID], 客户端ID);
                return null;
            };
            OnRead["导弹发射"] = (客户端ID, t) => {
                var 数据 = t["数据"].JsonToCS<导弹飞行数据>(false);
                广播导弹发射消息(所有玩家[客户端ID], 数据);
                return null;
            };
            OnRead["导弹爆炸"] = (客户端ID, t) => {
                var 数据 = t["数据"].JsonToCS<导弹飞行数据>(false);
                广播导弹爆炸消息(所有玩家[客户端ID], 数据);
                return null;
            };
            //发来此消息者，表示自己已被损坏。
            OnRead["损坏"] = (客户端ID, t) => {
                string 攻击者 = t["攻击者"];
                部位 数据 = Enum.Parse<部位>(t["数据"]);
                //收到损坏信息时，找到此玩家，并广播给其他玩家
                var 玩家客户端信息 = 所有玩家.FirstOrDefault(t => t.Key == 客户端ID);
                if (玩家客户端信息.Key == default) return null;
                var 玩家名称 = 玩家客户端信息.Value.u.n;
                if (数据 is 部位.身) {
                    发送死亡信息(客户端ID);
                    广播死亡消息(玩家名称);
                    lock (休闲计分板) {
                        休闲计分板.玩家死亡(玩家名称);
                        休闲计分板.玩家击杀(攻击者);
                    }
                    发送击杀提示(攻击者, 玩家名称);
                }
                return null;
            };
            //发来此消息者，表示成功攻击其他玩家，执行响应操作
            OnRead["击伤"] = (客户端ID, t) => {
                击伤信息 数据 = t["数据"].JsonToCS<击伤信息>();
                //收到击伤信息时，找到被击伤的玩家，向它发送击伤消息
                var 玩家客户端信息 = 所有玩家.FirstOrDefault(t => t.Value.u.n == 数据.ths);
                if (玩家客户端信息.Key == default) return null;
                var 玩家客户端ID = 玩家客户端信息.Key;
                var 玩家名称 = 玩家客户端信息.Value.u.n;

                var 攻击者客户端信息 = 所有玩家.FirstOrDefault(t => t.Key == 客户端ID);
                if (攻击者客户端信息.Key == default) return null;
                var 攻击者名称 = 攻击者客户端信息.Value.u.n;

                lock (休闲计分板) {
                    休闲计分板.玩家击伤(攻击者名称);
                }

                发送击伤消息(玩家客户端ID, 数据);
                广播系统消息("系统消息", $"{攻击者名称} 击中了 {玩家名称},对 {数据.bp.ToString()} 造成 {数据.dm} 点伤害");
                return null;
            };

            OnRead["验证登录"] = (客户端ID, t) => {
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

                    return [
                        ("标题", "登录验证结果"),
                        ("状态", "成功"),
                        ("消息", 验证结果.内容)];
                } else
                    return [
                        ("标题", "登录验证结果"),
                        ("状态", "失败"),
                        ("消息", 验证结果.内容)];
            };
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

        public void 更新计分板((计分板数据 蓝队数据, 计分板数据 红队数据) 计分板数据) {
            "更新计分板".log();
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
        public void 广播重生消息(玩家游玩数据 玩家数据, int 排除客户端ID) {
            SendAll(排除客户端ID,
                ("标题", "同步重生"),
                ("玩家", 玩家数据.ToJson()));
        }
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
    public class 网络信道服务器类_KCP {
        public int Tick = 128;
        public KcpServer server;
        ushort port;
        string 版本;
        public event Action<int> OnConnected;
        public event Action<int, ArraySegment<byte>, KcpChannel> OnData;
        public event Action<int> OnDisconnected;
        public event Action<int, ErrorCode, string> OnError;

        public event Action OnUpdate;

        public Dictionary<string, Func<int, Dictionary<string, string>, (string, string)[]>> OnRead = new();
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
                Timeout: 2000,
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
            OnDisconnected = (cId) => { };
            OnError = (cId, error, reason) => {
                $"[KCP] OnServerError({cId}, {error}, {reason}".logerror();
            };


            OnRead["数据错误"] = (c, t) => {
                $"[KCP] 收到错误标题数据：{t.ToJson(格式美化: false)}".logerror();
                return null;
            };
            OnData += (客户端ID, 数据, 频道) => {
                //处理空数据包
                if (数据.Array == null) {
                    $"[KCP] 收到空数据包，客户端ID：{客户端ID}，频道：{频道}".logerror();
                    //Send(客户端ID, ("标题", "数据错误"), ("内容", "数据内容为空"));
                    return;
                }
                //转码数据包
                string 数据json = Encoding.UTF8.GetString(数据.Array, 数据.Offset, 数据.Count);
                var 解析后 = 数据json.JsonToCS<Dictionary<string, string>>();
                //处理无标题数据包
                if (!解析后.ContainsKey("标题")) {
                    $"[KCP] 收到无标题数据包，客户端ID：{客户端ID}，频道：{频道}".logerror();
                    //Send(客户端ID, ("标题", "数据错误"), ("内容", "数据不含标题"));
                    return;
                }
                var 标题 = 解析后["标题"];
                //处理异常标题数据包
                if (!OnRead.ContainsKey(标题)) {
                    $"[KCP] 未知数据标题，请检查游戏版本。当前服务器版本：{版本}。发来的标题：{标题}".logerror();
                    //Send(客户端ID, ("标题", "数据错误"), ("内容", $"未知数据标题，请检查游戏版本。当前服务器版本：{版本}。发来的标题：{标题}"));
                    return;
                }
                //进入数据处理流程并返回对应消息
                var 返回消息 = OnRead[标题].Invoke(客户端ID, 解析后);
                if (返回消息 != null) Send(客户端ID, 返回消息);
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

        public Stopwatch 计时器 = new();
        public void Update() {
            server.Tick();

            if (计时器.ElapsedMilliseconds > 1000 / Tick) {
                OnUpdate?.Invoke();
                计时器.Restart();
            }
        }
    }
    public class 房间参数类 {
        public string 房间名;
        public string 房间描述;
        public string 房主;
        public int 人数;
        public string 地图名;
        public string 房间密码;
        public string 房间版本;
        public int 每秒同步次数;
        public DateTime 房间创建时间;
        //C/S端模组同步逻辑这块还待补充。
        public ModInfo[] 模组列表;
        public 模式类型 模式;
        public List<载具类型> 可选载具;
        public List<队伍> 可选队伍;
    }
    public enum 模式类型 {
        休闲,
        竞技,
        自定义
    }

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
    [JsonObject]
    public class ModInfo {
        [JsonProperty(Required = Required.Always, PropertyName = "Name")]
        internal string _name;
        [JsonProperty(Required = Required.Always, PropertyName = "Description")]
        internal string _description;
        [JsonProperty(Required = Required.Always, PropertyName = "Author")]
        internal string _author;
        [JsonProperty(Required = Required.Always, PropertyName = "Version")]
        internal string _version;
        [JsonProperty(Required = Required.Always, PropertyName = "Guid")]
        internal string _guid;
        [JsonProperty(Required = Required.Always, PropertyName = "ModPackSha512SumAsBase64EncodedString")]
        internal string _modPackSha512SumAsBase64EncodedString;

        /// <summary>
        /// 模组包的Sha512校验和(以BASE64编码)。
        /// </summary>
        [JsonIgnore]
        public string ModPackSha512SumAsBase64EncodedString { get => this._modPackSha512SumAsBase64EncodedString; }

        /// <summary>
        /// 模组的一般名称
        /// </summary>
        [JsonIgnore]
        public string Name { get => this._name; }

        /// <summary>
        /// 模组的描述
        /// </summary>
        [JsonIgnore]
        public string Description { get => this._description; }

        /// <summary>
        /// 模组的作者
        /// </summary>
        [JsonIgnore]
        public string Author { get => this._author; }

        /// <summary>
        /// 模组的版本
        /// </summary>
        [JsonIgnore]
        public string Version { get => this._version; }

        /// <summary>
        /// 模组的GUID标识符
        /// </summary>
        [JsonIgnore]
        public string Guid { get => this._guid; }
    }
}