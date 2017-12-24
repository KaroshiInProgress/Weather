using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Linq;

namespace WeatherApp
{
    class Program
    {
        private static string[] ValidSections = { "weather", "wind", "main", "clouds" };
        private static string PreviousApiCallsLocation = "C:\\weather_app_api_calls.txt";

        static void Main(string[] args)
        {
            //if (args.Length != 3 || args.Length != 4)
            //{
            //    Console.WriteLine("You are missing some arguments.");
            //    Environment.Exit(0);
            //}

            //var apiKey = args[0];
            //var cityName = args[1];
            //var countryCode = args[2];
            //string section = null;

            //if (args.Length == 4)
            //{
            //    if (ValidSections.Contains(args[3].ToLower()))
            //    {
            //        section = args[3].ToLower();
            //    }
            //    else
            //    {
            //        Console.WriteLine("Invalid section.");
            //        Environment.Exit(0);
            //    }
            //}

            // for dev
            var apiKey = "910212c8b2a6d73f2988da96ed5a56d0";
            var cityName = "Nantwich";
            var countryCode = "UK";
            string section = "main";

            var httpResponseTask = GetWeatherData(apiKey, cityName, countryCode);
            httpResponseTask.Wait();

            if (httpResponseTask.Result.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Unable to contact weather API.");
                Environment.Exit(0);
            }

            var weatherDataTask = httpResponseTask.Result.Content.ReadAsStringAsync();
            weatherDataTask.Wait();
            
            if (!File.Exists(PreviousApiCallsLocation))
            {
                File.WriteAllText(PreviousApiCallsLocation, "[]");
            }

            var previousApiCallsFileContents = File.ReadAllText(PreviousApiCallsLocation);
            var previousApiCalls = JsonConvert.DeserializeObject<List<ApiCall>>(previousApiCallsFileContents);

            var previousCallsSameCity = new List<ApiCall>();
            foreach (var previousApiCall in previousApiCalls)
            {
                if (previousApiCall.CityName == cityName && previousApiCall.CountryCode == countryCode)
                {
                    previousCallsSameCity.Add(previousApiCall);
                }
            }
            previousCallsSameCity = previousCallsSameCity.OrderByDescending(apiCall => apiCall.TimeCalled).Take(3).ToList();

            var newApiCall = new ApiCall();
            newApiCall.ApiKey = apiKey;
            newApiCall.CityName = cityName;
            newApiCall.CountryCode = countryCode;
            newApiCall.Results = weatherDataTask.Result;
            newApiCall.TimeCalled = DateTime.Now;

            previousApiCalls.Add(newApiCall);
            File.WriteAllText(PreviousApiCallsLocation, JsonConvert.SerializeObject(previousApiCalls));

            Console.WriteLine($"ApiKey: {apiKey}");
            Console.WriteLine($"CityName: {cityName}");
            Console.WriteLine($"CountryCode: {countryCode}");
            if (section != null)
            {
                Console.WriteLine($"Section: {section}");
                Console.WriteLine($"Results: {GetSectionOnly(section, weatherDataTask.Result)}");
                Console.WriteLine("Previous results:");
                foreach (var previousCall in previousCallsSameCity)
                {
                    Console.WriteLine(GetSectionOnly(section, previousCall.Results));
                }
            }
            else
            {
                Console.WriteLine($"Results: {weatherDataTask.Result}");
                Console.WriteLine($"Previous results: {JsonConvert.SerializeObject(previousCallsSameCity)}");
            }
            Console.ReadKey();
        }

        private static string GetSectionOnly(string section, string weatherData)
        {
            if (section == null)
                return weatherData;

            dynamic weatherObject = JsonConvert.DeserializeObject<ExpandoObject>(weatherData);

            switch (section)
            {
                case "weather":
                    return JsonConvert.SerializeObject(weatherObject.weather);

                case "wind":
                    return JsonConvert.SerializeObject(weatherObject.wind);

                case "main":
                    return JsonConvert.SerializeObject(weatherObject.main);

                case "clouds":
                    return JsonConvert.SerializeObject(weatherObject.clouds);
            }

            return "";
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

    class ApiCall
    {
        public string ApiKey { get; set; }
        public string CityName { get; set; }
        public string CountryCode { get; set; }
        public DateTime TimeCalled { get; set; }
        public string Results { get; set; }
    }
}
