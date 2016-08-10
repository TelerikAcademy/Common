namespace SlideBuilder.Models
{
  using Slides;
  using System;
  using System.Collections.Generic;

  public class MDSection
    {
        public MDSection()
        {
            this.Slides = new List<IMDSlide>();
        }

        public IList<IMDSlide> Slides { get; internal set; }

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

        internal void AddSlide(IMDSlide slide)
        {
            this.Slides.Add(slide);
        }
    }
}
