using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace COServer
{
    public class Discord
    {
        string API = "";
        Queue<string> Msgs;
        Uri webhook;

        public Discord(string API)
        {
            this.API = API;
            Msgs = new Queue<string>();
            webhook = new Uri(API);
            Console.WriteLine("Discord Server Ready.");
            var thread = new Thread(Dequeue);
            thread.Start();
        }
        private void Dequeue()
        {
            
            while (true)
            {
                try
                {
                    while (Msgs.Count != 0)
                    {
                        var msg = Msgs.Dequeue();
                        postToDiscord(msg);
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
        public void Enqueue(string str)
        {
            Msgs.Enqueue(/*$"[{DateTime.Now.ToString()}]: */$"{str}");
        }
        private void postToDiscord(string Text)
        {
            HttpClient client = new HttpClient();

            Dictionary<string, string> discordToPost = new Dictionary<string, string>();
            discordToPost.Add("content", Text);

            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                       | SecurityProtocolType.Tls11
                       | SecurityProtocolType.Tls12
                       | SecurityProtocolType.Ssl3;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://discord.com/api/webhooks/1278873234552389667/_H87R46AocADp1yrdxPNgxfOppibkBuxinlF25D714GZG96Jue_9gkMUvZ0clIFNowLh");

                var content = new FormUrlEncodedContent(discordToPost);

                var res = client.PostAsync(webhook, content).Result;
                //If you want to check result value
                if (res.IsSuccessStatusCode)
                {
                    //Console.WriteLine("ent {Text}!");
                }
            }
        }

        internal void Enqueue(string v1, object the, object game, bool v2, object you, object can, object login, object p)
        {
            throw new NotImplementedException();
        }
    }
}
