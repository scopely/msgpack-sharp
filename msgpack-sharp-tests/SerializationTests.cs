using NUnit.Framework;
using System;
using scopely.msgpacksharp.Extensions;
using System.Collections.Generic;

namespace scopely.msgpacksharp.tests
{
	[TestFixture]
	public class SerializationTests
	{
		[Test]
		public void TestRoundTripPrimitives()
		{
			int intVal = 0;
			byte[] payload = intVal.ToMsgPack();
			Assert.IsNotNull(payload);
			Assert.AreNotEqual(0, payload.Length);

			int restoredInt = MsgPackSerializer.Deserialize<int>(payload);
			Assert.AreEqual(intVal, restoredInt);
		}

		[Test]
		public void TestRoundTripComplexTypes()
		{
			AnimalMessage msg = new AnimalMessage();
			msg.HeightInches = 7;
			msg.AnimalKind = "Cat";
			msg.AnimalName = "Lunchbox";
			msg.AnimalColor = new AnimalColor() { Red = 1.0f, Green = 0.1f, Blue = 0.1f };
			msg.BirthDay = new DateTime(1974, 1, 4);
			msg.SomeNumbers = new int[5];
			for (int i = 0; i < msg.SomeNumbers.Length; i++)
				msg.SomeNumbers[i] = i * 2;
			msg.SpotColors = new List<AnimalColor>();
			for (int i = 0; i < 3; i++)
			{
				msg.SpotColors.Add(new AnimalColor() { Red = 1.0f, Green = 1.0f, Blue = 0.0f });
			}
			msg.Metadata = new Dictionary<string, string>();
			msg.Metadata["Key1"] = "Value1";
			msg.Metadata["Key2"] = "Value2";
			msg.ListOfInts = new List<int>();
			for (int i = 0; i < 5; i++)
			{
				msg.ListOfInts.Add(i * 2);
			}

			byte[] payload = msg.ToMsgPack();
			Assert.IsNotNull(payload);
			Assert.AreNotEqual(0, payload.Length);
			Console.Out.WriteLine("Payload is " + payload.Length + " bytes!");

			AnimalMessage restored = MsgPackSerializer.Deserialize<AnimalMessage>(payload);
			Assert.IsNotNull(restored);
			Assert.AreEqual(msg.HeightInches, restored.HeightInches);
			Assert.AreEqual(msg.AnimalKind, restored.AnimalKind);
			Assert.AreEqual(msg.AnimalName, restored.AnimalName);
			Assert.IsNotNull(msg.AnimalColor);
			Assert.AreEqual(msg.AnimalColor.Red, restored.AnimalColor.Red);
			Assert.AreEqual(msg.AnimalColor.Green, restored.AnimalColor.Green);
			Assert.AreEqual(msg.AnimalColor.Blue, restored.AnimalColor.Blue);
			Assert.AreEqual(msg.BirthDay, restored.BirthDay);
			Assert.AreEqual(msg.SomeNumbers.Length, restored.SomeNumbers.Length);
			for (int i = 0; i < msg.SomeNumbers.Length; i++)
			{
				Assert.AreEqual(msg.SomeNumbers[i], restored.SomeNumbers[i]);
			}
			Assert.AreEqual(msg.SpotColors.Count, restored.SpotColors.Count);
			for (int i = 0; i < msg.SpotColors.Count; i++)
			{
				Assert.AreEqual(msg.SpotColors[i], restored.SpotColors[i]);
			}
			Assert.IsEmpty(restored.MoreColors);
			Assert.IsNotNull(restored.Metadata);
			foreach (KeyValuePair<string,string> pair in msg.Metadata)
			{
				Assert.IsTrue(restored.Metadata.ContainsKey(pair.Key));
				Assert.AreEqual(pair.Value, restored.Metadata[pair.Key]);
			}
			Assert.AreEqual(msg.ListOfInts.Count, restored.ListOfInts.Count);
			for (int i = 0; i < msg.ListOfInts.Count; i++)
			{
				Assert.AreEqual(msg.ListOfInts[i], restored.ListOfInts[i]);
			}
		}
	}
}

