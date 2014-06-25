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
        public enum TestEnum
        {
            ENTRY_0 = 0,
            ENTRY_1,
        }

        [Test]
        public void TestNestedObject ()
        {
            var obj = new SerializationTestObject();
            byte[] msg = MsgPackSerializer.SerializeObject(obj);
            var desiz = MsgPackSerializer.Deserialize<SerializationTestObject>(msg);

            Assert.That(desiz != null, "No Nesting: null desiz");
            Assert.That(desiz.Equals(obj), "No Nesting: not equal");

            obj.AddChild();
            msg = MsgPackSerializer.SerializeObject(obj);
            desiz = MsgPackSerializer.Deserialize<SerializationTestObject>(msg);

            Assert.That(desiz != null, "Nesting: null desiz");
            Assert.That(desiz.Equals(obj), "Nesting: not equal");
        }

        [Test]
        public void TestDictionary()
        {
            TestGenericDictionary<bool, bool>(true, false);
            // BREAKS TestGenericDictionary<int, object>(int.MinValue, new object());
            TestGenericDictionary<string, string>("TESTKEY", "TESTVAL");
            TestGenericDictionary<string, string>("TESTNULLKEY", null);
            TestGenericDictionary<int, int>(int.MinValue, int.MaxValue);
            // BREAKS TestGenericDictionary<int, int?>(int.MinValue, null);
            // BREAKS TestGenericDictionary<uint, uint>(uint.MinValue, uint.MaxValue);
            TestGenericDictionary<short, short>(short.MinValue, short.MaxValue);
            TestGenericDictionary<ushort, ushort>(ushort.MinValue, ushort.MaxValue);
            TestGenericDictionary<long, long>(long.MinValue, long.MaxValue);
            // BREAKS TestGenericDictionary<ulong, ulong>(ulong.MinValue, ulong.MaxValue);
			TestGenericDictionary<float, float>(float.MinValue, float.MaxValue);
            TestGenericDictionary<double, double>(double.MinValue, double.MaxValue);
            // BREAKS TestGenericDictionary<decimal, decimal>(decimal.MinValue, decimal.MaxValue);
            TestGenericDictionary<byte, byte>(byte.MinValue, byte.MaxValue);
            // BREAKS TestGenericDictionary<sbyte, sbyte>(sbyte.MinValue, sbyte.MaxValue);
            TestGenericDictionary<char, char>(char.MinValue, char.MaxValue);
            TestGenericDictionary<TestEnum, TestEnum>(TestEnum.ENTRY_0, TestEnum.ENTRY_1);
            TestGenericDictionary<int, SerializationTestObject>(100, new SerializationTestObject());
            // BREAKS TestGenericDictionary<int, SerializationTestObject>(100, new SerializationTestObject().AddChild());
        }

        private void TestGenericDictionary<Key, Value> (Key testKey, Value testValue)
        {
            Dictionary<Key, Value> intDict = new Dictionary<Key, Value>();
            intDict.Add(testKey, testValue);

            var msg = MsgPackSerializer.SerializeObject(intDict);
            var desizDict = MsgPackSerializer.Deserialize<Dictionary<Key, Value>>(msg);

            string logHeader = string.Format("Dictionary<{0}, {1}>: ", typeof(Key).ToString(), typeof(Value).ToString());

            Assert.That(desizDict != null, logHeader + "null desiz");
            Assert.That(typeof(Dictionary<Key, Value>) == desizDict.GetType(), logHeader + "different types");

            if (testValue == null)
            {
                Assert.That(desizDict[testKey] == null);
            }
            else
            {
                Assert.That(desizDict[testKey].Equals(testValue), logHeader + "key value lost");
            }
        }

        [Test]
        public void TestList()
        {
            TestGenericList<bool>(true, false);
            // BREAKS TestGenericList<object>(new object(), null);
            TestGenericList<string>("TESTKEY", "TESTVAL");
            TestGenericList<string>(null, null);
            TestGenericList<int>(int.MinValue, int.MaxValue);
            // BREAKS TestGenericList<int?>(int.MinValue, null);
            // BREAKS TestGenericList<uint>(uint.MinValue, uint.MaxValue);
            TestGenericList<short>(short.MinValue, short.MaxValue);
            TestGenericList<ushort>(ushort.MinValue, ushort.MaxValue);
            TestGenericList<long>(long.MinValue, long.MaxValue);
            // BREAKS TestGenericList<ulong>(ulong.MinValue, ulong.MaxValue);
            TestGenericList<float>(float.MinValue, float.MaxValue);
            TestGenericList<double>(double.MinValue, double.MaxValue);
            // BREAKS TestGenericList<decimal>(decimal.MinValue, decimal.MaxValue);
            TestGenericList<byte>(byte.MinValue, byte.MaxValue);
            // BREAKS TestGenericList<sbyte>(sbyte.MinValue, sbyte.MaxValue);
            TestGenericList<char>(char.MinValue, char.MaxValue);
            TestGenericList<TestEnum>(TestEnum.ENTRY_0, TestEnum.ENTRY_1);
            TestGenericList<SerializationTestObject>(new SerializationTestObject(), new SerializationTestObject());
            // BREAKS TestGenericList<SerializationTestObject>(new SerializationTestObject().AddChild(), new SerializationTestObject().AddChild().AddChild());
        }

        private void TestGenericList<T> (T entry1, T entry2)
        {
            List<T> objList = new List<T>();
            objList.Add(entry1);
            objList.Add(entry2);

            string logHeader = string.Format("List<{0}>: ", typeof(T).ToString());

            var msg = MsgPackSerializer.SerializeObject(objList);
            var desizList = MsgPackSerializer.Deserialize<List<T>>(msg);

            Assert.That(desizList != null, logHeader + "null desiz");
            Assert.That(typeof(List<T>) == desizList.GetType(), logHeader + "different types");

            if (entry1 == null)
                Assert.That(objList[0] == null);
            else 
                Assert.That(desizList[0].Equals(entry1),logHeader + "value lost 1: " + entry1.ToString());

            if (entry2 == null)
                Assert.That(objList[1] == null);
            else 
                Assert.That(desizList[1].Equals(entry2),logHeader + "value lost 2: " + entry2.ToString());
        }

		[Test]
		public void TestDeserializationOfMixedTypeDictionary()
		{
			var dictionaryFromJava = "hKVGbG9hdMo/gAAApE51bGygplN0cmluZ6ZzdHJpbmenQm9vbGVhbsI=";

			var obj = new Dictionary<string, object> {
				{ "Boolean", false }, 
				{ "String", "string" },
				{ "Float", 1.0f },
				{ "Null", null }
			};

			var msg = MsgPackSerializer.SerializeObject (obj);
			var base64Msg = Convert.ToBase64String (msg);
			Console.WriteLine (base64Msg);

			var deserializedDictionary = MsgPackSerializer.Deserialize<Dictionary<string,object>>
				(Convert.FromBase64String (dictionaryFromJava));

			object value = null;
			deserializedDictionary.TryGetValue ("Boolean", out value);
			Assert.That (value.Equals(false));
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

