using System;
using System.Reflection;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using MsgPack.Serialization;

namespace scopely.msgpacksharp
{
	public class MsgPackSerializer
	{
		private static Dictionary<Type,MsgPackSerializer> serializers = new Dictionary<Type,MsgPackSerializer>();
		private List<SerializableProperty> props;
		private Type serializedType;
		private byte[] serializationBuffer = new byte[UInt16.MaxValue - 1];
		private static Dictionary<Type,TypeInfo> typeInfos = new Dictionary<Type, TypeInfo>();

		public MsgPackSerializer(Type type)
		{
			serializedType = type;
			BuildMap();
		}

		internal static bool IsGenericList(Type type)
		{
			TypeInfo info = null;
			if (!typeInfos.TryGetValue(type, out info))
			{
				info = new TypeInfo(type);
				typeInfos[type] = info;
			}
			return info.IsGenericList;
		}

		internal static bool IsGenericDictionary(Type type)
		{
			TypeInfo info = null;
			if (!typeInfos.TryGetValue(type, out info))
			{
				info = new TypeInfo(type);
				typeInfos[type] = info;
			}
			return info.IsGenericDictionary;
		}

		internal static bool IsSerializableGenericCollection(Type type)
		{
			TypeInfo info = null;
			if (!typeInfos.TryGetValue(type, out info))
			{
				info = new TypeInfo(type);
				typeInfos[type] = info;
			}
			return info.IsSerializableGenericCollection;
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

		public static int SerializeObject(object o, byte[] buffer, int offset)
		{
			return GetSerializer(o.GetType()).Serialize(o, buffer, offset);
		}

		public byte[] Serialize(object o)
		{
			byte[] result = null;
			using (MemoryStream stream = new MemoryStream(serializationBuffer))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					Serialize(o, writer);
					result = new byte[stream.Position];
				}
				Array.Copy(serializationBuffer, result, result.Length);
			}
			return result;
		}

		public int Serialize(object o, byte[] buffer, int offset)
		{
			int endPos = 0;
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				using (BinaryWriter writer = new BinaryWriter(stream))
				{
					stream.Seek(offset, SeekOrigin.Begin);
					Serialize(o, writer);
					endPos = (int)stream.Position;
				}
			}
			return endPos;
		}

		public static T Deserialize<T>(byte[] buffer) where T : new()
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					object o = DeserializeObject(typeof(T), reader);
					return (T)Convert.ChangeType(o, typeof(T));
				}
			}
		}

		public static object Deserialize(Type t, byte[] buffer)
		{
			return Deserialize(t, buffer, 0);
		}

		public static object Deserialize(Type t, byte[] buffer, int offset)
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				stream.Seek(offset, SeekOrigin.Begin);
				using (BinaryReader reader = new BinaryReader(stream))
				{
					object o = DeserializeObject(t, reader);
					return Convert.ChangeType(o, t);
				}
			}
		}

		public static void DeserializeObject(object o, byte[] buffer, int offset)
		{
			using (MemoryStream stream = new MemoryStream(buffer))
			{
				stream.Seek(offset, SeekOrigin.Begin);
				using (BinaryReader reader = new BinaryReader(stream))
				{
					GetSerializer(o.GetType()).Deserialize(o, reader);
				}
			}
		}

		internal static object DeserializeObject(object o, BinaryReader reader, NilImplication nilImplication = NilImplication.MemberDefault)
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

		internal static object DeserializeObject(Type type, BinaryReader reader, NilImplication nilImplication = NilImplication.MemberDefault)
		{
			if (type.IsPrimitive || 
				type == typeof(string) || 
				IsSerializableGenericCollection(type))
			{
				return MsgPackIO.DeserializeValue(type, reader, nilImplication);
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
					IsSerializableGenericCollection(serializedType))
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
				!IsSerializableGenericCollection(serializedType))
			{
				props = new List<SerializableProperty>();
				foreach (PropertyInfo prop in serializedType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					object[] customAttributes = prop.GetCustomAttributes(typeof(MsgPack.Serialization.MessagePackMemberAttribute), true);
					if (customAttributes != null && customAttributes.Length == 1)
					{
						MessagePackMemberAttribute att = (MsgPack.Serialization.MessagePackMemberAttribute)customAttributes[0];
						props.Add(new SerializableProperty(prop, att.Id, att.NilImplication));
					}
				}
				props.Sort((x, y) => (x.Sequence.CompareTo(y.Sequence)));
			}
		}
	}
}

