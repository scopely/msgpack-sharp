using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Collections;

namespace scopely.msgpacksharp
{
	public class MsgPackSerializer
	{
		private static Dictionary<Type,MsgPackSerializer> serializers = new Dictionary<Type,MsgPackSerializer>();
		private List<SerializableProperty> props;
		private Type serializedType;

		public MsgPackSerializer(Type type)
		{
			serializedType = type;
			BuildMap();
		}

		private static MsgPackSerializer GetSerializer(Type t)
		{
			MsgPackSerializer result = null;
			if (!serializers.TryGetValue(t, out result))
			{
				result = serializers[t] = new MsgPackSerializer(t);
			}
			return result;
		}

		public static byte[] SerializeObject(object o, bool asDictionary = false)
		{
			return GetSerializer(o.GetType()).Serialize(o, asDictionary);
		}

		public byte[] Serialize(object o, bool asDictionary = false)
		{
			byte[] result = null;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					Serialize(o, writer, asDictionary);
					result = stream.ToArray();
				}
			}
			return result;
		}

		public static T Deserialize<T>(byte[] buffer) where T : new()
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
                    bool asDictionary = buffer[0] >= MsgPackConstants.FixedMap.MIN && buffer[0] <= MsgPackConstants.FixedMap.MAX;
					return (T)DeserializeObject(typeof(T), reader, asDictionary);
				}
			}
		}

		internal static object DeserializeObject(object o, BinaryReader reader, bool asDictionary)
		{
			if (o is IList)
			{
				SerializableProperty.DeserializeCollection((IList)o, reader, asDictionary);
				return o;
			}
			else if (o is IDictionary)
			{
				SerializableProperty.DeserializeCollection((IDictionary)o, reader, asDictionary);
				return o;
			}
			else
				return GetSerializer(o.GetType()).Deserialize(o, reader, asDictionary);
		}

		internal static object DeserializeObject(Type type, BinaryReader reader, bool asDictionary)
		{
			ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);
			if (constructorInfo == null)
				throw new InvalidDataException("Can't deserialize Type [" + type + "] because it has no default constructor");
			object result = constructorInfo.Invoke(SerializableProperty.emptyObjArgs);
			return GetSerializer(type).Deserialize(result, reader, asDictionary);
		}

		internal object Deserialize(object result, BinaryReader reader, bool asDictionary)
		{
			foreach (SerializableProperty prop in props)
			{
				prop.Deserialize(result, reader, asDictionary);
			}
			return result;
		}

		internal static void SerializeObject(object o, BinaryWriter writer, bool asDictionary)
		{
			GetSerializer(o.GetType()).Serialize(o, writer, asDictionary);
		}

		private void Serialize(object o, BinaryWriter writer, bool asDictionary)
		{
			if (o == null)
				writer.Write((byte)MsgPackConstants.Formats.NIL);
			else
			{
				if (asDictionary)
				{
					byte val = (byte)(MsgPackConstants.FixedMap.MIN | props.Count);
					writer.Write(val);
				}
				foreach (SerializableProperty prop in props)
				{
					prop.Serialize(o, writer, asDictionary);
				}
			}
		}

		private void BuildMap()
        {
            props = new List<SerializableProperty>();
            foreach (PropertyInfo prop in serializedType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (props.Count >= MsgPackConstants.MAX_PROPERTY_COUNT)
                {
                    string exceptionStr = string.Format("Only Types with {0} or fewer properties can be handled by MsgPack. You are trying to serialize a Type with more properties than that. Consider using a simpler DTO to wrap your payload.",
                        MsgPackConstants.MAX_PROPERTY_COUNT);
                    throw new IndexOutOfRangeException(exceptionStr);
                }
				foreach (object att in prop.GetCustomAttributes(true))
				{
					MsgPackAttribute msgPackAttribute = att as MsgPackAttribute;
					if (msgPackAttribute != null)
					{
						props.Add(new SerializableProperty(prop, msgPackAttribute.Sequence));
					}
				}
			}
			props.Sort((x,y) => (x.Sequence.CompareTo(y.Sequence)));
		}
	}
}

