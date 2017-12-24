using System;

namespace WeatherApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("You are missing some arguments.");
                Environment.Exit(0);
            }

            var apiKey = args[0];
            var cityName = args[1];
            var countryCode = args[2];

            Console.WriteLine($"ApiKey: {apiKey}");
            Console.WriteLine($"CityName: {cityName}");
            Console.WriteLine($"CountryCode: {countryCode}");
            Console.ReadKey();
        }
    }
}
