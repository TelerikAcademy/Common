namespace SlideBuilder
{
    public class Program
    {
        static void Main()
        {
            SlideConverterV2.ExtractPPTXtoMD(
                @"D:\Materials\Telerik Academy\12. DSA\SVN 2014\7. Recursion", // Source folder
                "ASP.NET-Web-Forms", // GitHub repo name
                "cs", // Code language 
                @"D:\Materials\Telerik Academy\12. DSA\NewMDs\07. Recursion"); // destination folder
        }
    }
}
