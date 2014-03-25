using System;
using System.Reflection;
using System.IO;
using System.Text;

namespace scopely.msgpacksharp
{
	internal class SerializableProperty
	{
		private static readonly object[] emptyObjArgs = new object[] {};
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
			else if (ValueType == typeof(int))
			{
				WriteMsgPack(writer, (int)PropInfo.GetValue(o, emptyObjArgs));
			}
			else if (ValueType == typeof(uint))
			{
				WriteMsgPack(writer, (uint)PropInfo.GetValue(o, emptyObjArgs));
			}
			else if (ValueType == typeof(long))
			{
				WriteMsgPack(writer, (long)PropInfo.GetValue(o, emptyObjArgs));
			}
			else if (ValueType == typeof(ulong))
			{
				WriteMsgPack(writer, (ulong)PropInfo.GetValue(o, emptyObjArgs));
			}
			else
			{
				//throw new InvalidDataException("Unsupported property type [" + valueType + "]");
				MsgPackSerializer.SerializeObject(PropInfo.GetValue(o, emptyObjArgs), writer, true);
			}
		}

		internal void Deserialize(object o, BinaryReader reader, bool asDictionary)
		{
			if (asDictionary)
			{
				throw new NotSupportedException();
			}
			if (ValueType == typeof(string))
			{
				propInfo.SetValue(o, ReadMsgPackString(reader), emptyObjArgs);
			}
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

