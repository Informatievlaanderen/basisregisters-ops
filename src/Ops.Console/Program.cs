namespace Ops.Console
{
    using System.Globalization;
    using System.Threading.Tasks;
    using CsvHelper;
    using CsvHelper.Configuration;

    public sealed class Program
    {
        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = ";",

            };

            var csv = new CsvReader("input.csv");

        }
    }
}
