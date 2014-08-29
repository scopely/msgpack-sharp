using System;
using System.Reflection;
using System.IO;
using MsgPack.Serialization;

namespace scopely.msgpacksharp
{
	internal class SerializableProperty
	{
		internal static readonly object[] EmptyObjArgs = {};
	    private readonly NilImplication _nilImplication;

        internal SerializableProperty(PropertyInfo propInfo, int sequence = 0, NilImplication nilImplication = NilImplication.MemberDefault)
		{
			PropInfo = propInfo;
			Name = propInfo.Name;
			//this.valueType = propInfo.PropertyType;
			ValueType = Nullable.GetUnderlyingType(propInfo.PropertyType) ?? propInfo.PropertyType;
			_nilImplication = nilImplication;
			Sequence = sequence;
		}

	    internal PropertyInfo PropInfo { get; private set; }

	    internal string Name { get; private set; }

	    internal Type ValueType { get; private set; }

	    internal int Sequence { get; set; }

        internal void Serialize(object o, BinaryWriter writer, SerializationMethod serializationMethod)
		{
			// We don't use the simpler propInfo.GetValue because the getter might have been left behind
			// by AOT
            MsgPackIO.SerializeValue(PropInfo.GetGetMethod().Invoke(o, EmptyObjArgs), writer, serializationMethod);
		}
			
		internal void Deserialize(object o, BinaryReader reader)
		{
			object val = MsgPackIO.DeserializeValue(ValueType, reader, _nilImplication);
			object safeValue = (val == null) ? null : Convert.ChangeType(val, ValueType);
			PropInfo.SetValue(o, safeValue, EmptyObjArgs);
		}
	}
}

