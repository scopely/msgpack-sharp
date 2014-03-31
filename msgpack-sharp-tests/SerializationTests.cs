using NUnit.Framework;
using System;
using scopely.msgpacksharp.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace scopely.msgpacksharp.tests
{
	[TestFixture]
	public class SerializationTests
	{
		[Test]
		public void TestCompat()
		{
			AnimalMessage msg = AnimalMessage.CreateTestMessage();
			byte[] payload = msg.ToMsgPack();
			string msgFilename = Path.Combine(Environment.CurrentDirectory, "animal.msg");
			string verifierFilename = Path.Combine(Environment.CurrentDirectory, "msgpack-sharp-verifier.exe");
			File.WriteAllBytes(msgFilename, payload);
			Process.Start("mono", verifierFilename + " " + msgFilename);
			Assert.IsTrue(File.Exists(msgFilename + ".out"), "The verifier program that uses other people's msgpack libs failed to successfully handle our message");
			payload = File.ReadAllBytes(msgFilename + ".out");
			AnimalMessage restored = MsgPackSerializer.Deserialize<AnimalMessage>(payload);
			VerifyAnimalMessage(msg, restored);
		}

		[Test]
		public void TestLimits()
		{
			TestLimit(5);
			//TestLimit(20);
		}

		private void TestLimit(int count)
		{
			var msg = new AnimalMessage();
			msg.SpotColors = new List<AnimalColor>();
			for (int i = 0; i < count; i++)
				msg.SpotColors.Add(new AnimalColor() { Red = 1.0f });
			byte[] payload = msg.ToMsgPack();

			var restored = MsgPackSerializer.Deserialize<AnimalMessage>(payload);
			Assert.IsNotNull(restored.SpotColors);
			Assert.AreEqual(msg.SpotColors.Count, restored.SpotColors.Count);

			for (int i = 0; i < count; i++)
			{
				Assert.AreEqual(msg.SpotColors[i], restored.SpotColors[i]);
			}
		}

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
			AnimalMessage msg = AnimalMessage.CreateTestMessage();

			byte[] payload = msg.ToMsgPack();
			Assert.IsNotNull(payload);
			Assert.AreNotEqual(0, payload.Length);
			Console.Out.WriteLine("Payload is " + payload.Length + " bytes!");

			AnimalMessage restored = MsgPackSerializer.Deserialize<AnimalMessage>(payload);

			VerifyAnimalMessage(msg, restored);
		}

		private void VerifyAnimalMessage(AnimalMessage msg, AnimalMessage restored)
		{
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

