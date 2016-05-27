using System;

namespace Lidgren.Network
{
	public partial class NetConnection
	{
		private float m_sentPingTime;
		private int m_sentPingNumber;
		private float m_averageRoundtripTime;
		private float m_timeoutDeadline = float.MaxValue;

		// local time value + m_remoteTimeOffset = remote time value
		internal float m_remoteTimeOffset;

		/// <summary>
		/// Gets the current average roundtrip time in seconds
		/// </summary>
		public float AverageRoundtripTime { get { return (float)m_averageRoundtripTime; } }

		/// <summary>
		/// Time offset between this peer and the remote peer
		/// </summary>
		public float RemoteTimeOffset { get { return (float)m_remoteTimeOffset; } }

		// this might happen more than once
		internal void InitializeRemoteTimeOffset(float remoteSendTime)
		{
			m_remoteTimeOffset = (remoteSendTime + (m_averageRoundtripTime / 2f)) - NetTime.Now;
		}
		
		/// <summary>
		/// Gets local time value comparable to NetTime.Now from a remote value
		/// </summary>
		public double GetLocalTime(double remoteTimestamp)
		{
			return remoteTimestamp - m_remoteTimeOffset;
		}

		/// <summary>
		/// Gets the remote time value for a local time value produced by NetTime.Now
		/// </summary>
		public double GetRemoteTime(double localTimestamp)
		{
			return localTimestamp + m_remoteTimeOffset;
		}

		internal void InitializePing()
		{
			m_timeoutDeadline = NetTime.Now + (m_peerConfiguration.m_connectionTimeout * 2f); // initially allow a little more time
			SendPing();
		}

		internal void SendPing()
		{
			m_peer.VerifyNetworkThread();

			m_sentPingNumber++;

			m_sentPingTime = NetTime.Now;
			NetOutgoingMessage om = m_peer.CreateMessage(1);
			om.Write((byte)m_sentPingNumber); // truncating to 0-255
			om.m_messageType = NetMessageType.Ping;

			int len = om.Encode(m_peer.m_sendBuffer, 0, 0);
			bool connectionReset;
			m_peer.SendPacket(len, m_remoteEndPoint, 1, out connectionReset);

			m_statistics.PacketSent(len, 1);
			m_peer.Recycle(om);
		}

		internal void SendPong(int pingNumber)
		{
			m_peer.VerifyNetworkThread();

			NetOutgoingMessage om = m_peer.CreateMessage(5);
			om.Write((byte)pingNumber);
			om.Write((float)NetTime.Now); // we should update this value to reflect the exact point in time the packet is SENT
			om.m_messageType = NetMessageType.Pong;

			int len = om.Encode(m_peer.m_sendBuffer, 0, 0);
			bool connectionReset;

			m_peer.SendPacket(len, m_remoteEndPoint, 1, out connectionReset);

			m_statistics.PacketSent(len, 1);
			m_peer.Recycle(om);
		}

		internal void ReceivedPong(float now, int pongNumber, float remoteSendTime)
		{
			if ((byte)pongNumber != (byte)m_sentPingNumber)
			{
				m_peer.LogVerbose("Ping/Pong mismatch; dropped message?");
				return;
			}

			ResetTimeout(now);

			float rtt = now - m_sentPingTime;
			NetException.Assert(rtt >= 0);

			float diff = (remoteSendTime + (rtt / 2f)) - now;

			if (m_averageRoundtripTime < 0)
			{
				m_remoteTimeOffset = diff;
				m_averageRoundtripTime = rtt;
				m_peer.LogDebug("Initiated average roundtrip time to " + NetTime.ToReadable(m_averageRoundtripTime) + " Remote time is: " + (now + diff));
			}
			else
			{
				m_averageRoundtripTime = (m_averageRoundtripTime * 0.7f) + (rtt * 0.3f);

				m_remoteTimeOffset = (m_remoteTimeOffset * (m_sentPingNumber - 1) + diff) / (float)m_sentPingNumber;
				m_peer.LogVerbose("Updated average roundtrip time to " + NetTime.ToReadable(m_averageRoundtripTime) + ", remote time to " + (now + m_remoteTimeOffset) + " (ie. diff " + m_remoteTimeOffset + ")");
			}

			// update resend delay for all channels
			double resendDelay = GetResendDelay();
			for (int i = 0; i < m_sendChannels.Length; i++)
			{
				var chan = m_sendChannels[i];
				var rchan = chan as NetReliableSenderChannel;
				if (rchan != null)
					rchan.m_resendDelay = resendDelay;
			}

			// m_peer.LogVerbose("Timeout deadline pushed to  " + m_timeoutDeadline);

			// notify the application that average rtt changed
			if (m_peer.m_configuration.IsMessageTypeEnabled(NetIncomingMessageType.ConnectionLatencyUpdated))
			{
				NetIncomingMessage update = m_peer.CreateIncomingMessage(NetIncomingMessageType.ConnectionLatencyUpdated, 4);
				update.m_senderConnection = this;
				update.m_senderEndPoint = this.m_remoteEndPoint;
				update.Write((float)rtt);
				m_peer.ReleaseMessage(update);
			}
		}
	}
}
