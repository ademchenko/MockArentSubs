namespace MocksArentStubs
{
    public class Message
    {
        private readonly string text;
        private readonly string to;

        public Message(string to, string text)
        {
            this.to = to;
            this.text = text;
        }

        public string To { get { return to; } }
        public string Text { get { return text; } }
    }
}