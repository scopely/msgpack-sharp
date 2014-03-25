using System;

namespace scopely.msgpacksharp.tests
{
	public class AnimalMessage
	{
		public AnimalMessage()
		{
		}

		[MsgPack(Sequence = 10)]
		public string AnimalName { get; set; }
		[MsgPack(Sequence = 20)]
		public string AnimalKind { get; set; }
		[MsgPack(Sequence = 30)]
		public AnimalColor AnimalColor { get; set; }
		[MsgPack(Sequence = 40)]
		public DateTime BirthDay { get; set; }
	}
}
