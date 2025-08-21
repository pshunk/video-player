namespace video_player_wpf.Models
{
    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", ElementName = "JMdict", IsNullable = false)]
    public partial class Dictionary
    {

        private JMdictEntry[] entries;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("entry")]
        public JMdictEntry[] Entries
        {
            get
            {
                return this.entries;
            }
            set
            {
                this.entries = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class JMdictEntry
    {

        private JMdictEntryK_ele[] kanjiElements;

        private JMdictEntryR_ele[] readingElements;

        private JMdictEntrySense[] senses;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("k_ele")]
        public JMdictEntryK_ele[] KanjiElements
        {
            get
            {
                return this.kanjiElements;
            }
            set
            {
                this.kanjiElements = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("r_ele")]
        public JMdictEntryR_ele[] ReadingElements
        {
            get
            {
                return this.readingElements;
            }
            set
            {
                this.readingElements = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("sense")]
        public JMdictEntrySense[] Senses
        {
            get
            {
                return this.senses;
            }
            set
            {
                this.senses = value;
            }
        }

        public string Word
        {
            get { return this.kanjiElements == null ? this.readingElements.First().reb : this.kanjiElements.First().keb; }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class JMdictEntryK_ele
    {

        private string kanji;

        /// <remarks/>
        public string keb
        {
            get
            {
                return this.kanji;
            }
            set
            {
                this.kanji = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class JMdictEntryR_ele
    {

        private string reading;

        /// <remarks/>
        public string reb
        {
            get
            {
                return this.reading;
            }
            set
            {
                this.reading = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class JMdictEntrySense
    {

        private JMdictEntrySenseGloss[] definitions;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("gloss", typeof(JMdictEntrySenseGloss))]
        public JMdictEntrySenseGloss[] Definitions
        {
            get
            {
                return this.definitions;
            }
            set
            {
                this.definitions = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class JMdictEntrySenseGloss
    {

        private string definition;

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Definition
        {
            get
            {
                return this.definition;
            }
            set
            {
                this.definition = value;
            }
        }
    }
}