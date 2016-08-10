namespace SlideBuilder
{
  public class Program
  {
    static void Main()
    {
      SlideConverterV2.ExtractPPTXtoMD(
          @"D:\Dropbox\Telerik Academy\07. JavaScript UI\SVN 2015\Old pptx\HTML-Templates", // Source folder
          "JavaScript-OOP", // GitHub repo name
          "javascript", // Code language 
          @"D:\Dropbox\Telerik Academy\07. JavaScript UI\SVN 2015\Old pptx\HTML-Templates"); // destination folder

      //SlideConverterV2.ExtractWebPageToMD(
      //    @"https://www.typescriptlang.org/docs/handbook/", // Source folder
      //    new string[] { "basic-types", "variable-declarations", "interfaces", "classes", "functions", "generics", "enums", "type-inference", "type-compatibility", "advanced-types", "symbols", "iterators-and-generators", "modules", "namespaces", "namespaces-and-modules", "module-resolution", "declaration-merging", "writing-declaration-files", "jsx", "decorators", "mixins", "triple-slash-directives" },
      //    "JavaScript-OOP", // GitHub repo name
      //    "javascript", // Code language 
      //    @"D:\Dropbox\Telerik Academy\08. TypeScript OOP\Topics"); // destination folder
    }
  }
}
