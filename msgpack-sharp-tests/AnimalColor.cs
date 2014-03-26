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

		public override bool Equals(object obj)
		{
			bool areEqual = false;
			if (this == obj)
				areEqual = true;
			else
			{
				AnimalColor other = obj as AnimalColor;
				if (other != null)
				{
					areEqual = this.Red == other.Red &&
					this.Green == other.Green &&
					this.Blue == other.Blue;
				}
			}
			return areEqual;
		}

		public override int GetHashCode()
		{
			return (Red + Green * 2.0f + Blue * 3.0f).GetHashCode();
		}
	}
}

