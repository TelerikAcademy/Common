namespace SlideBuilder.Models.Slides
{
  public class MDSlideDemo : MDSlide
  {
    public MDSlideDemo()
      : base()
    {
      this.CssClass.Add("slide-section");
      this.CssClass.Add("demo");
    }
  }
}
