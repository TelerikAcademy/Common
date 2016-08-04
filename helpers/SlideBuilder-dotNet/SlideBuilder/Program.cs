namespace SlideBuilder
{
  public class Program
  {
    static void Main()
    {
      SlideConverterV2.ExtractPPTXtoMD(
          @"D:\TelerikRepos\High-Quality-Code-Part-1\00. Course-Intro", // Source folder
          "High-Quality-Code-Part-1", // GitHub repo name
          "cs", // Code language 
          @"D:\TelerikRepos\High-Quality-Code-Part-1\00. Course-Intro"); // destination folder
    }
  }
}
