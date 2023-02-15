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

[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=ncloc)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=bugs)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=vulnerabilities)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=code_smells)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=duplicated_lines_density)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=sqale_index)](https://sonarcloud.io/dashboard?id=energy-management-system)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=pieterh_energy-management-system&metric=coverage)](https://sonarcloud.io/dashboard?id=energy-management-system)

## Development Startup
backend: dotnet ems.dll --config config.json --nlogcfg NLog.config (listening on 5005)
frontend: npm start (listening on 5010)
browser: http://127.0.0.1:5005 (connecting to the backend that also services the static content)

## Installation
...

## Developing and Contributing
We'd love to get contributions from you! Once you are up and running, take a look at the
[Contribution Documents](https://github.com/pieterh/energy-management-system/blob/main/CONTRIBUTING.md) to see how to get your changes merged
in.

## TODO 
- [x] Integrate test results with SonarCloud
- [ ] Fix warnings with dependencies that might not be .NET 6/7 compatible
- [ ] Introduce charge mode that includes EPEX SPOT tarif for cheap charging
- [ ] Investigate blazor for frontend
- [ ] Investigate blazor backend


<!-- LICENSE -->
## License

Distributed under the BSD 3-Clause license. See `LICENSE` for more information.


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
