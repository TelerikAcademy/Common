namespace SlideBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Presentation;

    public class Program
    {
        public const string PPTX_EXTENSION = ".pptx";
        public const string INDEX_FILE_PATH = @"..\..\index.html";
        public const string PACKAGE_FILE_PATH = @"..\..\package.json";
        public const string OPENJS_FILE_CONTENT = @"require(""openurl"").open(""http://localhost:12000/index.html"");";
        public const string IMAGE_TAG = @"<img class=""slide-image"" src=""imgs/pic.png"" style=""width:80%; top:10%; left:10%"" />";

        // Change these for every repo
        public const string LANGUAGE = "cs";
        public const string GITHUB_NAME = "Data-Structures-and-Algorithms";
        public const string SVN_DIR_PATH = @"D:\Telerik Academy\12. DSA\SVN 2014\5. Advanced Data Structures";

        static void Main()
        {
            ExtractPPTXtoMD(SVN_DIR_PATH);
        }

        private static void ExtractPPTXtoMD(string rootDir)
        {
            var pptxFiles = GetAllPPTXFilesFrom(rootDir);
            var mdsDir = Directory.CreateDirectory(rootDir + @"\.mds");

            foreach (var file in pptxFiles)
            {
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
                    string.Format("### [View Presentation online](https://rawgit.com/TelerikAcademy/{1}/master/{0}/slides/index.html)", lectureDir.Name.Replace(" ", "%20"), GITHUB_NAME),
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

        private static ICollection<string> GetAllPPTXFilesFrom(string dir)
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

                            var slideText = GetAllTextInSlide(slidePart);
                            slides.Add(slideText);
                        }

                        return slides;
                    }
                }

                return null;
            }
        }

        public static string[] GetAllTextInSlide(SlidePart slidePart)
        {
            if (slidePart == null)
            {
                throw new ArgumentNullException("slidePart");
            }

            LinkedList<string> texts = new LinkedList<string>();
            bool hasTags = false;
            bool isTitleSlide = false;
            bool isDemoSlide = false;
            bool hasImage = false;

            if (slidePart.Slide != null)
            {
                hasImage = hasTags = slidePart.Slide.Descendants<Picture>().Any();
                foreach (var shape in slidePart.Slide.Descendants<Shape>())
                {
                    bool isTitle = false;
                    bool isSecTitle = false;
                    bool isMultiCode = false;
                    bool addedCodeOpen = false;
                    bool isBalloon = false;

                    var placeholder = shape.Descendants<PlaceholderShape>().FirstOrDefault();

                    if (placeholder != null && placeholder.Type != null)
                    {
                        isTitle = placeholder.Type.Value == PlaceholderValues.Title || placeholder.Type.Value == PlaceholderValues.CenteredTitle;
                        isSecTitle = placeholder.Type.Value == PlaceholderValues.SubTitle;
                        isTitleSlide |= placeholder.Type.Value == PlaceholderValues.CenteredTitle;
                    }

                    var bodyWrapp = shape.Descendants<TextBody>().FirstOrDefault()
                        .Descendants<DocumentFormat.OpenXml.Drawing.BodyProperties>().FirstOrDefault().Wrap;
                    if (bodyWrapp != null)
                    {
                        var presetGeometry = shape.Descendants<DocumentFormat.OpenXml.Drawing.PresetGeometry>().FirstOrDefault();
                         
                        if (bodyWrapp.Value == DocumentFormat.OpenXml.Drawing.TextWrappingValues.Square &&
                            presetGeometry != null && presetGeometry.Prefix != null)
                        {
                            var wrappShape = presetGeometry.Preset.Value;
                            isMultiCode = wrappShape == DocumentFormat.OpenXml.Drawing.ShapeTypeValues.Rectangle;
                            isBalloon = wrappShape == DocumentFormat.OpenXml.Drawing.ShapeTypeValues.WedgeRoundRectangleCallout;
                        }
                    }

                    foreach (var paragraph in shape.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                    {
                        StringBuilder paragraphText = new StringBuilder();
                        int indentCount = 0;
                        foreach (var run in paragraph.Descendants<DocumentFormat.OpenXml.Drawing.Run>())
                        {
                            var schemeColor = run.Descendants<DocumentFormat.OpenXml.Drawing.SchemeColor>().FirstOrDefault();
                            bool isCode = schemeColor != null && schemeColor.Val == DocumentFormat.OpenXml.Drawing.SchemeColorValues.Accent5;
                            var paragraphProperties = paragraph.Descendants<DocumentFormat.OpenXml.Drawing.ParagraphProperties>().FirstOrDefault();
                            indentCount = paragraphProperties != null && paragraphProperties.Level != null ? paragraphProperties.Level.Value : 0;

                            var text = run.Descendants<DocumentFormat.OpenXml.Drawing.Text>().FirstOrDefault();
                            if (text == null)
                            {
                                continue;
                            }
                            else if (isCode)
                            {
                                paragraphText.AppendFormat("`{0}`", text.Text);
                            }
                            else
                            {
                                paragraphText.Append(text.Text);
                            }
                        }

                        if (paragraphText.Length > 0)
                        {
                            var line = paragraphText.ToString().Replace("``", "").Replace("*", "x").Replace("", "&rarr;");
                            if (isTitle)
                            {
                                texts.AddLast(string.Format("# {0}", line));
                            }
                            else if (isSecTitle)
                            {
                                if (line == "Live Demo")
                                {
                                    isDemoSlide = true;
                                    line = "[Demo]()";
                                }

                                texts.AddLast(string.Format("##  {0}", line));
                            }
                            else if (isMultiCode)
                            {
                                if (!addedCodeOpen)
                                {
                                    texts.AddLast("");
                                    texts.AddLast(string.Format("```{0}", LANGUAGE));
                                    texts.AddLast(line);
                                    addedCodeOpen = true;
                                }
                                else
                                {
                                    texts.AddLast(line);
                                }
                            }
                            else if (isBalloon)
                            {
                                texts.AddLast(string.Format(@"<div class=""fragment balloon"">{0}</div>", line));
                                hasTags = true;
                            }
                            else
                            {
                                texts.AddLast(string.Format("{0}* {1}", new string(' ', indentCount * 2), line));
                            }
                        }
                    }

                    if (isMultiCode && addedCodeOpen)
                    {
                        texts.AddLast("```");
                        isMultiCode = false;
                        addedCodeOpen = false;
                    }

                }
            }

            if (texts.Count > 0)
            {
                string cssClass = isTitleSlide ? "slide-section" : null;
                if (isDemoSlide) { cssClass += " demo"; }

                texts.AddFirst(BuildAttr(hasTags, true, null, cssClass, null));

                if (hasImage)
                {
                    texts.AddLast("");
                    texts.AddLast(IMAGE_TAG);
                }

                return texts.ToArray();
            }
            else
            {
                return null;
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

        private static string BuildAttr(bool hasImages = false, bool showInSlide = false, string id = null, string cssClass = null, string style = null)
        {
            id = !string.IsNullOrEmpty(id) ? string.Format("id:'{0}', ", id) : "";
            cssClass = !string.IsNullOrEmpty(cssClass) ? string.Format("class:'{0}', ", cssClass) : "";
            var showInPresentation = showInSlide ? "showInPresentation:true, " : "";
            var hasScriptWrapper = hasImages ? "hasScriptWrapper:true, " : "";

            string attr = string.Format("{0}{1}{2}{3}style:'{4}'",
                id, cssClass, showInPresentation, hasScriptWrapper, style);

            return "<!-- attr: { " + attr + " } -->";
        }
    }
}
