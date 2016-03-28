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
  using Models.Slides;
  using Models.Shapes;
  using System.Runtime.InteropServices;

  public class SlideConverterV2
  {
    public const string PPTX_EXTENSION = ".pptx";
    public const string INDEX_FILE_PATH = @"..\..\index.html";
    public const string PACKAGE_FILE_PATH = @"..\..\package.json";
    public const string OPENJS_FILE_CONTENT = @"require(""openurl"").open(""http://localhost:10000/index.html"");";

    private static Dictionary<string, string> textReplace = new Dictionary<string, string>()
    {
      { "``", "" },
      { "", "&rarr;" },
      { "Example", "_Example_" },
      { "EXAMPLE", "_Example_" },
      { "Note", "_Note_" },
      { "NOTE", "_Note_" },
      { "Live Demo", "[Demo]()" },
    };

    private static string gitHub;
    private static string lang;
    private static string imagesRootFolder;
    private static int presentationImageIndex;

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
        using (PresentationDocument presentationDocument = PresentationDocument.Open(filePath, false))
        {
          presentationImageIndex = 0;
          IList<SlidePart> slideParts = GetSlideParts(presentationDocument);
          for (int slideIndex = 0; slideIndex < slideParts.Count; slideIndex++)
          {
            MDSlide mdSlide = ParseSlidePart(slideParts, slideIndex);
            mdPresentation.StartNewSection(mdSlide.IsTitleSlide);
            mdPresentation.AddSlide(mdSlide);
          }
        }

        // add all files
        AddTableOfContentsREADME(lectureDir);
        AddLocalServerFiles(slidesDir);
        File.WriteAllLines(slidesDir + @"\README.md", mdPresentation.ToStringArray());

        Console.WriteLine("Adde files for presentation:\n{0}\n", filePath.Substring(rootDir.Length));
      }
    }

    private static MDSlide ParseSlidePart(IList<SlidePart> slideParts, int slideIndex)
    {
      var slidePart = slideParts[slideIndex];
      if (slidePart == null) { throw new ArgumentNullException("slidePart"); }
      if (slidePart.Slide == null) { throw new ArgumentNullException("slidePart.Slide"); }

      SlideType slideType = GetSlideType(slidePart, slideIndex);
      MDSlide slide = AssignSlide(slideType);

      // Extract texts
      var shapes = slidePart.Slide.Descendants<Shape>().Where(s => s.Descendants<Drawing.Paragraph>().Any());
      foreach (Shape shape in shapes)
      {
        if (string.IsNullOrWhiteSpace(shape.InnerText)) { continue; }
        slide.AddShapes(GetShapes(shape));
      }

      // Extract images
      var pictures = slidePart.Slide.Descendants<Picture>().ToList();
      foreach (var picture in pictures)
      {
        string rId = picture.BlipFill.Blip.Embed.Value;
        ImagePart imagePart = (ImagePart)slidePart.Slide.SlidePart.GetPartById(rId);
        Image image = Image.FromStream(imagePart.GetStream());

        SaveImageToFile(image, slideIndex);
        MDShapeImage mdImage = GetImage(slideIndex, image, picture);
        presentationImageIndex++;

        slide.AddShape(mdImage);
      }

      // Extract graphics
      var graphics = slidePart.Slide.Descendants<GraphicFrame>().ToList();
      foreach (var graphic in graphics)
      {
        // TODO: Find a way to export graphics as images
      }

      return slide;
    }

    private static MDShapeImage GetImage(int slideIndex, Image image, Picture picture)
    {
      string rId = picture.BlipFill.Blip.Embed.Value;

      var offset = picture.ShapeProperties.Transform2D.Offset;
      long top = (offset.X.HasValue ? offset.X.Value : 0);
      long left = (offset.Y.HasValue ? offset.Y.Value : 0);
      long width = picture.ShapeProperties.Transform2D.Extents.Cx;

      return new MDShapeImage(image, presentationImageIndex, top, left, width); ;
    }

    public static void SaveImageToFile(Image img, int slideIndex)
    {
      try
      {
        string dirPath = string.Format(MDShapeImage.IMAGE_FULL_FOLDER_PATH, imagesRootFolder);
        if (!Directory.Exists(dirPath))
        {
          Directory.CreateDirectory(dirPath);
        }
        string filePath = string.Format(MDShapeImage.IMAGE_FULL_NAME, dirPath, presentationImageIndex);
        img.Save(filePath);
      }
      catch (ExternalException ex)
      {
        Console.WriteLine(string.Format("Problem with saving image {0} on slide N: {1}", presentationImageIndex, slideIndex), ex.Message);
      }
    }

    private static ICollection<IMDShape> GetShapes(Shape shape)
    {
      ICollection<IMDShape> mdShapes = new List<IMDShape>();
      var type = GetShapeType(shape);

      var paragraphs = shape.Descendants<Drawing.Paragraph>().ToList();
      for (int paragraphIndex = 0; paragraphIndex < paragraphs.Count(); paragraphIndex++)
      {
        var paragraph = paragraphs[paragraphIndex];
        string text = ParseText(paragraph);
        switch (type)
        {
          case ShapeType.CenteredTitle:
            mdShapes.Add(new MDShapeTitle(text));
            break;
          case ShapeType.SlideSection:
            mdShapes.Add(new MDShapeTitle(text));
            break;
          case ShapeType.Title:
            mdShapes.Add(new MDShapeTitle(text));
            break;
          case ShapeType.SubTitle:
            mdShapes.Add(new MDShapeTitle(text, true));
            break;
          case ShapeType.SlideDemo:
            mdShapes.Add(new MDShapeTitle(text, true));
            break;
          case ShapeType.MultilineCode:
            IMDShape mdShape = new MDShapeMultiCode(lang);
            while (paragraphIndex < paragraphs.Count())
            {
              paragraph = paragraphs[paragraphIndex];
              mdShape.AddLine(ParseText(paragraph), GetIndent(paragraph));
              paragraphIndex++;
            }
            mdShapes.Add(mdShape);
            break;
          case ShapeType.Balloon:
            var offset = shape.ShapeProperties.Transform2D.Offset;
            long top = (offset.Y.HasValue ? offset.Y.Value : 0);
            long left = (offset.X.HasValue ? offset.X.Value : 0);
            long width = shape.ShapeProperties.Transform2D.Extents.Cx;

            mdShapes.Add(new MDShapeBalloon(text, top, left, width));
            break;
          case ShapeType.None:
            if (!string.IsNullOrEmpty(text))
            {
              mdShapes.Add(new MDShapeText(text, GetIndent(paragraph)));
            }
            break;
          case ShapeType.Box:
            // TODO: render with dark blue box
            break;
          case ShapeType.SlideNumber:
            // Don't add this to the slides
            break;
          case ShapeType.UnknownBox:
            // Don't add this to the slides
            break;
        }
      }

      return mdShapes;
    }

    private static ShapeType GetShapeType(Shape shape)
    {
      ShapeType type = ShapeType.None;
      var placeHolders = shape.Descendants<PlaceholderShape>().Where(ph => ph.Type != null);
      var openXmlType = (placeHolders.Select(ph => ph.Type).FirstOrDefault() ?? PlaceholderValues.Object).Value;
      switch (openXmlType)
      {
        case PlaceholderValues.CenteredTitle:
          type = ShapeType.CenteredTitle; break;
        case PlaceholderValues.Title:
          type = ShapeType.Title; break;
        case PlaceholderValues.SubTitle:
          type = ShapeType.SubTitle; break;
        case PlaceholderValues.Object:
          if (IsMultilineCode(shape))
          {
            type = ShapeType.MultilineCode;
          }
          else if (IsBalloon(shape))
          {
            type = ShapeType.Balloon;
          }
          else if (IsOfShape(shape, Drawing.ShapeTypeValues.Rectangle))
          {
            type = ShapeType.UnknownBox;
          }
          break;
        case PlaceholderValues.Body:
          if (IsMultilineCode(shape))
          {
            type = ShapeType.MultilineCode;
          }
          break;
        case PlaceholderValues.SlideNumber:
          type = ShapeType.SlideNumber;
          break;
      }

      return type;
    }

    private static MDSlide AssignSlide(SlideType type)
    {
      MDSlide slide;
      switch (type)
      {
        case SlideType.PresentationTitle:
          slide = new MDSlidePresentationTitle();
          break;
        case SlideType.TableOfContents:
          slide = new MDSlide();
          slide.IsTitleSlide = true;
          break;
        case SlideType.SectionStart:
          slide = new MDSlideSection();
          break;
        case SlideType.Demo:
          slide = new MDSlideDemo();
          break;
        default:
          slide = new MDSlide();
          break;
      }

      return slide;
    }

    private static SlideType GetSlideType(SlidePart slidePart, int slideIndex)
    {
      SlideType type = SlideType.None;
      if (slideIndex == 0)
      {
        type = SlideType.PresentationTitle;
      }
      else if (slideIndex == 1)
      {
        type = SlideType.TableOfContents;
      }
      else
      {
        var placeHolders = slidePart.Slide.Descendants<PlaceholderShape>().Where(ph => ph.Type != null);
        bool hasCenteredTitle = (placeHolders.Any(ph => (ph.Type ?? PlaceholderValues.Object) == PlaceholderValues.CenteredTitle));

        if (hasCenteredTitle)
        {
          if (slidePart.Slide.InnerText.Contains("Demo"))
          {
            type = SlideType.Demo;
          }
          else
          {
            type = SlideType.SectionStart;
          }
        }
      }

      return type;
    }

    private static bool IsMultilineCode(Shape shape)
    {
      bool isMultilineCode = false;
      var shapeColor = shape.Descendants<Drawing.RgbColorModelHex>().Select(cm => cm.Val).FirstOrDefault();
      isMultilineCode = (shapeColor != null && shapeColor == "8CF4F2") ||
        shape.NonVisualShapeProperties.NonVisualDrawingProperties.Name.Value == "Text Placeholder 5";
      // TODO: Find better way to detect

      return isMultilineCode;
    }

    private static bool IsBalloon(Shape shape)
    {
      return IsOfShape(shape, Drawing.ShapeTypeValues.WedgeRoundRectangleCallout);
    }

    private static bool IsOfShape(Shape shape, Drawing.ShapeTypeValues shapeType)
    {
      bool isOfShape = false;
      var textBody = shape.Descendants<TextBody>().FirstOrDefault();
      if (textBody != null)
      {
        var bodyProps = textBody.Descendants<Drawing.BodyProperties>().FirstOrDefault();
        if (bodyProps != null)
        {
          var bodyWrapp = bodyProps.Wrap;
          if (bodyWrapp != null)
          {
            var presetGeometry = shape.Descendants<DocumentFormat.OpenXml.Drawing.PresetGeometry>().FirstOrDefault();

            if (presetGeometry != null && presetGeometry.Prefix != null)
            {
              var wrappShape = presetGeometry.Preset.Value;
              isOfShape = wrappShape == shapeType;
            }
          }
        }
      }

      return isOfShape;
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

    private static string ParseText(Drawing.Paragraph paragraph)
    {
      List<string> texts = new List<string>();

      var runs = paragraph.Descendants<Drawing.Run>();
      foreach (var run in runs)
      {
        bool isCode = run.Descendants<Drawing.SchemeColor>().Any(sc => sc.Val == Drawing.SchemeColorValues.Accent5);
        bool isMultiCode = run.Descendants<Drawing.RgbColorModelHex>().Any(cm => cm.Val == "8CF4F2");

        var text = run.Descendants<Drawing.Text>().FirstOrDefault();
        if (text != null)
        {
          string textToInsert = text.Text;
          if (isCode && !string.IsNullOrWhiteSpace(textToInsert))
          {
            textToInsert = string.Format("**{0}**", textToInsert.Trim());
          }

          texts.Add(textToInsert);
        }
      }

      string result = string.Join("", texts);
      foreach (var replace in textReplace)
      {
        result = result.Replace(replace.Key, replace.Value);
      }

      return result;
    }

    private static string GetLecturePath(string file, string rootDir)
    {
      var parts = file.Substring(rootDir.Length)
          .Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

      parts = string.Join("\\", parts.Take(parts.Length - 1)).Split(' ');
      if (parts[0].Length == 2)
      {
        parts[0] = "0" + parts[0];
      }

      return string.Format("{0} {1}", parts[0], string.Join("-", parts.Skip(1))).Replace(",", "");
    }

    private static void AddTableOfContentsREADME(DirectoryInfo lectureDir)
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
