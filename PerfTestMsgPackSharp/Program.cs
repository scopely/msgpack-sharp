using System;
using scopely.msgpacksharp.tests;
using scopely.msgpacksharp;
using System.IO;

namespace PerfTest
{
	class MainClass
	{
		private const int numMessagesToTest = 10000;
		private static AnimalMessage testMsg;

		public static void Main(string[] args)
		{            
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Map;
			testMsg = AnimalMessage.CreateTestMessage();

			// Warm-up for both
			//TestCli();
			//TestMsgPackSharp();

			// Actual tests...
			TimeTest(TestMsgPackSharp, "Serialize and Deserialize with MsgPack-Sharp");
		}

		private static void TimeTest(Action test, string testName)
		{
			DateTime start = DateTime.Now;
			test();
			DateTime end = DateTime.Now;
			TimeSpan diff = end.Subtract(start);
			double numPerSecond = (double)numMessagesToTest / diff.TotalSeconds;
			Console.Out.WriteLine(testName + ":\n\t{0:n0} messages per second", numPerSecond);
		}

		private static void TestMsgPackSharp()
		{
			for (int i = 0; i < numMessagesToTest; i++)
			{
				byte[] buffer = MsgPackSerializer.SerializeObject(testMsg);
#pragma warning disable 0219
				var deserialized = MsgPackSerializer.Deserialize<AnimalMessage>(buffer);
#pragma warning restore 0219
			}
		}
	}
}
