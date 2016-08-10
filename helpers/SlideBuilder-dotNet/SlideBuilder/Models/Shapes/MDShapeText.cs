namespace SlideBuilder.Models.Shapes
{
  using System;
  using System.Text;

  public class MDShapeText : IMDShape
  {
    protected const string TEXT_FORMAT = "{0}- {1}";
    protected const string COMMENT_FORMAT = "<!-- {0} -->";

    public MDShapeText()
    {
      this.Line = new StringBuilder();
    }

    public MDShapeText(string line, int indent)
      : this()
    {
      this.AddLine(line, indent);
    }

    public void AddLine(string line, int indent)
    {
      this.Line.Append(line);
      this.IndentCount = indent;
    }

    public StringBuilder Line { get; set; }

    public int IndentCount { get; set; }

    public override string ToString()
    {
      string text = this.Line.ToString();
      return string.Format(TEXT_FORMAT, GetIndent(), text);
    }

    public string GetIndent()
    {
      return new string(' ', this.IndentCount * 2);
    }

    public string GetLine()
    {
      return this.Line.ToString();
    }
  }
}
