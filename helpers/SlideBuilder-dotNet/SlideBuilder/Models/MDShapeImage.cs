namespace SlideBuilder.Models
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Runtime.InteropServices;

    public class MDShapeImage : MDShape
    {
        public const string IMAGE_TAG = @"<img class=""slide-image"" src=""imgs\slide{0:D2}\{1}.png"" style=""top:{2}%; left:{3}%; width:{4}%"" />";
        public const string IMAGE_FOLDER = @"{0}\imgs\slide{1:D2}\";
        public const string IMAGE_NAME = @"{0}{1}.png";

        public MDShapeImage(int slideIndex, Image image, string imageId, long x, long y, long width = 80)
        {
            this.SlideIndex = slideIndex;
            this.Image = image;
            this.ImageId = imageId;
            this.X = 10; // x;
            this.Y = 10; // y;
            this.Width = width;
        }

        // TODO: Move from here
        public void SaveImageToFile(string rootFolder = @"D:\Temp\FromPPTX")
        {
            try {
                // TODO: Folder should be relative to slides\imgs
                string dirPath = string.Format(IMAGE_FOLDER, rootFolder, this.SlideIndex);
                if (!Directory.Exists(dirPath))
                {
                    Directory.CreateDirectory(dirPath);
                }
                string filePath = string.Format(IMAGE_NAME, dirPath, this.ImageId);
                this.Image.Save(filePath);
            }
            catch (ExternalException ex)
            {
                Console.WriteLine(string.Format("Unable to save image {0} on slide N: {1}", this.ImageId, this.SlideIndex), ex.Message);
            }
        }

        public string ImageId { get; private set; }

        public int SlideIndex { get; private set; }

        public Image Image { get; private set; }

        public long Width { get; private set; }

        public long X { get; private set; }

        public long Y { get; private set; }

        public override string ToString()
        {
            SaveImageToFile();
            return string.Format(IMAGE_TAG, this.SlideIndex, this.ImageId, this.X, this.Y, this.Width);
        }
    }
}
