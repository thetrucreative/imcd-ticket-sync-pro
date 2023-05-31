using Newtonsoft.Json;
using imcd_ticket_sync;
using Nest;
using Elasticsearch.Net;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

public class Program
{
    //Display Topdesk Incidents
    public static async Task Main(string[] args)
    {
        try
        {
            while (true)
            {
                //await ProcessDataAsync();
                //await Task.Delay(5000);
                CreateHostBuilder(args).Build().Run();
            }
        }
        catch (Exception ex)
        {
            //Console.WriteLine(ex.Message);
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
    }

    //static async Task ProcessDataAsync()
    public static async Task<List<string>> ProcessDataAsync()
    {
        var listResults = new List<string>();   
        try
        {
            var incidentList = await GetIncidentsAsync();
            //var incidentList = JsonConvert.DeserializeObject<List<Incident>>(responseContent);
            listResults.Add($"Incidents checked: {incidentList.Count}");
            //listResults.Add(new Incident { Description = "$Incidents checked: {incidentList.Count}" });

            //filter incidents by Group operator name
            string operatorGroupName = "AAGRP-Integration Services";
            var filteredIncidents = incidentList.Where(Incident => Incident?.OperatorGroup?.Name == operatorGroupName).ToList();

            //display count of tickets in integration group
            listResults.Add($"Incidents for '{operatorGroupName}': {filteredIncidents.Count}\n");
            //listResults.Add(new Incident { Description = "$Incidents for '{operatorGroupName}': {filteredIncidents.Count}\\n\"" });

            //connect & send the logs to opensearch
            //await ConnectToOpenSearchAsync(filteredIncidents);
            var logList = await ConnectToOpenSearchAsync(filteredIncidents);
            listResults.AddRange(logList);

            //display ticket details and what was logged to Opensearch
            listResults.Add("TICKET DETAILS:");

            //Loop through all incidents > display chosen columns
            for (int i = 0; i < filteredIncidents.Count; i++)
            {
                var incident = filteredIncidents[i];
                listResults.Add($"Topdesk case number: {incident.Number}");
                //listResults.Add($"Operator group: {incident.OperatorGroup?.Name}");
                listResults.Add($"Status: {incident.ProcessingStatus?.Name}");
                listResults.Add($"Description: {incident.BriefDescription}");
            }
            if (filteredIncidents.Count == 0)
            {
                listResults.Add("No new tickets found!\n");
            }
            //Console.ReadLine();
            //Thread.Sleep(5000);
           // Console.Clear();
        }
        catch (Exception ex)
        {
            listResults.Add(ex.Message);
        }
        return listResults;
    }

    //GET Topdesk incidents asyncronously
    private static async Task<List<Incident>> GetIncidentsAsync()
    {
        var client = new HttpClient();

        /*START: retrieve incidents that have a creation date >= the start of the year*/
        var url = $"https://imcd.topdesk.net/tas/api/incidents?pageSize=100";
        var request = new HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
        /*END*/

        request.Headers.Add("Authorization", "Basic YOUR_BASE64_AUTHORIZATION_KEY");
        var response = await client.SendAsync(request);

        //success response passed to Main as parameter
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var incidentList = JsonConvert.DeserializeObject<List<Incident>>(responseContent);

            string operatorGroupName = "AAGRP-Integration Services";
            var filteredIncidents = incidentList.Where(incident => incident?.OperatorGroup?.Name == operatorGroupName).ToList();
            return filteredIncidents;
        }
        else
        {
            return null;
        }
    }

    //PUT data to openSearch 
    //use the Nest library to connect to OpenSearch and index the data
    static async Task<List<string>> ConnectToOpenSearchAsync(List<Incident> filteredIncidents)
    {
        var logList = new List<string>();   
        //initiate connection to opensearch
        var connectionSettings = new ConnectionSettings(new SniffingConnectionPool(new[]
        {
                new Uri("https://vpc-imcd-non-prod-os-4dnsyy2e7wnebm4mtxq5jezpke.eu-central-1.es.amazonaws.com/")
            }))
            .DefaultIndex("your_topdesk-index")
            .BasicAuthentication("topdesk_username", "topdesk_password");//change Auth if needed for prod
        var client = new ElasticClient(connectionSettings);

        //iterate over each incident & create an index request
        for (int i = 0; i < filteredIncidents.Count; i++)
        {
            logList.Add("Connecting to OpenSearch...");
            var incident = filteredIncidents[i];
            var indexRequest = new IndexRequest<Incident>(incident);

            /*Optimize this code LATER*/
            var indexResponse = client.Index(indexRequest, idx => idx
            .Index("my_topdesk_index")
            .Id(incident.Number.ToString()));

            if (indexResponse.IsValid)
            {
                logList.Add($"Indexed to OpenSearch: {incident.Number}");
            }
            else
            {
                try
                {
                    logList.Add($"Unable to index file(s) to OpenSearch: {incident.Number}");
                }
                catch (Exception ex)
                {
                    logList.Add(ex.Message);
                }
            }
        }
        return logList;
    }
}