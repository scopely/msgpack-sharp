using System;
using System.Reflection;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Collections;
using MsgPack.Serialization;

namespace scopely.msgpacksharp
{
	internal class SerializableProperty
	{
		internal static readonly object[] emptyObjArgs = new object[] {};
		private PropertyInfo propInfo;
		private string name;
		private Type valueType;
		private NilImplication nilImplication = NilImplication.MemberDefault; 

		internal SerializableProperty(PropertyInfo propInfo, int sequence, NilImplication nilImplication)
		{
			this.propInfo = propInfo;
			this.name = propInfo.Name;
			//this.valueType = propInfo.PropertyType;
			this.valueType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
			this.nilImplication = nilImplication;
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
            MsgPackIO.SerializeValue(propInfo.GetGetMethod().Invoke(o, emptyObjArgs), writer);
		}
			
		internal void Deserialize(object o, BinaryReader reader)
		{
			object val = MsgPackIO.DeserializeValue(valueType, reader, nilImplication);
			object safeValue = (val == null) ? null : Convert.ChangeType(val, valueType);
			propInfo.SetValue(o, safeValue, emptyObjArgs);
		}
	}
}

