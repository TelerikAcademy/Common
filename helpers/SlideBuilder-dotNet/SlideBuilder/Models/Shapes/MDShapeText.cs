namespace SlideBuilder.Models.Shapes
{
  using System.Text;

  public class MDShapeText : MDShape
  {
    private const string TEXT_FORMAT = "{0}- {1}";

    public MDShapeText()
      : this("", 0)
    {
    }

    public MDShapeText(string line)
        : this(line, 0)
    {
    }

    public MDShapeText(string line, int indent)
    {
      this.Line = new StringBuilder();
      this.Line.Append(line);
      this.IndentCount = indent;
    }

    public StringBuilder Line { get; set; }

    public int IndentCount { get; set; }

    public bool AddedCodeOpen { get; set; }

    public override string ToString()
    {
      return string.Format(TEXT_FORMAT, GetIndent(), this.Line.ToString());
    }

    public string GetIndent()
    {
      return new string(' ', this.IndentCount * 2);
    }
  }
}
