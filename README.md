<!-- PROJECT SHIELDS -->
<!--
*** https://www.markdownguide.org/basic-syntax/#reference-style-links
-->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]

[![License][license-shield]][license-url]
[![FOSSA Status][fossa-shield]][fossa-url]

![Build status][build-shield]

[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=energy-management-system&metric=ncloc)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=energy-management-system&metric=bugs)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=energy-management-system&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=energy-management-system&metric=code_smells)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=energy-management-system&metric=duplicated_lines_density)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=energy-management-system&metric=sqale_index)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=energy-management-system&metric=coverage)](https://sonarcloud.io/dashboard?id=energy-management-system)

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
mkdir /hems
cd /hems
```

*docker-compose.yml*

```
version: '3.8'

services:
  hems:
    image: pieterhil/energy-management-system:latest
    container_name: hems
    restart: unless-stopped
    ports:
      - "8080:8080"
    volumes:
      - ./config:/app/ems/userdata
    environment:
      - TZ=Europe/Amsterdam
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

Get the image from the repository:
```
docker pull pieterhil/energy-management-system:latest
```

```
docker run -d \
    -p 8080:8080 \
    -p 8443:443 \
    -v <path for config files>:/app/ems/userdata \
    -e TZ=Europe/Amsterdam
    --device=<device_id> \
    --name=hems \ 
    pieterhil/energy-management-system:latest
```

## Developing and Contributing
We'd love to get contributions from you! Take a look at the
[`Contribution Documents`](https://github.com/pieterh/energy-management-system/blob/main/CONTRIBUTING.md) to see how to setup a development environment and to get your changes merged in.

<!-- LICENSE -->
## License

Distributed under the BSD 3-Clause license. See [`LICENSE`](https://github.com/pieterh/energy-management-system/blob/main/LICENSE.md) for more information.

## TODO 
- [x] Integrate test results with SonarCloud
- [x] Fix warnings with dependencies that might not be .NET 6/7 compatible
- [ ] Introduce charge mode that includes EPEX SPOT tarif for cheap charging
- [ ] Investigate blazor for frontend
- [ ] Investigate blazor backend
- [ ] Automatically generate license and attribution information of all used components

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
