using System;
using System.Collections.Generic;
using System.Linq;
using static CMKZ.LocalStorage;

namespace CMKZ {
    public static partial class LocalStorage {
        public static KeyValue<float, float> ToKeyValue(this string X) {
            var A = X.Split("*");
            return new KeyValue<float, float>(A[0].ToFloat(), A[1].ToFloat());
        }
        public static KeyValueList<T1, T2> ToKeyValueList<T1, T2>(this IEnumerable<KeyValue<T1, T2>> X) {
            return new KeyValueList<T1, T2>(X.ToArray());
        }
    }
    public class KeyValueList<TKey, TValue> : CMKZList<KeyValue<TKey, TValue>> {
        public TValue this[TKey X] {
            get {
                if (this.Contains(t => t.Key.IsNull() ? t.IsNull() : t.Key.Equals(X))) {
                    return Find(t => t.Key.IsNull() ? t.IsNull() : t.Key.Equals(X)).Value;
                }
                if (typeof(TValue).IsClass()) {
                    return this[X] = CreateObject<TValue>();
                }
                return this[X] = default;
            }
            set {
                if (Find(t => t.Key.IsNull() ? t.IsNull() : t.Key.Equals(X)) == null) {
                    Add(X, value);
                } else {
                    Find(t => {
                        if (t.Key.IsNull()) {
                            if (t.IsNull()) return true;
                            else return false;
                        }
                        return t.Key.Equals(X);
                    }).Value = value;
                }
            }
        }
        public new KeyValue<TKey, TValue> this[int X] {
            get {
                return base[X];
            }
            set {
                base[X] = value;
            }
        }
        public KeyValueList() : base() {

        }
        public KeyValueList(KeyValue<TKey, TValue>[] X) : base(X) {

        }
        public CMKZList<TKey> Keys => this.Select(t => t.Key).ToList_CMKZ();
        public CMKZList<TValue> Values => this.Select(t => t.Value).ToList_CMKZ();
        public void Add(TKey X, TValue Y) => Add(new KeyValue<TKey, TValue> { Key = X, Value = Y });
        public void AddTop(TKey X, TValue Y) => Insert(0, new KeyValue<TKey, TValue> { Key = X, Value = Y });
        public void Sort(Func<KeyValue<TKey, TValue>, KeyValue<TKey, TValue>, bool> X) {
            int n = Count;
            for (int j = 0; j < n; j++) {
                for (int i = 0; i < n - 1; i++) {
                    if (!X(this[i], this[i + 1])) {
                        (this[i + 1], this[i]) = (this[i], this[i + 1]);
                    }
                }
            }
        }
        public new KeyValueList<TKey, TValue> Clone() {
            return new KeyValueList<TKey, TValue>(ToArray());
        }
        public CMKZ_Dictionary<TKey, TValue> ToDictionary() {
            var A = new CMKZ_Dictionary<TKey, TValue>();
            foreach (var i in this) {
                A[i.Key] = i.Value;
            }
            return A;
        }
        public KeyValueList<TKey, TValue> RemoveKey(TKey Y) {
            this.RemoveAll(t => {
                if (t.Key.IsNull()) {
                    if (Y.IsNull()) return true;
                    else return false;
                }
                return t.Key.Equals(Y);
            });
            return this;
        }
        public KeyValueList<TKey, TValue> ChangeKey(TKey Y, TKey Z) {
            this[Z] = this[Y];
            this.RemoveKey(Y);
            return this;
        }
        public KeyValue<TKey, TValue> FindKey(TKey Y) {
            return Find(t => {
                if (t.Key.IsNull()) {
                    if (Y.IsNull()) return true;
                    else return false;
                }
                return t.Key.Equals(Y);
            });
        }
        public CMKZList<KeyValue<TKey, TValue>> FindKeys(TKey Y) {
            return FindAll(t => t.Key.Equals(Y));
        }
        public bool ContainsKey(TKey Y) {//如果存在此名称，那么返回ture，否则Fasle
            return FindKey(Y) != null;
        }
        //重载加号，使得相当于addrange
        public static KeyValueList<TKey, TValue> operator +(KeyValueList<TKey, TValue> X, KeyValueList<TKey, TValue> Y) {
            X.AddRange(Y);
            return X;
        }
        public override string ToString() {
            return this.ToString(t => t.Key + " " + t.Value, ',');
        }
        public KeyValueList<T, TValue> TurnKey<T>(Func<TKey, T> X) {
            return new KeyValueList<T, TValue>(this.Select(t => new KeyValue<T, TValue> { Key = X(t.Key), Value = t.Value }).ToArray());
        }
    }
    public class KeyValue<T1, T2> {
        public T1 Key;
        public T2 Value;
        public KeyValue() { }
        public KeyValue(T1 X, T2 Y) {
            Key = X;
            Value = Y;
        }
        public KeyValue<T1, T2> SetKey(T1 X) {
            Key = X;
            return this;
        }
        public KeyValue<T1, T2> SetValue(T2 X) {
            Value = X;
            return this;
        }
        public override string ToString() {
            return $"{Key}*{Value}";
        }
        public static implicit operator KeyValuePair<T1, T2>(KeyValue<T1, T2> X) {
            return new KeyValuePair<T1, T2>(X.Key, X.Value);
        }
        public static implicit operator KeyValue<T1, T2>(KeyValuePair<T1, T2> X) {
            return new KeyValue<T1, T2>(X.Key, X.Value);
        }
    }
}