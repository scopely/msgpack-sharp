using System;

namespace scopely.msgpacksharp.tests
{
	public class AnimalMessage
	{
		public AnimalMessage()
		{
		}

		public string AnimalName { get; set; }
		public string AnimalKind { get; set; }
		public AnimalColor AnimalColor { get; set; }
	}
}
