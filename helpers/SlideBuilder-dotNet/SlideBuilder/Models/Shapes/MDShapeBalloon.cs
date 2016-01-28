namespace SlideBuilder.Models
{
    public class MDShapeBalloon : MDShapeText
    {
        private const string BALLOON_TAG = @"<div class=""fragment balloon"" style=""width:250px; top:60%; left:10%"">{0}</div>";

        public MDShapeBalloon(string line)
            : base(line)
        {
        }

        public override string ToString()
        {
            return string.Format(BALLOON_TAG, this.Line.ToString());
        }
    }
}
