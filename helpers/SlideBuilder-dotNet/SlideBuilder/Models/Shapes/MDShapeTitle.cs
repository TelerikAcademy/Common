namespace SlideBuilder.Models
{
  public class MDShapeTitle : MDShapeText
  {
    private const string TITLE_FORMAT = "# {0}";
    private const string SUBTITLE_FORMAT = "## {0}";

    private bool isSecTitle;

    public MDShapeTitle(string line)
        : base(line)
    {
    }

    public MDShapeTitle(string line, bool isSecondaryTitle)
        : this(line)
    {
      this.isSecTitle = true;
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
