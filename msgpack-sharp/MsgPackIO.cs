using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections;
using MsgPack.Serialization;

namespace scopely.msgpacksharp
{
	public static class MsgPackIO
	{
		private static readonly DateTime unixEpocUtc = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
		private const string nullProhibitedExceptionMessage = "Null value encountered but is prohibited";

		internal static bool DeserializeCollection(IList collection, BinaryReader reader)
		{
			bool isNull = true;
			if (!collection.GetType().IsGenericType)
				throw new NotSupportedException("Only generic List<T> lists are supported");
			Type elementType = collection.GetType().GetGenericArguments()[0];
			byte header = reader.ReadByte();
			int numElements = 0;
			if (header != MsgPackConstants.Formats.NIL)
			{
				if (header >= MsgPackConstants.FixedArray.MIN && header <= MsgPackConstants.FixedArray.MAX)
				{
					numElements = header - MsgPackConstants.FixedArray.MIN;
				}
				else if (header == MsgPackConstants.Formats.ARRAY_16)
				{
					numElements = (reader.ReadByte() << 8) + 
						reader.ReadByte();
				}
				else if (header == MsgPackConstants.Formats.ARRAY_32)
				{
					numElements = (reader.ReadByte() << 24) +
						(reader.ReadByte() << 16) +
						(reader.ReadByte() << 8) +
						reader.ReadByte();
				}
				else
				{
					throw new ApplicationException("The serialized data format is invalid due to an invalid array size specification at offset " + reader.BaseStream.Position);
				}
				isNull = false;
				for (int i = 0; i < numElements; i++)
				{
					object o = DeserializeValue(elementType, reader, NilImplication.MemberDefault);
					collection.Add(Convert.ChangeType(o, elementType));
				}
			}
			return isNull;
		}

		internal static bool DeserializeCollection(IDictionary collection, BinaryReader reader)
		{
			bool isNull = true;
			if (!collection.GetType().IsGenericType)
				throw new NotSupportedException("Only generic Dictionary<T,U> dictionaries are supported");
			Type keyType = collection.GetType().GetGenericArguments()[0];
			Type valueType = collection.GetType().GetGenericArguments()[1];
			byte header = reader.ReadByte();
			if (header != MsgPackConstants.Formats.NIL)
			{
				int numElements = 0;
				if (header >= MsgPackConstants.FixedMap.MIN && header <= MsgPackConstants.FixedMap.MAX)
				{
					numElements = header - MsgPackConstants.FixedMap.MIN;
				}
				else if (header == MsgPackConstants.Formats.MAP_16)
				{
					numElements = (reader.ReadByte() << 8) + 
						reader.ReadByte();
				}
				else if (header == MsgPackConstants.Formats.MAP_32)
				{
					numElements = (reader.ReadByte() << 24) +
						(reader.ReadByte() << 16) +
						(reader.ReadByte() << 8) +
						reader.ReadByte();
				}
				else
					throw new ApplicationException("The serialized data format is invalid due to an invalid map size specification");
				isNull = false;
				for (int i = 0; i < numElements; i++)
				{
					object key = DeserializeValue(keyType, reader, NilImplication.MemberDefault);
					object val = DeserializeValue(valueType, reader, NilImplication.MemberDefault);
					collection.Add(key, val);
				}
			}
			return isNull;
		}

		internal static long ToUnixMillis(DateTime dateTime)
		{
			return (long)dateTime.ToUniversalTime().Subtract(unixEpocUtc).TotalMilliseconds;
		}

		internal static DateTime ToDateTime(long value)
		{
			return unixEpocUtc.AddMilliseconds(value).ToLocalTime();
		}

