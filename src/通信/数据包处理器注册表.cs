using kcp2k;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using static CMKZ.LocalStorage;

namespace TGZG.战雷革命游戏服务器 {
	public class 数据包处理器注册表 : PacketHandlerRegistry.IRegistry {
		/// <summary>
		/// 特定数据包的回调函数列表的字典。
		/// </summary>
		/// <remarks>
		/// 不应直接操作此字段，应该通过API间接操作。
		/// </remarks>
		protected Dictionary<string, List<(string callbackId, PacketHandlerRegistry.PacketHandlerDelegate callbackFn)>> m_Registry = new();

		public PacketHandlerRegistry.OperateResult RegisterPacketType(string messageTitle) {
			if (CommunicateConstant.GetReservedPacketType().Contains(messageTitle)) {
				return PacketHandlerRegistry.OperateResult.RESERVED;
			}
			if (this.m_Registry.ContainsKey(messageTitle)) {
				return PacketHandlerRegistry.OperateResult.EXISTED;
			}
			this.m_Registry.Add(messageTitle, new List<(string callbackId, PacketHandlerRegistry.PacketHandlerDelegate callbackFn)>());
			this.OnPacketTypeRegistryUpdated();
			return PacketHandlerRegistry.OperateResult.OK;
		}

		public PacketHandlerRegistry.OperateResult UnregisterPacketType(string messageTitle) {
			KeyValuePair<string, List<ValueTuple<string, PacketHandlerRegistry.PacketHandlerDelegate>>> _ThePacketType = default;
			try {
				// 遍历字典中的每一个键值对寻找对应的注册表项！
				// 因为KeyValuePair是值类型所以得这样做而不是调用Linq的FirstOrDefault！
				_ThePacketType = this.m_Registry.First(_Kvp => _Kvp.Key == messageTitle);
			} catch (InvalidOperationException) {
				//没有找到相关的数据包类型！
				return PacketHandlerRegistry.OperateResult.NOT_FOUND;
			}
			if (_ThePacketType.Value.Count > 0) {
				//此数据包类型有注册处理程序，拒绝注销数据包类型！
				return PacketHandlerRegistry.OperateResult.EXISTED;
			}
			this.m_Registry.Remove(messageTitle);
			this.OnPacketTypeRegistryUpdated();
			return PacketHandlerRegistry.OperateResult.OK;
		}

		public PacketHandlerRegistry.OperateResult RegisterPacketHandler
				(string messageTitle, string handlerId, PacketHandlerRegistry.PacketHandlerDelegate callback) {
			var _theRegistryItem = this.m_Registry[messageTitle];
			if (_theRegistryItem != null) {
				try {
					// 寻找是否有同名的处理程序已经被注册？
					_theRegistryItem.First(_CallbackRegistryItem => _CallbackRegistryItem.callbackId == handlerId);
					// 如果有，则拒绝注册。
					return PacketHandlerRegistry.OperateResult.EXISTED;
				} catch (InvalidOperationException) {
					// 嗯，没有同名的处理程序已经被注册。
					_theRegistryItem.Add((handlerId, callback));
					this.OnPacketTypeHandlerRegistryUpdated();
					return PacketHandlerRegistry.OperateResult.OK;
				}
			} else {
				// 没有 messageTitle 所述的数据包类型被注册！
				return PacketHandlerRegistry.OperateResult.NOT_FOUND;
			}
		}

		public PacketHandlerRegistry.OperateResult UnregisterPacketHandler
				(string messageTitle, string handlerId, PacketHandlerRegistry.PacketHandlerDelegate callback) {
			var _theRegistryItem = this.m_Registry[messageTitle];
			if (_theRegistryItem != null) {
				_theRegistryItem.RemoveAll(_CallbackRegistryItem => _CallbackRegistryItem.callbackId == handlerId);
				this.OnPacketTypeHandlerRegistryUpdated();
				return PacketHandlerRegistry.OperateResult.OK;
			} else {
				// 没有 messageTitle 所述的数据包类型被注册！
				return PacketHandlerRegistry.OperateResult.NOT_FOUND;
			}
		}

		public IReadOnlyCollection<string> GetAllRegisteredPacketType() => this.m_Registry.Keys;

		public IReadOnlyCollection<(string handlerId, PacketHandlerRegistry.PacketHandlerDelegate callback)>
				GetPacketHandlers(string messageTitle) {
			var _PacketTypeRegistryItem = this.m_Registry[messageTitle];
			if (_PacketTypeRegistryItem is not null) {
				return _PacketTypeRegistryItem;
			}
			return Array.Empty<(string handlerId, PacketHandlerRegistry.PacketHandlerDelegate callback)>();
		}

		public event Action OnPacketTypeRegistryUpdated = delegate { };

		public event Action OnPacketTypeHandlerRegistryUpdated = delegate { };
	}
}
