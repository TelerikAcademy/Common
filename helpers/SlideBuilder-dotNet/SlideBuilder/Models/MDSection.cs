namespace SlideBuilder.Models
{
    using System;
    using System.Collections.Generic;

    public class MDSection
    {
        public MDSection()
        {
            this.Slides = new List<MDSlide>();
        }

        public IList<MDSlide> Slides { get; internal set; }

        public string[] ToStringArray()
        {
            List<string> stringList = new List<string>();
            for (int i = 0; i < this.Slides.Count; i++)
            {
                var slide = this.Slides[i];
                stringList.AddRange(slide.ToStringArray());
                stringList.Add(Environment.NewLine);
            }

            return stringList.ToArray();
        }

        internal void AddSlide(MDSlide slide)
        {
            this.Slides.Add(slide);
        }
    }
}
