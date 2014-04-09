using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections;

namespace scopely.msgpacksharp
{
	public static class MsgPackIO
	{
		private static readonly DateTime unixEpocUtc = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );

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
					numElements = reader.ReadByte() << 8 + reader.ReadByte();
				}
				else if (header == MsgPackConstants.Formats.ARRAY_32)
				{
					numElements = reader.ReadByte() << 24 +
					reader.ReadByte() << 16 +
					reader.ReadByte() << 8 +
					reader.ReadByte();
				}
				else
				{
					throw new ApplicationException("The serialized data format is invalid due to an invalid array size specification at offset " + reader.BaseStream.Position);
				}
				isNull = false;
				for (int i = 0; i < numElements; i++)
				{
					object o = DeserializeValue(elementType, reader);
					collection.Add(o);
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
					numElements = reader.ReadByte() << 8 + reader.ReadByte();
				}
				else if (header == MsgPackConstants.Formats.MAP_32)
				{
					numElements = reader.ReadByte() << 24 +
					reader.ReadByte() << 16 +
					reader.ReadByte() << 8 +
					reader.ReadByte();
				}
				else
					throw new ApplicationException("The serialized data format is invalid due to an invalid map size specification");
				isNull = false;
				for (int i = 0; i < numElements; i++)
				{
					object key = DeserializeValue(keyType, reader);
					object val = DeserializeValue(valueType, reader);
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

		internal static object DeserializeValue(Type type, BinaryReader reader)
		{
			object result = null;
			if (type == typeof(string))
			{
				result = ReadMsgPackString(reader);
			}
			else if (type == typeof(int))
			{
				result = (int)ReadMsgPackInt(reader);
			}
			else if (type == typeof(uint))
			{
				result = (uint)ReadMsgPackInt(reader);
			}
			else if (type == typeof(byte))
			{
				result = (byte)ReadMsgPackInt(reader);
			}
			else if (type == typeof(sbyte))
			{
				result = (sbyte)ReadMsgPackInt(reader);
			}
			else if (type == typeof(short))
			{
				result = (short)ReadMsgPackInt(reader);
			}
			else if (type == typeof(ushort))
			{
				result = (ushort)ReadMsgPackInt(reader);
			}
			else if (type == typeof(long))
			{
				result = (long)ReadMsgPackInt(reader);
			}
			else if (type == typeof(ulong))
			{
				result = (ulong)ReadMsgPackInt(reader);
			}
			else if (type == typeof(float))
			{
				result = ReadMsgPackFloat(reader);
			}
			else if (type == typeof(double))
			{
				result = ReadMsgPackDouble(reader);
			}
			else if (type == typeof(Boolean) || type == typeof(bool))
			{
				result = ReadMsgPackBoolean(reader);
			}
			else if (type == typeof(DateTime))
			{
				long unixEpochTicks = ReadMsgPackInt(reader);
				result = ToDateTime(unixEpochTicks);
			}
			else if (type.IsEnum)
			{
				string enumVal = ReadMsgPackString(reader);
				result = Enum.Parse(type, enumVal);
			}
			else if (type.IsArray)
			{
				byte header = reader.ReadByte();
				int length = 0;
				if (header != MsgPackConstants.Formats.NIL)
				{
					if (header >= MsgPackConstants.FixedArray.MIN && header <= MsgPackConstants.FixedArray.MAX)
					{
						length = header - MsgPackConstants.FixedArray.MIN;
					}
					else if (header == MsgPackConstants.Formats.ARRAY_16)
					{
						length = reader.ReadByte() << 8 + reader.ReadByte();
					}
					else if (header == MsgPackConstants.Formats.ARRAY_32)
					{
						length = reader.ReadByte() << 24 +
						reader.ReadByte() << 16 +
						reader.ReadByte() << 8 +
						reader.ReadByte();
					}
					else
						throw new ApplicationException("The serialized data format is invalid due to an invalid array size specification");
				}
				if (type.GetElementType() == typeof(int))
				{
					int[] arr = new int[length];
					for (int i = 0; i < length; i++)
					{
						arr[i] = (int)DeserializeValue(type.GetElementType(), reader);
					}
					result = arr;
				}
				else
				{
					Array array = Array.CreateInstance(type, length);
					for (int i = 0; i < length; i++)
					{
						object thing = DeserializeValue(type.GetElementType(), reader);
						array.SetValue(thing, i);
					}
					result = array;
				}
			}
			else
			{
				ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);
				if (constructorInfo == null)
					throw new ApplicationException("Can't deserialize Type [" + type + "] because it has no default constructor");
				result = constructorInfo.Invoke(SerializableProperty.emptyObjArgs);
				MsgPackSerializer.DeserializeObject(result, reader);
			}
			return result;
		}

		internal static bool ReadMsgPackBoolean(BinaryReader reader)
		{
			return reader.ReadByte() == MsgPackConstants.Bool.TRUE ? true : false;
		}

		internal static float ReadMsgPackFloat(BinaryReader reader)
		{
			if (reader.ReadByte() != 0xca)
				throw new ApplicationException("Serialized data doesn't match type being deserialized to");
			byte[] data = reader.ReadBytes(4);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			return BitConverter.ToSingle(data, 0);
		}

		internal static double ReadMsgPackDouble(BinaryReader reader)
		{
			if (reader.ReadByte() != 0xcb)
				throw new ApplicationException("Serialized data doesn't match type being deserialized to");
			byte[] data = reader.ReadBytes(8);
			if (BitConverter.IsLittleEndian)
				Array.Reverse(data);
			return BitConverter.ToDouble(data, 0);
		}

		internal static ulong ReadMsgPackULong(BinaryReader reader)
		{
			if (reader.ReadByte() != 0xcf)
				throw new InvalidDataException("Serialized data doesn't match type being deserialized to");
			return reader.ReadUInt64();
		}

		internal static long ReadMsgPackInt(BinaryReader reader)
		{
			byte header = reader.ReadByte();
			long result = 0;
			if (header < MsgPackConstants.FixedInteger.POSITIVE_MAX)
			{
				result = header;
			}
			else if (header >= MsgPackConstants.FixedInteger.NEGATIVE_MIN)
			{
				result = -(header - MsgPackConstants.FixedInteger.NEGATIVE_MIN);
			}
			else if (header == MsgPackConstants.Formats.UINT_8)
			{
				result = reader.ReadByte();
			}
			else if (header == MsgPackConstants.Formats.UINT_16)
			{
				result = reader.ReadByte() + reader.ReadByte() << 8;
			}
			else if (header == MsgPackConstants.Formats.UINT_32)
			{
				result = reader.ReadByte() + reader.ReadByte() << 8 + reader.ReadByte() << 16 + reader.ReadByte() << 24;
			}
			else if (header == MsgPackConstants.Formats.UINT_64)
			{
				result = reader.ReadByte() +
					reader.ReadByte() << 8 +
					reader.ReadByte() << 16 +
					reader.ReadByte() << 24 +
					reader.ReadByte() << 32 +
					reader.ReadByte() << 40 +
					reader.ReadByte() << 48 +
					reader.ReadByte() << 56;
			}
			else if (header == MsgPackConstants.Formats.INT_8)
			{
				result = reader.ReadSByte();
			}
			else if (header == MsgPackConstants.Formats.INT_16)
			{
				byte[] data = reader.ReadBytes(2);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				result = BitConverter.ToInt16(data, 0);
			}
			else if (header == MsgPackConstants.Formats.INT_32)
			{
				byte[] data = reader.ReadBytes(4);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				result = BitConverter.ToInt32(data, 0);
			}
			else if (header == MsgPackConstants.Formats.INT_64)
			{
				byte[] data = reader.ReadBytes(8);
				if (BitConverter.IsLittleEndian)
					Array.Reverse(data);
				result = BitConverter.ToInt64(data, 0);
			}
			else
				throw new ApplicationException("Serialized data doesn't match type being deserialized to");
			return result;
		}

		internal static string ReadMsgPackString(BinaryReader reader)
		{
			string result = null;
			int length = 0;
			byte header = reader.ReadByte();
			if (header >= MsgPackConstants.FixedString.MIN && header <= MsgPackConstants.FixedString.MAX)
			{
				length = header - MsgPackConstants.FixedString.MIN;
			}
			else if (header == MsgPackConstants.Formats.STR_8)
			{
				length = reader.ReadByte();
			}
			else if (header == MsgPackConstants.Formats.STR_16)
			{
				length = reader.ReadByte() + reader.ReadByte() << 8;
			}
			else if (header == MsgPackConstants.Formats.STR_32)
			{
				length = reader.ReadByte() + reader.ReadByte() << 8 + reader.ReadByte() << 16 + reader.ReadByte() << 24;
			}
			byte[] stringBuffer = reader.ReadBytes(length);
			result = UTF8Encoding.UTF8.GetString(stringBuffer);
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
					writer.Write((byte)((length | 0xFF00) >> 8));
					writer.Write((byte)(length | 0x00FF));
				}
				else
				{
					writer.Write(MsgPackConstants.Formats.STR_32);
					writer.Write((byte)((length | 0xFF000000) >> 24));
					writer.Write((byte)((length | 0x00FF0000) >> 16));
					writer.Write((byte)((length | 0x0000FF00) >> 8));
					writer.Write((byte)( length | 0x000000FF));
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
				else if (t.GetInterface("System.Collections.Generic.IList`1") != null)
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
				else if (t.GetInterface("System.Collections.Generic.IDictionary`2") != null)
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

