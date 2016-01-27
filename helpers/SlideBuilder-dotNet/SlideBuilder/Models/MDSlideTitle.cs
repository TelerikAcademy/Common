namespace SlideBuilder.Models
{
    using System;
    using System.Linq;
    using System.Collections.Generic;

    public class MDSlideTitle : MDSlide
    {
        public const string SIGNATURE = @"<div class=""signature"">
    <p class=""signature-course"">{0}</p>
    <p class=""signature-initiative"">{1}</p>
    <a href = ""{2}"" class=""signature-link"">{2}</a>
</div>";

        public IList<string> Signature { get; set; }

        public MDSlideTitle()
            : base()
        {
            this.Signature = new List<string>();
        }

        public override string[] ToStringArray()
        {
            if (this.Shapes.Count <= 0)
            {
                return null;
            }

            string cssClass = this.IsTitleSlide ? "slide-title" : (this.IsNewSection ? "slide-section" : null);
            if (this.IsDemoSlide) { cssClass += " demo"; }
            this.Shapes.AddFirst(new MDShapeText(base.BuildAttr(true, null, cssClass)));

            if (this.IsTitleSlide && this.Signature.Any())
            {
                this.Shapes.AddLast(new MDShapeText(string.Format(SIGNATURE, this.Signature[0], this.Signature[1], this.Signature[2])));
            }
            
            //if ((this.IsNewSection || this.IsTitleSlide) && !this.IsDemoSlide)
            //{
            //    this.Shapes.AddFirst(new MDShapeText(SECTION_START));
            //}

            return this.Shapes.Select(t => t.ToString()).ToArray();
        }
    }
}
