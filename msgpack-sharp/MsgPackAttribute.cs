using System;

namespace scopely.msgpacksharp
{
	[AttributeUsage(AttributeTargets.Property)]
	public class MsgPackAttribute : Attribute
	{
		public MsgPackAttribute()
		{
		}

		public int Sequence { get; set; }
	}
}
