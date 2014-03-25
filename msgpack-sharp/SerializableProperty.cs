using System;
using System.Reflection;
using System.IO;
using System.Text;

namespace scopely.msgpacksharp
{
	internal class SerializableProperty
	{
		internal static readonly object[] emptyObjArgs = new object[] {};
		private PropertyInfo propInfo;
		private string name;
		private Type valueType;

		internal SerializableProperty(PropertyInfo propInfo, int sequence)
		{
			this.propInfo = propInfo;
			this.name = propInfo.Name;
			this.valueType = propInfo.PropertyType;
			Sequence = sequence;
		}

		internal PropertyInfo PropInfo
		{
			get { return propInfo; }
		}

		internal string Name
		{
			get { return name; }
		}

		internal Type ValueType
		{
			get { return valueType; }
		}

		internal int Sequence { get; set; }

		internal void Serialize(object o, BinaryWriter writer, bool asDictionary)
		{
			if (asDictionary)
			{
				WriteMsgPack(writer, name);
			}
			if (ValueType == typeof(string))
			{
				WriteMsgPack(writer, PropInfo.GetValue(o, emptyObjArgs) as string);
			}
			else if (ValueType == typeof(float))
			{
				WriteMsgPack(writer, (float)PropInfo.GetValue(o, emptyObjArgs));
			}
			else if (ValueType == typeof(double))
			{
				WriteMsgPack(writer, (double)PropInfo.GetValue(o, emptyObjArgs));
			}
			else if (ValueType == typeof(int) || ValueType == typeof(uint) || ValueType == typeof(short) ||
				ValueType == typeof(ushort) || ValueType == typeof(long) || ValueType == typeof(ulong) ||
				ValueType == typeof(sbyte) || ValueType == typeof(byte))
			{
				WriteMsgPack(writer, (long)PropInfo.GetValue(o, emptyObjArgs));
			}
			else
			{
				//throw new InvalidDataException("Unsupported property type [" + valueType + "]");
				MsgPackSerializer.SerializeObject(PropInfo.GetValue(o, emptyObjArgs), writer, asDictionary);
			}
		}

		internal void Deserialize(object o, BinaryReader reader, bool asDictionary)
		{
			if (asDictionary)
			{
				throw new NotImplementedException();
			}
			if (ValueType == typeof(string))
			{
				propInfo.SetValue(o, ReadMsgPackString(reader), emptyObjArgs);
			}
			else if (ValueType == typeof(int))
			{
				propInfo.SetValue(o, (int)ReadMsgPackInt(reader), emptyObjArgs);
			}
			else if (ValueType == typeof(uint))
			{
				propInfo.SetValue(o, (uint)ReadMsgPackInt(reader), emptyObjArgs);
			}
			else if (ValueType == typeof(byte))
			{
				propInfo.SetValue(o, (byte)ReadMsgPackInt(reader), emptyObjArgs);
			}
			else if (ValueType == typeof(sbyte))
			{
				propInfo.SetValue(o, (sbyte)ReadMsgPackInt(reader), emptyObjArgs);
			}
			else if (ValueType == typeof(short))
			{
				propInfo.SetValue(o, (short)ReadMsgPackInt(reader), emptyObjArgs);
			}
			else if (ValueType == typeof(ushort))
			{
				propInfo.SetValue(o, (ushort)ReadMsgPackInt(reader), emptyObjArgs);
			}
			else if (ValueType == typeof(long))
			{
				propInfo.SetValue(o, (long)ReadMsgPackInt(reader), emptyObjArgs);
			}
			else if (ValueType == typeof(ulong))
			{
				propInfo.SetValue(o, (ulong)ReadMsgPackInt(reader), emptyObjArgs);
			}
			else if (ValueType == typeof(float))
			{
				propInfo.SetValue(o, (float)ReadMsgPackFloat(reader), emptyObjArgs);
			}
			else if (ValueType == typeof(double))
			{
				propInfo.SetValue(o, (double)ReadMsgPackDouble(reader), emptyObjArgs);
			}
			else
			{
				object newInstance = MsgPackSerializer.DeserializeObject(ValueType, reader, asDictionary);
				propInfo.SetValue(o, newInstance, emptyObjArgs);
			}
		}

		private float ReadMsgPackFloat(BinaryReader reader)
		{
			reader.ReadByte(); // 0xca
			return reader.ReadSingle();
		}

		private double ReadMsgPackDouble(BinaryReader reader)
		{
			reader.ReadByte(); // 0xcb
			return reader.ReadDouble();
		}

