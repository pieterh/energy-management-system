<!-- PROJECT SHIELDS -->
<!--
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![License][license-shield]][license-url]
[![FOSSA Status][fossa-shield]][fossa-url]
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]

![Build status][build-shield]

[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=ncloc)](https://sonarcloud.io/dashboard?id=pieterh_energy-management-system)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=bugs)](https://sonarcloud.io/dashboard?id=pieterh_energy-management-system)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=pieterh_energy-management-system)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=code_smells)](https://sonarcloud.io/dashboard?id=pieterh_energy-management-system)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=duplicated_lines_density)](https://sonarcloud.io/dashboard?id=pieterh_energy-management-system)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=sqale_index)](https://sonarcloud.io/dashboard?id=pieterh_energy-management-system)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=coverage)](https://sonarcloud.io/dashboard?id=pieterh_energy-management-system)

# HEMS
HEMS is a Home Energy Management System that lets you monitor, configure and automate energy management for various devices like: Smart meter, Charge point/EVSE, Solar Panels.

## Supported hardware

*Currently supported hardware:*
| Type | Model |
| ----------- | --- |
| Charge Point | Alfen NG9xx (Eve Single S-line/Eve Single Pro-line/Eve Double Pro-line) |
| Smart meter |  P1 v5 |
| Solar Panels | Enphase Envoy Gateway |

## Docker
Docker containers with  HEMS (beta/feature) builds.

*Currently available for the following platforms:*
| Operating system | Architecture |
| ----------- | ----------- |
| Linux | amd64 |
| Linux | arm |
| Linux | arm64 |

Docker repository: [https://hub.docker.com/repository/docker/pieterhil/energy-management-system](https://hub.docker.com/repository/docker/pieterhil/energy-management-system)


## How to use
The recommended method is to use Docker compose (See below). For instructions how to install docker desktop see https://docs.docker.com/desktop/ and  docker engine (server) see https://docs.docker.com/engine/install/.




### Docker compose
```
mkdir hems
cd hems
mkdir config
```

create config.json and a NLog.config in the 'config' subfolder. You can find examples of these in this repository folder docker/tests/config/


*docker-compose.yml*

```
version: '3.8'

services:
  hems:
    image: pieterhil/energy-management-system:latest
    container_name: hems
    user: 1001:123
    restart: unless-stopped
    ports:
      - "8080:5000"
    volumes:
      - ./config:/app/ems/userdata
    environment:
      - TZ=Europe/Amsterdam
      - EMS_PATHS_CONFIG=/app/ems/userdata/config.json
      - EMS_PATHS_NLOG=/app/ems/userdata/NLog.config
```

You can use the following two options to start the container:
```
docker-compose up -d
```
or
```
docker compose up -d
```

#### Updating the docker image
```
docker-compose pull pieterhil/energy-management-system:latest
docker-compose down
docker-compose up -d --remove-orphans
docker image prune
```

### Docker

Get the latest image from the repository:
```
docker pull pieterhil/energy-management-system:latest
```

Remove any old containers from the system that where left over from a previous run.
```
docker container rm hems
```

```
docker run \
    -p 8080:5000 \
    --mount type=bind,source="$(pwd)"/config,destination=/app/ems/userdata \
    --user 1001:123
    -e TZ=Europe/Amsterdam \
    -e EMS_PATHS_CONFIG=/app/ems/userdata/config.json \
    -e EMS_PATHS_NLOG=/app/ems/userdata/NLog.config \
    --name=hems \
    pieterhil/energy-management-system:latest
```

### Write access to local storage

The application does as user 'app' inside the docker container. For some systems it is mandatory that this user has write access to the volume so that logfiles and the databse can be saved. Some operating systems this is nod needed. For instance macOS is able to write in the mounted volume. It is recommended always to supply a user and group id that has write access to the local storage. You can do this in the docker-compose.yml file, or
Some background information and other strategies for handling this issue can be found at the following blog post:
[Docker and the Host Filesystem Owner Matching Problem](https://www.fullstaq.com/knowledge-hub/blogs/docker-and-the-host-filesystem-owner-matching-problem) and can also be found at 
[The Internet Archive WayBackMachine](https://web.archive.org/web/20221204121741/https://www.fullstaq.com/knowledge-hub/blogs/docker-and-the-host-filesystem-owner-matching-problem)

### Frontend
The frontend is accessible using the browser http://127.0.0.1:8080 if you are running the docker on you own local system.

## Developing and Contributing
We'd love to get contributions from you! Take a look at the
[`Contribution Documents`](https://github.com/pieterh/energy-management-system/blob/main/CONTRIBUTING.md) to see how to setup a development environment and to get your changes merged in.

<!-- LICENSE -->
## License

Distributed under the BSD 3-Clause license. See [`LICENSE`](https://github.com/pieterh/energy-management-system/blob/main/LICENSE.md) for more information.

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/pieterh/energy-management-system.svg?style=flat-square
[contributors-url]: https://github.com/pieterh/energy-management-system/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/pieterh/energy-management-system.svg?style=flat-square
[forks-url]: https://github.com/pieterh/energy-management-system/network/members
[stars-shield]: https://img.shields.io/github/stars/pieterh/energy-management-system.svg?style=flat-square
[stars-url]: https://github.com/pieterh/energy-management-system/stargazers
[issues-shield]: https://img.shields.io/github/issues/pieterh/energy-management-system.svg?style=flat-square
[issues-url]: https://github.com/pieterh/energy-management-system/issues
[license-shield]: https://img.shields.io/github/license/pieterh/energy-management-system.svg?style=flat-square
[license-url]: https://github.com/pieterh/energy-management-system/blob/main/LICENSE.md
[fossa-shield]: https://app.fossa.com/api/projects/git%2Bgithub.com%2Fpieterh%2Fenergy-management-system.svg?type=shield
[fossa-url]: https://app.fossa.com/projects/git%2Bgithub.com%2Fpieterh%2Fenergy-management-system?ref=badge_shield
[build-shield]: https://github.com/pieterh/energy-management-system/actions/workflows/build.yml/badge.svg
