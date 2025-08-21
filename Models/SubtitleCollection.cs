using Nikse.SubtitleEdit.Core.Common;
using NMeCab.Specialized;

namespace video_player_wpf.Models
{
    internal class SubtitleCollection
    {
        public List<SubtitleChunk> chunks;
        public int currentSubtitleId = 0;
        public SubtitleChunk CurrentChunk
        {
            get { return chunks[currentSubtitleId]; }
        }
        private bool displaySubs = false;
        public bool DisplaySubs
        {
            get { return displaySubs; }
        }

        public SubtitleCollection()
        {
            this.chunks = [];
        }

        public void LoadSubtitle(Subtitle subtitle)
        {
            this.chunks = [];
            subtitle.Sort(Nikse.SubtitleEdit.Core.Enums.SubtitleSortCriteria.StartTime);
            List<Paragraph> paragraphs = subtitle.Paragraphs;
            using (var tagger = MeCabIpaDicTagger.Create())
            {
                for (int i = 0; i < paragraphs.Count; i++)
                {
                    var result = tagger.Parse(paragraphs[i].Text);
                    List<Word> text = [];
                    foreach (MeCabIpaDicNode node in result)
                    {
                        Word word = new(node.Surface, Kawazu.Utilities.ToRawHiragana(node.Reading), node.OriginalForm);
                        text.Add(word);
                    }
                    SubtitleChunk chunk = new(i, text, paragraphs[i].StartTime.TotalMilliseconds, paragraphs[i].EndTime.TotalMilliseconds);
                    chunks.Add(chunk);
                }
            }
            for (int i = 0; i < chunks.Count - 1; i++)
            {
                if (chunks[i].endTime >= chunks[i + 1].startTime)
                {
                    chunks[i + 1].startTime = chunks[i].endTime + 1;
                }
            }
        }

        public void UpdateSub(long currentTime)
        {
            try
            {
                if (!(CurrentChunk.startTime - 10 <= currentTime && currentTime < chunks[currentSubtitleId + 1].startTime))
                {
                    var currentSubtitle = chunks.Where(chunk => chunk.startTime <= currentTime && currentTime <= chunks[chunk.id + 1].startTime);
                    if (currentSubtitle.Any())
                    {
                        currentSubtitleId = currentSubtitle.First().id;
                    }
                }
                if (currentTime < CurrentChunk.endTime)
                {
                    displaySubs = true;
                }
                else
                {
                    displaySubs = false;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                displaySubs = false;
            }
        }

        public long PreviousSub(long currentTime)
        {
            if (chunks[currentSubtitleId].endTime >= currentTime)
            {
                currentSubtitleId--;
            }
            return chunks[currentSubtitleId].startTime;
        }

        public long NextSub()
        {
            if (currentSubtitleId < chunks.Count - 1)
            {
                currentSubtitleId++;
            }
            return chunks[currentSubtitleId].startTime;
        }
    }
}
