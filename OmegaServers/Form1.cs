using System.Net;
using System;
using Firebase.Database;
using Firebase.Database.Query;
using System.Collections.Generic;
using System.Threading;

namespace OmegaServers
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public String ServerName = "";
        public ServersService service = new ServersService();
        private async void Form1_Load(object sender, EventArgs e)
        {
            var st =new StreamReader("ServerName.dll");
            ServerName = st.ReadLine();
            st.Close();
            st.Dispose();
             
            DoUpdate();
             

        } 
        public async Task DoUpdate()
        {
            Text =await service.GetServerIP(this.ServerName);

            while (true)
            {

                string ip = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
                if (ip == Text)
                {
                    await Task.Delay(1000);
                    continue;   
                }

                var server = new Server
                {
                    ServerName = this.ServerName,
                    IP = ip
                };
                await service.UpdateServer(server.ServerName, server);
            }
        }
        

    }
}

public class Server
{
    public string? ServerName { get; set; }
    public string? IP { get; set; }
}

public class ServersService
{
    private const string FirebaseDatabaseUrl = "https://omegaservers-7d969-default-rtdb.firebaseio.com/"; // XXXXXX should be replaced with your instance name
    private readonly FirebaseClient firebaseClient;

    public ServersService()
    {
        firebaseClient = new FirebaseClient(FirebaseDatabaseUrl);
    }

    public async Task<List<KeyValuePair<string, Server>>?> GetServers()
    {
        var servers = await firebaseClient
          .Child("servers")
          .OnceAsync<Server>();

        return servers?
          .Select(x => new KeyValuePair<string, Server>(x.Key, x.Object))
          .ToList();
    }

    public async Task<String> GetServerIP(String Servername)
    {
        var server = await firebaseClient
          .Child("server")
          .OrderByKey()
          .OnceAsync<Server>();

        var myServer=server.Where(x=>x.Key==Servername).FirstOrDefault();

        if (myServer == null)
            return "";
        return myServer.Object.IP;


    }

    public async Task AddServer(Server server)
    {
        await firebaseClient
          .Child("servers")
          .PostAsync(server);
    }

    public async Task UpdateServer(string id, Server server)
    {
         await firebaseClient
          .Child("server")
          .Child(id)
          .PutAsync(server);
    }
}