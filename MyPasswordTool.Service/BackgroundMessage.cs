using Newtonsoft.Json.Linq;

namespace MyPasswordTool.Service
{
    public class BackgroundMessage
    {
        public JToken Message { get; set; }
        public string Token { get; set; }
    }
}