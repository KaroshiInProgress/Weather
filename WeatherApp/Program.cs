using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

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

            // for dev
            //var apiKey = "910212c8b2a6d73f2988da96ed5a56d0";
            //var cityName = "Nantwich";
            //var countryCode = "UK";

            var httpResponseTask = GetWeatherData(apiKey, cityName, countryCode);
            httpResponseTask.Wait();

            if (httpResponseTask.Result.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Unable to contact weather API.");
                Environment.Exit(0);
            }

            var weatherDataTask = httpResponseTask.Result.Content.ReadAsStringAsync();
            weatherDataTask.Wait();

            Console.WriteLine($"ApiKey: {apiKey}");
            Console.WriteLine($"CityName: {cityName}");
            Console.WriteLine($"CountryCode: {countryCode}");
            Console.WriteLine($"Results: {weatherDataTask.Result}");
            Console.ReadKey();
        }

        private static async Task<HttpResponseMessage> GetWeatherData(string apiKey, string cityName, string countryCode)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("http://api.openweathermap.org/data/2.5/weather");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var queryString = $"?q={cityName},{countryCode}&APPID={apiKey}";
                return await client.GetAsync(queryString);
            }
        }
    }
}