		private long ReadMsgPackInt(BinaryReader reader)
		{
			byte header = reader.ReadByte();
			long result = 0;
			if (header < 128)
			{
				result = header & 128;
			}
            else if (header >= MsgPackConstants.FixedInteger.NEGATIVE_MIN)
			{
				result = -(header - 224);
			}
            else if (header == MsgPackConstants.Formats.UINT_8)
			{
				result = reader.ReadByte();
			}
            else if (header == MsgPackConstants.Formats.UINT_16)
			{
				result = reader.ReadByte() << 8 + reader.ReadByte();
			}
            else if (header == MsgPackConstants.Formats.UINT_32)
			{
				result = reader.ReadByte() << 24 + reader.ReadByte() << 16 + reader.ReadByte() << 8 + reader.ReadByte();
			}
            else if (header == MsgPackConstants.Formats.UINT_64)
			{
				result = reader.ReadByte() << 56 + reader.ReadByte() << 48 + reader.ReadByte() << 40 +
				reader.ReadByte() << 32 + reader.ReadByte() << 24 + reader.ReadByte() << 16 +
				reader.ReadByte() << 8 + reader.ReadByte();
			}
            else if (header == MsgPackConstants.Formats.INT_8)
			{
				result = reader.ReadSByte();
			}
            else if (header == MsgPackConstants.Formats.INT_16)
			{
				result = reader.ReadInt16();
			}
            else if (header == MsgPackConstants.Formats.INT_32)
			{
				result = reader.ReadInt32();
			}
            else if (header == MsgPackConstants.Formats.INT_64)
			{
				result = reader.ReadInt64();
			}
			else
				throw new InvalidDataException();
			return result;
		}

		private string ReadMsgPackString(BinaryReader reader)
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
				length = reader.ReadByte() << 8 + reader.ReadByte();
			}
            else if (header == MsgPackConstants.Formats.STR_32)
			{
				length = reader.ReadByte() << 24 + reader.ReadByte() << 16 + reader.ReadByte() << 8 + reader.ReadByte();
			}
			byte[] stringBuffer = reader.ReadBytes(length);
			result = UTF8Encoding.UTF8.GetString(stringBuffer);
			return result;
		}
			
		private void WriteMsgPack(BinaryWriter writer, float val)
		{
            writer.Write(MsgPackConstants.Formats.FLOAT_32);
			writer.Write(val);
		}

		private void WriteMsgPack(BinaryWriter writer, double val)
		{
            writer.Write(MsgPackConstants.Formats.FLOAT_64);
			writer.Write(val);
		}

		private void WriteMsgPack(BinaryWriter writer, long val)
		{
            if (val >= MsgPackConstants.FixedInteger.POSITIVE_MIN && val <= MsgPackConstants.FixedInteger.POSITIVE_MAX)
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
			else if (val >= short.MinValue && val <= short.MaxValue)
			{
                writer.Write(MsgPackConstants.Formats.INT_16);
				writer.Write((short)val);
			}
			else if (val >= Int32.MinValue && val <= Int32.MaxValue)
			{
                writer.Write(MsgPackConstants.Formats.INT_32);
				writer.Write((int)val);
			}
			else if (val < 0)
			{
                writer.Write(MsgPackConstants.Formats.INT_64);
				writer.Write((long)val);
			}
			else if (val >= 0 && val <= 65535)
			{
                writer.Write(MsgPackConstants.Formats.UINT_16);
				writer.Write((byte)((val & 0xFF00) >> 8));
				writer.Write((byte)(val & 0x00FF));
			}
			else if (val >= 0 && val <= UInt32.MaxValue)
			{
                writer.Write(MsgPackConstants.Formats.UINT_32);
				writer.Write((byte)((val & 0xFF000000) >> 24));
				writer.Write((byte)((val & 0x00FF0000) >> 16));
				writer.Write((byte)((val & 0x0000FF00) >> 8));
				writer.Write((byte)(val & 0x000000FF));
			}
			else if (val >= 0)
			{
                writer.Write(MsgPackConstants.Formats.UINT_64);
				writer.Write((byte)(((ulong)val & 0xFF00000000000000) >> 56));
				writer.Write((byte)((val & 0x00FF000000000000) >> 48));
				writer.Write((byte)((val & 0x0000FF0000000000) >> 40));
				writer.Write((byte)((val & 0x000000FF00000000) >> 32));
				writer.Write((byte)((val & 0x00000000FF000000) >> 24));
				writer.Write((byte)((val & 0x0000000000FF0000) >> 16));
				writer.Write((byte)((val & 0x000000000000FF00) >> 8));
				writer.Write((byte)((val & 0x00000000000000FF)));
			}
		}

		private void WriteMsgPack(BinaryWriter writer, string s)
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
				else if (length <= 255)
				{
                    writer.Write(MsgPackConstants.Formats.STR_8);
					writer.Write((byte)length);
				}
				else if (length <= 65535)
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
	}
}

