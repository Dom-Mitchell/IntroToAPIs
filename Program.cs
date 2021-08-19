using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ConsoleTables;

namespace IntroToAPIs
{
    class Program
    {
        class Item
        {
            [JsonPropertyName("id")]
            public int Id { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; }

            [JsonPropertyName("complete")]
            public bool Complete { get; set; }

            [JsonPropertyName("created_at")]
            public DateTime CreatedAt { get; set; }

            [JsonPropertyName("updated_at")]
            public DateTime UpdatedAt { get; set; }

            public string CompletedStatus
            {
                get
                {
                    // Uses a ternary to return "completed" if the `complete` variable is true, returns "not completed" if false
                    return Complete ? "completed" : "not completed";
                }
            }
        }

        static async Task ShowAllItems(string token)
        {
            var client = new HttpClient();

            var url = $"https://one-list-api.herokuapp.com/items?access_token={token}";
            var responseAsStream = await client.GetStreamAsync(url);

            // Supply that *stream of data* to a Deserialize that will interpret it as a List of Item objects.
            var items = await JsonSerializer.DeserializeAsync<List<Item>>(responseAsStream);

            var table = new ConsoleTable("ID", "Description", "Created At", "Completed");

            // For each item in our deserialized List of Item
            foreach (var item in items)
            {
                // Add one row to our table
                table.AddRow(item.Id, item.Text, item.CreatedAt, item.CompletedStatus);
            }

            // Write the table
            table.Write(Format.Minimal);
        }

        static async Task GetOneItem(string token, int id)
        {
            try
            {
                var client = new HttpClient();

                // Generate a URL specifically referencing the endpoint for getting a single
                // todo item and provide the id we were supplied
                var url = $"https://one-list-api.herokuapp.com/items/{id}?access_token={token}";

                var responseAsStream = await client.GetStreamAsync(url);

                // Supply that *stream of data* to a Deserialize that will interpret it as a *SINGLE* `Item`
                var item = await JsonSerializer.DeserializeAsync<Item>(responseAsStream);

                var table = new ConsoleTable("ID", "Description", "Created At", "Updated At", "Completed");

                // Add one row to our table
                table.AddRow(item.Id, item.Text, item.CreatedAt, item.UpdatedAt, item.CompletedStatus);

                // Write the table
                table.Write(Format.Minimal);
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("I could not find that item!");
            }
        }

        static async Task AddOneItem(string token, Item newItem)
        {
            var client = new HttpClient();

            // Generate a URL specifically referencing the endpoint for getting a single
            // todo item and provide the id we were supplied
            var url = $"https://one-list-api.herokuapp.com/items?access_token={token}";

            // Take the `newItem` and serialize it into JSON
            var jsonBody = JsonSerializer.Serialize(newItem);

            // We turn this into a StringContent object and indicate we are using JSON
            // by ensuring there is a media type header of `application/json`
            var jsonBodyAsContent = new StringContent(jsonBody);
            jsonBodyAsContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Send the POST request to the URL and supply the JSON body
            var response = await client.PostAsync(url, jsonBodyAsContent);

            // Get the response as a stream.
            var responseJson = await response.Content.ReadAsStreamAsync();

            // Supply that *stream of data* to a Deserialize that will interpret it as a *SINGLE* `Item`
            var item = await JsonSerializer.DeserializeAsync<Item>(responseJson);

            // Make a table to output our new item.
            var table = new ConsoleTable("ID", "Description", "Created At", "Updated At", "Completed");

            // Add one row to our table
            table.AddRow(item.Id, item.Text, item.CreatedAt, item.UpdatedAt, item.CompletedStatus);

            // Write the table
            table.Write(Format.Minimal);
        }

