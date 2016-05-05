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

            // Retrieve stored Queries (which you / your authenticated user can see and access), up to 2 (sub-)levels or -hierarchies deep.
            // .. it appears that (currently?) you can specify a max value of '2' for the 'depth' parameter which means you might need to 
            // retrieve queries deeper in the hierarchy using another approach:
            // > check the corresponding QueryHierarchyItem for its .HasChildren having a Value (.HasValue==true) and that Value being 'true' BUT
            // .. the .Children being 'null'. Go ahead and use that QueryHierarchyItem's .Path value for the .GetQueryAsync(projectId, queryHierarchyItemPath, ...) method
            // .. to drill down further into the hierarchy
            var allStoredQueriesAccessibleByAuthenticatedUser = workItemTrackingHttpClient.GetQueriesAsync(ExistingProjectId, expand: QueryExpand.All, depth: 2, includeDeleted: false).Result;

            // then go ahead and use these queries (this should in real code be placed in a proper method / set of method(s) obviously
            foreach (var storedQueryItem in allStoredQueriesAccessibleByAuthenticatedUser)
            {
                if (storedQueryItem.IsFolder.HasValue && storedQueryItem.IsFolder.Value == true)
                {
                    // this storedQueryItem is a Folder, it may have children.. or not
                    if (storedQueryItem.HasChildren.HasValue && storedQueryItem.HasChildren.Value == true)
                    {
                        if (storedQueryItem.Children != null)
                        {
                            foreach (var childQueryItem in storedQueryItem.Children)
                            {
                                // iterate over child items.. and so and and so on and so on
                            }
                        }
                        else
                        {
                            // this folder HAS children, but the deeper hierarchy hasn't been retrieved, yet
                            // > see note above how to do just that
                        }
                    }
                    else
                    {
                        // this query folder is empty
                    }
                }
                else if (storedQueryItem.IsFolder.HasValue && storedQueryItem.IsFolder.Value == false)
                {
                    // this storedQueryItem is a query (and not a folder)
                    // you can use it to run the query.. or access / modify etc its query statement
                    var resultsForStoredQuery = workItemTrackingHttpClient.QueryByIdAsync(storedQueryItem.Id).Result;
                }
                else
                {
                    // this 'should' not happen
                    throw new InvalidOperationException($"Well this is odd - QueryHierarchyItem '{storedQueryItem.Id}' is neither a folder, nor a query");
                }
            }
        }
    }
}
