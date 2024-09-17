using System;//Action
using static CMKZ.LocalStorage;

namespace CMKZ {
    public static partial class LocalStorage {
        public delegate EventData EventFunction(EventData X);
    }
    public class Event {
        public EventFunction Main;
        public Event() {

        }
        public Event(EventFunction X) {
            Main = X;
        }
        public Event(Action<EventData> X) {
            Main = e => {
                X(e);
                return e;
            };
        }
        public void Add(EventFunction X) {
            Main += X;
        }
        public void Add(Event X) {
            Add(X.Main);
        }
        public void Add(Action<EventData> X) { //没有Remove
            Add(e => {
                X(e);
                return e;
            });
        }
        public void Add<T>(Func<T,EventData> X) where T:EventData {
            Add(e => {
                return X(e as T);
            });
        }
        public void Remove(EventFunction X) {
            Main -= X;
        }
        public void Remove(Event X) {
            Remove(X.Main);
        }
        public void Clear() {
            Main = null;
        }
        public EventData Invoke(EventData X = null) {
            return Main?.Invoke(X);
        }
    }
    public class EventData {
        public bool Success;
        public EventData(bool X=true) {
            Success = X;
        }
    }
}