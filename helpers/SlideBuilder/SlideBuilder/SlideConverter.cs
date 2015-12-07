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

    public class SlideConverter
    {
        public const string PPTX_EXTENSION = ".pptx";
        public const string INDEX_FILE_PATH = @"..\..\index.html";
        public const string PACKAGE_FILE_PATH = @"..\..\package.json";
        public const string OPENJS_FILE_CONTENT = @"require(""openurl"").open(""http://localhost:12000/index.html"");";

        public static void ExtractPPTXtoMD(string rootDir, string githubName, string language)
        {
            GitHubName = githubName;
            Language = language;

            var pptxFiles = GetAllPPTXFilesFrom(rootDir);
            var mdsDir = Directory.CreateDirectory(rootDir + @"\.mds");

            for (int i = 0; i < pptxFiles.Count; i++)
            {
                var file = pptxFiles[i];

                string lectureDirStr = GetLecturePath(file, rootDir);
                var lectureDir = mdsDir.CreateSubdirectory(lectureDirStr);
                var slidesDir = lectureDir.CreateSubdirectory("slides");

                AddLocalServerFiles(slidesDir);
                AddTableOfContentsMD(lectureDir);

                var slides = GetAllSlides(file);
                var mdLines = BuildMDLines(slides);

                File.WriteAllLines(slidesDir + @"\README.md", mdLines);
            }
        }

        public static string GitHubName { get; set; }

        public static string Language { get; set; }

        private static IList<string> BuildMDLines(ICollection<IEnumerable<string>> slides)
        {
            IList<string> text = new List<string>();
            foreach (var slide in slides)
            {
                foreach (var line in slide)
                {
                    text.Add(line);
                }
                text.Add("");
            }

            return text;
        }

        private static void AddTableOfContentsMD(DirectoryInfo lectureDir)
        {
            string[] tableOfContents = {
                    string.Format("## {0}", lectureDir.Name.Replace("-", " ")),
                    string.Format("### [View Presentation online](https://rawgit.com/TelerikAcademy/{1}/master/{0}/slides/index.html)", lectureDir.Name.Replace(" ", "%20"), GitHubName),
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

        public static ICollection<IEnumerable<string>> GetAllSlides(string path)
        {
            using (PresentationDocument presentationDocument = PresentationDocument.Open(path, false))
            {
                PresentationPart presentationPart = presentationDocument.PresentationPart;
                if (presentationPart != null && presentationPart.Presentation != null)
                {
                    Presentation presentation = presentationPart.Presentation;

                    if (presentation.SlideIdList != null)
                    {
                        var slideIds = presentation.SlideIdList.ChildElements;
                        ICollection<IEnumerable<string>> slides = new List<IEnumerable<string>>();

                        for (int i = 0; i < slideIds.Count; i++)
                        {
                            string slidePartRelationshipId = (slideIds[i] as SlideId).RelationshipId;
                            SlidePart slidePart = (SlidePart)presentationPart.GetPartById(slidePartRelationshipId);

                            var slideText = GetAllTextInSlide(slidePart, i);
                            slides.Add(slideText);
                        }

                        return slides;
                    }
                }

                return null;
            }
        }

        public static string[] GetAllTextInSlide(SlidePart slidePart, int slideNum)
        {
            if (slidePart == null) { throw new ArgumentNullException("slidePart"); }
            if (slidePart.Slide == null) { throw new ArgumentNullException("slidePart.Slide"); }

            var mdSlide = new MDSlide();
            mdSlide.IsTitleSlide = slideNum == 0;
            mdSlide.HasImage = slidePart.Slide.Descendants<Picture>().Any();

            foreach (var shape in slidePart.Slide.Descendants<Shape>())
            {
                var mdShape = new MDShape();

                CheckSlideType(shape, mdSlide, mdShape);
                CheckWrappingShape(shape, mdShape);

                foreach (var paragraph in shape.Descendants<Drawing.Paragraph>())
                {
                    string paragraphText = ExtractTextFromParagraph(mdShape, paragraph);
                    ParseParagraphText(mdSlide, mdShape, paragraphText);
                }

                if (mdShape.IsMultiCode && mdShape.AddedCodeOpen)
                {
                    mdSlide.Texts.AddLast(new MDShape("```"));
                    mdShape.IsMultiCode = false;
                    mdShape.AddedCodeOpen = false;
                }
            }

            return mdSlide.ToStringArray();
        }

        private static void ParseParagraphText(MDSlide mdSlide, MDShape mdShape, string paragraphText)
        {
            if (paragraphText.Length > 0)
            {
                var line = paragraphText.ToString()
                    .Replace("``", "")
                    .Replace("*", "x")
                    .Replace("", "&rarr;");

                if (mdShape.IsTitle)
                {
                    mdSlide.Texts.AddLast(new MDShape(string.Format("# {0}", line)));
                }
                else if (mdShape.IsSecTitle)
                {
                    if (line == "Live Demo")
                    {
                        mdSlide.IsDemoSlide = true;
                        line = "[Demo]()";

                        var demoTitle = mdSlide.Texts.Last.Value.ToString();
                        mdSlide.Texts.RemoveLast();
                        mdSlide.Texts.AddLast(new MDShape(string.Format("<!-- {0} -->", demoTitle)));
                    }

                    mdSlide.Texts.AddLast(new MDShape(string.Format("##  {0}", line)));
                }
                else if (mdShape.IsMultiCode)
                {
                    if (!mdShape.AddedCodeOpen)
                    {
                        mdSlide.Texts.AddLast(new MDShape(""));
                        mdSlide.Texts.AddLast(new MDShape(string.Format("```{0}", Language)));
                        mdSlide.Texts.AddLast(new MDShape(line));
                        mdShape.AddedCodeOpen = true;
                    }
                    else
                    {
                        mdSlide.Texts.AddLast(new MDShape(line));
                    }
                }
                else if (mdShape.IsBalloon)
                {
                    mdSlide.Texts.AddLast(new MDShape(string.Format(@"<div class=""fragment balloon"" style=""width:250px; top:60%; left:10%"">{0}</div>", line)));
                    mdSlide.HasTags = true;
                }
                else if (mdSlide.IsTitleSlide)
                {
                    mdSlide.Signature.Add(line);
                }
                else
                {
                    mdSlide.Texts.AddLast(new MDShape(string.Format("{0}- {1}", new string(' ', mdShape.IndentCount * 2), line)));
                }
            }
        }

        private static string ExtractTextFromParagraph(MDShape mdShape, Drawing.Paragraph paragraph)
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

        private static void CheckSlideType(Shape shape, MDSlide mdSlide, MDShape mdText)
        {
            var placeholder = shape.Descendants<PlaceholderShape>().FirstOrDefault();

            if (placeholder != null && placeholder.Type != null)
            {
                mdText.IsTitle = placeholder.Type.Value == PlaceholderValues.Title || placeholder.Type.Value == PlaceholderValues.CenteredTitle;
                mdText.IsSecTitle = placeholder.Type.Value == PlaceholderValues.SubTitle;
                mdSlide.IsSlideSection |= placeholder.Type.Value == PlaceholderValues.CenteredTitle;
            }
        }

        private static void CheckWrappingShape(Shape shape, MDShape mdText)
        {
            var bodyWrapp = shape.Descendants<TextBody>().FirstOrDefault()
                                    .Descendants<DocumentFormat.OpenXml.Drawing.BodyProperties>().FirstOrDefault().Wrap;
            if (bodyWrapp != null)
            {
                var presetGeometry = shape.Descendants<DocumentFormat.OpenXml.Drawing.PresetGeometry>().FirstOrDefault();

                if (bodyWrapp.Value == DocumentFormat.OpenXml.Drawing.TextWrappingValues.Square &&
                    presetGeometry != null && presetGeometry.Prefix != null)
                {
                    var wrappShape = presetGeometry.Preset.Value;
                    mdText.IsMultiCode = wrappShape == DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle;
                    mdText.IsBalloon = wrappShape == DocumentFormat.OpenXml.Drawing.ShapeTypeValues.WedgeRoundRectangleCallout;
                }
            }
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
