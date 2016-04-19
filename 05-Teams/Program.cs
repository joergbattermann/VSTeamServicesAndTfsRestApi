using System;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;

namespace _05_Teams
{
    public static class Program
    {
        // the Base Uri used used the Project Collection one, in my case on a local development Server
        const string TeamProjectCollectionUri = @"http://win2012:8080/tfs/DefaultCollection/";
        // Teams are per-project scoped, so work is done a particular project on my system
        const string ProjectId = "dc68c474-2ce0-4be6-9617-abe97f66ec1e";
        // newly created Teams do not contain any members, but this one (on my system) does:
        const string TeamIdKnownToContainMembers = "f2da8772-5461-47e5-8595-1b95bc6f69dd";

        static void Main(string[] args)
        {
            // instantiate a vss connection using the BaseUri (not the Project Collection Uri!)
            var visualStudioServicesConnection = new VssConnection(new Uri(TeamProjectCollectionUri), new VssCredentials());

            // Get a Team client
            var teamHttpClient = visualStudioServicesConnection.GetClient<TeamHttpClient>();

            // Retrieve existing Team(s)
            // ##############################
            // First lets retrieve the teams (first 10 again) for the given Project(Id)
            var allTeams = teamHttpClient.GetTeamsAsync(ProjectId, 10, 0).Result;
            foreach (var team in allTeams)
            {
                Console.WriteLine("Team '{0}' (Id: {1})", team.Name, team.Id);
            }

            // Create Team(s)
            // ##############################
            var somewhatRandomValueForTeamName = (int)(DateTime.UtcNow - DateTime.UtcNow.Date).TotalSeconds;
            
            // We can also create new Team(s), the minimum amount of information you have to provide is
            var newTeam = new WebApiTeam()
            {
                // .. only the .Name of the new Team
                // albeit it may NOT contain these characters: @ ~ ;  ' + = , < > | / \ ? : & $ * " # [ ]
                // but i.e. whitespaces are just fine
                Name = $"My new Team {somewhatRandomValueForTeamName}"
            };

            // once we've prepared the team
            var newlyCreatedTeam = teamHttpClient.CreateTeamAsync(newTeam, ProjectId).Result;

            Console.WriteLine("Team '{0}' (Id: {1}) created", newlyCreatedTeam.Name, newlyCreatedTeam.Id);

            // Retrieve Team Members
            // ##############################
            Console.WriteLine("Team with Id '{0}' contains the following member(s)", TeamIdKnownToContainMembers);
            foreach (var identityReference in teamHttpClient.GetTeamMembersAsync(ProjectId, TeamIdKnownToContainMembers, 10, 0) .Result)
            {
                Console.WriteLine("-- '{0}' (Id: {1})", identityReference.DisplayName, identityReference.Id);
            }
            
            // Delete Team(s)
            // ##############################
            // we can als delete existing Teams, i.e. the one we've just created
            teamHttpClient.DeleteTeamAsync(ProjectId, newlyCreatedTeam.Id.ToString()).Wait();
        }
    }
}
