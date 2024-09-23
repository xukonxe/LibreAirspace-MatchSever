using System;
using System.Text;
using static CMKZ.LocalStorage;

namespace TGZG.战雷革命游戏服务器 {
	public static partial class 公共空间 {
        public static string 版本 => "v0.10-Beta";
        public static 房间参数类 房间数据;
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
        public static 房间管理信道类 房间管理信道 = new("127.0.0.1:16312", "0.0.4");
        //public static 房间管理信道类 房间管理信道 = new("47.97.112.35:16312", "0.0.3");
        public static 玩家管理信道类 玩家管理信道 = new(16314, 版本);

        public static int 启动() {
			if (GetCfg() != 0) {
				return 1;
			}
            房间数据.房间版本 = 版本;
            房间数据.人数 = 0;


            玩家管理信道.Start(房间数据.每秒同步次数);

            房间管理信道.连接();
            房间管理信道.发送注册消息();

            while (true) {
                玩家管理信道.Update();
                //string 指令 = Console.ReadLine();
            }
        }

		public static int GetCfg() {
			var 当前程序路径 = System.IO.Directory.GetCurrentDirectory();
			var 配置路径 = System.IO.Path.Combine(当前程序路径, "房间配置.json");
			if (!System.IO.File.Exists(配置路径)) {
				var 默认配置 = new 房间参数类() {
					房间名 = "沈伊利服务器",
					房间描述 = "aaa",
					房主 = "沈伊利",
					人数 = 0,
					地图名 = "测试地图",
					房间密码 = "",
					每秒同步次数 = 32,
				};
				//将默认配置写入配置文件
				var json = 默认配置.ToJson();
				System.IO.File.WriteAllText(配置路径, json, Encoding.UTF8);
			}
			try {
				房间数据 = System.IO.File.ReadAllText(配置路径, Encoding.UTF8).JsonToCS<房间参数类>();
			} catch (Exception e) {
				Console.WriteLine(e.Message);
				Console.WriteLine("配置文件读取失败，请检查配置格式。");
				return 1;
			}
			return 0;
		}
    }
}