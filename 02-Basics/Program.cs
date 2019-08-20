// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// -----------------------------------------------------------------------

using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace _02_Basics
{
    public static class Program
    {
        const string TeamProjectCollectionUri = @"http://win2012:8080/tfs/DefaultCollection"; // In my case this is a locally running development TFS (2015 Update 2) system
        const int WorkItemIdentifier = 1; // This is the Work Item .Id which we will be retrieving

        static void Main(string[] args)
        {
            var visualStudioServicesConnection = new VssConnection(new Uri(TeamProjectCollectionUri), new VssCredentials());
            var workItemTrackingHttpClient = visualStudioServicesConnection.GetClient<WorkItemTrackingHttpClient>();
            var workItemInstance = workItemTrackingHttpClient.GetWorkItemAsync(1).Result;

            Console.WriteLine("Work Item {0} - '{1}' retrieved", workItemInstance.Id, workItemInstance.Fields["System.Title"]);
        }
    }
}