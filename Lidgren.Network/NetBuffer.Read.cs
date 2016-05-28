using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Net;

#if !__NOIPENDPOINT__
using NetEndPoint = System.Net.IPEndPoint;
#endif

namespace Lidgren.Network
{
	/// <summary>
	/// Base class for NetIncomingMessage and NetOutgoingMessage
	/// </summary>
	public partial class NetBuffer
	{
		private const string c_readOverflowError = "Trying to read past the buffer size - likely caused by mismatching Write/Reads, different size or order.";

		private static SimpleArrayPoolThreadSafe<Byte> bytePool = new SimpleArrayPoolThreadSafe<Byte>();

		/// <summary>
		/// Reads a boolean value (stored as a single bit) written using Write(bool)
		/// </summary>
		public bool ReadBoolean()
		{
			if (m_bitLength - m_readPosition < 1)
				throw new NetException(c_readOverflowError);
			byte retval = NetBitWriter.ReadByte(m_data, 1, m_readPosition);
			m_readPosition += 1;
			return (retval > 0 ? true : false);
		}
		
		/// <summary>
		/// Reads a byte
		/// </summary>
		public byte ReadByte()
		{
			if (m_bitLength - m_readPosition < 8)
				throw new NetException(c_readOverflowError);
			byte retval = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
			m_readPosition += 8;
			return retval;
		}

		/// <summary>
		/// Reads a byte and returns true or false for success
		/// </summary>
		public bool ReadByte(out byte result)
		{
			if (m_bitLength - m_readPosition < 8)
			{
				result = 0;
				return false;
			}
			result = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
			m_readPosition += 8;
			return true;
		}

		/// <summary>
		/// Reads a signed byte
		/// </summary>
		[CLSCompliant(false)]
		public sbyte ReadSByte()
		{
			if (m_bitLength - m_readPosition < 8)
				throw new NetException(c_readOverflowError);
			byte retval = NetBitWriter.ReadByte(m_data, 8, m_readPosition);
			m_readPosition += 8;
			return (sbyte)retval;
		}

		/// <summary>
		/// Reads 1 to 8 bits into a byte
		/// </summary>
		public byte ReadByte(int numberOfBits)
		{
			if (numberOfBits < 1 || numberOfBits > 8)
				throw new NetException("ReadByte(bits) can only read between 1 and 8 bits");
			byte retval = NetBitWriter.ReadByte(m_data, numberOfBits, m_readPosition);
			m_readPosition += numberOfBits;
			return retval;
		}

		/// <summary>
		/// Reads the specified number of bytes
		/// </summary>
		public byte[] ReadBytes(int numberOfBytes)
		{
			if (m_bitLength - m_readPosition + 7 < numberOfBytes * 8)
				throw new NetException(c_readOverflowError);

			byte[] retval = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, retval, 0);
			m_readPosition += (8 * numberOfBytes);
			return retval;
		}

