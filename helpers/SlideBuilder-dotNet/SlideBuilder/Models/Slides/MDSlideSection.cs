namespace SlideBuilder.Models.Slides
{
  using System.Collections.Generic;
  using System.Linq;

  public class MDSlideSection : MDSlide
  {
    public MDSlideSection()
      : base()
    {
      this.CssClass.Add("slide-section");
    }

    public override bool IsTitleSlide
    {
      get
      {
        return true;
      }
    }

    public override string[] ToStringArray()
    {
      if ((!this.IsTitleSlide && this.Shapes.Count <= 0) || this.Titles.Count <= 0)
      {
        return new string[0];
      }

      List<string> result = new List<string>();

      // Add or Remove attributes
      result.Add(this.BuildAttr(true));

      result.AddRange(this.Titles.Select(t => string.Format("<!-- {0} -->", t.ToString())));
      result.AddRange(this.Shapes.Select(t => t.ToString()));

      return result.ToArray();
    }
  }
}
