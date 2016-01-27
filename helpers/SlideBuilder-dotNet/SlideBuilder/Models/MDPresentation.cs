namespace SlideBuilder.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class MDPresentation
    {
        private const string SECTION_START = @"<!-- section start -->";
        
        public MDPresentation()
        {
            this.Sections = new LinkedList<MDSection>();
        }

        public ICollection<MDSection> Sections { get; internal set; }

        public void AddSlide(MDSlide slide)
        {
            this.Sections.Last().AddSlide(slide);
        }

        public void AddSlides(ICollection<MDSlide> slides)
        {
            foreach (var slide in slides)
            {
                this.AddSlide(slide);
            }
        }

        public void StartNewSection()
        {
            this.Sections.Add(new MDSection());
        }
        
        public string[] ToStringArray()
        {
            List<string> stringList = new List<string>();
            foreach (var secton in this.Sections)
            {
                stringList.Add(SECTION_START);
                stringList.AddRange(secton.ToStringArray());
                stringList.Add(Environment.NewLine);
            }

            return stringList.ToArray();
        }
    }
}
