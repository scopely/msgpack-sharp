/// <summary>
/// Use the .net 4.0 "official" msgpack-cli to verify our serialization
/// </summary>

using System;
using System.IO;
using scopely.msgpacksharp.tests;
using MsgPack.Serialization;

namespace scopely.msgpacksharp.verifier
{
	class MainClass
	{
		public static int Main(string[] args)
		{
			int result = 1;
			if (args[0] == "capturecli")
			{
				AnimalMessage msg = AnimalMessage.CreateTestMessage();
				var serializer = MessagePackSerializer.Create<AnimalMessage>();
				using (MemoryStream outStream = new MemoryStream())
				{	
					serializer.Pack(outStream, msg);
					byte[] output = outStream.ToArray();
					File.WriteAllBytes("cli.msg", output);
				}
			}
			else
			{
				try
				{
					byte[] payload = File.ReadAllBytes(args[0]);
					Console.Out.WriteLine("read " + payload.Length + " bytes");
					var serializer = MessagePackSerializer.Create<AnimalMessage>();
					using (MemoryStream stream = new MemoryStream(payload))
					{
						var restored = serializer.Unpack(stream);
						using (MemoryStream outStream = new MemoryStream())
						{	
							serializer.Pack(outStream, restored);
							byte[] output = outStream.ToArray();
							File.WriteAllBytes(args[0] + ".out", output);
						}			
						result = 0;
					}
				}
				catch (Exception ex)
				{
					Console.Out.WriteLine("Error: " + ex);
				}
			}
			return result;
		}
	}
}
