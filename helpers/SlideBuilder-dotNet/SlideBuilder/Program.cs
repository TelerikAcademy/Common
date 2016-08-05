namespace SlideBuilder
{
  public class Program
  {
    static void Main()
    {
      SlideConverterV2.ExtractPPTXtoMD(
          @"D:\TelerikRepos\High-Quality-Code-Part-1", // Source folder
          "High-Quality-Code-Part-1", // GitHub repo name
          "cs", // Code language 
          @"D:\TelerikRepos\High-Quality-Code-Part-1"); // destination folder
    }
  }
}
