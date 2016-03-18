using System;
using System.Collections.Generic;

namespace scopely.msgpacksharp.tests
{
    public class GameEvent
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string DeepLinkUrl { get; set; }
        public string DetailsPageDescription { get; set; }
        public string DetailsPageButtonText { get; set; }
        public string DetailsPageButtonDeepLinkUrl { get; set; }
        public List<Prize> Rewards { get; set; }
        public string ButtonText { get; set; }
    }
}
