using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;

namespace _06_WorkItems_Reading
{
    class Program
    {
        // the Base Uri used used the Project Collection one, in my case on a local development Server
        const string TeamProjectCollectionUri = @"http://win2012:8080/tfs/DefaultCollection/";

        const int ExistingWorkItemId = 1; // This is the Work Item .Id which we will be retrieving

        static void Main(string[] args)
        {
            // instantiate a vss connection using the Project Collection URI as Base Uri
            var visualStudioServicesConnection = new VssConnection(new Uri(TeamProjectCollectionUri), new VssCredentials());

            // Get a Work Item Tracking client
            var workItemTrackingHttpClient = visualStudioServicesConnection.GetClient<WorkItemTrackingHttpClient>();

            // This will include Work Item Relations (if any Relations exist at all - otherwise & unfortunately .Relations will be 'null' (rather than an empty List<>)
            var workItemIncludingRelations = workItemTrackingHttpClient.GetWorkItemAsync(ExistingWorkItemId, expand: WorkItemExpand.Relations).Result;

            // but this won't
            var workItemExcludingRelations = workItemTrackingHttpClient.GetWorkItemAsync(ExistingWorkItemId).Result;



            var relationTypes = workItemTrackingHttpClient.GetRelationTypesAsync().Result;
        }
    }
}
