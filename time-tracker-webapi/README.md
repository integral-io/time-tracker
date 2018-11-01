# Integral time tracker - server side

Server side portion of the Integral time tracker. For now, It's purpose is to expose an API to push time entries, and an endpoint to generate PDF Invoices. Authentication is against Google, using your Organization account.

## Run Locally

Run the following commands in a terminal in the `TimeTracker.Api` folder. 

```bash

  dotnet user-secrets set "Authentication:Google:ClientSecret" "secret"
  dotnet user-secrets set "Authentication:Google:ClientId" "integralId"

```

This should be replaced with your organizations values.

## PDF generation

Options we're looking at:

- https://www.nuget.org/packages/itext7 (Licensing makes unclear if can use with O/S project)
- https://www.nuget.org/packages/iTextSharp.LGPLv2.Core/ (port of last free version, but for non-Windows-based operating systems, you will need to install libgdiplus)
- https://www.nuget.org/packages/DinkToPdf (uses free C lib wkhtmltopdf, but must be copied locally)
