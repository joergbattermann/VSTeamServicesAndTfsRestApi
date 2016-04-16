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

                // the 'web' Url is the one for the PC itself, the API endpoint one is different, see below
                var urlForProjectCollection = ((ReferenceLink)projectCollection.Links.Links["web"]).Href;

                Console.WriteLine("Project Collection '{0}' (Id: {1}) at Web Url: '{2}' & API Url: '{3}'",
                    projectCollection.Name,
                    projectCollection.Id,
                    urlForProjectCollection,
                    projectCollection.Url);

                // Iterate down into the Project Collection's Projects

                // first we need to create a new connection based on the current Projec Collections 'web' Url
                var projectVssConnection = new VssConnection(new Uri(urlForProjectCollection), new VssCredentials());

                // and retrieve the corresponding project client 
                var projectHttpClient = projectVssConnection.GetClient<ProjectHttpClient>();

                // then - same as above.. iterate over the project references (with a hard-coded pagination of the first 10 entries only)
                foreach (var projectReference in projectHttpClient.GetProjects(top: 10, skip: 0).Result)
                {
                    // and then get ahold of the actual project
                    var teamProject = projectHttpClient.GetProject(projectReference.Id.ToString()).Result;
                }
            }

            // or if we already have the project collection url and the project's Id (Guid) upfront, we can access the later directly via:
            var knownProjectCollectionUrl = @"http://win2012:8080/tfs/DefaultCollection/";
            var projectVssConnectionForKnownProjectCollection = new VssConnection(new Uri(knownProjectCollectionUrl), new VssCredentials());
            var projectHttpClientForKnownProjectCollection = projectVssConnectionForKnownProjectCollection.GetClient<ProjectHttpClient>();
            var knownTeamProject = projectHttpClientForKnownProjectCollection.GetProject("dc68c474-2ce0-4be6-9617-abe97f66ec1e").Result;

        }
    }
}