		internal static object DeserializeValue(Type type, BinaryReader reader, NilImplication nilImplication)
		{
			type = Nullable.GetUnderlyingType(type) ?? type;
			object result = null;
			if (type == typeof(string))
			{
				result = ReadMsgPackString(reader, nilImplication);
			}
			else if (type == typeof(int) || type == typeof(uint) ||
					 type == typeof(byte) || type == typeof(sbyte) ||
	    			 type == typeof(short) || type == typeof(ushort) ||
					 type == typeof(long) || type == typeof(ulong))
			{
				result = ReadMsgPackInt(reader, nilImplication);
			}
			else if (type == typeof(float))
			{
				result = ReadMsgPackFloat(reader, nilImplication);
			}
			else if (type == typeof(double))
			{
				result = ReadMsgPackDouble(reader, nilImplication);
			}
			else if (type == typeof(Boolean) || type == typeof(bool))
			{
				result = ReadMsgPackBoolean(reader, nilImplication);
			}
			else if (type == typeof(DateTime))
			{
				long unixEpochTicks = (long)ReadMsgPackInt(reader, nilImplication);
				result = ToDateTime(unixEpochTicks);
			}
			else if (type.IsEnum)
			{
				string enumVal = (string)ReadMsgPackString(reader, nilImplication);
				result = Enum.Parse(type, enumVal);
			}
			else if (type.IsArray)
			{
				throw new ApplicationException("Raw arrays are not supported by msgpack-sharp");
			}
			else
			{
				ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);
				if (constructorInfo == null)
					throw new ApplicationException("Can't deserialize Type [" + type + "] because it has no default constructor");
				result = constructorInfo.Invoke(SerializableProperty.emptyObjArgs);
				MsgPackSerializer.DeserializeObject(result, reader, nilImplication);
			}
			return result;
		}

		internal static byte ReadHeader(Type t, BinaryReader reader, NilImplication nilImplication, out object result)
		{
			result = null;
			byte v = reader.ReadByte();
			if (v == MsgPackConstants.Formats.NIL)
			{
				if (nilImplication == NilImplication.MemberDefault)
				{
					if (t.IsValueType)
						result = Activator.CreateInstance(t);
				}
				else if (nilImplication == NilImplication.Prohibit)
					throw new ApplicationException(nullProhibitedExceptionMessage);
			}
			return v;
		}

		internal static object ReadMsgPackBoolean(BinaryReader reader, NilImplication nilImplication)
		{
			object result;
			byte v = ReadHeader(typeof(bool), reader, nilImplication, out result);
			if (v != MsgPackConstants.Formats.NIL)
			{
				result = v == MsgPackConstants.Bool.TRUE ? true : false;
			}
			return result;
		}

		internal static object ReadMsgPackFloat(BinaryReader reader, NilImplication nilImplication)
		{
			object result;
			byte v = ReadHeader(typeof(float), reader, nilImplication, out result);
			if (v != MsgPackConstants.Formats.NIL)
			{
				if (v != MsgPackConstants.Formats.FLOAT_32)
					throw new ApplicationException("Serialized data doesn't match type being deserialized to");
				byte[] data = reader.ReadBytes(4);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				result = BitConverter.ToSingle(data, 0);
			}
			return result;
		}

		internal static object ReadMsgPackDouble(BinaryReader reader, NilImplication nilImplication)
		{
			object result;
			byte v = ReadHeader(typeof(double), reader, nilImplication, out result);
			if (v != MsgPackConstants.Formats.NIL)
			{
				if (v != MsgPackConstants.Formats.FLOAT_32)
					throw new ApplicationException("Serialized data doesn't match type being deserialized to");
				byte[] data = reader.ReadBytes(8);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				result = BitConverter.ToDouble(data, 0);
			}
			return result;
		}

		internal static object ReadMsgPackULong(BinaryReader reader, NilImplication nilImplication)
		{
			object result;
			byte v = ReadHeader(typeof(ulong), reader, nilImplication, out result);
			if (v != MsgPackConstants.Formats.NIL)
			{
				if (v != MsgPackConstants.Formats.UINT_64)
					throw new ApplicationException("Serialized data doesn't match type being deserialized to");
				result = reader.ReadUInt64();
			}
			return result;
		}

