namespace SlideBuilder.Models.Slides
{
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
  }
}
