#!/usr/bin/env bash
id
uname -a
 
printf "\n\033[0;44m---> Starting HEMS \033[0m\n"

# remove environment variable if it is empty anyway
if [[ -z "${ASPNETCORE_URLS}" ]]; then unset ASPNETCORE_URLS; fi

#params=$(echo "$@" "--nlogdebug true")
params=$(echo "$@" )
paramsStr=$( IFS=$'\n'; echo "${params}" )

cd /app/ems
printf "\n\033[0;44m---> dotnet EMS.dll -- %s \033[0m\n" "$paramsStr"
dotnet EMS.dll -- $params

printf "\n\033[0;44m---> Done\033[0m\n"
