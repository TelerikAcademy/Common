namespace SlideBuilder.Models
{
    using System.Linq;
    using System.Collections.Generic;

    public class MDSlide
    {
        public const string IMAGE_TAG = @"<img class=""slide-image"" src=""imgs/pic.png"" style=""width:80%; top:10%; left:10%"" />";
        public const string SECTION_START = @"<!-- section start -->";
        public const string SIGNATURE = @"<div class=""signature"">
    <p class=""signature-course"">{0}</p>
    <p class=""signature-initiative"">{1}</p>
    <a href = ""{2}"" class=""signature-link"">{2}</a>
</div>";

        public MDSlide()
        {
            this.Texts = new LinkedList<MDShape>();
            this.Signature = new List<string>();
        }

        public IList<string> Signature { get; set; }

        public bool HasTags { get; set; }

        public bool IsTitleSlide { get; set; }

        public bool IsSlideSection { get; set; }

        public bool IsDemoSlide { get; set; }

        public bool HasImage { get; set; }

        public LinkedList<MDShape> Texts { get; set; }

        public override string ToString()
        {
            return base.ToString();
        }

        internal string[] ToStringArray()
        {
            if (this.Texts.Count > 0)
            {
                string cssClass = this.IsTitleSlide ? "slide-title" : (this.IsSlideSection ? "slide-section" : null);
                if (this.IsDemoSlide) { cssClass += " demo"; }
                this.Texts.AddFirst(new MDShape(BuildAttr(true, null, cssClass)));

                if (this.IsTitleSlide && this.Signature.Any())
                {
                    this.Texts.AddLast(new MDShape(string.Format(SIGNATURE, this.Signature[0], this.Signature[1], this.Signature[2])));
                }

                if (this.HasImage)
                {
                    this.Texts.AddLast(new MDShape(""));
                    this.Texts.AddLast(new MDShape(IMAGE_TAG));
                }

                if ((this.IsSlideSection || this.IsTitleSlide) && !this.IsDemoSlide)
                {
                    this.Texts.AddFirst(new MDShape(SECTION_START));
                }

                return this.Texts.Select(t => t.ToString()).ToArray();
            }
            else
            {
                return null;
            }
        }

        private string BuildAttr(bool showInSlide = false, string id = null, string cssClass = null)
        {
            id = !string.IsNullOrEmpty(id) ? string.Format("id:'{0}', ", id) : "";
            cssClass = !string.IsNullOrEmpty(cssClass) ? string.Format("class:'{0}', ", cssClass) : "";
            var showInPresentation = showInSlide ? "showInPresentation:true, " : "";
            var hasScriptWrapper = this.HasTags || this.HasImage ? "hasScriptWrapper:true, " : "";

            string attr = string.Format("{0}{1}{2}{3}style:'{4}'",
                id, cssClass, showInPresentation, hasScriptWrapper, null);

            return "<!-- attr: { " + attr + " } -->";
        }
    }
}
