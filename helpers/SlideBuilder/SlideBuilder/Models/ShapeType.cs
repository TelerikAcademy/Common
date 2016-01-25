namespace SlideBuilder.Models
{
    using System;

    [Flags]
    public enum ShapeType
    {
        None = 0,
        Title = 1,
        SecTitle = 2,
        Balloon = 4,
        Code = 8,
        MultiCode = 16,
    }
}
