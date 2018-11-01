# Integral time tracker - server side

Server side portion of the Integral time tracker. For now, It's purpose is to expose an API to push time entries, and an endpoint to generate PDF Invoices. Authentication is against Google, using your Organization account.

## Run Locally

Create `appsettings.Development.json` in the `TimeTracker.Api` folder. It should have same format as the `appsettings.json` that is in source control. Just replace GoogleConfig with your organizations parameters or something to test with.