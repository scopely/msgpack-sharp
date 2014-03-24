using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;

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

		internal static void SerializeObject(object o, BinaryWriter writer, bool asDictionary)
		{
			GetSerializer(o.GetType()).Serialize(o, writer, asDictionary);
		}

		public byte[] Serialize(object o, bool asDictionary = false)
		{
			if (o == null)
				throw new ArgumentException("Can't serialize a null reference", "o");
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

		private void Serialize(object o, BinaryWriter writer, bool asDictionary)
		{
			if (asDictionary)
			{
				byte val = (byte)(0x80 | props.Count);
				writer.Write(val);
			}
			foreach (SerializableProperty prop in props)
			{
				prop.Serialize(o, writer, asDictionary);
			}
		}

		private void BuildMap()
		{
			props = new List<SerializableProperty>();
			foreach (PropertyInfo prop in serializedType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (props.Count >= 15)
					throw new IndexOutOfRangeException("Only Types with 15 or fewer properties can be handled by MsgPack. You are trying to serialize a Type with more properties than that. Consider using a simpler DTO to wrap your payload.");
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