		internal static object ReadMsgPackInt(BinaryReader reader, NilImplication nilImplication)
		{
			object result;
			byte v = ReadHeader(typeof(long), reader, nilImplication, out result);
			if (v != MsgPackConstants.Formats.NIL)
			{
				if (v < MsgPackConstants.FixedInteger.POSITIVE_MAX)
				{
					result = v;
				}
				else if (v >= MsgPackConstants.FixedInteger.NEGATIVE_MIN)
				{
					result = -(v - MsgPackConstants.FixedInteger.NEGATIVE_MIN);
				}
				else if (v == MsgPackConstants.Formats.UINT_8)
				{
					result = reader.ReadByte();
				}
				else if (v == MsgPackConstants.Formats.UINT_16)
				{
					result = (reader.ReadByte() << 8) + 
						reader.ReadByte();
				}
				else if (v == MsgPackConstants.Formats.UINT_32)
				{
					result = (reader.ReadByte() << 24) + 
						(reader.ReadByte() << 16) + 
						(reader.ReadByte() << 8) + 
						reader.ReadByte();
				}
				else if (v == MsgPackConstants.Formats.UINT_64)
				{
					result = (reader.ReadByte() << 56) +
						(reader.ReadByte() << 48) +
						(reader.ReadByte() << 40) +
						(reader.ReadByte() << 32) +
						(reader.ReadByte() << 24) +
						(reader.ReadByte() << 16) +
						(reader.ReadByte() << 8) +
						reader.ReadByte();
				}
				else if (v == MsgPackConstants.Formats.INT_8)
				{
					result = reader.ReadSByte();
				}
				else if (v == MsgPackConstants.Formats.INT_16)
				{
					byte[] data = reader.ReadBytes(2);
					if (BitConverter.IsLittleEndian)
						Array.Reverse(data);
					result = BitConverter.ToInt16(data, 0);
				}
				else if (v == MsgPackConstants.Formats.INT_32)
				{
					byte[] data = reader.ReadBytes(4);
					if (BitConverter.IsLittleEndian)
						Array.Reverse(data);
					result = BitConverter.ToInt32(data, 0);
				}
				else if (v == MsgPackConstants.Formats.INT_64)
				{
					byte[] data = reader.ReadBytes(8);
					if (BitConverter.IsLittleEndian)
						Array.Reverse(data);
					result = BitConverter.ToInt64(data, 0);
				}
				else
					throw new ApplicationException("Serialized data doesn't match type being deserialized to");
			}
			return result;
		}

		internal static object ReadMsgPackString(BinaryReader reader, NilImplication nilImplication)
		{
			object result;
			byte v = ReadHeader(typeof(string), reader, nilImplication, out result);
			if (v != MsgPackConstants.Formats.NIL)
			{
				int length = 0;
				if (v >= MsgPackConstants.FixedString.MIN && v <= MsgPackConstants.FixedString.MAX)
				{
					length = v - MsgPackConstants.FixedString.MIN;
				}
				else if (v == MsgPackConstants.Formats.STR_8)
				{
					length = reader.ReadByte();
				}
				else if (v == MsgPackConstants.Formats.STR_16)
				{
					length = (reader.ReadByte() << 8) + 
						reader.ReadByte();
				}
				else if (v == MsgPackConstants.Formats.STR_32)
				{
					length = (reader.ReadByte() << 24) + 
						(reader.ReadByte() << 16) + 
						(reader.ReadByte() << 8) + 
						reader.ReadByte();
				}
				byte[] stringBuffer = reader.ReadBytes(length);
				result = UTF8Encoding.UTF8.GetString(stringBuffer);
			}
			return result;
		}

		internal static void WriteMsgPack(BinaryWriter writer, bool val)
		{
			if (val)
				writer.Write(MsgPackConstants.Bool.TRUE);
			else
				writer.Write(MsgPackConstants.Bool.FALSE);
		}

		internal static void WriteMsgPack(BinaryWriter writer, float val)
		{
			byte[] data = BitConverter.GetBytes(val);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			writer.Write(MsgPackConstants.Formats.FLOAT_32);
			writer.Write(data);
		}

		internal static void WriteMsgPack(BinaryWriter writer, double val)
		{
			byte[] data = BitConverter.GetBytes(val);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			writer.Write(MsgPackConstants.Formats.FLOAT_64);
			writer.Write(data);
		}

