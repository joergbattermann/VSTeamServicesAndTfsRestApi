// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="Joerg Battermann">
//   Copyright (c) 2016 Joerg Battermann. All rights reserved.
// </copyright>
// <author>Joerg Battermann</author>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Operations;
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
                    var urlForTeamProject = ((ReferenceLink)teamProject.Links.Links["web"]).Href;

                    Console.WriteLine("Team Project '{0}' (Id: {1}) at Web Url: '{2}' & API Url: '{3}'",
                        teamProject.Name,
                        teamProject.Id,
                        urlForTeamProject,
                        teamProject.Url);
                }
            }

            // or if we already have the project collection url and the project's Id (Guid) upfront, we can access the later directly via:
            var knownProjectCollectionUrl = @"http://win2012:8080/tfs/DefaultCollection/";
            var projectVssConnectionForKnownProjectCollection = new VssConnection(new Uri(knownProjectCollectionUrl), new VssCredentials());
            var projectHttpClientForKnownProjectCollection = projectVssConnectionForKnownProjectCollection.GetClient<ProjectHttpClient>();
            var knownTeamProject = projectHttpClientForKnownProjectCollection.GetProject("dc68c474-2ce0-4be6-9617-abe97f66ec1e", includeCapabilities: true).Result;

            Console.WriteLine("Default Team is: '{0}' (Id: {1})", knownTeamProject.DefaultTeam.Name, knownTeamProject.DefaultTeam.Id);

            // check whether Project uses Git or TFS Version Control
            if (knownTeamProject.Capabilities.ContainsKey("versioncontrol") && knownTeamProject.Capabilities["versioncontrol"].ContainsKey("sourceControlType"))
            {
                Console.WriteLine("{0} is used!", knownTeamProject.Capabilities["versioncontrol"]["sourceControlType"]);
            }

            // 'processTemplate' is apparently only an existing capability IF you use the standard templates (Agile, Scrum, CMMI)
            // and /or your custom Process Template provides proper Process Configurations (which earlier versions of TFS did not have)
            if (knownTeamProject.Capabilities.ContainsKey("processTemplate"))
            {
                Console.WriteLine("Process Template '{0}' (Id: '{1}') is used!",
                    knownTeamProject.Capabilities["processTemplate"]["templateName"],
                    knownTeamProject.Capabilities["processTemplate"]["templateTypeId"]);
            }

            // We can also create new projects, i.e. like this:
            var newTeamProjectToCreate = new TeamProject();
            var somewhatRandomValueForProjectName = (int)(DateTime.UtcNow - DateTime.UtcNow.Date).TotalSeconds;

            // mandatory information is name,
            newTeamProjectToCreate.Name = $"Dummy Project {somewhatRandomValueForProjectName}";
            
            // .. description
            newTeamProjectToCreate.Description = $"This is a dummy project";

            // and capabilities need to be provided
            newTeamProjectToCreate.Capabilities = new Dictionary<string, Dictionary<string, string>>
            {
                {
                    // particularly which version control the project shall use (as of writing 'TFVC' and 'Git' are available
                    "versioncontrol", new Dictionary<string, string>()
                    {
                        {"sourceControlType", "TFVC"}
                    }
                },
                {
                    // and which Process Template to use
                    "processTemplate", new Dictionary<string, string>()
                    {
                        {"templateTypeId", "adcc42ab-9882-485e-a3ed-7678f01f66bc"} // This is the Id for the Agile template, on my TFS server at least.
                    }
                }
            };

            // because project creation takes some time on the server, the creation is queued and you'll get back a 
            // ticket / reference to the operation which you can use to track the progress and/or completion
            var projectCreationOperationReference = projectHttpClientForKnownProjectCollection.QueueCreateProject(newTeamProjectToCreate).Result;
            
            Console.WriteLine("Project '{0}' Creation is '{1}'", newTeamProjectToCreate.Name, projectCreationOperationReference.Status);

            // tracking the status via a OperationsHttpClient (for the Project collection)
            var operationsHttpClientForKnownProjectCollection = projectVssConnectionForKnownProjectCollection.GetClient<OperationsHttpClient>();

            var projectCreationOperation = operationsHttpClientForKnownProjectCollection.GetOperation(projectCreationOperationReference.Id).Result;
            while (projectCreationOperation.Status != OperationStatus.Succeeded
                && projectCreationOperation.Status != OperationStatus.Failed
                && projectCreationOperation.Status != OperationStatus.Cancelled)
            {
                Console.Write(".");
                Thread.Sleep(1000); // yuck

                projectCreationOperation = operationsHttpClientForKnownProjectCollection.GetOperation(projectCreationOperationReference.Id).Result;
            }

            // alright - creation is finished, successfully or not
            Console.WriteLine("Project '{0}' Creation finished with State '{1}' & Message: '{2}'",
                newTeamProjectToCreate.Name,
                projectCreationOperation.Status,
                projectCreationOperation.ResultMessage ?? "n.a.");

            // we can also update a teamProject
            // i.e. the just created one - which we need to retrieve first to get its .Id / Guid
            var allTeamProjectsOfProjectCollection = projectHttpClientForKnownProjectCollection
                .GetProjects(top: Int32.MaxValue, skip: 0).Result // don't do this in real code
                .ToList();

            var newTeamProjectReferenceForProjectJustCreated = allTeamProjectsOfProjectCollection
                .FirstOrDefault(projectReference => string.Equals(projectReference.Name, newTeamProjectToCreate.Name));
            var newTeamProjectJustCreated = projectHttpClientForKnownProjectCollection.GetProject(newTeamProjectReferenceForProjectJustCreated.Id.ToString()).Result;

            newTeamProjectJustCreated.Description = "Some updated Description";

            var projectUpdateOperationReference = projectHttpClientForKnownProjectCollection.UpdateProject(newTeamProjectJustCreated.Id, newTeamProjectJustCreated).Result; // .Id stays the same, even after updates

            Console.WriteLine("Project '{0}' Update is '{1}'", newTeamProjectToCreate.Name, projectUpdateOperationReference.Status);

            // and again, we track the queued deletion work / operation like before
            var projectUpdateOperation = operationsHttpClientForKnownProjectCollection.GetOperation(projectUpdateOperationReference.Id).Result;
            while (projectUpdateOperation.Status != OperationStatus.Succeeded
                && projectUpdateOperation.Status != OperationStatus.Failed
                && projectUpdateOperation.Status != OperationStatus.Cancelled)
            {
                Console.Write(".");
                Thread.Sleep(1000); // again, yuck

                projectUpdateOperation = operationsHttpClientForKnownProjectCollection.GetOperation(projectUpdateOperationReference.Id).Result;
            }

            Console.WriteLine("Project '{0}' Update finished with State '{1}' & Message: '{2}'",
                newTeamProjectJustCreated.Name,
                projectUpdateOperation.Status,
                projectUpdateOperation.ResultMessage ?? "n.a.");

            // aaand we can also delete projects

            // i.e. the new created / updated one
            var projectDeletionOperationReference = projectHttpClientForKnownProjectCollection.QueueDeleteProject(newTeamProjectJustCreated.Id).Result; // .Id stays the same, even after updates

            Console.WriteLine("Project '{0}' Deletion is '{1}'", newTeamProjectJustCreated.Name, projectCreationOperationReference.Status);

            // and again, we track the queued deletion work / operation like before
            var projectDeletionOperation = operationsHttpClientForKnownProjectCollection.GetOperation(projectDeletionOperationReference.Id).Result;
            while (projectDeletionOperation.Status != OperationStatus.Succeeded
                && projectDeletionOperation.Status != OperationStatus.Failed
                && projectDeletionOperation.Status != OperationStatus.Cancelled)
            {
                Console.Write(".");
                Thread.Sleep(1000); // again, yuck

                projectDeletionOperation = operationsHttpClientForKnownProjectCollection.GetOperation(projectDeletionOperationReference.Id).Result;
            }

            // alright - creation is finished, successfully or not
            Console.WriteLine("Project '{0}' Deletion finished with State '{1}' & Message: '{2}'",
                newTeamProjectJustCreated.Name,
                projectCreationOperation.Status,
                projectCreationOperation.ResultMessage ?? "n.a.");

            
        }
    }
}