namespace SlideBuilder.Models
{
    using System.Linq;
    using System.Collections.Generic;

    public class MDSlide
    {
        public MDSlide()
        {
            this.Shapes = new LinkedList<MDShape>();
        }

        public bool HasTags { get; set; }

        public bool IsTitleSlide { get; set; }

        public bool IsNewSection { get; set; }

        public bool IsDemoSlide { get; set; }

        public bool HasImage
        {
            get
            {
                return this.Shapes.Any(s => s is MDShapeImage);
            }
        }

        public MDShape PrimaryTitle { get; set; }

        public MDShape SecondaryTitle { get; set; }

        public LinkedList<MDShape> Shapes { get; set; }

        public virtual string[] ToStringArray()
        {
            if (this.Shapes.Count <= 0)
            {
                return null;
            }

            string cssClass = this.IsTitleSlide ? "slide-title" : (this.IsNewSection ? "slide-section" : null);
            if (this.IsDemoSlide) { cssClass += " demo"; }
            this.Shapes.AddFirst(new MDShapeText(BuildAttr(true, null, cssClass)));

            //if (this.HasImage)
            //{
            //    this.Shapes.AddLast(new MDShape(""));
            //    for (int i = 0; i < this.Images.Count; i++)
            //    {
            //        this.Shapes.AddLast(new MDShape(IMAGE_TAG));
            //    }
            //}

            //if ((this.IsNewSection || this.IsTitleSlide) && !this.IsDemoSlide)
            //{
            //    this.Shapes.AddFirst(new MDShape(SECTION_START));
            //}

            return this.Shapes.Select(t => t.ToString()).ToArray();
        }

        protected string BuildAttr(bool showInSlide = false, string id = null, string cssClass = null)
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
