using Shared;

namespace RaftElection
{
    public class Gateway
    {
        private readonly List<string> nodes;
        private string leadersUrl;

        public HttpClient httpClient { get; set; }
        public Gateway(List<string> nodes, ILogger<Gateway> logger)
        {
            this.httpClient = new HttpClient();
            this.nodes = nodes;
        }
        public async Task GetLeaderAsync()
        {
            foreach (var node in nodes)
            {
                try
                {
                    var nodes = node.Trim('"');

                    Console.WriteLine(nodes);
                    var isLeader = await httpClient.GetFromJsonAsync<bool>($"{nodes}/Node/leader");
                    if (isLeader)
                    {
                        leadersUrl = nodes;
                        Console.WriteLine($"leader {leadersUrl}");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in getLeader async: {ex.Message}");

                }
            }

        }

        public async Task WriteAsync(string key, string value)
        {
            try
            {
                await GetLeaderAsync();

                var pair = new KeyValue
                {
                    key = key,
                    value = value
                };

                //send the value
                var response = await httpClient.PostAsJsonAsync($"{leadersUrl}/Node/write", pair);
                Console.WriteLine($"{response}");

            }
            catch (Exception ex)
            {
                Console.WriteLine(leadersUrl);
                Console.WriteLine($"Error in write async: {ex.Message}");
            }
            //make sure we have current leader
        }

        public async Task<(string?, int?)> EventualGetAsync(string value)
        {
            var url = nodes.FirstOrDefault();
            if (url != null)
            {
                return await httpClient.GetFromJsonAsync<(string?, int?)>($"{url}/Node/eventalGet/{value}");
            }
            else
                return (null, null);
        }

        public async Task<(string?, int?)> StrongGetAsync(string value)
        {
            return await httpClient.GetFromJsonAsync<(string?, int?)>($"{leadersUrl}/Node/strongGet/{value}");

        }

        public async Task<bool> CompareVersionAndSwapAsync(SwapInfo swap)
        {
            var response = await httpClient.PostAsJsonAsync($"{leadersUrl}/Node/compareandswap", swap);
            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }


    }


}

