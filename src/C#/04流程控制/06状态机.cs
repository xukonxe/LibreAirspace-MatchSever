using System;

namespace CMKZ {
    public static partial class LocalStorage {

    }
    public class 状态机 : CMKZ_Dictionary<string, 状态机状态> {
        public string 当前状态;
        public void Update() {
            foreach (var i in this[当前状态].切换) {
                if (i.Key()) {
                    this[当前状态].OnExit?.Invoke();
                    当前状态 = i.Value;
                    this[当前状态].OnEnter?.Invoke();
                    return;
                }
            }
            this[当前状态].Update?.Invoke();
        }
        public 状态机状态 Add(string X) {
            return this[X] = new 状态机状态();
        }
    }
    public class 状态机状态 {
        public Action OnEnter;
        public Action OnExit;
        public CMKZ_Dictionary<Func<bool>, string> 切换;
        public Action Update;
        public 状态机状态 SetOnEnter(Action X) {
            OnEnter = X;
            return this;
        }
        public 状态机状态 SetOnExit(Action X) {
            OnExit = X;
            return this;
        }
        public 状态机状态 SetOnUpdate(Action X) {
            Update = X;
            return this;
        }
        public 状态机状态 SetTurn(Func<bool> X, string Y) {
            切换[X] = Y;
            return this;
        }
    }
}