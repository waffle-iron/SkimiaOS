# SkimiaOS

(.NET 4.5/Mono 3.x): [![Build status](https://ci.appveyor.com/api/projects/status/hq6k6wqgtkqh037r?svg=true)](https://ci.appveyor.com/project/kesslerdev/skimiaos)
[![Build Status](https://travis-ci.org/kesslerdev/SkimiaOS.svg?branch=master)](https://travis-ci.org/kesslerdev/SkimiaOS)

## Getting started

### Using Docker

#### compile
```
wget https://raw.githubusercontent.com/kesslerdev/SkimiaOS/master/Dockerfile
docker build -t skimiaos .
docker run -p 80:8080 skimiaos
```
#### Docker Hub

```
docker run -p 80:8080 skimia/os
```
