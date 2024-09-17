using Newtonsoft.Json;//Json
using System;//Action
using System.Collections;
using System.Collections.Generic;//List

namespace CMKZ {
    public class Tree : IEnumerable<Tree> {
        public string Name;
        public CMKZList<Tree> Children = new();
        public bool IsNoChild => Children.Count == 0;
        public Tree() {

        }
        public Tree(string X) {
            Name = X;
        }
        public Tree Add(string X) {
            var A = this;
            foreach (var i in X.Split('/')) {
                if (!A.Children.Exists(t => t.Name == i)) {
                    A.Children.Add(new Tree(i));
                }
                A = A.Children.Find(t => t.Name == i);
            }
            return A;
        }
        public Tree AddChild(Tree X) {
            Children.Add(X);
            return this;
        }
        public Tree First() {
            return IsNoChild ? this : Children[0].First();
        }
        public string GetPath(string X) {
            return GetPath(t => t.Name == X);
        }
        public string GetPath(Predicate<Tree> X, string Y = "") {
            if (X(this)) return Y + Name;
            foreach (var i in Children) {
                var A = i.GetPath(X, $"{Y + Name}/");
                if (A != null) return A;
            }
            return null;

        }
        public IEnumerator<Tree> GetEnumerator() {
            foreach (var i in Children) {
                yield return i;
                foreach (var j in i) {
                    yield return j;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();//实现接口的方法，不用管
    }
    [JsonObject(MemberSerialization.OptOut)]
    public class Tree<T> : IEnumerable<Tree<T>> {
        public string Name;
        public T Data;//中间节点通常不保存数据，只有叶节点保存。但也会有例外
        public CMKZList<Tree<T>> Children = new();//叶节点没有子节点。部分中间节点也没有子节点。不存在目录与文件的区分，一切都是目录，一切都是文件
        public bool IsNoChild => Children.Count == 0;
        /// <summary>
        /// 传入参数：目录、文件内容
        /// </summary>
        public event Action<string, Tree<T>> OnAdd;//event关键字，此事件不可被外部调用
        public event Action<string> OnRemove;
        public Tree<T> Add(string path, T data) {
            var A = path.Split('/');
            Tree<T> B = this;
            foreach (var i in A) {
                if (!B.Children.Exists(t => t.Name == i)) {
                    B.Children.Add(new Tree<T> { Name = i });
                }
                B = B.Children.Find(t => t.Name == i);
            }
            B.Data = data;
            OnAdd?.Invoke(path, B);
            return B;
        }
        public Tree<T> AddChild(Tree<T> Child) {
            Children.Add(Child);
            return Child;
        }
        //移除节点
        public void Remove(string path) {
            Remove(t => GetPath(t) == path);
        }
        public void Remove(Tree<T> X) {
            Remove(t => t == X);
        }
        public void Remove(Predicate<Tree<T>> X) {
            foreach (var i in Children) {
                i.Remove(X);
                if (X(i)) {
                    OnRemove?.Invoke(GetPath(i));
                    Children.Remove(i);
                }
            }
        }
        //查找路径
        public string GetPath(Tree<T> X) {
            return GetPath(t => t == X);
        }
        public string GetPath(T X) {
            return GetPath(t => t.Data.Equals(X));
        }
        public string GetPath(string X) {
            return GetPath(t => t.Name == X);
        }
        public string GetPath(Predicate<Tree<T>> X, string Y = "") {
            if (X(this)) return Y + Name;
            foreach (var i in Children) {
                var A = i.GetPath(X, $"{Y + Name}/");
                if (A != null) return A;
            }
            return null;
        }
        //查找目录
        public Tree<T> Find(string path) {
            return Find(t => GetPath(t) == path);
        }
        public Tree<T> Find(T X) {
            return Find(t => t.Data.Equals(X));
        }
        public Tree<T> Find(Predicate<Tree<T>> X) {
            foreach (var i in this) {
                if (X(i)) return i;
            }
            return null;
        }

        public Tree RemoveDatas() {
            var A = new Tree();
            foreach (var i in this) {
                A.Add(GetPath(i));
            }
            return A;
        }
        public IEnumerator<Tree<T>> GetEnumerator() {
            foreach (var i in Children) {
                yield return i;
                foreach (var j in i) {
                    yield return j;
                }
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();//实现接口的方法，不用管
    }
}