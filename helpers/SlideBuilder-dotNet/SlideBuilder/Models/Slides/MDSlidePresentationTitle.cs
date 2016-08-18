namespace SlideBuilder.Models.Slides
{
    using System.Linq;
    using System.Collections.Generic;
    using Shapes;

    public class MDSlidePresentationTitle : MDSlide, IMDSlide
    {
        public const string SIGNATURE_FORMAT = "<article class=\"signature\">\n\t{0}\t{1}\t{2}</article>";
        public const string SIGNATURE_COURSE_FORMAT = "<p class=\"signature-course\">{0}</p>\n";
        public const string SIGNATURE_INITIATIVE_FORMAT = "<p class=\"signature-initiative\">{0}</p>\n";
        public const string SIGNATURE_LINK_FORMAT = "<a href=\"{0}\" class=\"signature-link\">{0}</a>\n";

        private IList<string> signature;

        public MDSlidePresentationTitle()
            : base()
        {
            this.signature = new List<string>();
            this.CssClass.Add("slide-title");
        }

        public override bool IsTitleSlide
        {
            get
            {
                return true;
            }
        }

        public override void AddShape(IMDShape mdShape)
        {
            if (mdShape is MDShapeTitle)
            {
                this.Titles.Add(mdShape);
            }
            else if (mdShape is MDShapeImage)
            {
                this.Shapes.AddLast(mdShape);
            }
            else
            {
                this.signature.Add(mdShape.GetLine());
            }
        }

        public override void AddShapes(IEnumerable<IMDShape> mdShapes)
        {
            foreach (IMDShape mdShape in mdShapes)
            {
                this.AddShape(mdShape);
            }
        }

        public override string[] ToStringArray()
        {
            if (this.Titles.Count <= 0)
            {
                return new string[0];
            }

            List<string> result = new List<string>();

            result.AddRange(base.ToStringArray());
            result.Add(ParseSignature());

            return result.ToArray();
        }

        private string ParseSignature()
        {
            string initiative = string.Format(SIGNATURE_INITIATIVE_FORMAT, this.signature.FirstOrDefault(s => s.Contains("Telerik") && s.Contains("Academy")));
            string link = string.Format(SIGNATURE_LINK_FORMAT, this.signature.FirstOrDefault(s => s.Contains("http")));
            string course = string.Format(SIGNATURE_COURSE_FORMAT, this.signature.FirstOrDefault(s => !s.Contains("http") && !s.Contains("Telerik")));

            return string.Format(SIGNATURE_FORMAT, course, initiative, link);
        }
    }
}
