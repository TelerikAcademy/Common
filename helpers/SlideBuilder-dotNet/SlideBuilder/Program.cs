namespace SlideBuilder
{
  public class Program
  {
    static void Main()
    {
      SlideConverterV2.ExtractPPTXtoMD(
          @"D:\TelerikRepos\HTML\02. Web-basics", // Source folder
          "CSharp-Fundamentals", // GitHub repo name
          "cs", // Code language 
          @"D:\TelerikRepos\HTML\02. Web-basics"); // destination folder
    }
  }
}
