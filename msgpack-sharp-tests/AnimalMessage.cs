using System;
using System.Collections.Generic;
#if VERIFIER
using MsgPack.Serialization;
#endif

namespace scopely.msgpacksharp.tests
{
	public class AnimalMessage
	{
		public AnimalMessage()
		{
		}

#if VERIFIER
		[MessagePackMember( 0 )]
#endif
		[MsgPack(Sequence = 5)]
		public int HeightInches { get; set; }
#if VERIFIER
		[MessagePackMember( 1 )]
#endif
		[MsgPack(Sequence = 10)]
		public string AnimalName { get; set; }
#if VERIFIER
		[MessagePackMember( 2 )]
#endif
		[MsgPack(Sequence = 20)]
		public string AnimalKind { get; set; }
#if VERIFIER
		[MessagePackMember( 3 )]
#endif
		[MsgPack(Sequence = 30)]
		public AnimalColor AnimalColor { get; set; }
#if VERIFIER
		[MessagePackMember( 4 )]
#endif
		[MsgPack(Sequence = 40)]
		public DateTime BirthDay { get; set; }
#if VERIFIER
		[MessagePackMember( 5 )]
#endif
		[MsgPack(Sequence = 50)]
		public int[] SomeNumbers { get; set; }
#if VERIFIER
		[MessagePackMember( 6 )]
#endif
		[MsgPack(Sequence = 60)]
		public List<AnimalColor> SpotColors { get; set; }
#if VERIFIER
		[MessagePackMember( 7 )]
#endif
		[MsgPack(Sequence = 70)]
		public List<AnimalColor> MoreColors { get; set; }
#if VERIFIER
		[MessagePackMember( 8 )]
#endif
		[MsgPack(Sequence = 80)]
		public Dictionary<string,string> Metadata { get; set; }
#if VERIFIER
		[MessagePackMember( 9 )]
#endif
		[MsgPack(Sequence = 90)]
		public List<int> ListOfInts { get; set; }

#if VERIFIER
		[MessagePackMember( 10 )]
#endif
		[MsgPack(Sequence = 100)]
		public Habitat CurrentHabitat { get; set; }

		public static AnimalMessage CreateTestMessage()
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
			msg.CurrentHabitat = Habitat.Wild;
			return msg;
		}
	}
}
