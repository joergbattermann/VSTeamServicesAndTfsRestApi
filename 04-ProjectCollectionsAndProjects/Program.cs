// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// -----------------------------------------------------------------------

using System;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace _04_ProjectCollectionsAndProjects
{
    public class Program
    {
        const string BaseUri = @"http://win2012:8080/tfs/"; // In my case this is a locally running development TFS (2015 Update 2) system

        static void Main(string[] args)
        {
            // instantiate a vss connection using the BaseUri (not the Project Collection Uri!)
            var visualStudioServicesConnection = new VssConnection(new Uri(BaseUri), new VssCredentials());

            // get ahold of the Project Collection client
            var projectCollectionHttpClient = visualStudioServicesConnection.GetClient<ProjectCollectionHttpClient>();

            // iterate over the first 10 Project Collections (I am allowed to see)
            // however, if no parameter(s) were provided to the .GetProjectCollections() method, it would only retrieve one Collection,
            // so basically this allows / provides fine-grained pagination control
            foreach (var projectCollectionReference in projectCollectionHttpClient.GetProjectCollections(10, 0).Result)
            {
                // retrieve a reference to the actual project collection based on its (reference) .Id
                var projectCollection = projectCollectionHttpClient.GetProjectCollection(projectCollectionReference.Id.ToString()).Result;

                var webUrlForProjectCollection = projectCollection.Links.Links["web"] as ReferenceLink;

                Console.WriteLine("Project Collection '{0}' (Id: {1}) at Url: '{2}' & API Url: '{3}'",
                    projectCollection.Name,
                    projectCollection.Id,
                    webUrlForProjectCollection.Href,
                    projectCollection.Url);
            }
        }
    }
}