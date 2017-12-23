using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RateLimiterLib
{
    public class RateLimiter
    {

        private Queue<Request> requestsQueue;

        private int pauseTime;

        private bool stop;

        public RateLimiter(int rateLimitPerSecond)
        {
            requestsQueue = new Queue<Request>();
            pauseTime = (int)TimeSpan.FromSeconds(1.0 / (double) rateLimitPerSecond).TotalMilliseconds;
            stop = false;
        }

        public void AddRequest(Request request)
        {
            requestsQueue.Enqueue(request);
        }

        public void LaunchRequests()
        {
            Thread myThread = new Thread(new ThreadStart(Loop));

            // Lancement du thread
            myThread.Start();
        }

        public void Loop()
        {
            while(!stop)
            {
                if(requestsQueue.Count != 0)
                {
                    Request request = requestsQueue.Dequeue();
                    if(request != null)
                    {
                        request.Todo(request.WebRequest);
                    }

                }
                Thread.Sleep(pauseTime);
            }
        }

        public void Stop()
        {
            stop = true;
        }



    }
}
