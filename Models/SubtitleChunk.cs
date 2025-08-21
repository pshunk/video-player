namespace video_player_wpf.Models
{
    internal class SubtitleChunk
    {
        public int id;
        public List<Word> text;
        public long startTime;
        public long endTime;

        public SubtitleChunk(int id, List<Word> text, double startTime, double endTime)
        {
            this.id = id;
            this.text = text;
            this.startTime = (long)startTime;
            this.endTime = (long)endTime;
        }
    }
}
