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

		public static byte[] SerializeObject(object o)
		{
			return GetSerializer(o.GetType()).Serialize(o);
		}

		public byte[] Serialize(object o)
		{
			byte[] result = null;
			using (MemoryStream stream = new MemoryStream())
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					Serialize(o, writer);
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
					return (T)DeserializeObject(typeof(T), reader);
				}
			}
		}

		internal static object DeserializeObject(object o, BinaryReader reader)
		{
			if (o is IList)
			{
				if (MsgPackIO.DeserializeCollection((IList)o, reader))
					return null;
				else
					return o;
			}
			else if (o is IDictionary)
			{
				if (MsgPackIO.DeserializeCollection((IDictionary)o, reader))
					return null;
				else
					return o;
			}
			else
				return GetSerializer(o.GetType()).Deserialize(o, reader);
		}

		internal static object DeserializeObject(Type type, BinaryReader reader)
		{
			if (type.IsPrimitive || 
				type == typeof(string) || 
				type.GetInterface("System.Collections.Generic.IList`1") != null ||
				type.GetInterface("System.Collections.Generic.IDictionary`2") != null)
			{
				return MsgPackIO.DeserializeValue(type, reader);
			}
			else
			{
				ConstructorInfo constructorInfo = type.GetConstructor(Type.EmptyTypes);
				if (constructorInfo == null)
					throw new ApplicationException("Can't deserialize Type [" + type + "] because it has no default constructor");
				object result = constructorInfo.Invoke(SerializableProperty.emptyObjArgs);
				return GetSerializer(type).Deserialize(result, reader);
			}
		}

		internal object Deserialize(object result, BinaryReader reader)
		{
			byte header = reader.ReadByte();
			if (header == MsgPackConstants.Formats.NIL)
				result = null;
			else
			{
				bool isArray = false;
				if (header >= MsgPackConstants.FixedArray.MIN && header <= MsgPackConstants.FixedArray.MAX)
					isArray = true;
				else if (header == MsgPackConstants.Formats.ARRAY_16)
				{
					isArray = true;
					reader.ReadByte();
					reader.ReadByte();
				}
				else if (header == MsgPackConstants.Formats.ARRAY_32)
				{
					isArray = true;
					reader.ReadByte();
					reader.ReadByte();
					reader.ReadByte();
					reader.ReadByte();
				}
				if (!isArray)
					throw new ApplicationException("All objects are expected to begin as arrays for their properties - the serialized data format isn't valid");
				foreach (SerializableProperty prop in props)
				{
					prop.Deserialize(result, reader);
				}
			}
			return result;
		}

		internal static void SerializeObject(object o, BinaryWriter writer)
		{
			GetSerializer(o.GetType()).Serialize(o, writer);
		}

		private void Serialize(object o, BinaryWriter writer)
		{
			if (o == null)
				writer.Write((byte)MsgPackConstants.Formats.NIL);
			else
			{
				if (serializedType.IsPrimitive || 
					serializedType == typeof(string) ||
					serializedType.GetInterface("System.Collections.Generic.IDictionary`2") != null ||
					serializedType.GetInterface("System.Collections.Generic.IList`1") != null)
				{
					MsgPackIO.SerializeValue(o, writer);
				}
				else
				{
					if (props.Count <= 15)
					{
						byte arrayVal = (byte)(MsgPackConstants.FixedArray.MIN + props.Count);
						writer.Write(arrayVal);
					}
					else if (props.Count <= UInt16.MaxValue)
					{
						writer.Write((byte)MsgPackConstants.Formats.ARRAY_16);
						byte[] data = BitConverter.GetBytes((ushort)props.Count);
						if (BitConverter.IsLittleEndian)
							Array.Reverse(data);
						writer.Write(data);
					}
					else
					{
						writer.Write((byte)MsgPackConstants.Formats.ARRAY_32);
						byte[] data = BitConverter.GetBytes((uint)props.Count);
						if (BitConverter.IsLittleEndian)
							Array.Reverse(data);
						writer.Write(data);
					}
					foreach (SerializableProperty prop in props)
					{
						prop.Serialize(o, writer);
					}
				}
			}
		}

		private void BuildMap()
        {
			if (!serializedType.IsPrimitive && 
				serializedType != typeof(string) &&
				serializedType.GetInterface("System.Collections.Generic.IList`1") == null &&
				serializedType.GetInterface("System.Collections.Generic.IDictionary`2") == null)
			{
				props = new List<SerializableProperty>();
				foreach (PropertyInfo prop in serializedType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					foreach (object att in prop.GetCustomAttributes(true))
					{
						MsgPackAttribute msgPackAttribute = att as MsgPackAttribute;
						if (msgPackAttribute != null)
						{
							props.Add(new SerializableProperty(prop, msgPackAttribute.Sequence));
						}
					}
				}
				props.Sort((x, y) => (x.Sequence.CompareTo(y.Sequence)));
			}
		}
	}
}

