using System.Collections.Generic;

namespace scopely.msgpacksharp.tests
{
    public class GameEventsGetResponse
    {
        public List<GameMarquee> Marquees { get; set; }
        public List<GameEvent> Events { get; set; }
        public string Version { get; set; }
    }
}
