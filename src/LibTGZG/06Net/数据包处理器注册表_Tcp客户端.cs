using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TGZG.战雷革命游戏服务器;
using TouchSocket.Sockets;
using static CMKZ.LocalStorage;
using static TGZG.Net.PacketHandlerRegistry;

namespace TGZG.Net {
	/// <summary>
	/// 
	/// </summary>
	/// <remarks>
	/// 注：此类中所谓的对"客户端 int 对象"的引用不过是个占位符罢了。
	/// </remarks>
	public class 数据包处理器注册表_Tcp客户端 : IRegistry<int, object> {
		public HashSet<OperateResult> m_ThrowOnResultList;

		public event Action OnPacketTypeRegistryUpdated = delegate { };

		public event Action OnPacketTypeHandlerRegistryUpdated = delegate { };

		public HashSet<OperateResult> ThrowOnResultList => this.m_ThrowOnResultList;

		public 数据包处理器注册表_Tcp客户端() {
			this.m_ThrowOnResultList = new HashSet<OperateResult>();
		}

		/// <summary>
		/// 特定数据包的回调函数列表的字典。
		/// </summary>
		/// <remarks>
		/// 不应直接操作此字段，应该通过API间接操作。
		/// </remarks>
		protected Dictionary<string, List<(string callbackId, PacketHandlerDelegate<int, object> callbackFn)>> m_Registry = new();

		public OperateResult RegisterPacketType(string messageTitle, bool ignoreReserved = false, bool dontThrowExWhenThrowOnStatus = false) {
			if (!ignoreReserved && CommunicateConstant.GetReservedPacketType().Contains(messageTitle)) {
				return OperateResult.RESERVED;
			}
			if (this.m_Registry.ContainsKey(messageTitle)) {
				return OperateResult.EXISTED;
			}
			this.m_Registry.Add(messageTitle, new List<(string callbackId, PacketHandlerDelegate<int, object> callbackFn)>());
			this.OnPacketTypeRegistryUpdated();
			return OperateResult.OK;
		}

		public OperateResult UnregisterPacketType(string messageTitle, bool dontThrowExWhenThrowOnStatus = false) {
			KeyValuePair<string, List<ValueTuple<string, PacketHandlerDelegate<int, object>>>> _ThePacketType = default;
			try {
				// 遍历字典中的每一个键值对寻找对应的注册表项！
				// 因为KeyValuePair是值类型所以得这样做而不是调用Linq的FirstOrDefault！
				_ThePacketType = this.m_Registry.First(_Kvp => _Kvp.Key == messageTitle);
			} catch (InvalidOperationException) {
				//没有找到相关的数据包类型！
				return OperateResult.NOT_FOUND;
			}
			if (_ThePacketType.Value.Count > 0) {
				//此数据包类型有注册处理程序，拒绝注销数据包类型！
				return OperateResult.EXISTED;
			}
			this.m_Registry.Remove(messageTitle);
			this.OnPacketTypeRegistryUpdated();
			return OperateResult.OK;
		}

		public OperateResult RegisterPacketHandler
				(string messageTitle, string handlerId, PacketHandlerDelegate<int, object> callback, bool dontThrowExWhenThrowOnStatus = false) {
			var _theRegistryItem = this.m_Registry[messageTitle];
			if (_theRegistryItem != null) {
				try {
					// 寻找是否有同名的处理程序已经被注册？
					_theRegistryItem.First(_CallbackRegistryItem => _CallbackRegistryItem.callbackId == handlerId);
					// 如果有，则拒绝注册。
					return OperateResult.EXISTED;
				} catch (InvalidOperationException) {
					// 嗯，没有同名的处理程序已经被注册。
					_theRegistryItem.Add((handlerId, callback));
					this.OnPacketTypeHandlerRegistryUpdated();
					return OperateResult.OK;
				}
			} else {
				// 没有 messageTitle 所述的数据包类型被注册！
				return OperateResult.NOT_FOUND;
			}
		}

		public OperateResult UnregisterPacketHandler
				(string messageTitle, string handlerId, PacketHandlerDelegate<int, object> callback, bool dontThrowExWhenThrowOnStatus = false) {
			var _theRegistryItem = this.m_Registry[messageTitle];
			if (_theRegistryItem != null) {
				_theRegistryItem.RemoveAll(_CallbackRegistryItem => _CallbackRegistryItem.callbackId == handlerId);
				this.OnPacketTypeHandlerRegistryUpdated();
				return OperateResult.OK;
			} else {
				// 没有 messageTitle 所述的数据包类型被注册！
				return OperateResult.NOT_FOUND;
			}
		}

		public IReadOnlyCollection<string> GetAllRegisteredPacketType() => this.m_Registry.Keys;

		public IReadOnlyCollection<(string handlerId, PacketHandlerDelegate<int, object> callback)>
				GetPacketHandlers(string messageTitle) {
			var _PacketTypeRegistryItem = this.m_Registry[messageTitle];
			if (_PacketTypeRegistryItem is not null) {
				return _PacketTypeRegistryItem;
			}
			return Array.Empty<(string handlerId, PacketHandlerDelegate<int, object> callback)>();
		}

		protected OperateResult _RetStatCodeButThrowOnSpecVal(OperateResult returnValue) {
			if (this.m_ThrowOnResultList.Contains(returnValue)) {
				throw new PacketHandlerRegistryOperateException($"The registry object is set to throw exception on {returnValue}", returnValue);
			} else {
				return returnValue;
			}
		}
	}
}
