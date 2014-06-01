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
        public void TestDictionary()
        {
            TestGenericDictionary<string, string>("TESTKEY", "TESTVAL");
            //TestGenericDictionary<int, int>(1, 1);



        }

        private void TestGenericDictionary<Key, Value> (Key testKey, Value testValue)
        {
            Dictionary<Key, Value> intDict = new Dictionary<Key, Value>();
            intDict.Add(testKey, testValue);

            var msg = MsgPackSerializer.SerializeObject(intDict);
            var desizDict = MsgPackSerializer.Deserialize<Dictionary<Key, Value>>(msg);

            string logHeader = string.Format("<{0}, {1}>: ", typeof(Key).ToString(), typeof(Value).ToString());

            Assert.That(desizDict != null, logHeader + "null desiz");
            Assert.That(typeof(Dictionary<Key, Value>) == desizDict.GetType(), logHeader + "different types");
            Assert.That(desizDict[testKey].Equals(testValue),logHeader + "key value lost");
        }

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
		public void TestNulls()
		{
			var msg = AnimalMessage.CreateTestMessage();
			msg.AnimalColor = null;
			byte[] payload = msg.ToMsgPack();
			Assert.IsNotNull(payload);
			Assert.AreNotEqual(0, payload.Length);
			var restored = MsgPackSerializer.Deserialize<AnimalMessage>(payload);
			Assert.IsNull(restored.AnimalColor);
		}

		[Test]
		public void TestRoundTripPrimitives()
		{
			TestRoundTrip(0);
			TestRoundTrip(127);

			var stuff = new Dictionary<string, string>();
			stuff["Foo"] = "Value1";
			stuff["Bar"] = "Value2";
			byte[] payload = stuff.ToMsgPack();
			Assert.IsNotNull(payload);
			Assert.AreNotEqual(0, payload.Length);

			var restoredStuff = MsgPackSerializer.Deserialize<Dictionary<string,string>>(payload);
			Assert.AreEqual(stuff.Count, restoredStuff.Count);
		}

		private void TestRoundTrip(int intVal)
		{
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
			Assert.AreEqual(msg.IsAlive, restored.IsAlive);
			Assert.AreEqual(msg.HeightInches, restored.HeightInches);
			Assert.AreEqual(msg.AnimalKind, restored.AnimalKind);
			Assert.AreEqual(msg.AnimalName, restored.AnimalName);
			Assert.IsNotNull(msg.AnimalColor);
			Assert.AreEqual(msg.AnimalColor.Red, restored.AnimalColor.Red);
			Assert.AreEqual(msg.AnimalColor.Green, restored.AnimalColor.Green);
			Assert.AreEqual(msg.AnimalColor.Blue, restored.AnimalColor.Blue);
			Assert.AreEqual(msg.BirthDay, restored.BirthDay);
			Assert.AreEqual(msg.SpotColors.Count, restored.SpotColors.Count);
			for (int i = 0; i < msg.SpotColors.Count; i++)
			{
				Assert.AreEqual(msg.SpotColors[i], restored.SpotColors[i]);
			}
			Assert.IsNull(restored.MoreColors);
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

			Assert.AreEqual(msg.CurrentHabitat, restored.CurrentHabitat);
			Assert.AreEqual(msg.TheLongString, restored.TheLongString);

			Assert.IsFalse(restored.NullableIntOne.HasValue);
			Assert.IsTrue(restored.NullableIntTwo.HasValue);
			Assert.IsTrue(restored.NullableIntThree.HasValue && msg.NullableIntThree.Value == 1);
		}
	}
}

