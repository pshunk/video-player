using System.Diagnostics;

namespace video_player_wpf.Models
{
    internal class Word
    {
        public string Surface { get; set; }
        public string Reading { get; set; }
        public string OriginalForm { get; set; }
        public string Furigana
        {
            get { return GetFurigana(); }
        }

        public Word(string surface, string reading, string originalForm)
        {
            this.Surface = surface;
            this.Reading = reading;
            this.OriginalForm = originalForm;
        }

        private string GetFurigana()
        {
            char c;
            string beforeKanji = "";
            string afterKanji = "";
            bool before = true;
            for (int i = 0; i < Surface.Length; i++)
            {
                c = Surface.ElementAt(i);
                if (!Kawazu.Utilities.IsKanji(c) && before)
                {
                    beforeKanji += c;
                }
                else if (!Kawazu.Utilities.IsKanji(c))
                {
                    afterKanji += c;
                }
                else
                {
                    before = false;
                }
            }
            string furigana = "";
            if (beforeKanji.Length <= Reading.LastIndexOf(afterKanji))
            {
                furigana = Reading[beforeKanji.Length..Reading.LastIndexOf(afterKanji)];
            }
            return furigana;
        }
    }
}
