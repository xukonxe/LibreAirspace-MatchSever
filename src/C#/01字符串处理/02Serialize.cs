using Newtonsoft.Json;//Json
using Newtonsoft.Json.Serialization;
using System;//Action
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace CMKZ {
    public static partial class LocalStorage {
        public static string ToJson(this object X, bool 完全保存 = false, bool 格式美化 = true) {
            return JsonConvert.SerializeObject(X, new JsonSerializerSettings {
                TypeNameHandling = 完全保存 ? TypeNameHandling.All : TypeNameHandling.None,//不要还原为基类
                //ContractResolver = new IgnoreActionContractResolver(),//不要序列化委托与属性
                MetadataPropertyHandling = 完全保存 ? MetadataPropertyHandling.Default : MetadataPropertyHandling.Ignore,
                Formatting = 格式美化 ? Formatting.Indented : Formatting.None,//美化格式
                PreserveReferencesHandling = 完全保存 ? PreserveReferencesHandling.Objects : PreserveReferencesHandling.None,//保留引用
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,//循环引用
            });
        }
        public static T JsonToCS<T>(this string X, bool 完全保存 = false, bool 格式美化 = true) {
            return JsonConvert.DeserializeObject<T>(X, new JsonSerializerSettings {
                TypeNameHandling = 完全保存 ? TypeNameHandling.All : TypeNameHandling.None,
                //ContractResolver = new IgnoreActionContractResolver(),
                MetadataPropertyHandling = 完全保存 ? MetadataPropertyHandling.Default : MetadataPropertyHandling.Ignore,
                Formatting = 格式美化 ? Formatting.Indented : Formatting.None,//美化格式
                PreserveReferencesHandling = 完全保存 ? PreserveReferencesHandling.Objects : PreserveReferencesHandling.None,//保留引用
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            });
        }
        public static byte[] StringToBytes(this string X) {
            return Encoding.UTF8.GetBytes(X);
        }
        public static string BytesToString(this byte[] X) {
            return Encoding.UTF8.GetString(X);
        }
        public static string Serialize(this object X) {
            var A = "";
            foreach (var B in X.GetType().GetFields()) {
                A += $"{B.Name}：{B.GetValue(X)}\n";
            }
            foreach (var B in X.GetType().GetProperties()) {
                A += $"{B.Name}：{B.GetValue(X)}\n";
            }
            return A;
        }
    }
    public class IgnoreActionContractResolver : DefaultContractResolver {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization) {
            var property = base.CreateProperty(member, memberSerialization);
            if (typeof(Delegate).IsAssignableFrom(property.PropertyType) || (member.MemberType == MemberTypes.Property && member.GetCustomAttribute<JsonPropertyAttribute>() == null)) {
                property.ShouldSerialize = instance => false;
            }
            return property;
        }
    }
}