        static async Task UpdateOneItem(string token, int id, Item updatedItem)
        {
            var client = new HttpClient();

            // Generate a URL specifically referencing the endpoint for getting a single
            // todo item and provide the id we were supplied
            var url = $"https://one-list-api.herokuapp.com/items/{id}?access_token={token}";

            // Take the `newItem` and serialize it into JSON
            var jsonBody = JsonSerializer.Serialize(updatedItem);

            // We turn this into a StringContent object and indicate we are using JSON
            // by ensuring there is a media type header of `application/json`
            var jsonBodyAsContent = new StringContent(jsonBody);
            jsonBodyAsContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            // Send the POST request to the URL and supply the JSON body
            var response = await client.PutAsync(url, jsonBodyAsContent);

            // Get the response as a stream.
            var responseJson = await response.Content.ReadAsStreamAsync();

            // Supply that *stream of data* to a Deserialize that will interpret it as a *SINGLE* `Item`
            var item = await JsonSerializer.DeserializeAsync<Item>(responseJson);

            // Make a table to output our new item.
            var table = new ConsoleTable("ID", "Description", "Created At", "Updated At", "Completed");

            // Add one row to our table
            table.AddRow(item.Id, item.Text, item.CreatedAt, item.UpdatedAt, item.CompletedStatus);

            // Write the table
            table.Write(Format.Minimal);
        }

        static async Task DeleteOneItem(string token, int id)
        {
            try
            {
                var client = new HttpClient();

                // Generate a URL specifically referencing the endpoint for getting a single
                // todo item and provide the id we were supplied
                var url = $"https://one-list-api.herokuapp.com/items/{id}?access_token={token}";

                var response = await client.DeleteAsync(url);

                // Get the response as a stream.
                await response.Content.ReadAsStreamAsync();
            }
            catch (HttpRequestException)
            {
                Console.WriteLine("I could not find that item!");
            }
        }

        static async Task Main(string[] args)
        {
            var token = "";

            if (args.Length == 0)
            {
                Console.Write("What list would you like? ");
                token = Console.ReadLine();
            }
            else
            {
                token = args[0];
            }

            var keepGoing = true;
            while (keepGoing)
            {
                Console.Clear();
                Console.Write("What would you like to do?\nGet (A)ll todos\nGet (O)ne todo\n(C)reate a new item\n(U)pdate an item\n(D)elete an item\n(Q)uit\n: ");
                var choice = Console.ReadLine().ToUpper();

                switch (choice)
                {
                    case "Q":
                        Console.WriteLine();
                        keepGoing = false;
                        break;

                    case "A":
                        Console.WriteLine();
                        await ShowAllItems(token);

                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                        break;

                    case "C":
                        Console.WriteLine("\nEnter the description of your new todo: ");
                        var text = Console.ReadLine();

                        Console.WriteLine();
                        var newItem = new Item
                        {
                            Text = text
                        };

                        await AddOneItem(token, newItem);

                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                        break;

                    case "O":
                        Console.WriteLine("\nEnter the ID of the item to show: ");
                        var id = int.Parse(Console.ReadLine());

                        Console.WriteLine();
                        await GetOneItem(token, id);

                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                        break;

                    case "U":
                        Console.WriteLine("\nEnter the ID of the item to update: ");
                        var existingId = int.Parse(Console.ReadLine());

                        Console.WriteLine("\nEnter the new description: ");
                        var newText = Console.ReadLine();

                        Console.WriteLine("\nEnter yes or no to indicate if the item is complete: ");
                        var newComplete = Console.ReadLine().ToLower() == "yes";

                        Console.WriteLine();
                        var updatedItem = new Item
                        {
                            Text = newText,
                            Complete = newComplete
                        };

                        await UpdateOneItem(token, existingId, updatedItem);

                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                        break;

                    case "D":
                        Console.WriteLine("\nEnter the ID of the item to delete: ");
                        var idToDelete = int.Parse(Console.ReadLine());

                        Console.WriteLine();
                        await DeleteOneItem(token, idToDelete);

                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                        break;

                    default:
                        break;
                }
            }

        }
    }
}