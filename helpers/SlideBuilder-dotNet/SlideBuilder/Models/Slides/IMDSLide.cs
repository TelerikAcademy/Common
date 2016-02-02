namespace SlideBuilder.Models.Slides
{
  using System.Collections.Generic;

  using Shapes;

  public interface IMDSlide
  {
    void AddShape(IMDShape mdShape);

    void AddShapes(IEnumerable<IMDShape> mdShapes);
  }
}
