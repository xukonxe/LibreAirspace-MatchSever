using System;

namespace CMKZ {
    public class CMKZList<T> : System.Collections.Generic.List<T> {
        public event Action<T, int> OnAdd;
        public event Action<T, int> OnRemove;
        //public new void Add(T X) {
        //    base.Add(X);
        //    OnAdd?.Invoke(X, Count - 1);
        //}
        public new CMKZList<T> Add(T X) {
            base.Add(X);
            OnAdd?.Invoke(X, Count - 1);
            return this;
        }
        public new void Remove(T X) {
            var A = IndexOf(X);
            base.Remove(X);
            OnRemove?.Invoke(X, A);
        }
        public void Remove(Func<T, bool> X) {
            for (int i = Count - 1; i >= 0; i--) {
                if (X(this[i])) {
                    RemoveAt(i);
                }
            }
        }
        public CMKZList<T> Remove<T1>() {
            RemoveAll(t => t is T1);
            return this;
        }
        public new void RemoveAt(int X) {
            var A = this[X];
            base.RemoveAt(X);
            OnRemove?.Invoke(A, X);
        }
        public new void Insert(int X, T Y) {
            base.Insert(X, Y);
            OnAdd?.Invoke(Y, X);
        }
        public new void Clear() {
            foreach (var i in this) {
                OnRemove?.Invoke(i, IndexOf(i));
            }
            base.Clear();
        }
        public CMKZList() : base() { }
        public CMKZList(int X) : base(X) {
            //如果T拥有构造函数，那么创建X个实例并加入列表
            if (typeof(T).GetConstructors().Length > 0) {
                for (int i = 0; i < X; i++) {
                    Add((T)Activator.CreateInstance(typeof(T)));
                }
            }
        }
        public CMKZList(T[] X) : base(X) { }
        //定义字段【分页数】：如果有50个元素，那么分页数是1。如果有101个，那么分页数是2。每100个是一页。
        //定义函数【获取分页(int X)】：如果获取第一页，那么返回0到99元素组成的list。第二页是100到199，如果索引只到150，那么返回100到150。假设索引只到150，如果获取第三页，那么返回空。
        public int 分页数 => (int)Math.Ceiling((float)Count / 100);
        public CMKZList<T> 获取分页(int X) {
            if (X <= 0 || X > 分页数) {
                return new CMKZList<T>();
            }
            X--;
            return GetRange(X * 100, Math.Min(Count - X * 100, 100));
        }
        public new CMKZList<T> GetRange(int start, int length) {
            var A = new CMKZList<T>();
            for (int i = start; i < start + length; i++) {
                A.Add(this[i]);
            }
            return A;
        }
        public CMKZList<T> FindAll(Func<T, bool> X) {
            var A = new CMKZList<T>();
            foreach (var i in this) {
                if (X(i)) {
                    A.Add(i);
                }
            }
            return A;
        }
        public CMKZList<T2> Parse<T2>(Func<T, T2> X) {
            var A = new CMKZList<T2>();
            foreach (var i in this) {
                A.Add(X(i));
            }
            return A;
        }
        public string Join(char X) {
            return this.ToString(t => t.ToString(), X);
        }
        public bool AddWithNotRepeat(T X) {
            if (!Contains(X)) {
                Add(X);
                return true;
            }
            return false;
        }
        public CMKZList<T> Clone() => new(ToArray());
        public T AddWithReturn(T Y) {
            Add(Y);
            return Y;
        }
        public CMKZList<T> AddWithReturnList(T Y) {
            Insert(0,Y);
            return this;
        }
        public CMKZList<T> MoveToLast(Func<T, bool> 判断) {
            //对于列表中的每一个元素，如果判断函数返回true，则将其移动到列表最后。
            for (int i = 0; i < this.Count; i++) {
                if (判断(this[i])) {
                    this.Move(i, this.Count - 1);
                }
            }
            return this;
        }
        internal CMKZList<T> MoveToFirst(Func<T, bool> value) {
            //对于列表中的每一个元素，如果判断函数返回true，则将其移动到列表最前。
            for (int i = 0; i < this.Count; i++) {
                if (value(this[i])) {
                    this.Move(i, 0);
                }
            }
            return this;
        }
        internal CMKZList<T> MoveTo(int Index,Func<T, bool> value) {
            //对于列表中的每一个元素，如果判断函数返回true，则将其移动到列表此位置。
            for (int i = 0; i < this.Count; i++) {
                if (value(this[i])) {
                    this.Move(i, Index);
                }
            }
            return this;
        }
        public void Move(int fromIndex, int toIndex) {
            if (fromIndex < 0 || fromIndex >= this.Count || toIndex < 0 || toIndex >= this.Count) {
                throw new ArgumentOutOfRangeException("索引超出范围");
            }
            T item = this[fromIndex];
            this.RemoveAt(fromIndex);
            OnRemove?.Invoke(item, fromIndex);
            this.Insert(toIndex, item);
            OnAdd?.Invoke(item, toIndex);
        }
        //重载加号运算符，返回一个新的列表，包含两个列表的所有元素
        public static CMKZList<T> operator +(CMKZList<T> X, T Y) {
            X.Add(Y);
            return X;
        }
        public static CMKZList<T> operator -(CMKZList<T> X, T Y) {
            X.Remove(Y);
            return X;
        }
        public static CMKZList<T> operator +(CMKZList<T> X, CMKZList<T> Y) {
            var A = new CMKZList<T>();
            A.AddRange(X);
            A.AddRange(Y);
            return A;
        }
        public CMKZList<T1> Where<T1>() where T1 : class {
            var A = new CMKZList<T1>();
            foreach (var i in this) {
                if (i is T1) {
                    A.Add(i as T1);
                }
            }
            return A;
        }
        //except
        public CMKZList<T> Except<T2>() { 
            return Except(t => t is T2); 
        }
        public CMKZList<T> Except(Func<T, bool> X) {
            var A = new CMKZList<T>();
            foreach (var i in this) {
                if (!X(i)) {
                    A.Add(i);
                }
            }
            return A;
        }
        public T1 Find<T1>() where T1 : class {
            return Find(t => t is T1) as T1;
        }
        public void RemoveFirst(Predicate<T> X) {
            var A = FindIndex(X);
            if (A != -1) {
                RemoveAt(A);
            }
        }
    }
}