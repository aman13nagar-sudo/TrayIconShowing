using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TrayIconShowing
{
    public class LocalServer
    {
        private HttpListener _listener;
        private const string url = "http://localhost:5001/";

        public LocalServer()
        {
            _listener = new HttpListener();
            _listener.Prefixes.Add(url);
        }

        public async void Start()
        {
            _listener.Start();
            Console.WriteLine("Local server running...");

            while (true)
            {
                var context = await _listener.GetContextAsync();
                var request = context.Request;
                var response = context.Response;

                if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/chat")
                {
                    using var reader = new StreamReader(request.InputStream, request.ContentEncoding);
                    var requestBody = await reader.ReadToEndAsync();
                    var requestData = JsonSerializer.Deserialize<ChatRequest>(requestBody);

                    var aiResponse = await GetAiResponse(requestData.Message);

                    var responseData = new ChatResponse { Response = aiResponse };
                    var responseJson = JsonSerializer.Serialize(responseData);

                    byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
                    response.ContentLength64 = buffer.Length;
                    using var output = response.OutputStream;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                }

                response.Close();
            }
        }

        private async Task<string> GetAiResponse(string userMessage)
        {
            using var client = new HttpClient();
            var requestBody = JsonSerializer.Serialize(new { prompt = userMessage });
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:11434/api/generate", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var data = JsonSerializer.Deserialize<Phi4Response>(jsonResponse);

            return data?.Response ?? "Error fetching response";
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }

    public class ChatResponse
    {
        public string Response { get; set; }
    }

    public class Phi4Response
    {
        public string Response { get; set; }
    }
}

