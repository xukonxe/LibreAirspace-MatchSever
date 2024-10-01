using System;
using System.IO;
using System.Linq;
using System.Text;
using static CMKZ.LocalStorage;

namespace TGZG.战雷革命游戏服务器 {
    public static partial class 公共空间 {
        public static string 版本 => "v0.13";
        public static 房间参数类 房间数据;
        public static 积分数据 房间积分;
        //==========端口定义===========
        //16312:服务器<=>房间服务器
        //16313:客户端<=>房间服务器
        //16314:客户端<=>服务器
        //============================

        //16312:服务器 <=> 房间服务器 <=> 客户端
        //         ^ 16312     16313 ^
        //        ||      16314      ||
        //        ||=================||

        //============================
        public static 房间管理信道类 房间管理信道 = new("47.97.112.35:16312", "0.0.5");
        //public static 房间管理信道类 房间管理信道 = new("127.0.0.1:16312", "0.0.5");
        public static 玩家管理信道类 玩家管理信道 = new(16314, 版本);
        public static WTRev.TKTLib.Modding.ModManager.ModManager 模组管理器 = null;

        public static int 启动() {
            if (GetCfg() != 0) {
                return 1;
            }
            InitModSystem();

            房间数据.房间版本 = 版本;
            房间数据.人数 = 0;
            房间数据.房间创建时间 = DateTime.UtcNow;
            房间数据.模组列表 = 模组管理器.GetLoadedMods().Select<WTRev.TKTLib.Modding.InfoCls.ModInfo, TGZG.战雷革命游戏服务器.ModInfo>(
                _ModSys_ModInfo => new TGZG.战雷革命游戏服务器.ModInfo() {
                    _author = _ModSys_ModInfo.Author,
                    _description = _ModSys_ModInfo.Description,
                    _guid = _ModSys_ModInfo.Guid,
                    _name = _ModSys_ModInfo.Name,
                    _version = _ModSys_ModInfo.Version,
                    _modPackSha512SumAsBase64EncodedString = Convert.ToBase64String(_ModSys_ModInfo.m_ModPackSha512Sum)
                }).ToArray();

            玩家管理信道.Start(房间数据.每秒同步次数);

            $"当前房间配置:\n{房间数据.ToJson()}".logwarning();
            if (房间数据.模式 is 模式类型.休闲) {
                休闲计分板.On计分板更新 += t => {
                    (计分板数据, 计分板数据) 数据;
                    lock (休闲计分板) {
                        数据 = t.To计分板数据();
                    }
                    玩家管理信道.更新计分板(数据);
                };
            }

            房间管理信道.连接();
            房间管理信道.发送注册房间();

            while (true) {
                玩家管理信道.Update();
                //string 指令 = Console.ReadLine();
            }
        }

        private static int GetCfg() {
            var 当前程序工作目录路径 = Directory.GetCurrentDirectory();
            var 配置路径 = Path.Combine(当前程序工作目录路径, "房间配置.json");
            if (!File.Exists(配置路径)) {
                var 默认配置 = new 房间参数类() {
                    房间名 = "沈伊利服务器",
                    房间描述 = "aaa",
                    房主 = "沈伊利",
                    人数 = 0,
                    地图名 = "休闲地图24.9.21",
                    房间密码 = "",
                    每秒同步次数 = 32,
                    模式 = 模式类型.休闲,
                    可选载具 = [载具类型.P51h],
                    可选队伍 = [队伍.红, 队伍.蓝],
                };
                //将默认配置写入配置文件
                var json = 默认配置.ToJson();
                File.WriteAllText(配置路径, json, Encoding.UTF8);
            }
            try {
                房间数据 = File.ReadAllText(配置路径, Encoding.UTF8).JsonToCS<房间参数类>();
            } catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.WriteLine("配置文件读取失败，请检查配置格式。");
                return 1;
            }
            return 0;
        }

        private static void InitModSystem() {
            模组管理器 = new WTRev.TKTLib.Modding.ModManager.ModManager(isServerSide: true);

            string _当前程序工作目录路径 = Directory.GetCurrentDirectory();
            string _模组包目录 = Path.Combine(_当前程序工作目录路径, "Mod");
            if (Directory.Exists(_模组包目录)) {
                foreach (FileInfo _ModPakFile in new DirectoryInfo(_模组包目录).GetFiles("*", SearchOption.AllDirectories)) {
                    using (Stream _ModPakFileStream = _ModPakFile.OpenRead()) {
                        WTRev.TKTLib.Modding.InfoCls.ModOperateResult _Result =
                            模组管理器.LoadMod(_ModPakFileStream);
                        if ((_Result & WTRev.TKTLib.Modding.InfoCls.ModOperateResult.失败) != 0) {
                            TGZG.公共空间.Log($"加载模组包文件\"{_ModPakFile}\"失败！错误值: {string.Format("0x{0:X16}", (ulong)_Result)}");
                        }
                    }
                }
            }
        }
    }
}