# Time Tracker

Developer focused time track tool where users interact via slack to record their hours.

## Server side

Server side portion of the Integral time tracker. For now, It's purpose is to expose an API to push time entries, and an endpoint to generate PDF Invoices. Authentication is against Google, using your Organization account.

## high level architecture

[The worker](src/TimeTracker.Worker) provides the server-less functions-as-a-service code. The worker calls the common logic library which formulates a response. This response is sent to slack user using the asynchronous url.

[The library](src/TimeTracker.Library) is where all the common business logic lives and data is persisted.

[The API](src/TimeTracker.Api) provides access points to generate reports/invoices and test the business logic without the need for Slack. 

The reason to build solution this way was because slack requires a response either within 3 seconds OR using the async url. Working with the async url provides more reliability and better scaling, specially when trying to run on the cheapest available infrastructure (ie. free shared tier in Azure or others) where you can't guarantee "Always On" or dedicated availability.

## Local dev Setup
### Database setup on mac / linux

You'll need to run a local docker instance of ms sql server on your mac or linux box to be able to run 
the app locally. Here are detailed instructions on how to do that: https://medium.com/@jamesrf/local-sql-server-running-in-container-mac-linux-for-use-with-ef-e37c17308754
or just run:
```bash
sudo docker pull mcr.microsoft.com/mssql/server:2017-latest
docker run -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=YourStrong!Passw0rd' -p 1433:1433 --name sql2 -d mcr.microsoft.com/mssql/server:2017-latest
```
The database name, userid and password are configured in the app under the appsettings.json (Development one for local), shoudl already be set to above password.

Then, to update the database schema to latest version run (assuming path in the repo root):

```bash
dotnet ef database update --startup-project ./src/TimeTracker.Api --project ./src/TimeTracker.Data 
```
 Next, you will need to seed some data:
 - create a billing client
 - create a project
 - create a user with a real googleId so that the login works - copy from prod db
 TODO: automate this
 
### Setting google app setting secrets.

These are not checked in to source control in our appsettings but are instead managed via `dotnet` secrets
Run the following commands in a terminal in the `TimeTracker.Api` folder.

```bash

  dotnet user-secrets set "Authentication:Google:ClientId" "<Google Client ID>"
  dotnet user-secrets set "Authentication:Google:ClientSecret" "<Google Client Secret>"

```

This should be replaced with your organizations values.

## Testing
### Locally against Slack

You'll need to change the url to which Slack calls out in the config here:
https://api.slack.com/apps/{your-app-id}/slash-commands?

Then start a ngrok session locally that aims from the url to you local machine:
From the terminal: `# ngrok http 5000`
This will tunnel http port to local port 5000. If need https replace `http` for `https`.

Edit the `/hours` slack command and replace the url portion with what is generated by ngrok.

### Testing commands using local unit tests

Go to project TimeTracker.Api.Test and look at the:
[SlackSlashCommandControllerTest](test/TimeTracker.Api.Test/SlackSlashCommandControllerTest.cs)

These examples bring up a server and test end to end using the API.

## Useful links

API for Slash commands: https://api.slack.com/slash-commands
Our Apps: https://api.slack.com/apps (this is where we can find the integral time tracker setup)
Message Builder: https://api.slack.com/tools/block-kit-builder

## Issues to take into consideration
### Slack security

When a command comes in, we currently don't validate the message really being from slack, there is a way to do this: https://api.slack.com/docs/verifying-requests-from-slack

### PDF generation

For now just have a web url that creates the pretty invoice. User can then print to pdf from here.

Options we're looking at for pdf:

- https://www.nuget.org/packages/itext7 (Licensing makes unclear if can use with O/S project)
- https://www.nuget.org/packages/iTextSharp.LGPLv2.Core/ (port of last free version, but for non-Windows-based operating systems, you will need to install libgdiplus)
- https://www.nuget.org/packages/DinkToPdf (uses free C lib wkhtmltopdf, but must be copied locally)

