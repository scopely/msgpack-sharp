using System;

namespace scopely.msgpacksharp.tests
{
	public class AnimalColor
	{
		public AnimalColor()
		{
		}

		[MsgPack(Sequence = 10)]
		public float Red { get; set; }
		[MsgPack(Sequence = 20)]
		public float Green { get; set; }
		[MsgPack(Sequence = 30)]
		public float Blue { get; set; }
	}
}

