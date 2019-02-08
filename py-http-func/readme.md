# Running locally

to get this to work you will need python version 3.6.0 specifically.

Best way is to install `pyenv` with brew install pyenv

then you can do `pyenv install 3.6.0`

then to switch the version do `pyenv versions` to list them all
to set the version: `pyenv global 3.6.0`
then to verify: `pyenv version` or `python --version`

You'll need to activate an environment in 3.6 once you have set to run version 3.6.0 like so in bash:
```bash
python -m venv .env
source .env/bin/activate
```
FOr more about pyenv: https://github.com/pyenv/pyenv

Once you have version 3.6.0 active, and you have activated the virtual env using above bash commands:
to run locally, from terminal: `func host start`

The python script will need access to an environment variable containing the connection string to service bus.
To set the env variable do:
Note wrapping ' around connection string

```bash
export SB_CONN_STR='Endpoint=sb://yourstuff'
```

^^You must run above command in the python `venv` after activating it.

http requests locally will go to: `http://localhost:7071/api/HttpSlackSlashCommand` so therefore slack slash-command will need to be set to point to `/api/HttpSlackSlashCommand`
