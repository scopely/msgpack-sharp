using System;
using System.IO;
using System.Text;
using scopely.msgpacksharp.enums;

namespace scopely.msgpacksharp
{
	public static class BinaryWriterExtensions
	{
		private const int MaxFixedStringLength = 31;

		public static void WriteMsgPack(this BinaryWriter writer, float val)
		{
            writer.Write((byte)MsgPackConstants.Formats.Float32);
			writer.Write(val);
		}

		public static void WriteMsgPack(this BinaryWriter writer, double val)
		{
            writer.Write((byte)MsgPackConstants.Formats.Float64);
			writer.Write(val);
		}

		public static void WriteMsgPack(this BinaryWriter writer, string s)
		{
            if (string.IsNullOrEmpty(s))
				writer.Write((byte)0xa0);
			else
			{
				byte[] utf8Bytes = UTF8Encoding.UTF8.GetBytes(s);
				uint length = (uint)utf8Bytes.Length;
				if (length <= MaxFixedStringLength)
				{
					byte val = (byte)(0xa0 | length);
					writer.Write(val);
				}
				else if (length <= 255)
				{
                    writer.Write((byte)MsgPackConstants.Formats.Str8);
					writer.Write((byte)length);
				}
				else if (length <= 65535)
				{
                    writer.Write((byte)MsgPackConstants.Formats.Str16);
					writer.Write((byte)((length | 0xFF00) >> 8));
					writer.Write((byte)(length | 0x00FF));
				}
				else
				{
                    writer.Write((byte)MsgPackConstants.Formats.Str32);
					writer.Write((byte)((length | 0xFF000000) >> 24));
					writer.Write((byte)((length | 0x00FF0000) >> 16));
					writer.Write((byte)((length | 0x0000FF00) >> 8));
					writer.Write((byte)( length | 0x000000FF));
				}
				for (int i = 0; i < utf8Bytes.Length; i++)
				{
					writer.Write(utf8Bytes[i]);
				}
			}
		}
	}
}
