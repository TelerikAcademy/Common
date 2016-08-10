namespace SlideBuilder.Models.Shapes
{
  public class MDShapeTitle : MDShapeText, IMDShape
  {
    private const string TITLE_FORMAT = "# {0}";
    private const string SUBTITLE_FORMAT = "## {0}";
    private const string COMMENTED_TITLE_FORMAT = "<!-- # {0} -->";

    private bool isSecTitle;

    public bool isTitleCommented;

    public MDShapeTitle(string line, bool isSecondaryTitle, bool isTitleCommented = false)
            : base()
    {
      this.AddLine(line, 0);
      this.isSecTitle = isSecondaryTitle;
      this.isTitleCommented = isTitleCommented;
    }

    public MDShapeTitle(string line)
      : this(line, false)
    {
    }

    public override string ToString()
    {
      string result;

      string line = this.Line.ToString();
      if (this.isSecTitle)
      {
        result = string.Format(SUBTITLE_FORMAT, line);
      }
      else
      {
        if (line.EndsWith(")")) // repeating titles
        {
          // Remove repeating title
          result = null;
          return result;

          // Add repeating title
          //line = line.Substring(0, line.Length - 3).TrimEnd();
          //result = string.Format(COMMENT_FORMAT, string.Format(TITLE_FORMAT, line));
        }
        else if (this.isTitleCommented)
        {
          result = string.Format(COMMENTED_TITLE_FORMAT, line);
        }
        else
        {
          result = string.Format(TITLE_FORMAT, line);
        }
      }

      return result;
    }
  }
}
