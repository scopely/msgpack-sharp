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

        internal SerializableProperty(PropertyInfo propInfo, int sequence = 0, NilImplication? nilImplication = null)
		{
			PropInfo = propInfo;
			Name = propInfo.Name;
            _nilImplication = nilImplication ?? NilImplication.MemberDefault;
            Sequence = sequence;
			ValueType = propInfo.PropertyType;
            Type underlyingType = Nullable.GetUnderlyingType(propInfo.PropertyType);
            if (underlyingType != null)
            {
                ValueType = underlyingType;
                if (nilImplication.HasValue == false)
                {
                    _nilImplication = NilImplication.Null;
                }
            }
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
            // TODO REMOVE THIS
            // BREAKPOINT IN HERE FOR CULPRIT OF LONG DESERIALIZE PROBLEM
//		    if (o is WheelOfFortune.Common.Models.Achievements.Achievement)
//		    {
//		        if ((o as WheelOfFortune.Common.Models.Achievements.Achievement).MetricQuantity > 0)
//		        {
//		            WoFDebug.Log("");
//		        }
//		    }

			object val = MsgPackIO.DeserializeValue(ValueType, reader, _nilImplication);
			object safeValue = (val == null) ? null : Convert.ChangeType(val, ValueType);
			PropInfo.SetValue(o, safeValue, EmptyObjArgs);
		}
	}
}

