namespace SlideBuilder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.IO;

    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Presentation;

    public class Program
    {
        static void Main()
        {
            SlideConverter.ExtractPPTXtoMD(
                @"D:\Telerik Academy\12. DSA\SVN 2014\7. Recursion",
                "Data-Structures-and-Algorithms", "cs");
        }
    }
}
