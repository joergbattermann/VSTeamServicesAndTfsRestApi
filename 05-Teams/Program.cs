using System;
using JB.Common.TeamFoundationServer.Client.ExtensionMethods;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace _05_Teams
{
    public static class Program
    {
        const string BaseUri = @"http://win2012:8080/tfs/"; // In my case this is a locally running development TFS (2015 Update 2) system

        static void Main(string[] args)
        {
            // instantiate a vss connection using the BaseUri (not the Project Collection Uri!)
            var visualStudioServicesConnection = new VssConnection(new Uri(BaseUri), new VssCredentials());
        }
    }
}
