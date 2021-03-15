#!/usr/bin/env bash
id
set -e

ls -la /app/ems
#dotnet --version
#dotnet --list-runtimes
#dotnet --info

printf "\n\033[0;44m---> Starting the EMS server.\033[0m\n"
dotnet /app/ems/ems.dll

printf "\n\033[0;44m---> Are we done?\033[0m\n"
tail -f /dev/null

exec "$@"
