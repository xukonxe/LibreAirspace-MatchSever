using System;//Action

namespace CMKZ {
    public class 限数 {
        public double 上限;
        public double 下限 = 0;
        public double 当前;
        public double 比例 {
            get {
                if (上限 == 下限) {
                    return 1;
                }
                return (当前 - 下限) / (上限 - 下限);
            }
            set {
                //如果大于1或小于0则报错
                var A = value;
                //if (value is > 1 or < 0) {
                //    throw new Exception("错误：比例值必须在0-1之间");
                //}
                if (A > 1) {
                    A = 1;
                }
                if (A < 0) {
                    A = 0;
                }
                当前 = 下限 + (上限 - 下限) * A;
            }
        }
        public bool 已满 => 当前 == 上限;
        public bool 已空 => 当前 == 下限;
        public void SetToMax() => 当前 = 上限;
        public void SetToMin() => 当前 = 下限;
        public 限数() { }
        public 限数(double X, bool 拉满 = false) {
            //初始化的时候，当前值为0
            上限 = X;
            if (拉满) {
                当前 = X;
            }
        }
        public 限数(double X, double Y) {
            上限 = X;
            下限 = Y;
        }
        public double 增加_返回余数(double X) {
            当前 += X;
            if (当前 > 上限) {
                var A = 当前 - 上限;
                当前 = 上限;
                return A;//加多了
            }
            if (当前 < 下限) {
                var A = 下限 - 当前;
                当前 = 下限;
                return A;//减多了
            }
            return 0;
        }
        public 限数 增加(double X) {
            当前 += X;
            if (当前 > 上限) {
                当前 = 上限;
                return this;
            }
            if (当前 < 下限) {
                当前 = 下限;
                return this;
            }
            return this;
        }
        //隐式转化到double
        public static implicit operator double(限数 X) {
            return X.当前;
        }
        public 限数 Clone() {
            return new 限数 { 上限 = 上限, 下限 = 下限, 当前 = 当前 };
        }
        public override string ToString() {
            //return 当前 + "/" + 上限;
            return 上限.ToString();
        }
    }
}