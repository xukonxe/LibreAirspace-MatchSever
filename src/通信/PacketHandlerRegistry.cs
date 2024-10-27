using System;
using System.Collections.Generic;

namespace TGZG.战雷革命游戏服务器 {
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

		//Huh这个签名。
		//这不也是返回了一个键值对的集合吗？我觉得要重新设计一下这个签名。
		public delegate void PacketHandlerDelegate<TClientId, TAdditionalArg>(TClientId clientId, Dictionary<string, string> packetMessage, TAdditionalArg additionalArg);

		/// <summary>
		/// KCP服务器的数据包处理程序注册表操作的抽象。
		/// </summary>
		public interface IRegistry<TClientId, TAdditionalArg> {
			/// <summary>
			/// 注册数据包类型，需要数据包类型没有被注册或者数据包类型没有被协议保留。
			/// </summary>
			/// <param name="messageTitle">数据包类型名</param>
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
			OperateResult RegisterPacketType(string messageTitle);

			/// <summary>
			/// 注销数据包类型，需要注册表内此数据包类型没有注册任何处理程序
			/// </summary>
			/// <param name="messageTitle">数据包类型名</param>
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
			OperateResult UnregisterPacketType(string messageTitle);

			/// <summary>
			/// 注册数据包处理程序。
			/// </summary>
			/// <param name="messageTitle">数据包类型</param>
			/// <param name="handlerId">处理程序的识别符，请命名为有意义的处理程序描述。</param>
			/// <param name="callback">数据包处理程序</param>
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
			OperateResult RegisterPacketHandler(string messageTitle, string handlerId, PacketHandlerDelegate<TClientId, TAdditionalArg> callback);

			/// <summary>
			/// 注销数据包处理程序。
			/// </summary>
			/// <param name="messageTitle">数据包类型</param>
			/// <param name="handlerId">处理程序的识别符</param>
			/// <param name="callback">数据包处理程序</param>
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
			OperateResult UnregisterPacketHandler(string messageTitle, string handlerId, PacketHandlerDelegate<TClientId, TAdditionalArg> callback);

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
			/// <returns>注册表内特定数据包类型的所有处理程序，可能为空集合</returns>
			IReadOnlyCollection<(string handlerId, PacketHandlerDelegate<TClientId, TAdditionalArg> callback)> GetPacketHandlers(string messageTitle);

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