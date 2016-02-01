namespace SlideBuilder.Models.Shapes
{
  using System.Collections.Generic;
  using System.Text;

  public class MDShapeMultiCode : MDShape
  {
    private const string CODE_FORMAT = "{0}{1}";
    private const string CODE_BEGIN_FORMAT = "```{0}";
    private const string CODE_END_FORMAT = "```";

    public MDShapeMultiCode(string lang)
    //: base(line)
    {
      this.Lang = lang;
      this.Lines = new List<MDShapeText>();
    }

    public string Lang { get; private set; }

    public IList<MDShapeText> Lines { get; private set; }

    public void AddCode(MDShapeText shapeText)
    {
      this.Lines.Add(shapeText);
    }

    public override string ToString()
    {
      StringBuilder result = new StringBuilder();

      result.AppendLine();
      result.AppendLine(string.Format(CODE_BEGIN_FORMAT, this.Lang));
      foreach (MDShapeText shapeText in this.Lines)
      {
        result.AppendLine(string.Format(CODE_FORMAT, shapeText.GetIndent(), shapeText.Line.ToString()));
      }
      result.AppendLine(CODE_END_FORMAT);

      return result.ToString();
    }
  }
}
