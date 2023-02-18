#!/usr/bin/env bash

set -e

id
ls -la /app/ems
ls -la /app/ems/userdata

printf "\n\033[0;44m---> Starting HEMS \033[0m\n"
cd /app/ems
printf "\n\033[0;44m---> dotnet EMS.dll -- --config $EMS_PATHS_CONFIG --nlogcfg $EMS_PATHS_NLOG \033[0m\n"
dotnet EMS.dll -- --config $EMS_PATHS_CONFIG --nlogcfg $EMS_PATHS_NLOG

printf "\n\033[0;44m---> Are we done?\033[0m\n"
tail -f /dev/null
