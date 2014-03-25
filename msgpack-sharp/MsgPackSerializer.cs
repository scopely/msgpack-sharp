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

		public static T Deserialize<T>(byte[] buffer) where T : new()
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					bool asDictionary = buffer[0] >= 0x80 && buffer[0] <= 0x8F;
					return GetSerializer(typeof(T)).Deserialize<T>(reader, asDictionary);
				}
			}
		}

		public T Deserialize<T>(BinaryReader reader, bool asDictionary) where T : new()
		{
			T result = new T();
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
				if (props.Count == 15)
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

