using System;
using System.Reflection;
using System.IO;

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
				writer.WriteMsgPack(name);
			}
			if (ValueType == typeof(string))
			{
				writer.WriteMsgPack(PropInfo.GetValue(o, emptyObjArgs) as string);
			}
			else if (ValueType == typeof(float))
			{
				writer.WriteMsgPack((float)PropInfo.GetValue(o, emptyObjArgs));
			}
			else if (ValueType == typeof(double))
			{
				writer.WriteMsgPack((double)PropInfo.GetValue(o, emptyObjArgs));
			}
			else if (ValueType == typeof(int))
			{
				writer.WriteMsgPack((int)PropInfo.GetValue(o, emptyObjArgs));
			}
			else if (ValueType == typeof(uint))
			{
				writer.WriteMsgPack((uint)PropInfo.GetValue(o, emptyObjArgs));
			}
			else if (ValueType == typeof(long))
			{
				writer.WriteMsgPack((long)PropInfo.GetValue(o, emptyObjArgs));
			}
			else if (ValueType == typeof(ulong))
			{
				writer.WriteMsgPack((ulong)PropInfo.GetValue(o, emptyObjArgs));
			}
			else
			{
				//throw new InvalidDataException("Unsupported property type [" + valueType + "]");
				MsgPackSerializer.SerializeObject(PropInfo.GetValue(o, emptyObjArgs), writer, true);
			}
		}
	}
}

