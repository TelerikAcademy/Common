namespace SlideBuilder.Models.Slides
{
  using System.Linq;
  using System.Collections.Generic;
  using Shapes;

  public class MDSlide
  {
    public MDSlide()
    {
      this.Titles = new List<MDShapeTitle>();
      this.Shapes = new LinkedList<MDShape>();
      this.CssClass = new List<string>();
    }

    public bool IsTitleSlide { get; set; }

    public bool IsDemoSlide { get; set; }

    public bool HasImages
    {
      get
      {
        return this.Shapes.Any(s => s is MDShapeImage);
      }
    }

    public bool HasTags
    {
      get
      {
        return this.Shapes.Any(s => s is MDShapeImage || s is MDShapeBalloon);
      }
    }

    public IList<string> CssClass { get; protected set; }

    public IList<MDShapeTitle> Titles { get; set; }

    public LinkedList<MDShape> Shapes { get; set; }

    public virtual void AddShape(MDShape mdShape)
    {
      this.Shapes.AddLast(mdShape);
    }

    public virtual string[] ToStringArray()
    {
      if (this.Shapes.Count <= 0) { return new string[0]; }

      List<string> result = new List<string>();
      result.Add(this.BuildAttr(true));
      result.AddRange(this.Titles.Select(t => t.ToString()));
      result.AddRange(this.Shapes.Select(t => t.ToString()));

      return result.ToArray();
    }

    protected string BuildAttr(bool showInPresentation = false, string id = null)
    {
      id = !string.IsNullOrEmpty(id) ? string.Format("id:'{0}', ", id) : "";
      var hasScriptWrapper = this.HasTags || this.HasImages;

      List<string> attr = new List<string>();
      attr.Add(string.Format("id:'{0}'", id));
      attr.Add(string.Format("class:'{0}'", string.Join(" ", this.CssClass)));
      attr.Add(string.Format("showInPresentation:'{0}'", showInPresentation));
      attr.Add(string.Format("hasScriptWrapper:'{0}'", hasScriptWrapper));
      attr.Add(string.Format("style:'{0}'", ""));

      return "<!-- attr: { " + string.Join(", ", attr) + " } -->";
    }
  }
}
