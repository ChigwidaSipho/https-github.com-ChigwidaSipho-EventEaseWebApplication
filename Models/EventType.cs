namespace EventEase.Models
{
    public class EventType
    {
        public int EventTypeID { get; set; }
        public string TypeName { get; set; }

        public ICollection<Event>? Events { get; set; }
    }
}