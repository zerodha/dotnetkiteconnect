using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace KiteConnectTest
{

    class MockServer
    {
        readonly HttpListener httpListener = new();
        string contentType = "";
        string responseString = "";
        int statusCode = 200;

        public string LastRequestBody { get; private set; } = "";

        public MockServer(string url)
        {

            httpListener.Prefixes.Add(url);
            httpListener.Start();
            Task.Run(() => HandleRequest());
        }

        public void SetStatusCode(int code) {
            statusCode = code;
        }

        public void SetResponse(string contentType, string responseString)
        {
            this.contentType = contentType;
            this.responseString = responseString;
        }

        public void HandleRequest()
        {
            var context = httpListener.GetContext();

            using (var reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                LastRequestBody = reader.ReadToEnd();

            var response = context.Response;
            response.StatusCode = statusCode;
            response.ContentType = contentType;

            var buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.Close(buffer, true);
        }

        public void Stop()
        {
            httpListener.Stop();
        }
    }
}