		internal static void WriteMsgPack(BinaryWriter writer, DateTime val)
		{
			WriteMsgPack(writer, ToUnixMillis(val));
		}

		internal static void WriteMsgPack(BinaryWriter writer, sbyte val)
		{
			writer.Write(MsgPackConstants.Formats.INT_8);
			writer.Write(val);
		}

		internal static void WriteMsgPack(BinaryWriter writer, byte val)
		{
			writer.Write(MsgPackConstants.Formats.UINT_8);
			writer.Write(val);
		}

		internal static void WriteMsgPack(BinaryWriter writer, ushort val)
		{
			if (val <= MsgPackConstants.FixedInteger.POSITIVE_MAX)
			{
				writer.Write((byte)val);
			}
			else if (val <= Byte.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_8);
				writer.Write((byte)val);
			}
			else
			{
				writer.Write(MsgPackConstants.Formats.UINT_16);
				byte[] data = BitConverter.GetBytes(val);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
		}

		internal static void WriteMsgPack(BinaryWriter writer, short val)
		{
			if (val >= 0 && val <= MsgPackConstants.FixedInteger.POSITIVE_MAX)
			{
				writer.Write((byte)val);
			}
			else if (val >= 0 && val <= byte.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_8);
				writer.Write((byte)val);
			}
			else if (val >= SByte.MinValue && val <= SByte.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.INT_8);
				writer.Write((sbyte)val);
			}
			else
			{
				writer.Write(MsgPackConstants.Formats.INT_16);
				byte[] data = BitConverter.GetBytes(val);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
		}

		internal static void WriteMsgPack(BinaryWriter writer, uint val)
		{
			if (val <= MsgPackConstants.FixedInteger.POSITIVE_MAX)
			{
				writer.Write((byte)val);
			}
			else if (val <= byte.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_8);
				writer.Write((byte)val);
			}
			else if (val <= UInt16.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_16);
				ushort outVal = (ushort)val;
				byte[] data = BitConverter.GetBytes(outVal);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
			else
			{
				writer.Write(MsgPackConstants.Formats.UINT_32);
				byte[] data = BitConverter.GetBytes(val);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
		}

		internal static void WriteMsgPack(BinaryWriter writer, int val)
		{
			if (val >= 0 && val <= MsgPackConstants.FixedInteger.POSITIVE_MAX)
			{
				writer.Write((byte)val);
			}
			else if (val >= 0 && val <= byte.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_8);
				writer.Write((byte)val);
			}
			else if (val >= sbyte.MinValue && val <= sbyte.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.INT_8);
				writer.Write((sbyte)val);
			}
			else if (val >= Int16.MinValue && val <= Int16.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.INT_16);
				short outVal = (short)val;
				byte[] data = BitConverter.GetBytes(outVal);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
			else if (val >= 0 && val <= UInt16.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_16);
				ushort outVal = (ushort)val;
				byte[] data = BitConverter.GetBytes(outVal);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
			else
			{
				writer.Write(MsgPackConstants.Formats.INT_32);
				byte[] data = BitConverter.GetBytes(val);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
		}

		internal static void WriteMsgPack(BinaryWriter writer, ulong val)
		{
			if (val <= MsgPackConstants.FixedInteger.POSITIVE_MAX)
			{
				writer.Write((byte)val);
			}
			else if (val <= byte.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_8);
				writer.Write((byte)val);
			}
			else if (val <= UInt16.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_16);
				ushort outVal = (ushort)val;
				byte[] data = BitConverter.GetBytes(outVal);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
			else if (val <= UInt32.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_32);
				uint outVal = (uint)val;
				byte[] data = BitConverter.GetBytes(outVal);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
			else
			{
				writer.Write(MsgPackConstants.Formats.UINT_64);
				byte[] data = BitConverter.GetBytes(val);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
		}

		internal static void WriteMsgPack(BinaryWriter writer, long val)
		{
			if (val >= 0 && val <= MsgPackConstants.FixedInteger.POSITIVE_MAX)
			{
				writer.Write((byte)val);
			}
			else if (val >= 0 && val <= byte.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_8);
				writer.Write((byte)val);
			}
			else if (val >= sbyte.MinValue && val <= sbyte.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.INT_8);
				writer.Write((sbyte)val);
			}
			else if (val >= Int16.MinValue && val <= Int16.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.INT_16);
				short outVal = (short)val;
				byte[] data = BitConverter.GetBytes(outVal);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
			else if (val >= 0 && val <= UInt16.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_16);
				ushort outVal = (ushort)val;
				byte[] data = BitConverter.GetBytes(outVal);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
			else if (val >= Int32.MinValue && val <= Int32.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.INT_32);
				int outVal = (int)val;
				byte[] data = BitConverter.GetBytes(outVal);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
			else if (val >= 0 && val <= UInt32.MaxValue)
			{
				writer.Write(MsgPackConstants.Formats.UINT_32);
				uint outVal = (uint)val;
				byte[] data = BitConverter.GetBytes(outVal);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
			else
			{
				writer.Write(MsgPackConstants.Formats.INT_64);
				byte[] data = BitConverter.GetBytes(val);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				writer.Write(data);
			}
		}

		internal static void WriteMsgPack(BinaryWriter writer, string s)
		{
			if (string.IsNullOrEmpty(s))
				writer.Write(MsgPackConstants.FixedString.MIN);
			else
			{
				byte[] utf8Bytes = UTF8Encoding.UTF8.GetBytes(s);
				uint length = (uint)utf8Bytes.Length;
				if (length <= MsgPackConstants.FixedString.MAX_LENGTH)
				{
					byte val = (byte)(MsgPackConstants.FixedString.MIN | length);
					writer.Write(val);
				}
				else if (length <= byte.MaxValue)
				{
					writer.Write(MsgPackConstants.Formats.STR_8);
					writer.Write((byte)length);
				}
				else if (length <= ushort.MaxValue)
				{
					writer.Write(MsgPackConstants.Formats.STR_16);
					ushort outVal = (ushort)length;
					byte[] data = BitConverter.GetBytes(outVal);
					if (BitConverter.IsLittleEndian)
						Array.Reverse(data);
					writer.Write(data);
				}
				else
				{
					writer.Write(MsgPackConstants.Formats.STR_32);
					uint outVal = (uint)length;
					byte[] data = BitConverter.GetBytes(outVal);
					if (BitConverter.IsLittleEndian)
						Array.Reverse(data);
					writer.Write(data);
				}
				for (int i = 0; i < utf8Bytes.Length; i++)
				{
					writer.Write(utf8Bytes[i]);
				}
			}
		}
			
		internal static void SerializeEnumerable(IEnumerator collection, BinaryWriter writer)
		{
			while (collection.MoveNext())
			{
				object val = collection.Current;
				SerializeValue(val, writer);
			}
		}
			
		internal static void SerializeValue(object val, BinaryWriter writer)
		{
			if (val == null)
				writer.Write(MsgPackConstants.Formats.NIL);
			else
			{
				Type t = val.GetType();
				t = Nullable.GetUnderlyingType(t) ?? t;
				if (t == typeof(string))
				{
					WriteMsgPack(writer, (string)val);
				}
				else if (t == typeof(float) || t == typeof(Single))
				{
					WriteMsgPack(writer, (float)val);
				}
				else if (t == typeof(double) || t == typeof(Double))
				{
					WriteMsgPack(writer, (double)val);
				}
				else if (t == typeof(byte) || t == typeof(Byte))
				{
					WriteMsgPack(writer, (byte)val);
				}
				else if (t == typeof(short) || t == (typeof(Int16)))
				{
					WriteMsgPack(writer, (short)val);
				}
				else if (t == typeof(ushort) || t == (typeof(UInt16)))
				{
					WriteMsgPack(writer, (ushort)val);
				}
				else if (t == typeof(int) || t == (typeof(Int32)))
				{
					WriteMsgPack(writer, (int)val);
				}
				else if (t == typeof(uint) || t == (typeof(UInt32)))
				{
					WriteMsgPack(writer, (uint)val);
				}
				else if (t == typeof(long) || t == (typeof(Int64)))
				{
					WriteMsgPack(writer, (long)val);
				}
				else if (t == typeof(ulong) || t == (typeof(UInt64)))
				{
					WriteMsgPack(writer, (ulong)val);
				}
				else if (t == typeof(bool) || t == (typeof(Boolean)))
				{
					WriteMsgPack(writer, (bool)val);
				}
				else if (t == typeof(DateTime))
				{
					WriteMsgPack(writer, (DateTime)val);
				}
				else if (t.IsEnum)
				{
					WriteMsgPack(writer, Enum.GetName(t, val));
				}
				else if (t.IsArray)
				{
					Array array = val as Array;
					if (array == null)
					{
						writer.Write((byte)MsgPackConstants.Formats.NIL);
					}
					else
					{
						if (array.Length <= 15)
						{
							byte arrayVal = (byte)(MsgPackConstants.FixedArray.MIN + array.Length);
							writer.Write(arrayVal);
						}
						else if (array.Length <= UInt16.MaxValue)
						{
							writer.Write((byte)MsgPackConstants.Formats.ARRAY_16);
							byte[] data = BitConverter.GetBytes((ushort)array.Length);
							if (BitConverter.IsLittleEndian)
								Array.Reverse(data);
							writer.Write(data);
						}
						else
						{
							writer.Write((byte)MsgPackConstants.Formats.ARRAY_32);
							byte[] data = BitConverter.GetBytes((uint)array.Length);
							if (BitConverter.IsLittleEndian)
								Array.Reverse(data);
							writer.Write(data);
						}
						SerializeEnumerable(array.GetEnumerator(), writer);
					}
				}
				else if (MsgPackSerializer.IsGenericList(t))
				{
					if (val == null)
					{
						writer.Write((byte)MsgPackConstants.Formats.NIL);
					}
					else
					{
						IList list = val as IList;
						if (list.Count <= 15)
						{
							byte arrayVal = (byte)(MsgPackConstants.FixedArray.MIN + list.Count);
							writer.Write(arrayVal);
						}
						else if (list.Count <= UInt16.MaxValue)
						{
							writer.Write((byte)MsgPackConstants.Formats.ARRAY_16);
							byte[] data = BitConverter.GetBytes((ushort)list.Count);
							if (BitConverter.IsLittleEndian)
								Array.Reverse(data);
							writer.Write(data);
						}
						else
						{
							writer.Write((byte)MsgPackConstants.Formats.ARRAY_32);
							byte[] data = BitConverter.GetBytes((uint)list.Count);
							if (BitConverter.IsLittleEndian)
								Array.Reverse(data);
							writer.Write(data);
						}
						SerializeEnumerable(list.GetEnumerator(), writer);
					}
				}
				else if (MsgPackSerializer.IsGenericDictionary(t))
				{
					if (val == null)
					{
						writer.Write((byte)MsgPackConstants.Formats.NIL);
					}
					else
					{
						IDictionary dictionary = val as IDictionary;
						if (dictionary.Count <= 15)
						{
							byte header = (byte)(MsgPackConstants.FixedMap.MIN + dictionary.Count);
							writer.Write(header);
						}
						else if (dictionary.Count <= UInt16.MaxValue)
						{
							writer.Write((byte)MsgPackConstants.Formats.ARRAY_16);
							byte[] data = BitConverter.GetBytes((ushort)dictionary.Count);
							if (BitConverter.IsLittleEndian)
								Array.Reverse(data);
							writer.Write(data);
						}
						else
						{
							writer.Write((byte)MsgPackConstants.Formats.ARRAY_32);
							byte[] data = BitConverter.GetBytes((uint)dictionary.Count);
							if (BitConverter.IsLittleEndian)
								Array.Reverse(data);
							writer.Write(data);
						}
						IDictionaryEnumerator enumerator = dictionary.GetEnumerator();
						while (enumerator.MoveNext())
						{
							SerializeValue(enumerator.Key, writer);
							SerializeValue(enumerator.Value, writer);
						}
					}
				}
				else
				{
					MsgPackSerializer.SerializeObject(val, writer);
				}
			}
		}
	}
}

