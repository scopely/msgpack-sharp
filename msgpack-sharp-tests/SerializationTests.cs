using NUnit.Framework;
using System;
using scopely.msgpacksharp.Extensions;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace scopely.msgpacksharp.tests
{
	[TestFixture]
	public class SerializationTests
	{
        private int numVerified;

        public enum TestEnum
        {
            ENTRY_0 = 0,
            ENTRY_1,
        }

        [Test]
        public void RoundTripNestedDictionary()
        {
            var parent = new Dictionary<string,object>();
            var child = new Dictionary<string,object>();
            child["Foo"] = "Bar";
            parent["Child"] = child;
            byte[] buffer = parent.ToMsgPack();
            var restored = MsgPackSerializer.Deserialize<Dictionary<string,object>>(buffer);
            Assert.IsNotNull(restored);
            Assert.AreEqual(1, restored.Count);
            Assert.IsTrue(restored.ContainsKey("Child"));
            Assert.IsTrue(restored["Child"] is Dictionary<string,object>);
            Assert.AreEqual("Bar", ((Dictionary<string,object>)restored["Child"])["Foo"]);
        }

		[Test]
		public void TestAsMaps()
		{
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Map;
			AnimalMessage msg = AnimalMessage.CreateTestMessage();
			byte[] payload = msg.ToMsgPack();
			Assert.IsNotNull(payload);
			Assert.AreNotEqual(0, payload.Length);
			AnimalMessage restored = MsgPackSerializer.Deserialize<AnimalMessage>(payload);
            VerifyAnimalMessage(msg, restored);
		}

        [Test]
        public void TestNestedObject()
        {
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Array;
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
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Array;

            TestGenericDictionary<int, int?>(1, 1);
            TestGenericDictionary<int, SerializationTestObject>(100, new SerializationTestObject().AddChild());
            TestGenericDictionary<sbyte, sbyte>(sbyte.MinValue, sbyte.MaxValue);
            TestGenericDictionary<bool, bool>(true, false);
            TestGenericDictionary<string, string>("TESTKEY", "TESTVAL");
            TestGenericDictionary<string, string>("TESTNULLKEY", null);
            TestGenericDictionary<int, int>(int.MinValue, int.MaxValue);
            TestGenericDictionary<int, int?>(int.MinValue, null);
            TestGenericDictionary<uint, uint>(uint.MinValue, uint.MaxValue);
            TestGenericDictionary<short, short>(short.MinValue, short.MaxValue);
            TestGenericDictionary<ushort, ushort>(ushort.MinValue, ushort.MaxValue);
            TestGenericDictionary<long, long>(long.MinValue, long.MaxValue);			
            TestGenericDictionary<float, float>(float.MinValue, float.MaxValue);
            TestGenericDictionary<double, double>(double.MinValue, double.MaxValue);
            TestGenericDictionary<byte, byte>(byte.MinValue, byte.MaxValue);
            TestGenericDictionary<char, char>(char.MinValue, char.MaxValue);
            TestGenericDictionary<TestEnum, TestEnum>(TestEnum.ENTRY_0, TestEnum.ENTRY_1);
            TestGenericDictionary<int, SerializationTestObject>(100, new SerializationTestObject());
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
                Assert.That(desizDict.ContainsKey(testKey) && desizDict[testKey].Equals(testValue), logHeader + "key value lost");
            }
        }

        [Test]
        public void TestList()
        {
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Array;
            TestGenericList<int?>(int.MinValue, null);
            TestGenericList<bool>(true, false);
            TestGenericList<string>("TESTKEY", "TESTVAL");
            TestGenericList<string>(null, null);
            TestGenericList<int>(int.MinValue, int.MaxValue);
            TestGenericList<uint>(uint.MinValue, uint.MaxValue);
            TestGenericList<short>(short.MinValue, short.MaxValue);
            TestGenericList<ushort>(ushort.MinValue, ushort.MaxValue);
            TestGenericList<long>(long.MinValue, long.MaxValue);
            TestGenericList<float>(float.MinValue, float.MaxValue);
            TestGenericList<double>(double.MinValue, double.MaxValue);
            TestGenericList<byte>(byte.MinValue, byte.MaxValue);
            TestGenericList<sbyte>(sbyte.MinValue, sbyte.MaxValue);
            TestGenericList<char>(char.MinValue, char.MaxValue);
            TestGenericList<TestEnum>(TestEnum.ENTRY_0, TestEnum.ENTRY_1);
            TestGenericList<SerializationTestObject>(new SerializationTestObject(), new SerializationTestObject());
            TestGenericList<SerializationTestObject>(new SerializationTestObject().AddChild(), new SerializationTestObject().AddChild().AddChild());
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
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Array;
			var obj = new Dictionary<string, object> {
				{ "Boolean", false }, 
				{ "String", "string" },
				{ "Float", 1.0f },
				{ "Null", null }
			};

			var msg = MsgPackSerializer.SerializeObject (obj);

			var deserializedDictionary = MsgPackSerializer.Deserialize<Dictionary<string,object>>(msg);

			object value = null;
			deserializedDictionary.TryGetValue ("Boolean", out value);
			Assert.That (value.Equals(false));
		}

		[Test]
		public void TestMixedTypeDictionaryRoundTrip()
        {
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Array;
			var obj = new Dictionary<string, object> {
				{ "Boolean", false }, 
				{ "String", "string" },
				{ "Float", 1.0f },
				{ "Null", null }
			};

			var msg = MsgPackSerializer.SerializeObject (obj);

			var deserializedDictionary = MsgPackSerializer.Deserialize<Dictionary<string, object>> (msg);
			Assert.That (deserializedDictionary.ContainsValue (false));
		}

		[Test]
        [Ignore]
		public void TestCompat()
		{
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Array;
			AnimalMessage msg = AnimalMessage.CreateTestMessage();
			byte[] payload = msg.ToMsgPack();
			string msgFilename = Path.Combine(Environment.CurrentDirectory, "animal.msg");
			string verifierFilename = Path.Combine(Environment.CurrentDirectory, "msgpack-sharp-verifier.exe");
			File.WriteAllBytes(msgFilename, payload);
            string args = verifierFilename + " " + msgFilename;
			Process.Start("mono", args);
            Assert.IsTrue(File.Exists(msgFilename + ".out"), "The verifier program that uses other people's msgpack libs failed to successfully handle our message while running [mono " + args + "]");
			payload = File.ReadAllBytes(msgFilename + ".out");
			AnimalMessage restored = MsgPackSerializer.Deserialize<AnimalMessage>(payload);
			VerifyAnimalMessage(msg, restored);
		}

		[Test]
		public void TestLimits()
		{
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Array;
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
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Array;
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
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Array;
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
            MsgPackSerializer.DefaultContext.SerializationMethod = SerializationMethod.Array;
			AnimalMessage msg = AnimalMessage.CreateTestMessage();

			byte[] payload = msg.ToMsgPack();
			Assert.IsNotNull(payload);
			Assert.AreNotEqual(0, payload.Length);

			AnimalMessage restored = MsgPackSerializer.Deserialize<AnimalMessage>(payload);

			VerifyAnimalMessage(msg, restored);
		}

        [Test]
        public void TestManualConfig()
        {
            var tank = new Tank()
            {
                Name = "M1",
                MaxSpeed = 65.0f,
                Cargo = AnimalMessage.CreateTestMessage()
            };
            MsgPackSerializer.DefaultContext.RegisterSerializer<Tank>("MaxSpeed", "Name", "Cargo");
            byte[] payload = tank.ToMsgPack();
            Assert.IsNotNull(payload);
            Assert.AreNotEqual(0, payload.Length);
            var restoredTank = MsgPackSerializer.Deserialize<Tank>(payload);
            Assert.IsNotNull(restoredTank);
            Assert.AreEqual(tank.Name, restoredTank.Name);
            Assert.AreEqual(tank.MaxSpeed, restoredTank.MaxSpeed);
            VerifyAnimalMessage(tank.Cargo, restoredTank.Cargo);
        }

        [Test]
        public void TestEventDtos()
        {
            var dto = new GameEventsGetResponse();
            dto.Marquees = new List<GameMarquee>();
            dto.Events = new List<GameEvent>();
            dto.Events.Add(new GameEvent()
            {
                Id = 66,
                Order = 0,
                StartTime = DateTime.Now.Subtract(new TimeSpan(1,0,0)),
                EndTime = DateTime.Now.Add(new TimeSpan(1,0,0)),
                Icon = "https://s3.amazonaws.com/dev-withbuddies-event-lobby/41d62a1b-7926-4754-84c8-0c6c91da401c",
                Title = "Amber Special Event",
                Subtitle = "Testing 4.17 features"
            });
            dto.Version = "2";
            MsgPackSerializer.DefaultContext.RegisterSerializer<GameEventsGetResponse>("Events", "Marquees", "Version");
            MsgPackSerializer.DefaultContext.RegisterSerializer<GameEvent>("Id", "Order", "StartTime", "EndTime", "Icon", "Title", "Subtitle");
            byte[] payload = dto.ToMsgPack();
            Assert.IsNotNull(payload);
            Assert.AreNotEqual(0, payload.Length);
            var restoredDto = MsgPackSerializer.Deserialize<GameEventsGetResponse>(payload);
            Assert.IsNotNull(restoredDto);
            Assert.AreEqual("2", restoredDto.Version);
            Assert.IsNotNull(restoredDto.Events);
            Assert.AreEqual(1, restoredDto.Events.Count);
            Assert.AreEqual(dto.Events[0].Id, restoredDto.Events[0].Id);
            Assert.AreEqual(dto.Events[0].Order, restoredDto.Events[0].Order);
            Assert.AreEqual(dto.Events[0].Icon, restoredDto.Events[0].Icon);
            Assert.AreEqual(dto.Events[0].Title, restoredDto.Events[0].Title);
            Assert.AreEqual(dto.Events[0].Subtitle, restoredDto.Events[0].Subtitle);
        }

        [Test]
        public void TestMultiThreading()
        {
            const int numTestIterations = 5;
            for (int j = 0; j < numTestIterations; j++)
            {
                const int numThreads = 10;
                const int numPerThread = 1000;
                var threads = new List<Thread>(numThreads);
                var source = AnimalMessage.CreateTestMessage();
                for (int i = 0; i < numThreads; i++)
                {
                    threads.Add(new Thread(() =>
                    {
                        RoundTrip(source, numPerThread, j > 0);
                    }));
                }
                numVerified = 0;
                foreach (var thread in threads)
                {
                    thread.Start();
                }
                foreach (var thread in threads)
                {
                    thread.Join(20000);
                }
                Assert.AreEqual(numThreads * numPerThread, numVerified);
            }
        }

        private void RoundTrip(AnimalMessage input, int numTrips, bool verify)
        {
            for (int i = 0; i < numTrips; i++)
            {
                byte[] buffer = input.ToMsgPack();
                if (verify)
                    VerifyAnimalMessage(input, MsgPackSerializer.Deserialize<AnimalMessage>(buffer));
                Interlocked.Increment(ref numVerified);
            }
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
            if (MsgPackSerializer.DefaultContext.SerializationMethod == SerializationMethod.Array)
            {
                Assert.IsTrue(restored.NullableIntTwo.HasValue);  
            }
            else
            {
                Assert.IsFalse(restored.NullableIntTwo.HasValue);
            }
            Assert.IsTrue(restored.NullableIntThree.HasValue && msg.NullableIntThree.Value == 1); 
		}
	}
}

