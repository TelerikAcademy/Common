namespace SlideBuilder.Models.Slides
{
  using SlideBuilder.Models.Shapes;

  public class MDSlideQuestions : MDSlide, IMDSlide
  {
    public MDSlideQuestions(string title)
    {
      this.CssClass.Add("slide-questions");
      this.Titles.Add(new MDShapeTitle(title));
      this.Titles.Add(new MDShapeTitle("Questions", true));
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
