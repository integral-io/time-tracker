namespace TimeTracker.Library.Models
{
    public class TextMessagePart
    {
        /// <summary>
        /// when processing the text message, mark this as true once has already been used
        /// </summary>
        public bool IsUsed { get; set; }
        public string Text { get; set; }
    }
}