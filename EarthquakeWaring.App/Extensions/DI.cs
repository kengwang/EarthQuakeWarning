using System;
using System.Windows.Markup;

namespace EarthquakeWaring.App.Extensions
{
    public class DI : MarkupExtension
    {
        public static IServiceProvider Services;

        public Type Type { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Services.GetService(Type)!;
        }
    }
}