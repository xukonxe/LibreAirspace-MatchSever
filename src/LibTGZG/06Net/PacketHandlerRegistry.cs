using System;
using System.Collections.Generic;

namespace TGZG.Net {
	/// <summary>
	/// 数据包处理程序注册表相关的东西
	/// </summary>
	public static class PacketHandlerRegistry {
		public enum OperateResult {
			OK = 0,
			NOT_FOUND,
			EXISTED,
			RESERVED
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Only packet handler registry implementations should throw this exception.
		/// </remarks>
		[Serializable]
		public sealed class PacketHandlerRegistryOperateException : Exception {
			private OperateResult m_operateResult;
			public OperateResult OperateResult => this.m_operateResult;
			public PacketHandlerRegistryOperateException(string message, OperateResult operateResult) : base(message) {
				this.m_operateResult = operateResult;
			}
			public PacketHandlerRegistryOperateException(string message, Exception inner) : base(message, inner) { }
		}

		public delegate void PacketHandlerDelegate<TClientId, TAdditionalArg>(TClientId clientId, Dictionary<string, string> packetMessage, TAdditionalArg additionalArg);

		/// <summary>
		/// KCP服务器的数据包处理程序注册表操作的抽象。
		/// </summary>
		/// <exception cref="PacketHandlerRegistryOperateException">
		/// 若调用本类中的方法/属性操作注册表出现错误，则可能会因为注册表的设置，抛出此异常。
		/// </exception>
		public interface IRegistry<TClientId, TAdditionalArg> {
			/// <summary>
			/// 注册数据包类型，需要数据包类型没有被注册或者数据包类型没有被协议保留。
			/// </summary>
			/// <param name="messageTitle">数据包类型名</param>
			/// <param name="dontThrowExWhenThrowOnStatus">
			/// 若设置为真，则方法不会在注册表设置了方法欲返回特定状态值时抛出异常的设定时，抛出异常，而是返回状态值
			/// </param>
			/// <returns>
			/// 是否成功？
			/// <para>
			/// <see cref="OperateResult.OK"/>
			/// 成功
			/// </para>
			/// <para>
			/// <see cref="OperateResult.EXISTED"/>
			/// 此数据包名被通讯协议保留
			/// </para>
			/// <para>
			/// <see cref="OperateResult.EXISTED"/>
			/// 此数据包类型已经被注册了
			/// </para>
			/// </returns>
			OperateResult RegisterPacketType(string messageTitle, bool ignoreReserved = false, bool dontThrowExWhenThrowOnStatus = false);

			/// <summary>
			/// 注销数据包类型，需要注册表内此数据包类型没有注册任何处理程序
			/// </summary>
			/// <param name="messageTitle">数据包类型名</param>
			/// <param name="dontThrowExWhenThrowOnStatus">
			/// 若设置为真，则方法不会在注册表设置了方法欲返回特定状态值时抛出异常的设定时，抛出异常，而是返回状态值
			/// </param>
			/// <remarks>不建议使用。</remarks>
			/// <returns>
			/// 是否成功？
			/// <para>
			/// <see cref="OperateResult.OK"/>
			/// 成功
			/// </para>
			/// <para>
			/// <see cref="OperateResult.NOT_FOUND"/>
			/// 注册表内没有<paramref name="messageTitle"/>所述的数据包类型！
			/// </para>
			/// <para>
			/// <see cref="OperateResult.EXISTED"/>
			/// 此数据包类型有注册处理程序，拒绝注销数据包类型！
			/// </para>
			/// </returns>
			OperateResult UnregisterPacketType(string messageTitle, bool dontThrowExWhenThrowOnStatus = false);

			/// <summary>
			/// 注册数据包处理程序。
			/// </summary>
			/// <param name="messageTitle">数据包类型</param>
			/// <param name="handlerId">处理程序的识别符，请命名为有意义的处理程序描述。</param>
			/// <param name="callback">数据包处理程序</param>
			/// <param name="dontThrowExWhenThrowOnStatus">
			/// 若设置为真，则方法不会在注册表设置了方法欲返回特定状态值时抛出异常的设定时，抛出异常，而是返回状态值
			/// </param>
			/// <returns>
			/// 是否成功？
			/// <para>
			/// <see cref="OperateResult.OK"/>
			/// 成功。
			/// </para>
			/// <para>
			/// <see cref="OperateResult.EXISTED"/>
			/// 有同名的处理程序已经被注册
			/// </para>
			/// <para>
			/// <see cref="OperateResult.NOT_FOUND"/>
			/// 没有 <paramref name="messageTitle"/> 所述的数据包类型被注册！
			/// </para>
			/// </returns>
			OperateResult RegisterPacketHandler(string messageTitle, string handlerId, PacketHandlerDelegate<TClientId, TAdditionalArg> callback, bool dontThrowExWhenThrowOnStatus = false);

			/// <summary>
			/// 注销数据包处理程序。
			/// </summary>
			/// <param name="messageTitle">数据包类型</param>
			/// <param name="handlerId">处理程序的识别符</param>
			/// <param name="callback">数据包处理程序</param>
			/// <param name="dontThrowExWhenThrowOnStatus">
			/// 若设置为真，则方法不会在注册表设置了方法欲返回特定状态值时抛出异常的设定时，抛出异常，而是返回状态值
			/// </param>
			/// <returns>
			/// 是否成功？
			/// <para>
			/// <see cref="OperateResult.OK"/>
			/// 成功
			/// </para>
			/// <para>
			/// <see cref="OperateResult.NOT_FOUND"/>
			/// 没有 <paramref name="messageTitle"/> 所述的数据包类型被注册！
			/// </para>
			/// </returns>
			OperateResult UnregisterPacketHandler(string messageTitle, string handlerId, PacketHandlerDelegate<TClientId, TAdditionalArg> callback, bool dontThrowExWhenThrowOnStatus = false);

			/// <summary>
			/// <para>获取注册表内所有已注册的数据包类型</para>
			/// </summary>
			/// <remarks>
			/// 不推荐使用
			/// </remarks>
			/// <returns>注册表内所有已注册的数据包类型，可能为空集合</returns>
			IReadOnlyCollection<string> GetAllRegisteredPacketType();

			/// <summary>
			/// 获取注册表内特定数据包类型的所有处理程序
			/// </summary>
			/// <remarks>
			/// 不推荐使用
			/// </remarks>
			/// <param name="messageTitle">数据包类型</param>
			/// <param name="dontThrowExWhenThrowOnStatus">
			/// 若设置为真，则方法不会在注册表设置了方法欲返回特定状态值时抛出异常的设定时，抛出异常，而是返回状态值
			/// </param>
			/// <returns>注册表内特定数据包类型的所有处理程序，可能为空集合</returns>
			IReadOnlyCollection<(string handlerId, PacketHandlerDelegate<TClientId, TAdditionalArg> callback)> GetPacketHandlers(string messageTitle);

			/// <summary>
			/// 在这个表内注册的操作结果值，会使会返回状态值的注册表操作出现特定结果时抛出异常。
			/// </summary>
			HashSet<OperateResult> ThrowOnResultList { get; }

			/// <summary>
			/// 数据包类型注册表更改事件
			/// </summary>
			event Action OnPacketTypeRegistryUpdated;

			/// <summary>
			/// 数据包处理程序注册表更改事件
			/// </summary>
			event Action OnPacketTypeHandlerRegistryUpdated;
		}
	}
}