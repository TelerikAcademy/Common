namespace SlideBuilder
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using System.IO;

  using DocumentFormat.OpenXml.Packaging;
  using DocumentFormat.OpenXml.Presentation;
  using Drawing = DocumentFormat.OpenXml.Drawing;

  using Models;
  using DocumentFormat.OpenXml;
  using System.Drawing;

  public class SlideConverterV2
  {
    public const string PPTX_EXTENSION = ".pptx";
    public const string INDEX_FILE_PATH = @"..\..\index.html";
    public const string PACKAGE_FILE_PATH = @"..\..\package.json";
    public const string OPENJS_FILE_CONTENT = @"require(""openurl"").open(""http://localhost:10000/index.html"");";

    private static string gitHub;
    private static string lang;
    private static string imagesRootFolder;

    public static void ExtractPPTXtoMD(string rootDir, string githubName, string language, string destinationDir = null)
    {
      gitHub = githubName;
      lang = language;

      IList<string> pptxFiles = GetAllPPTXFilesFrom(rootDir);
      DirectoryInfo mdsDir = Directory.CreateDirectory(destinationDir ?? (rootDir + @"\.mds"));

      for (int presentationIndex = 0; presentationIndex < pptxFiles.Count; presentationIndex++)
      {
        string filePath = pptxFiles[presentationIndex];

        // create sub-directorues
        string lectureDirStr = GetLecturePath(filePath, rootDir);
        DirectoryInfo lectureDir = mdsDir.CreateSubdirectory(lectureDirStr);
        DirectoryInfo slidesDir = lectureDir.CreateSubdirectory("slides");
        DirectoryInfo demosDir = lectureDir.CreateSubdirectory("demos");
        DirectoryInfo homeworkDir = lectureDir.CreateSubdirectory("homework");
        imagesRootFolder = slidesDir.ToString();


        // get and convert slide info
        MDPresentation mdPresentation = new MDPresentation();
        mdPresentation.StartNewSection();
        using (PresentationDocument presentationDocument = PresentationDocument.Open(filePath, false))
        {
          IList<SlidePart> slideParts = GetSlideParts(presentationDocument);
          for (int slideIndex = 0; slideIndex < slideParts.Count; slideIndex++)
          {
            MDSlide mdSlide = ParseSlidePart(slideParts, slideIndex);
            if (mdSlide.IsNewSection)
            {
              mdPresentation.StartNewSection();
            }

            mdPresentation.AddSlide(mdSlide);
          }
        }

        // add all files
        AddTableOfContentsMD(lectureDir);
        AddLocalServerFiles(slidesDir);
        File.WriteAllLines(slidesDir + @"\README.md", mdPresentation.ToStringArray());
      }
    }

    private static MDSlide ParseSlidePart(IList<SlidePart> slideParts, int slideIndex)
    {
      var slidePart = slideParts[slideIndex];
      if (slidePart == null) { throw new ArgumentNullException("slidePart"); }
      if (slidePart.Slide == null) { throw new ArgumentNullException("slidePart.Slide"); }

      MDSlide slide = slideIndex != 0 ? new MDSlide() : new MDSlideTitle();
      slide.IsNewSection = slideIndex == 1;

      var shapes = slidePart.Slide.Descendants<Shape>().Where(s => s.Descendants<Drawing.Paragraph>().Any());
      foreach (Shape shape in shapes)
      {
        var paragraphs = shape.Descendants<Drawing.Paragraph>();
        foreach (var paragraph in paragraphs)
        {
          var type = GetType(shape);
          switch (type)
          {
            case PlaceholderValues.CenteredTitle:
              slide.Titles.Add(new MDShapeTitle(paragraph.InnerText));
              break;
            case PlaceholderValues.Title:
              slide.Titles.Add(new MDShapeTitle(paragraph.InnerText));
              break;
            case PlaceholderValues.SubTitle:
              slide.Titles.Add(new MDShapeTitle(paragraph.InnerText, true));
              break;
            case PlaceholderValues.SlideNumber: break;
            case PlaceholderValues.Object:
              int indent = GetIndent(paragraph);
              if (paragraph.InnerText.Length > 0)
              {
                slide.AddShape(new MDShapeText(GetParagraphText(paragraph), indent));
              }
              break;
            default: slide.AddShape(new MDShapeText(paragraph.InnerText)); break;
          }
        }
      }

      GetImages(slideIndex, slidePart, slide);

      return slide;
    }

    private static int GetIndent(Drawing.Paragraph paragraph)
    {
      int indent = 0;
      if (paragraph.ParagraphProperties != null && paragraph.ParagraphProperties.Level != null)
      {
        indent = paragraph.ParagraphProperties.Level.Value;
      }

      return indent;
    }

    private static PlaceholderValues GetType(Shape shape)
    {
      var placeHolders = shape.Descendants<PlaceholderShape>().Where(ph => ph.Type != null);
      var type = placeHolders.Select(ph => ph.Type).FirstOrDefault();

      return type ?? PlaceholderValues.Object;
    }

    private static void GetImages(int slideIndex, SlidePart slidePart, MDSlide slide)
    {
      var images = slidePart.Slide.Descendants<Picture>().ToList();
      foreach (var image in images)
      {
        string rId = image.BlipFill.Blip.Embed.Value;
        ImagePart imagePart = (ImagePart)slidePart.Slide.SlidePart.GetPartById(rId);
        var img = Image.FromStream(imagePart.GetStream());

        var offset = image.ShapeProperties.Transform2D.Offset;
        long xCoordinate = (offset.X.HasValue ? offset.X.Value : 0);
        long yCoordinate = (offset.Y.HasValue ? offset.Y.Value : 0);
        long width = image.ShapeProperties.Transform2D.Extents.Cx;

        MDShapeImage mdImage = new MDShapeImage(slideIndex, img, rId, xCoordinate, yCoordinate, width);
        slide.Shapes.AddLast(mdImage);
        mdImage.SaveImageToFile(imagesRootFolder);
      }
    }

    private static IList<string> GetAllPPTXFilesFrom(string dir)
    {
      List<string> pptxFiles = new List<string>();
      string[] files = Directory.GetFiles(dir);
      foreach (string file in files)
      {
        if (file.EndsWith(PPTX_EXTENSION) && !file.Contains("~$"))
        {
          pptxFiles.Add(file);
        }
      }

      string[] directories = Directory.GetDirectories(dir);
      foreach (string subDir in directories)
      {
        if (dir.EndsWith(".svn"))
        { // skip .svn folder
          continue;
        }

        pptxFiles.AddRange(GetAllPPTXFilesFrom(subDir));
      }

      return pptxFiles;
    }

    private static IList<SlidePart> GetSlideParts(PresentationDocument presentationDocument)
    {
      PresentationPart presentationPart = presentationDocument.PresentationPart;
      if (presentationPart != null && presentationPart.Presentation != null)
      {
        Presentation presentation = presentationPart.Presentation;
        if (presentation.SlideIdList != null)
        {
          var slideIds = presentation.SlideIdList.ChildElements;
          IList<SlidePart> slideParts = new List<SlidePart>();

          for (int i = 0; i < slideIds.Count; i++)
          {
            string slidePartRelationshipId = (slideIds[i] as SlideId).RelationshipId;
            SlidePart slidePart = (SlidePart)presentationPart.GetPartById(slidePartRelationshipId);

            slideParts.Add(slidePart);
          }

          return slideParts;
        }
      }

      // No slide found
      return null;
    }

    private static string GetParagraphText(Drawing.Paragraph paragraph)
    {
      // .Replace("", "&rarr;");

      List<string> texts = new List<string>();

      var runs = paragraph.Descendants<Drawing.Run>();
      foreach (var run in runs)
      {
        bool isCode = run.Descendants<Drawing.SchemeColor>().Any(sc => sc.Val == Drawing.SchemeColorValues.Accent5);
        bool isMultiCode = run.Descendants<Drawing.RgbColorModelHex>().Any(cm => cm.Val == "8CF4F2");

        // var paragraphProperties = paragraph.Descendants<Drawing.ParagraphProperties>().FirstOrDefault();

        var text = run.Descendants<Drawing.Text>().FirstOrDefault();
        if (text != null)
        {
          string textToInsert = text.Text;
          if (isCode)
          {
            textToInsert = string.Format("`{0}`", textToInsert.Trim());
          }

          texts.Add(textToInsert);
        }
      }

      return string.Join("", texts);
    }

    private static void ParseParagraphText(MDSlide mdSlide, MDShapeText mdShape, Drawing.Text paragraphText)
    {
      if (paragraphText != null && paragraphText.Text.Length > 0)
      {
        var line = paragraphText.ToString();
        //.Replace("``", "")
        //.Replace("*", "x")
        //.Replace("", "&rarr;");

        //if (mdShape.IsTitle)
        //{
        //    mdSlide.Shapes.AddFirst(new MDShapeText(string.Format("# {0}", line)));
        //}
        //else if (mdShape.IsSecTitle)
        //{
        //    if (line == "Live Demo")
        //    {
        //        mdSlide.IsDemoSlide = true;
        //        line = string.Format("[Demo]({0})", gitHub + @"/demos");

        //        var demoTitle = mdSlide.Shapes.Last.Value.ToString();
        //        mdSlide.Shapes.RemoveLast();
        //        mdSlide.Shapes.AddLast(new MDShape(string.Format("<!-- {0} -->", demoTitle)));
        //    }

        //    mdSlide.Shapes.AddLast(new MDShape(string.Format("##  {0}", line)));
        //}
        //else if (mdShape.IsMultiCode)
        //{
        //    if (!mdShape.AddedCodeOpen)
        //    {
        //        mdSlide.Shapes.AddLast(new MDShapeText(""));
        //        mdSlide.Shapes.AddLast(new MDShapeText(string.Format("```{0}", lang)));
        //        mdSlide.Shapes.AddLast(new MDShapeText(line));
        //        mdShape.AddedCodeOpen = true;
        //    }
        //    else
        //    {
        //        mdSlide.Shapes.AddLast(new MDShapeText(line));
        //    }
        //}
        //else if (mdShape.IsBalloon)
        //{
        //    mdSlide.Shapes.AddLast(new MDShapeBalloon(line));
        //}
        //else
        //{
        //    mdSlide.Shapes.AddLast(new MDShapeText(string.Format("{0}- {1}", new string(' ', mdShape.IndentCount * 2), line)));
        //}
      }
    }

    private static string GetLecturePath(string file, string rootDir)
    {
      var parts = file.Substring(rootDir.Length)
          .Split(new char[] { '\\', '.' }, StringSplitOptions.RemoveEmptyEntries);

      parts = string.Join("\\", parts.Take(parts.Length - 1)).Split(' ');

      if (parts[0].Length == 2)
      {
        parts[0] = "0" + parts[0];
      }

      return string.Format("{0}. {1}", parts[0], string.Join("-", parts.Skip(1))).Replace(",", "");
    }

    private static void AddTableOfContentsMD(DirectoryInfo lectureDir)
    {
      string[] tableOfContents = {
                    string.Format("## {0}", lectureDir.Name.Replace("-", " ")),
                    string.Format("### [View Presentation online](https://rawgit.com/TelerikAcademy/{0}/slides/index.html)", lectureDir.Name.Replace(" ", "%20"), gitHub),
                    "### Table of Contents"
                };

      File.WriteAllLines(lectureDir + @"\README.md", tableOfContents);
    }

    private static void AddLocalServerFiles(DirectoryInfo slidesDir)
    {
      File.WriteAllLines(slidesDir + @"\index.html", ReadFile(INDEX_FILE_PATH));
      File.WriteAllLines(slidesDir + @"\package.json", ReadFile(PACKAGE_FILE_PATH));
      File.WriteAllLines(slidesDir + @"\open.js", new string[] { OPENJS_FILE_CONTENT });
    }

    public static string[] ReadFile(string path)
    {
      LinkedList<string> lines = new LinkedList<string>();
      try
      {
        using (StreamReader sr = new StreamReader(path))
        {
          string line;
          while ((line = sr.ReadLine()) != null)
          {
            lines.AddLast(line);
          }
        }
      }
      catch (Exception e)
      {
        Console.WriteLine("The file could not be read:");
        Console.WriteLine(e.Message);
      }

      return lines.ToArray();
    }
  }
}
