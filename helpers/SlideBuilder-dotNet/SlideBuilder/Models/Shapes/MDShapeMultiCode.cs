namespace SlideBuilder.Models.Shapes
{
  using System;
  using System.Collections.Generic;
  using System.Text;

  public class MDShapeMultiCode : IMDShape
  {
    private const string CODE_FORMAT = "{0}{1}";
    private const string CODE_BEGIN_FORMAT = "```{0}";
    private const string CODE_END_FORMAT = "```";

    public MDShapeMultiCode(string lang)
    {
      this.Lang = lang;
      this.Lines = new List<MDShapeText>();
    }

    public string Lang { get; private set; }

    public IList<MDShapeText> Lines { get; private set; }

    public void AddCodeLine(MDShapeText shapeText)
    {
    }

    public void AddLine(string line, int indent)
    {
      var shapeText =  new MDShapeText();
      shapeText.AddLine(line, indent);

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
