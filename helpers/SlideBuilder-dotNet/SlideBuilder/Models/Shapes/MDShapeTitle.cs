namespace SlideBuilder.Models.Shapes
{
  public class MDShapeTitle : MDShapeText
  {
    private const string TITLE_FORMAT = "# {0}";
    private const string SUBTITLE_FORMAT = "## {0}";

    private bool isSecTitle;

    public MDShapeTitle(string line)
        : this(line, false)
    {
    }

    public MDShapeTitle(string line, bool isSecondaryTitle)
        : base(line)
    {
      this.isSecTitle = isSecondaryTitle;
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
