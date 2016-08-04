namespace SlideBuilder.Models.Slides
{
    using System.Linq;
    using System.Collections.Generic;
    using Shapes;
    using System;

    public class MDSlide : IMDSlide
    {
        public MDSlide()
        {
            this.Titles = new List<IMDShape>();
            this.Shapes = new LinkedList<IMDShape>();
            this.CssClass = new List<string>();
            this.IsTitleSlide = false;
        }

        public virtual bool IsTitleSlide { get; set; }

        public bool IsDemoSlide { get; set; }

        public bool HasImages
        {
            get
            {
                return this.Shapes.Any(s => s is MDShapeImage);
            }
        }

        public bool HasTags
        {
            get
            {
                return this.Shapes.Any(s => s is MDShapeImage || s is MDShapeBalloon);
            }
        }

        public IList<string> CssClass { get; protected set; }

        public IList<IMDShape> Titles { get; set; }

        public LinkedList<IMDShape> Shapes { get; set; }

        public virtual void AddShape(IMDShape mdShape)
        {
            if (mdShape is MDShapeTitle)
            {
                this.Titles.Add(mdShape);
            }
            else
            {
                this.Shapes.AddLast(mdShape);
            }
        }

        public virtual void AddShapes(IEnumerable<IMDShape> mdShapes)
        {
            foreach (IMDShape mdShape in mdShapes)
            {
                this.AddShape(mdShape);
            }
        }

        public virtual string[] ToStringArray()
        {
            if (this.Shapes.Count <= 0 || this.Titles.Count <= 0)
            {
                return new string[0];
            }

            List<string> result = new List<string>();

            // Add or Remove attributes
            result.Add(this.BuildAttr(true));

            result.AddRange(this.Titles.Select(t => t.ToString()));
            result.AddRange(this.Shapes.Select(t => t.ToString()));

            return result.ToArray();
        }

        protected string BuildAttr(bool showInPresentation = false, string id = null)
        {
            //id = !string.IsNullOrEmpty(id) ? string.Format("id:'{0}', ", id) : "";

            // hasScriptWrapper should always be true
            //var hasScriptWrapper = this.HasTags || this.HasImages;
            var hasScriptWrapper = true;

            List<string> attr = new List<string>();
            if (id != null || this.IsTitleSlide)
            {
                attr.Add(string.Format("id:'{0}'", id));
            }

            if(this.CssClass.Count != 0)
            {
                attr.Add(string.Format("class:'{0}'", string.Join(" ", this.CssClass)));
            }

            attr.Add(string.Format("showInPresentation:{0}", showInPresentation.ToString().ToLower()));
            attr.Add(string.Format("hasScriptWrapper:{0}", hasScriptWrapper.ToString().ToLower()));

            //attr.Add(string.Format("style:'{0}'", ""));

            return "<!-- attr: { " + string.Join(", ", attr) + " } -->";
        }
    }
}
