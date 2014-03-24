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

		internal SerializableProperty(PropertyInfo propInfo)
		{
			this.propInfo = propInfo;
			this.name = propInfo.Name;
			this.valueType = propInfo.PropertyType;
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

		internal void Serialize(object o, BinaryWriter writer)
		{
			writer.WriteMsgPack(name);
			if (ValueType == typeof(string))
			{
				writer.WriteMsgPack(PropInfo.GetValue(o, emptyObjArgs) as string);
			}
			else if (ValueType == typeof(float) || ValueType == typeof(double))
			{
				writer.WriteMsgPack((float)PropInfo.GetValue(o, emptyObjArgs));
			}
			else
			{
				//throw new InvalidDataException("Unsupported property type [" + valueType + "]");
				MsgPackSerializer.SerializeObject(PropInfo.GetValue(o, emptyObjArgs), writer);
			}
		}
	}
}