		/// <summary>
		/// Reads the specified number of bytes and returns true for success
		/// </summary>
		public bool ReadBytes(int numberOfBytes, out byte[] result)
		{
			if (m_bitLength - m_readPosition + 7 < (numberOfBytes * 8))
			{
				result = null;
				return false;
			}

			result = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, result, 0);
			m_readPosition += (8 * numberOfBytes);
			return true;
		}

		/// <summary>
		/// Reads the specified number of bytes into a preallocated array
		/// </summary>
		/// <param name="into">The destination array</param>
		/// <param name="offset">The offset where to start writing in the destination array</param>
		/// <param name="numberOfBytes">The number of bytes to read</param>
		public void ReadBytes(byte[] into, int offset, int numberOfBytes)
		{
			if (m_bitLength - m_readPosition + 7 < numberOfBytes * 8)
				throw new NetException(c_readOverflowError);
			NetException.Assert(offset + numberOfBytes <= into.Length);

			NetBitWriter.ReadBytes(m_data, numberOfBytes, m_readPosition, into, offset);
			m_readPosition += (8 * numberOfBytes);
			return;
		}

		/// <summary>
		/// Reads the specified number of bits into a preallocated array
		/// </summary>
		/// <param name="into">The destination array</param>
		/// <param name="offset">The offset where to start writing in the destination array</param>
		/// <param name="numberOfBits">The number of bits to read</param>
		public void ReadBits(byte[] into, int offset, int numberOfBits)
		{
			if (m_bitLength - m_readPosition < numberOfBits)
				throw new NetException(c_readOverflowError);
			NetException.Assert(offset + NetUtility.BytesToHoldBits(numberOfBits) <= into.Length);

			int numberOfWholeBytes = numberOfBits / 8;
			int extraBits = numberOfBits - (numberOfWholeBytes * 8);

			NetBitWriter.ReadBytes(m_data, numberOfWholeBytes, m_readPosition, into, offset);
			m_readPosition += (8 * numberOfWholeBytes);

			if (extraBits > 0)
				into[offset + numberOfWholeBytes] = ReadByte(extraBits);

			return;
		}

		/// <summary>
		/// Reads a 16 bit signed integer written using Write(Int16)
		/// </summary>
		public Int16 ReadInt16()
		{
			if (m_bitLength - m_readPosition < 16)
				throw new NetException(c_readOverflowError);
			uint retval = NetBitWriter.ReadUInt16(m_data, 16, m_readPosition);
			m_readPosition += 16;
			return (short)retval;
		}

		/// <summary>
		/// Reads a 16 bit unsigned integer written using Write(UInt16)
		/// </summary>
		[CLSCompliant(false)]
		public UInt16 ReadUInt16()
		{
			if (m_bitLength - m_readPosition < 16)
				throw new NetException(c_readOverflowError);
			uint retval = NetBitWriter.ReadUInt16(m_data, 16, m_readPosition);
			m_readPosition += 16;
			return (ushort)retval;
		}

		/// <summary>
		/// Reads a 32 bit signed integer written using Write(Int32)
		/// </summary>
		public Int32 ReadInt32()
		{
			if (m_bitLength - m_readPosition < 32)
				throw new NetException(c_readOverflowError);
			uint retval = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			return (Int32)retval;
		}

		/// <summary>
		/// Reads a 32 bit signed integer written using Write(Int32)
		/// </summary>
		[CLSCompliant(false)]
		public bool ReadInt32(out Int32 result)
		{
			if (m_bitLength - m_readPosition < 32)
			{
				result = 0;
				return false;
			}

			result = (Int32)NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			return true;
		}

		/// <summary>
		/// Reads a signed integer stored in 1 to 32 bits, written using Write(Int32, Int32)
		/// </summary>
		public Int32 ReadInt32(int numberOfBits)
		{
			if (numberOfBits < 1 || numberOfBits > 32)
				throw new NetException("ReadInt32(bits) can only read between 1 and 32 bits");
			if (m_bitLength - m_readPosition < numberOfBits)
				throw new NetException(c_readOverflowError);

			uint retval = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			m_readPosition += numberOfBits;

			if (numberOfBits == 32)
				return (int)retval;

			int signBit = 1 << (numberOfBits - 1);
			if ((retval & signBit) == 0)
				return (int)retval; // positive

			// negative
			unchecked
			{
				uint mask = ((uint)-1) >> (33 - numberOfBits);
				uint tmp = (retval & mask) + 1;
				return -((int)tmp);
			}
		}

		/// <summary>
		/// Reads an 32 bit unsigned integer written using Write(UInt32)
		/// </summary>
		[CLSCompliant(false)]
		public UInt32 ReadUInt32()
		{
			if (m_bitLength - m_readPosition < 32)
				throw new NetException(c_readOverflowError);
			uint retval = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			return retval;
		}

		/// <summary>
		/// Reads an 32 bit unsigned integer written using Write(UInt32) and returns true for success
		/// </summary>
		[CLSCompliant(false)]
		public bool ReadUInt32(out UInt32 result)
		{
			if (m_bitLength - m_readPosition < 32)
			{
				result = 0;
				return false;
			}
			result = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			return true;
		}

		/// <summary>
		/// Reads an unsigned integer stored in 1 to 32 bits, written using Write(UInt32, Int32)
		/// </summary>
		[CLSCompliant(false)]
		public UInt32 ReadUInt32(int numberOfBits)
		{
			if (numberOfBits < 1 || numberOfBits > 32)
				throw new NetException("ReadUInt32(bits) can only read between 1 and 32 bits");
			if (m_bitLength - m_readPosition < numberOfBits)
				throw new NetException(c_readOverflowError);

			UInt32 retval = NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			m_readPosition += numberOfBits;
			return retval;
		}

		/// <summary>
		/// Reads a 64 bit unsigned integer written using Write(UInt64)
		/// </summary>
		[CLSCompliant(false)]
		public UInt64 ReadUInt64()
		{
			if (m_bitLength - m_readPosition < 64)
				throw new NetException(c_readOverflowError);

			ulong low = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
			m_readPosition += 32;
			ulong high = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);

			ulong retval = low + (high << 32);

			m_readPosition += 32;
			return retval;
		}

		/// <summary>
		/// Reads a 64 bit signed integer written using Write(Int64)
		/// </summary>
		public Int64 ReadInt64()
		{
			if (m_bitLength - m_readPosition < 64)
				throw new NetException(c_readOverflowError);
			unchecked
			{
				ulong retval = ReadUInt64();
				long longRetval = (long)retval;
				return longRetval;
			}
		}

		/// <summary>
		/// Reads an unsigned integer stored in 1 to 64 bits, written using Write(UInt64, Int32)
		/// </summary>
		[CLSCompliant(false)]
		public UInt64 ReadUInt64(int numberOfBits)
		{
			if (numberOfBits < 1 || numberOfBits > 64)
				throw new NetException("ReadUInt64(bits) can only read between 1 and 64 bits");
			if (m_bitLength - m_readPosition < numberOfBits)
				throw new NetException(c_readOverflowError);

			ulong retval;
			if (numberOfBits <= 32)
			{
				retval = (ulong)NetBitWriter.ReadUInt32(m_data, numberOfBits, m_readPosition);
			}
			else
			{
				retval = NetBitWriter.ReadUInt32(m_data, 32, m_readPosition);
				retval |= (UInt64)NetBitWriter.ReadUInt32(m_data, numberOfBits - 32, m_readPosition + 32) << 32;
			}
			m_readPosition += numberOfBits;
			return retval;
		}

		/// <summary>
		/// Reads a signed integer stored in 1 to 64 bits, written using Write(Int64, Int32)
		/// </summary>
		public Int64 ReadInt64(int numberOfBits)
		{
			if(numberOfBits < 1 || numberOfBits > 64)
				throw new NetException("ReadInt64(bits) can only read between 1 and 64 bits");
			return (long)ReadUInt64(numberOfBits);
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using Write(Single)
		/// </summary>
		public float ReadFloat()
		{
			return ReadSingle();
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using Write(Single)
		/// </summary>
		public float ReadSingle()
		{
			if (m_bitLength - m_readPosition < 32)
				throw new NetException(c_readOverflowError);

			if ((m_readPosition & 7) == 0) // read directly
			{
				float retval = BitConverter.ToSingle(m_data, m_readPosition >> 3);
				m_readPosition += 32;
				return retval;
			}

			byte[] bytes = bytePool.Get(4);
			ReadBytes(bytes, 0, 4);
			float res = BitConverter.ToSingle(bytes, 0);
			bytePool.Recycle(bytes);
			return res;
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using Write(Single)
		/// </summary>
		public bool ReadSingle(out float result)
		{
			if (m_bitLength - m_readPosition < 32)
			{
				result = 0.0f;
				return false;
			}

			if ((m_readPosition & 7) == 0) // read directly
			{
				result = BitConverter.ToSingle(m_data, m_readPosition >> 3);
				m_readPosition += 32;
				return true;
			}

			byte[] bytes = bytePool.Get(4);
			ReadBytes(bytes, 0, 4);
			result = BitConverter.ToSingle(bytes, 0);
			bytePool.Recycle(bytes);
			return true;
		}

		/// <summary>
		/// Reads a 64 bit floating point value written using Write(Double)
		/// </summary>
		public double ReadDouble()
		{
			if(m_bitLength - m_readPosition < 64)
				throw new NetException(c_readOverflowError);

			if ((m_readPosition & 7) == 0) // read directly
			{
				// read directly
				double retval = BitConverter.ToDouble(m_data, m_readPosition >> 3);
				m_readPosition += 64;
				return retval;
			}
			
			byte[] bytes = bytePool.Get(8);
			ReadBytes(bytes, 0, 8);
			double res = BitConverter.ToDouble(bytes, 0);
			bytePool.Recycle(bytes);
			return res;
		}

		//
		// Variable bit count
		//

		/// <summary>
		/// Reads a variable sized UInt32 written using WriteVariableUInt32()
		/// </summary>
		[CLSCompliant(false)]
		public uint ReadVariableUInt32()
		{
			int num1 = 0;
			int num2 = 0;
			while (m_bitLength - m_readPosition >= 8)
			{
				byte num3 = this.ReadByte();
				num1 |= (num3 & 0x7f) << num2;
				num2 += 7;
				if ((num3 & 0x80) == 0)
					return (uint)num1;
			}

			// ouch; failed to find enough bytes; malformed variable length number?
			return (uint)num1;
		}

		/// <summary>
		/// Reads a variable sized UInt32 written using WriteVariableUInt32() and returns true for success
		/// </summary>
		[CLSCompliant(false)]
		public bool ReadVariableUInt32(out uint result)
		{
			int num1 = 0;
			int num2 = 0;
			while (m_bitLength - m_readPosition >= 8)
			{
				byte num3;
				if (ReadByte(out num3) == false)
				{
					result = 0;
					return false;
				}
				num1 |= (num3 & 0x7f) << num2;
				num2 += 7;
				if ((num3 & 0x80) == 0)
				{
					result = (uint)num1;
					return true;
				}
			}
			result = (uint)num1;
			return false;
		}

		/// <summary>
		/// Reads a variable sized Int32 written using WriteVariableInt32()
		/// </summary>
		public int ReadVariableInt32()
		{
			uint n = ReadVariableUInt32();
			return (int)(n >> 1) ^ -(int)(n & 1); // decode zigzag
		}

		/// <summary>
		/// Reads a variable sized Int64 written using WriteVariableInt64()
		/// </summary>
		public Int64 ReadVariableInt64()
		{
			UInt64 n = ReadVariableUInt64();
			return (Int64)(n >> 1) ^ -(long)(n & 1); // decode zigzag
		}

		/// <summary>
		/// Reads a variable sized UInt32 written using WriteVariableInt64()
		/// </summary>
		[CLSCompliant(false)]
		public UInt64 ReadVariableUInt64()
		{
			UInt64 num1 = 0;
			int num2 = 0;
			while (m_bitLength - m_readPosition >= 8)
			{
				//if (num2 == 0x23)
				//	throw new FormatException("Bad 7-bit encoded integer");

				byte num3 = this.ReadByte();
				num1 |= ((UInt64)num3 & 0x7f) << num2;
				num2 += 7;
				if ((num3 & 0x80) == 0)
					return num1;
			}

			// ouch; failed to find enough bytes; malformed variable length number?
			return num1;
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using WriteSignedSingle()
		/// </summary>
		/// <param name="numberOfBits">The number of bits used when writing the value</param>
		/// <returns>A floating point value larger or equal to -1 and smaller or equal to 1</returns>
		public float ReadSignedSingle(int numberOfBits)
		{
			uint encodedVal = ReadUInt32(numberOfBits);
			int maxVal = (1 << numberOfBits) - 1;
			return ((float)(encodedVal + 1) / (float)(maxVal + 1) - 0.5f) * 2.0f;
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using WriteUnitSingle()
		/// </summary>
		/// <param name="numberOfBits">The number of bits used when writing the value</param>
		/// <returns>A floating point value larger or equal to 0 and smaller or equal to 1</returns>
		public float ReadUnitSingle(int numberOfBits)
		{
			uint encodedVal = ReadUInt32(numberOfBits);
			int maxVal = (1 << numberOfBits) - 1;
			return (float)(encodedVal + 1) / (float)(maxVal + 1);
		}

		/// <summary>
		/// Reads a 32 bit floating point value written using WriteRangedSingle()
		/// </summary>
		/// <param name="min">The minimum value used when writing the value</param>
		/// <param name="max">The maximum value used when writing the value</param>
		/// <param name="numberOfBits">The number of bits used when writing the value</param>
		/// <returns>A floating point value larger or equal to MIN and smaller or equal to MAX</returns>
		public float ReadRangedSingle(float min, float max, int numberOfBits)
		{
			float range = max - min;
			int maxVal = (1 << numberOfBits) - 1;
			float encodedVal = (float)ReadUInt32(numberOfBits);
			float unit = encodedVal / (float)maxVal;
			return min + (unit * range);
		}

		/// <summary>
		/// Reads a 32 bit integer value written using WriteRangedInteger()
		/// </summary>
		/// <param name="min">The minimum value used when writing the value</param>
		/// <param name="max">The maximum value used when writing the value</param>
		/// <returns>A signed integer value larger or equal to MIN and smaller or equal to MAX</returns>
		public int ReadRangedInteger(int min, int max)
		{
			uint range = (uint)(max - min);
			int numBits = NetUtility.BitsToHoldUInt(range);

			uint rvalue = ReadUInt32(numBits);
			return (int)(min + rvalue);
		}

	        /// <summary>
	        /// Reads a 64 bit integer value written using WriteRangedInteger() (64 version)
	        /// </summary>
	        /// <param name="min">The minimum value used when writing the value</param>
	        /// <param name="max">The maximum value used when writing the value</param>
	        /// <returns>A signed integer value larger or equal to MIN and smaller or equal to MAX</returns>
	        public long ReadRangedInteger(long min, long max)
	        {
	            ulong range = (ulong)(max - min);
	            int numBits = NetUtility.BitsToHoldUInt64(range);
	
	            ulong rvalue = ReadUInt64(numBits);
	            return min + (long)rvalue;
	        }

		/// <summary>
		/// Reads a string written using Write(string)
		/// </summary>
		public string ReadString()
		{
			int byteLen = (int)ReadVariableUInt32();

			if (byteLen <= 0)
				return String.Empty;

			if ((ulong)(m_bitLength - m_readPosition) < ((ulong)byteLen * 8))
			{
				// not enough data
#if DEBUG
				
				throw new NetException(c_readOverflowError);
#else
				m_readPosition = m_bitLength;
				return null; // unfortunate; but we need to protect against DDOS
#endif
			}

			if ((m_readPosition & 7) == 0)
			{
				// read directly
				string retval = System.Text.Encoding.UTF8.GetString(m_data, m_readPosition >> 3, byteLen);
				m_readPosition += (8 * byteLen);
				return retval;
			}

			byte[] bytes = ReadBytes(byteLen);
			return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}

		/// <summary>
		/// Reads a string written using Write(string) and returns true for success
		/// </summary>
		public bool ReadString(out string result)
		{
			uint byteLen;
			if (ReadVariableUInt32(out byteLen) == false)
			{
				result = String.Empty;
				return false;
			}

			if (byteLen <= 0)
			{
				result = String.Empty;
				return true;
			}

			if (m_bitLength - m_readPosition < (byteLen * 8))
			{
				result = String.Empty;
				return false;
			}

			if ((m_readPosition & 7) == 0)
			{
				// read directly
				result = System.Text.Encoding.UTF8.GetString(m_data, m_readPosition >> 3, (int)byteLen);
				m_readPosition += (8 * (int)byteLen);
				return true;
			}

			byte[] bytes;
			if (ReadBytes((int)byteLen, out bytes) == false)
			{
				result = String.Empty;
				return false;
			}

			result = System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
			return true;
		}

		/// <summary>
		/// Reads a value, in local time comparable to NetTime.Now, written using WriteTime() for the connection supplied
		/// </summary>
		public float ReadTime(NetConnection connection)
		{
			float remoteTime = ReadSingle();

			if (connection == null)
				throw new NetException("Cannot call ReadTime() on message without a connected sender (ie. unconnected messages)");

			// lets bypass NetConnection.GetLocalTime for speed
			return remoteTime - connection.m_remoteTimeOffset;
		}

		/// <summary>
		/// Reads a stored IPv4 endpoint description
		/// </summary>
		public NetEndPoint ReadIPEndPoint()
		{
			byte len = ReadByte();
			byte[] addressBytes = ReadBytes(len);
			int port = (int)ReadUInt16();

			var address = NetUtility.CreateAddressFromBytes(addressBytes);
			return new NetEndPoint(address, port);
		}

		/// <summary>
		/// Pads data with enough bits to reach a full byte. Decreases cpu usage for subsequent byte writes.
		/// </summary>
		public void SkipPadBits()
		{
			m_readPosition = ((m_readPosition + 7) >> 3) * 8;
		}

		/// <summary>
		/// Pads data with enough bits to reach a full byte. Decreases cpu usage for subsequent byte writes.
		/// </summary>
		public void ReadPadBits()
		{
			m_readPosition = ((m_readPosition + 7) >> 3) * 8;
		}

		/// <summary>
		/// Pads data with the specified number of bits.
		/// </summary>
		public void SkipPadBits(int numberOfBits)
		{
			m_readPosition += numberOfBits;
		}
	}

	/// <summary>
	/// Pool to store the arrays needed to read numbers bigger than one byte
	/// There is no limit to the number of stored arrays
	/// </summary>
	/// <typeparam name="T">Type of the array. Probably byte</typeparam>
	public class SimpleArrayPoolThreadSafe<T> {
		// keep track of the stored arrays to prevent storing 2 times the same array
		HashSet<int> currentReferences = new HashSet<int>();
		Dictionary<int, Stack<T[]>> reusable = new Dictionary<int, Stack<T[]>>();
		
		/// <summary>
		/// Get an array from the pool
		/// </summary>
		/// <param name="size">size of the array</param>
		public T[] Get(int size) {
			Stack<T[]> s;
			lock (reusable) {
				if (reusable.TryGetValue(size, out s) && s.Count > 0) {
					T[] t = s.Pop();
					currentReferences.Remove(t.GetHashCode());
					return t;
				} else {
					return new T[size];
				}
			}
		}
		
		/// <summary>
		/// Recycle an array
		/// Trying to recycle a already recycled array will make nothing, but cheking if it was recycled is not free
		/// </summary>
		/// <param name="array_to_recycle">The array to recycle</param>
		public void Recycle(T[] array_to_recycle) {
			if (array_to_recycle == null) return;
			if (array_to_recycle.Length == 0) return;

			Stack<T[]> s;
			lock (reusable) {
				if (reusable.TryGetValue(array_to_recycle.Length, out s)) {
					if (currentReferences.Contains(array_to_recycle.GetHashCode())) {
						// Trying to recycle an already recycled element;
						return;
					}
				} else {
					s = new Stack<T[]>();
					reusable.Add(array_to_recycle.Length, s);
				}

				currentReferences.Add(array_to_recycle.GetHashCode());
				s.Push(array_to_recycle);
			}
		}

		public void Reset() {
			lock (reusable) {
				currentReferences = new HashSet<int>();
				reusable = new Dictionary<int, Stack<T[]>>();
			}
		}
	}
}
