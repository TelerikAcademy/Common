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

                    mdPresentation.AddSlides(ParseSlides(slideParts));
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

            MDSlide slide;
            if (slideIndex != 0)
            {
                slide = new MDSlide();
            }
            else
            {
                slide = new MDSlideTitle();
            }
            slide.IsNewSection = slideIndex == 1;

            var shapes = slidePart.Slide.Descendants<Shape>();
            foreach (var shape in slidePart.Slide.Descendants<Shape>())
            {
                var mdShape = new MDShapeText();

                // CheckSlideType
                var placeholder = shape.Descendants<PlaceholderShape>().FirstOrDefault();
                if (placeholder != null && placeholder.Type != null)
                {
                    slide.IsNewSection |= placeholder.Type.Value == PlaceholderValues.CenteredTitle;
                    mdShape.IsTitle = placeholder.Type.Value == PlaceholderValues.CenteredTitle ||
                                        placeholder.Type.Value == PlaceholderValues.Title;
                    mdShape.IsSecTitle = placeholder.Type.Value == PlaceholderValues.SubTitle;
                }

                // CheckWrappingShape
                TextBody textBody = shape.Descendants<TextBody>().FirstOrDefault();
                if (textBody != null)
                {
                    var bodyProps = textBody.Descendants<Drawing.BodyProperties>().FirstOrDefault();
                    if (bodyProps != null)
                    {
                        var bodyWrapp = bodyProps.Wrap;
                        if (bodyWrapp != null)
                        {
                            var presetGeometry = shape.Descendants<Drawing.PresetGeometry>().FirstOrDefault();
                            if (bodyWrapp.Value == Drawing.TextWrappingValues.Square &&
                                presetGeometry != null && presetGeometry.Prefix != null)
                            {
                                var wrappShape = presetGeometry.Preset.Value;
                                mdShape.IsMultiCode = wrappShape == Drawing.ShapeTypeValues.Rectangle;
                                mdShape.IsBalloon = wrappShape == Drawing.ShapeTypeValues.WedgeRoundRectangleCallout;
                            }
                        }
                    }
                }

                foreach (var paragraph in shape.Descendants<Drawing.Paragraph>())
                {
                    string paragraphText = ExtractTextFromParagraph(mdShape, paragraph);
                    ParseParagraphText(slide, mdShape, paragraphText);
                }

                if (mdShape.IsMultiCode && mdShape.AddedCodeOpen)
                {
                    slide.Shapes.AddLast(new MDShapeText("```"));
                    mdShape.IsMultiCode = false;
                    mdShape.AddedCodeOpen = false;
                }
            }

            // Get images
            var images = slidePart.Slide.Descendants<Picture>().ToList();
            foreach (var image in images)
            {
                string rId = image.BlipFill.Blip.Embed.Value;
                ImagePart imagePart = (ImagePart)slidePart.Slide.SlidePart.GetPartById(rId);
                var offset = image.Descendants<Drawing.Offset>().FirstOrDefault();
                var parent = offset.Parent;

                var img = Image.FromStream(imagePart.GetStream());
                long xCoordinate = offset.X.HasValue ? offset.X.Value : 0;
                long yCoordinate = offset.Y.HasValue ? offset.Y.Value : 0;

                MDShapeImage mdImage = new MDShapeImage(slideIndex, img, rId, xCoordinate, yCoordinate);
                slide.Shapes.AddLast(mdImage);
                mdImage.SaveImageToFile(imagesRootFolder);
            }

            return slide;
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

        private static ICollection<MDSlide> ParseSlides(IList<SlidePart> slideParts)
        {
            ICollection<MDSlide> mdSlides = new List<MDSlide>();
            for (int i = 0; i < slideParts.Count; i++)
            {
                var slidePart = slideParts[i];
                if (slidePart == null) { throw new ArgumentNullException("slidePart"); }
                if (slidePart.Slide == null) { throw new ArgumentNullException("slidePart.Slide"); }

                MDSlide mdSlide;
                if (i != 0)
                {
                    mdSlide = new MDSlide();
                }
                else
                {
                    mdSlide = new MDSlideTitle();
                }

                mdSlide.IsNewSection = i == 1;

                var shapes = slidePart.Slide.Descendants<Shape>();
                foreach (var shape in slidePart.Slide.Descendants<Shape>())
                {
                    var mdShape = new MDShapeText();

                    // CheckSlideType
                    var placeholder = shape.Descendants<PlaceholderShape>().FirstOrDefault();
                    if (placeholder != null && placeholder.Type != null)
                    {
                        mdSlide.IsNewSection |= placeholder.Type.Value == PlaceholderValues.CenteredTitle;
                        mdShape.IsTitle = placeholder.Type.Value == PlaceholderValues.CenteredTitle ||
                                            placeholder.Type.Value == PlaceholderValues.Title;
                        mdShape.IsSecTitle = placeholder.Type.Value == PlaceholderValues.SubTitle;
                    }

                    // CheckWrappingShape
                    TextBody textBody = shape.Descendants<TextBody>().FirstOrDefault();
                    if (textBody != null)
                    {
                        var bodyProps = textBody.Descendants<Drawing.BodyProperties>().FirstOrDefault();
                        if (bodyProps != null)
                        {
                            var bodyWrapp = bodyProps.Wrap;
                            if (bodyWrapp != null)
                            {
                                var presetGeometry = shape.Descendants<Drawing.PresetGeometry>().FirstOrDefault();
                                if (bodyWrapp.Value == Drawing.TextWrappingValues.Square &&
                                    presetGeometry != null && presetGeometry.Prefix != null)
                                {
                                    var wrappShape = presetGeometry.Preset.Value;
                                    mdShape.IsMultiCode = wrappShape == Drawing.ShapeTypeValues.Rectangle;
                                    mdShape.IsBalloon = wrappShape == Drawing.ShapeTypeValues.WedgeRoundRectangleCallout;
                                }
                            }
                        }
                    }

                    foreach (var paragraph in shape.Descendants<Drawing.Paragraph>())
                    {
                        string paragraphText = ExtractTextFromParagraph(mdShape, paragraph);
                        ParseParagraphText(mdSlide, mdShape, paragraphText);
                    }

                    if (mdShape.IsMultiCode && mdShape.AddedCodeOpen)
                    {
                        mdSlide.Shapes.AddLast(new MDShapeText("```"));
                        mdShape.IsMultiCode = false;
                        mdShape.AddedCodeOpen = false;
                    }
                }

                // Get images
                var images = slidePart.Slide.Descendants<Picture>().ToList();
                foreach (var image in images)
                {
                    string rId = image.BlipFill.Blip.Embed.Value;
                    ImagePart imagePart = (ImagePart)slidePart.Slide.SlidePart.GetPartById(rId);
                    var offset = image.Descendants<Drawing.Offset>().FirstOrDefault();
                    var parent = offset.Parent;

                    var img = Image.FromStream(imagePart.GetStream());
                    long xCoordinate = offset.X.HasValue ? offset.X.Value : 0;
                    long yCoordinate = offset.Y.HasValue ? offset.Y.Value : 0;

                    mdSlide.Shapes.AddLast(new MDShapeImage(i, img, rId, xCoordinate, yCoordinate));
                }

                mdSlides.Add(mdSlide);
            }

            return mdSlides;
        }

        private static string ExtractTextFromParagraph(MDShapeText mdShape, Drawing.Paragraph paragraph)
        {
            StringBuilder paragraphText = new StringBuilder();
            foreach (var run in paragraph.Descendants<Drawing.Run>())
            {
                var schemeColor = run.Descendants<Drawing.SchemeColor>().FirstOrDefault();
                var color = run.Descendants<Drawing.RgbColorModelHex>().FirstOrDefault();
                mdShape.IsMultiCode = color != null && color.Val != null && color.Val == "8CF4F2";

                mdShape.IsCode = schemeColor != null && schemeColor.Val == Drawing.SchemeColorValues.Accent5;

                var paragraphProperties = paragraph.Descendants<Drawing.ParagraphProperties>().FirstOrDefault();
                mdShape.IndentCount = paragraphProperties != null && paragraphProperties.Level != null ? paragraphProperties.Level.Value : 0;

                var text = run.Descendants<Drawing.Text>().FirstOrDefault();
                if (text != null)
                {
                    if (mdShape.IsCode)
                    {
                        string textToInsert = text.Text.Trim();
                        paragraphText.AppendFormat("`{0}`", text.Text);
                    }
                    else
                    {
                        paragraphText.Append(text.Text);
                    }
                }
            }

            return paragraphText.ToString();
        }

        private static void ParseParagraphText(MDSlide mdSlide, MDShapeText mdShape, string paragraphText)
        {
            if (paragraphText.Length > 0)
            {
                var line = paragraphText.ToString()
                    .Replace("``", "")
                    //.Replace("*", "x")
                    .Replace("", "&rarr;");

                if (mdShape.IsTitle)
                {
                    mdSlide.Shapes.AddFirst(new MDShapeText(string.Format("# {0}", line)));
                }
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
                else if (mdShape.IsMultiCode)
                {
                    if (!mdShape.AddedCodeOpen)
                    {
                        mdSlide.Shapes.AddLast(new MDShapeText(""));
                        mdSlide.Shapes.AddLast(new MDShapeText(string.Format("```{0}", lang)));
                        mdSlide.Shapes.AddLast(new MDShapeText(line));
                        mdShape.AddedCodeOpen = true;
                    }
                    else
                    {
                        mdSlide.Shapes.AddLast(new MDShapeText(line));
                    }
                }
                else if (mdShape.IsBalloon)
                {
                    mdSlide.Shapes.AddLast(new MDShapeText(string.Format(@"<div class=""fragment balloon"" style=""width:250px; top:60%; left:10%"">{0}</div>", line)));
                    mdSlide.HasTags = true;
                }
                else
                {
                    mdSlide.Shapes.AddLast(new MDShapeText(string.Format("{0}- {1}", new string(' ', mdShape.IndentCount * 2), line)));
                }
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
