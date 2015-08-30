using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterMeta.Models
{
    public class Match
    {
        public List<Participiant> participants { get; set; }
        public Timeline timeline { get; set; }

        public int? GetParticipientId(int chmpionId, int teamId)
        {
            var matchedParticipients = participants.FindAll(p => p.championId.Equals(chmpionId) && p.teamId.Equals(teamId));

            if (matchedParticipients.Count == 1)
            {
                return matchedParticipients.First().participantId;
            }

            return null;
        }
    }

    public class Participiant
    {
        public int participantId { get; set; }
        public int championId { get; set; }
        public int teamId { get; set; }
    }

    public class Timeline
    {
        public List<Frame> frames { get; set; }

        public Dictionary<long, Dictionary<int, int>> GetItemsEvents(int participientId)
        {
            var result = new Dictionary<long, Dictionary<int, int>>();

            if (frames != null)
            {
                foreach (var frame in frames)
                {
                    result.Add(frame.timestamp, frame.GetItemIdToItemCountFromEvents(participientId));
                }
            }


            return result;
        }
    }

    public class Frame
    {
        public long timestamp { get; set; }
        public List<TimelineEvent> events { get; set; }

        public Dictionary<int, int> GetItemIdToItemCountFromEvents(int participientId)
        {
            var result = new Dictionary<int, int>();

            if (events != null)
            {
                foreach (var item in events.FindAll(e => e.participantId.Equals(participientId)) )
                {
                    if (item.eventType == "ITEM_PURCHASED")
                    {
                        if (result.ContainsKey(item.itemId))
                        {
                            result[item.itemId]++;
                        }
                        else
                        {
                            result.Add(item.itemId, 1);
                        }
                    }
                    else if (item.eventType == "ITEM_UNDO" && result.ContainsKey(item.itemId))
                    {
                        result[item.itemId]--;
                    }
                }
            }

            return result;
        }
    }

    public class TimelineEvent
    {
        public int participantId { get; set; }
        public string eventType { get; set; }
        public int itemId { get; set; }
    }
}