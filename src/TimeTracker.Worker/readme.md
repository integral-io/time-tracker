# To run locally:

from the TimeTracker.Worker folder:

```bash
func host start -p 7075
```

^^Note that we pass in port number of `7075` so that it does not conflict with python function that runs on default `7071`

### Requirements:
have a local.settings.json file in root of TimeTracker.Worker directory. Format of this file is:
```json
{
    "IsEncrypted": false,
    "Values": {
        "itt-commands-ServiceBus": "Endpoint=sb://yourstuff",
        "AzureWebJobsStorage": "DefaultEndpointsProtocol=https;AccountName=iyourstuff",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet"
    },
    "ConnectionStrings": {
        "DefaultConnection": "Server=tcp:127.0.0.1,1433;Initial Catalog=time_tracker;Persist Security Info=False;User ID=SA;Password=YourStrong!Passw0rd;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=False;Connection Timeout=30;"
    }
}
```