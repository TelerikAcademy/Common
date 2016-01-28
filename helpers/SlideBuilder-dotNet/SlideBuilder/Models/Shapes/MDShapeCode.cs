namespace SlideBuilder.Models
{
    public class MDShapeCode : MDShapeText
    {
        private const string CODE_FORMAT = "`{0}`";

        public MDShapeCode(string line)
            : base(line)
        {
        }

        public bool IsMultiCode { get; set; }

        public override string ToString()
        {
            string result;

            if (this.IsMultiCode)
            {
                result = "";
            }
            else
            {
                result = string.Format(CODE_FORMAT, this.Line.ToString());
            }

            return result;
        }
    }
}
