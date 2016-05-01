using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;

namespace _06_WorkItems_Reading
{
    class Program
    {
        // the Base Uri used used the Project Collection one, in my case on a local development Server
        static readonly string TeamProjectCollectionUri = @"http://win2012:8080/tfs/DefaultCollection/";
        static readonly string ExistingProjectId = "dc68c474-2ce0-4be6-9617-abe97f66ec1e";

        static readonly int ExistingWorkItemId = 1; // This is a single Work Item .Id known to exist (on my system)
        static readonly List<int> ExistingWorkItemIds = new List<int>() { 1, 2, 3 ,4 };  // This is a list of Work Item .Id values known to exist (on my system)
        static readonly Guid ExistingStoredQueryId = new Guid("31902604-31bf-4b59-9df1-eea6e8b61093");

        static void Main(string[] args)
        {
            // instantiate a vss connection using the Project Collection URI as Base Uri
            var visualStudioServicesConnection = new VssConnection(new Uri(TeamProjectCollectionUri), new VssCredentials());

            // Get a Work Item Tracking client
            var workItemTrackingHttpClient = visualStudioServicesConnection.GetClient<WorkItemTrackingHttpClient>();


            // 1. Retrieve a single Work Item by its .Id
            // #############################

            // This will include Work Item Relations (work item links, test result associations etc) via the 'expand' parameter
            // However, if any no Relations exist at all .Relations will be 'null' (rather than an empty List<>)
            var workItemIncludingRelations = workItemTrackingHttpClient.GetWorkItemAsync(ExistingWorkItemId, expand: WorkItemExpand.Relations).Result;

            // same as not specifying an 'expand' parameter at all - .Relations is 'null'
            var workItemExcludingRelations = workItemTrackingHttpClient.GetWorkItemAsync(ExistingWorkItemId).Result;


            // 2. Retrieve multiple Work Items by their .Ids
            // #############################

            // You can also retrieve multiple work items in bulk via their .Id values in one request to the VSTS/TFS backend like this:
            var multipleWorkItemsRetrievedByTheirId = workItemTrackingHttpClient.GetWorkItemsAsync(ExistingWorkItemIds, expand: WorkItemExpand.All).Result;

            // 3. Query for Work Items

            // 3.1 .. using a Wiql (Work Item Query Language) Query
            var wiqlQuery = new Wiql() {Query = "Select * from WorkItems"};
            var workItemQueryResultForWiqlBasedQuery = workItemTrackingHttpClient.QueryByWiqlAsync(wiqlQuery).Result;

            var workItemsForQueryResultForWiqlBasedQuery = workItemTrackingHttpClient
                .GetWorkItemsAsync(
                    workItemQueryResultForWiqlBasedQuery.WorkItems.Select(workItemReference => workItemReference.Id),
                    expand: WorkItemExpand.All).Result;

            // 3.2 .. or by using a stored Query by its id
            var workItemQueryResultForStoredQuery = workItemTrackingHttpClient.QueryByIdAsync(ExistingStoredQueryId).Result;

            var workItemsForQueryResultForStoredQuery = workItemTrackingHttpClient
                .GetWorkItemsAsync(
                    workItemQueryResultForStoredQuery.WorkItems.Select(workItemReference => workItemReference.Id),
                    expand: WorkItemExpand.All).Result;

            // 4. Stored Queries

            var relationTypes = workItemTrackingHttpClient.GetRelationTypesAsync().Result;
        }
    }
}
