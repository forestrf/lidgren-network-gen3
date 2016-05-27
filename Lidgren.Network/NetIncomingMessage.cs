/* Copyright (c) 2010 Michael Lidgren

Permission is hereby granted, free of charge, to any person obtaining a copy of this software
and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom
the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE
USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Net;
using System.Diagnostics;

#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
#endif

namespace Lidgren.Network
{
	/// <summary>
	/// Incoming message either sent from a remote peer or generated within the library
	/// </summary>
	[DebuggerDisplay("Type={MessageType} LengthBits={LengthBits}")]
	public sealed class NetIncomingMessage : NetBuffer
	{
		internal NetIncomingMessageType m_incomingMessageType;
		internal NetEndPoint m_senderEndPoint;
		internal NetConnection m_senderConnection;
		internal int m_sequenceNumber;
		internal NetMessageType m_receivedMessageType;
		internal bool m_isFragment;
		internal float m_receiveTime;

		float userMsgTime = float.NaN;

		/// <summary>
		/// Constructor
		/// </summary>
		public NetIncomingMessage(NetIncomingMessageType msgType, float rTime) {
			m_incomingMessageType = msgType;
			m_receiveTime = rTime;
		}

		/// <summary>
		/// Create a copy of this Message
		/// </summary>
		public NetIncomingMessage Clone() {
			NetIncomingMessage cloned = new NetIncomingMessage();
			cloned.m_incomingMessageType = m_incomingMessageType;
			cloned.m_senderEndPoint = m_senderEndPoint;
			cloned.m_senderConnection = m_senderConnection;
			cloned.m_sequenceNumber = m_sequenceNumber;
			cloned.m_receivedMessageType = m_receivedMessageType;
			cloned.m_isFragment = m_isFragment;
			cloned.m_receiveTime = m_receiveTime;
			cloned.m_data = m_data;
			cloned.m_bitLength = m_bitLength;
			cloned.m_readPosition = m_readPosition;
			cloned.userMsgTime = userMsgTime;
			return cloned;
		}

		/// <summary>
		/// Gets the type of this incoming message
		/// </summary>
		public NetIncomingMessageType MessageType {	get { return m_incomingMessageType; } }

		/// <summary>
		/// Gets the delivery method this message was sent with (if user data)
		/// </summary>
		public NetDeliveryMethod DeliveryMethod { get { return NetUtility.GetDeliveryMethod(m_receivedMessageType); } }

		/// <summary>
		/// Gets the sequence channel this message was sent with (if user data)
		/// </summary>
		public int SequenceChannel { get { return (int)m_receivedMessageType - (int)NetUtility.GetDeliveryMethod(m_receivedMessageType); } }

		/// <summary>
		/// endpoint of sender, if any
		/// </summary>
		public NetEndPoint SenderEndPoint { get { return m_senderEndPoint; } }

		/// <summary>
		/// NetConnection of sender, if any
		/// </summary>
		public NetConnection SenderConnection { get { return m_senderConnection; } }

		/// <summary>
		/// What local time the message was received from the network
		/// </summary>
		public float ReceiveTime { get { return m_receiveTime; } }

		internal NetIncomingMessage()
		{
		}

		internal NetIncomingMessage(NetIncomingMessageType tp)
		{
			m_incomingMessageType = tp;
		}

		internal void Reset()
		{
			m_incomingMessageType = NetIncomingMessageType.Error;
			m_readPosition = 0;
			m_receivedMessageType = NetMessageType.LibraryError;
			m_senderConnection = null;
			m_bitLength = 0;
			m_isFragment = false;
			userMsgTime = float.NaN;
		}

		public float ReadMsgTime() {
			if (float.IsNaN(userMsgTime)) {
				userMsgTime = ReadSingle();
			}
			return userMsgTime;
		}

		/// <summary>
		/// Decrypt a message
		/// </summary>
		/// <param name="encryption">The encryption algorithm used to encrypt the message</param>
		/// <returns>true on success</returns>
		public bool Decrypt(NetEncryption encryption)
		{
			return encryption.Decrypt(this);
		}

		/// <summary>
		/// Reads a value, in local time comparable to NetTime.Now, written using WriteTime()
		/// Must have a connected sender
		/// </summary>
		public double ReadTime(bool highPrecision)
		{
			return ReadTime(m_senderConnection, highPrecision);
		}

		/// <summary>
		/// Returns a string that represents this object
		/// </summary>
		public override string ToString()
		{
			return "[NetIncomingMessage #" + m_sequenceNumber + " " + this.LengthBytes + " bytes]";
		}
	}
}
