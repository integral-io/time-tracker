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

to run locally, from terminal: `func host start`

The python script will need access to an environment variable containing the connection string to service bus.
To set the env variable do: `export SB_CONN_STR=Endpoint=sb://yourstuff`
