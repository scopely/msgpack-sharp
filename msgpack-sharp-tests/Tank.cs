using System;

namespace scopely.msgpacksharp.tests
{
    public class Tank
    {
        public Tank()
        {
        }

        public float MaxSpeed { get; set; }
        public string Name { get; set; }
        public AnimalMessage Cargo { get; set; }
    }
}
