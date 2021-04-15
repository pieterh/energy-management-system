#!/usr/bin/env bash
#id
set -e

#ls -la /app/ems
#dotnet --version
#dotnet --list-runtimes
#dotnet --info

printf "\n\033[0;44m---> Starting HEMS \033[0m\n"
dotnet /app/ems/EMS.dll -- --config $EMS_PATHS_CONFIG --nlogcfg $EMS_PATHS_NLOG

printf "\n\033[0;44m---> Are we done?\033[0m\n"
tail -f /dev/null
