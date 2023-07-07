using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gmapseeker
{
    public class GoogleMapsAPI
    {
        private const string BaseUrl = "https://maps.googleapis.com/maps/api/place/textsearch/json";
        private const string DetailsUrl = "https://maps.googleapis.com/maps/api/place/details/json";
        private const string APIKey = "AIzaSyAwLYaTNneAmKlXX0WTUr35w5xkxXX1w20";

        public (List<Shop> shops, string nextPageToken) GetShops(string industry, string city, string country)
        {
            string url = $"{BaseUrl}?query={industry}+in+{city}+{country}&key={APIKey}";

            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine($"API Request URL: {url}");
                var response = client.GetAsync(url).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<GoogleMapsResponse>(content);
                    var shops = result.Results;
                    var nextPageToken = result.NextPageToken;
                    Console.WriteLine("Working!");

                    // Set PlaceId for each shop
                    foreach (var shop in shops)
                    {
                        shop.place_id = shop.place_id;
                    }

                    // Check if there is a next page token
                    if (!string.IsNullOrEmpty(result.NextPageToken))
                    {
                        // Fetch the next page of results using the next_page_token
                        (List<Shop> nextPageShops, string nextPageTokenResponse) = FetchNextPage(result.NextPageToken);
                        shops.AddRange(nextPageShops);
                        result.NextPageToken = nextPageTokenResponse;
                    }

                    // Retrieve shop details concurrently
                    Task.WhenAll(shops.Select(GetShopDetailsAsync)).Wait();

                    return (shops, nextPageToken);
                }
                else
                {
                    throw new Exception("Error occurred while calling the Google Maps API.");
                }
            }
        }

        public (List<Shop> shops, string nextPageToken) FetchNextPage(string nextPageToken)
        {
            string nextPageUrl = $"{BaseUrl}?pagetoken={nextPageToken}&key={APIKey}";

            using (HttpClient client = new HttpClient())
            {
                var response = client.GetAsync(nextPageUrl).Result;
                var content = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonConvert.DeserializeObject<GoogleMapsResponse>(content);
                    var shops = result.Results;
                    Console.WriteLine("Working!");

                    // Set PlaceId for each shop
                    foreach (var shop in shops)
                    {
                        shop.place_id = shop.place_id;
                    }

                    // Retrieve shop details concurrently
                    Task.WhenAll(shops.Select(GetShopDetailsAsync)).Wait();

                    return (result.Results, result.NextPageToken);
                }
                else
                {
                    throw new Exception("Error occurred while fetching the next page of results.");
                }
            }
        }

        private async Task GetShopDetailsAsync(Shop shop)
        {
            string url = $"{DetailsUrl}?place_id={shop.place_id}&key={APIKey}";
            Console.WriteLine($"API Details URL: {url}");

            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        var result = JsonConvert.DeserializeObject<GoogleMapsDetailsResponse>(content);
                        var details = result.Result;

                        if (details != null)
                        {
                            shop.address = details.FormattedAddress;
                            shop.international_phone = details.InternationalPhoneNumber;
                            shop.rating = details.Rating;
                            shop.website = details.Website;


                            if (!string.IsNullOrEmpty(details.Website))
                            {
                                try
                                {
                                    string email = await GetEmailAsync(details.Website);
                                    shop.email = email;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error occurred while extracting email: {ex.Message}");
                                    shop.email = "None";
                                }
                            }
                            else
                            {
                                shop.email = "None";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error occurred while parsing the details API response: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine($"Error occurred while calling the Google Places Details API. Status Code: {response.StatusCode}");
                }
            }
        }

        private async Task<string> GetEmailAsync(string url)
        {
            EmailExtractor emailExtractor = new EmailExtractor();
            string emailAddress = "";

            if (url != null)
            {
                try
                {
                    emailAddress = await emailExtractor.ExtractEmailFromWebsiteAsync(url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error occurred while extracting email: {ex.Message}");
                }
            }

            return emailAddress;
        }

    }

    public class GoogleMapsResponse
    {
        [JsonProperty("results")]
        public List<Shop> Results { get; set; }

        [JsonProperty("next_page_token")]
        public string NextPageToken { get; set; }
    }

    public class GoogleMapsDetailsResponse
    {
        [JsonProperty("result")]
        public ShopDetails Result { get; set; }
    }

    public class ShopDetails
    {
        [JsonProperty("formatted_phone_number")]
        public string FormattedPhoneNumber { get; set; }

        [JsonProperty("international_phone_number")]
        public string InternationalPhoneNumber { get; set; }

        [JsonProperty("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("user_ratings_total")]
        public int UserRatingsTotal { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}