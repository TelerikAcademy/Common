namespace SlideBuilder.Models.Shapes
{
  public class MDShapeBalloon : MDShapeBox, IMDShape
  {
    private const string BALLOON_TAG =
      @"<div class=""fragment balloon"" style=""top:{1:F2}%; left:{2:F2}%; width:{3:F2}%"">{0}</div>";

    public MDShapeBalloon(string line, long top, long left, long width)
        : base(top, left, width)
    {
      this.Line = line;
    }

    public string Line { get; private set; }

    public override string ToString()
    {
      return string.Format(BALLOON_TAG, this.Line.ToString(), this.Top, this.Left, this.Width);
    }
  }
}
