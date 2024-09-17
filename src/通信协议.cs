using kcp2k;
using System.Numerics;
using System.Text;
using static CMKZ.LocalStorage;
using static TGZG.战雷革命游戏服务器.公共空间;

namespace TGZG.战雷革命游戏服务器 {
    public static partial class 公共空间 {

    }
    public struct 玩家游玩数据 {
        public 玩家登录数据 u;
        public 玩家世界数据 p;
        public 队伍 tm;
        public int[] 射;
        public List<导弹飞行数据> msl;
        public void 三位保留() {
            //所有float只保留三位小数
            p.p = p.p.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.d = p.d.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.v = p.v.Select(t => (float)Math.Round(t, 3)).ToArray();
            p.r = p.r.Select(t => (float)Math.Round(t, 3)).ToArray();
            for (int i = 0; i < msl.Count; i++) {
                var n = new 导弹飞行数据();
                n.i = msl[i].i;
                n.tp = msl[i].tp;
                n.p = msl[i].p.Select(t => (float)Math.Round(t, 3)).ToArray();
                n.d = msl[i].d.Select(t => (float)Math.Round(t, 3)).ToArray();
                n.v = msl[i].v.Select(t => (float)Math.Round(t, 3)).ToArray();
                n.r = msl[i].r.Select(t => (float)Math.Round(t, 3)).ToArray();
                msl[i] = n;
            }
        }
    }
    public struct 导弹飞行数据 {
        public int i;
        public 导弹类型 tp;
        public float[] p;
        public float[] d;
        public float[] v;
        public float[] r;
    }
    public enum 导弹类型 {
        AIM9E,
    }
    public enum 部位 {
        无,
        身,
        左外,
        右外,
        左内,
        右内,
        左尾,
        右尾,
        垂,
    }
    public struct 击伤信息 {
        public string ths;
        public float dm;
        public 部位 bp;
    }
    public struct 玩家登录数据 {
        public string n;
        public 载具类型 tp;
    }
    public struct 玩家世界数据 {
        public float[] p;
        public float[] d;
        public float[] v;
        public float[] r;
        public static 玩家世界数据 初始化() {
            var data = new 玩家世界数据();
            data.p = [0, 5, 0];
            data.d = [1, 0, 0, 0];
            data.v = [0, 0, 0];
            data.r = [0, 0, 0];
            return data;
        }
    }
    public enum 载具类型 {
        无,
        m15n23,
        f86f25,
        f4c,
        m21pfm,
        P51h
    }
    public enum 队伍 {
        无,
        蓝,
        红,
        系统
    }
}