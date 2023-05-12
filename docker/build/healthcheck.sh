#!/usr/bin/env bash

# remove environment variable if it is empty anyway
if [[ -z "${ASPNETCORE_URLS}" ]]; then unset ASPNETCORE_URLS; fi

params=$(echo "$@" )
paramsStr=$( IFS=$'\n'; echo "${params}" )

cd /app/ems
printf "\n\033[0;44m---> dotnet EMS.dll -- %s \033[0m\n" "$paramsStr" "--healthcheck"
dotnet EMS.dll -- $params --healthcheck