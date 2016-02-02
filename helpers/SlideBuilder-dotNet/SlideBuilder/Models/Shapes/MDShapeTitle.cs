namespace SlideBuilder.Models.Shapes
{
  public class MDShapeTitle : MDShapeText, IMDShape
  {
    private const string TITLE_FORMAT = "# {0}";
    private const string SUBTITLE_FORMAT = "## {0}";

    private bool isSecTitle;

    public MDShapeTitle(string line, bool isSecondaryTitle)
        : base()
    {
      this.AddLine(line, 0);
      this.isSecTitle = isSecondaryTitle;
    }

    public MDShapeTitle(string line)
      : this(line, false)
    {
    }

    public override string ToString()
    {
      string result;
      if (this.isSecTitle)
      {
        result = string.Format(SUBTITLE_FORMAT, this.Line);
      }
      else
      {
        result = string.Format(TITLE_FORMAT, this.Line);
      }

      return result;
    }
  }
}
