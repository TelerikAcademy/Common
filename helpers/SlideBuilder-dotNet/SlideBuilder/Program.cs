namespace SlideBuilder
{
  public class Program
  {
    static void Main()
    {
      SlideConverterV2.ExtractPPTXtoMD(
          @"D:\Dropbox\Telerik Academy\01. C# part 1\SVN 2015\Lectures\1. Introduction to Programming", // Source folder
          "CSharp-Part-1", // GitHub repo name
          "cs", // Code language 
          @"D:\Dropbox\Telerik Academy\01. C# part 1\NewGitHub\01. Introduction-to-Programming"); // destination folder
    }
  }
}
