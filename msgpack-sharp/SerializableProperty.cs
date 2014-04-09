using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;

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

		internal void Serialize(object o, BinaryWriter writer)
		{
			MsgPackIO.SerializeValue(propInfo.GetValue(o, emptyObjArgs), writer);
		}
			
		internal void Deserialize(object o, BinaryReader reader)
		{
			object val = MsgPackIO.DeserializeValue(ValueType, reader);
			propInfo.SetValue(o, val, emptyObjArgs);
		}

	}
}

