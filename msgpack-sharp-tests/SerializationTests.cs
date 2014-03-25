using NUnit.Framework;
using System;
using scopely.msgpacksharp.Extensions;

namespace scopely.msgpacksharp.tests
{
	[TestFixture]
	public class SerializationTests
	{
		[Test]
		public void TestRoundTrip()
		{
			AnimalMessage msg = new AnimalMessage();
			msg.AnimalKind = "Cat";
			msg.AnimalName = "Lunchbox";
			msg.AnimalColor = new AnimalColor() { Red = 1.0f, Green = 0.1f, Blue = 0.1f };

			byte[] payload = msg.ToMsgPack();
			Assert.IsNotNull(payload);
			Assert.AreNotEqual(0, payload.Length);
			Console.Out.WriteLine("Payload is " + payload.Length + " bytes!");

			AnimalMessage restored = MsgPackSerializer.Deserialize<AnimalMessage>(payload);
			Assert.IsNotNull(restored);
			Assert.AreEqual(msg.AnimalKind, restored.AnimalKind);
			Assert.AreEqual(msg.AnimalName, restored.AnimalName);
		}
	}
}

