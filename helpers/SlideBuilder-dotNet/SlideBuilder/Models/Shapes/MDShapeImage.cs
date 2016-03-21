namespace SlideBuilder.Models.Shapes
{
  using System;
  using System.Drawing;
  using System.IO;
  using System.Runtime.InteropServices;

  public class MDShapeImage : MDShapeBox, IMDShape
  {
    public const string IMAGE_FULL_FOLDER_PATH = @"{0}\imgs\";
    public const string IMAGE_FULL_NAME = @"{0}pic{1:D2}.png";
    private const string IMAGE_FOLDER_PATH = @"\imgs\";
    
    // Added <!-- --> to prevent images to show in md
    private const string IMAGE_TAG = @"<!-- <img class=""slide-image"" src=""{0}"" style=""top:{1:F2}%; left:{2:F2}%; width:{3:F2}%; z-index:-1"" /> -->";

    public MDShapeImage(Image image, int imageIndex, long top, long left, long width)
      :base(top, left, width)
    {
      this.Image = image;
      this.ImageIndex = imageIndex;
    }

    public int ImageIndex { get; private set; }

    public Image Image { get; private set; }

    public override string ToString()
    {
      string src = string.Format(IMAGE_FULL_NAME, IMAGE_FOLDER_PATH, this.ImageIndex);

      return string.Format(IMAGE_TAG, src, this.Left, this.Top, this.Width);
    }
  }
}
