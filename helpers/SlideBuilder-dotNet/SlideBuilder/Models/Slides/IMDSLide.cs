namespace SlideBuilder.Models.Slides
{
  using System.Collections.Generic;

  using Shapes;

  public interface IMDSlide
  {
    string CssId { get; set; }

    void AddShape(IMDShape mdShape);

    void AddShapes(IEnumerable<IMDShape> mdShapes);

    string[] ToStringArray();
  }
}
