namespace SlideBuilder.Models.Slides
{
  using System.Linq;
  using System.Collections.Generic;
  using Shapes;

  public class MDSlidePresentationTitle : MDSlide
  {
    public const string SIGNATURE_FORMAT = "<div class=\"signature\">\n{0}{1}{2}</div>";
    public const string SIGNATURE_COURSE_FORMAT = "<p class=\"signature-course\">{0}</p>\n";
    public const string SIGNATURE_INITIATIVE_FORMAT = "<p class=\"signature-initiative\">{0}</p>\n";
    public const string SIGNATURE_LINK_FORMAT = "<a href=\"{0}\" class=\"signature-link\">{0}</a>\n";

    private IList<string> Signature { get; set; }

    public MDSlidePresentationTitle()
        : base()
    {
      this.Signature = new List<string>();
      this.CssClass.Add("slide-title");
    }

    public override void AddShape(MDShape mdShape)
    {
      this.Signature.Add(mdShape.ToString());
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
      string initiative = string.Format(SIGNATURE_INITIATIVE_FORMAT, this.Signature.FirstOrDefault(s => s.Contains("Telerik") && s.Contains("Academy")).Trim(new char[] { '-', ' ' }));
      string link = string.Format(SIGNATURE_LINK_FORMAT, this.Signature.FirstOrDefault(s => s.Contains("http")).Trim(new char[] { '-', ' ' }));
      string course = string.Format(SIGNATURE_COURSE_FORMAT, this.Signature.FirstOrDefault(s => !s.Contains("http") && !s.Contains("Telerik")).Trim(new char[] { '-', ' ' }));

      return string.Format(SIGNATURE_FORMAT, course, initiative, link);
    }
  }
}
