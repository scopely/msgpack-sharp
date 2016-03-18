using System;
using System.Collections.Generic;

namespace scopely.msgpacksharp.tests
{
    public class GameMarquee
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationSeconds { get; set; }
        public string Icon { get; set; }
        public string BannerBackground { get; set; }
        public string ButtonText { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string DeepLinkUrl { get; set; }
        public List<Prize> rewards { get; set; }
    }
}
