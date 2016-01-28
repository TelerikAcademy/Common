namespace SlideBuilder.Models
{
  using System;
  using System.Linq;
  using System.Collections.Generic;

  public class MDSlideTitle : MDSlide
  {
    public const string SIGNATURE_FORMAT = "<div class=\"signature\">\n{0}{1}{2}</div>";
    public const string SIGNATURE_COURSE_FORMAT = "<p class=\"signature-course\">{0}</p>\n";
    public const string SIGNATURE_INITIATIVE_FORMAT = "<p class=\"signature-initiative\">{0}</p>\n";
    public const string SIGNATURE_LINK_FORMAT = "<a href=\"{0}\" class=\"signature-link\">{0}</a>\n";

    private IList<string> Signature { get; set; }

    public MDSlideTitle()
        : base()
    {
      this.Signature = new List<string>();
      this.CssClass = "slide-title";
    }

    public override void AddShape(MDShapeText mdShape)
    {
      this.Signature.Add(mdShape.Line.ToString());
    }

    public override string[] ToStringArray()
    {
      if (this.Shapes.Count <= 0) { return new string[0]; }

      List<string> result = new List<string>();
      
      result.AddRange(base.ToStringArray());
      result.Add(ParseSignature());

      return result.ToArray();
    }

    private string ParseSignature()
    {
      string course = string.Format(SIGNATURE_COURSE_FORMAT, this.Signature.FirstOrDefault(s => !s.StartsWith("http") && !s.StartsWith("Telerik")));
      string initiative = string.Format(SIGNATURE_INITIATIVE_FORMAT, this.Signature.FirstOrDefault(s => s.StartsWith("Telerik")));
      string link = string.Format(SIGNATURE_LINK_FORMAT, this.Signature.FirstOrDefault(s => s.StartsWith("http")));

      return string.Format(SIGNATURE_FORMAT, course, initiative, link);
    }
  }
}
