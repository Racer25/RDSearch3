using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RateLimiterLib
{
    public class Request
    {
        public HttpWebRequest WebRequest { get; set; }

        public Action<HttpWebRequest> Todo { get; set; }

        public Request(HttpWebRequest request, Action<HttpWebRequest> todo)
        {
            WebRequest = request;
            Todo = todo;
        }
    }
}
