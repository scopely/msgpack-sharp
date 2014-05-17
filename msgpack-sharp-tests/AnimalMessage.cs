using System;
using System.Collections.Generic;
using MsgPack.Serialization;

namespace scopely.msgpacksharp.tests
{
	public class AnimalMessage
	{
		public AnimalMessage()
		{
		}

		[MessagePackMember( 0 )]
		public bool IsAlive { get; set; }

		[MessagePackMember( 1 )]
		public int HeightInches { get; set; }

		[MessagePackMember( 2 )]
		public string AnimalName { get; set; }

		[MessagePackMember( 3 )]
		public string AnimalKind { get; set; }

		[MessagePackMember( 4 )]
		public AnimalColor AnimalColor { get; set; }

		[MessagePackMember( 5 )]
		public DateTime BirthDay { get; set; }

		[MessagePackMember( 6 )]
		public List<AnimalColor> SpotColors { get; set; }

		[MessagePackMember( 7 )]
		public List<AnimalColor> MoreColors { get; set; }

		[MessagePackMember( 8 )]
		public Dictionary<string,string> Metadata { get; set; }

		[MessagePackMember( 9 )]
		public List<int> ListOfInts { get; set; }

		[MessagePackMember( 10 )]
		public Habitat CurrentHabitat { get; set; }

		[MessagePackMember( 11 )]
		public string TheLongString { get; set; }

		[MessagePackMember( 12, NilImplication = NilImplication.Null )]
		public int? NullableIntOne { get; set; }

		[MessagePackMember( 13, NilImplication = NilImplication.MemberDefault )]
		public int? NullableIntTwo { get; set; }

		[MessagePackMember( 14, NilImplication = NilImplication.MemberDefault )]
		public int? NullableIntThree { get; set; }

		[MessagePackMember(15)]
		public int[] SomeInts { get; set; }

		[MessagePackMember(16)]
		public byte[] SomeBytes { get; set; }

		public static AnimalMessage CreateTestMessage()
		{
			AnimalMessage msg = new AnimalMessage();
			msg.IsAlive = true;
			msg.HeightInches = 7;
			msg.AnimalKind = "Cat";
			msg.AnimalName = "Lunchbox";
			msg.AnimalColor = new AnimalColor() { Red = 1.0f, Green = 0.1f, Blue = 0.1f };
			msg.BirthDay = new DateTime(1974, 1, 4);
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
			msg.CurrentHabitat = Habitat.Wild;

			msg.TheLongString = String.Empty;
			for (int i = 0; i < 257; i++)
			{
				msg.TheLongString += "+";
			}

			msg.NullableIntOne = null;
			msg.NullableIntTwo = null;
			msg.NullableIntThree = 1;

			msg.SomeInts = new int[5];
			for (int i = 0; i < msg.SomeInts.Length; i++)
				msg.SomeInts[i] = i;

			msg.SomeBytes = new byte[5];
			for (int i = 0; i < msg.SomeBytes.Length; i++)
				msg.SomeBytes[i] = (byte)i;

			return msg;
		}

		public override string ToString()
		{
			return string.Format("[AnimalMessage: IsAlive={0}, HeightInches={1}, AnimalName={2}, AnimalKind={3}, AnimalColor={4}, BirthDay={5}, SpotColors={6}, MoreColors={7}, Metadata={8}, ListOfInts={9}, CurrentHabitat={10}, TheLongString={11}]", IsAlive, HeightInches, AnimalName, AnimalKind, AnimalColor, BirthDay, SpotColors, MoreColors, Metadata, ListOfInts, CurrentHabitat, TheLongString);
		}
	}
}
