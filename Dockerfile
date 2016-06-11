FROM mono:latest

MAINTAINER Skimia Agency <contact@skimia.agency>

RUN apt-get update

#install git
RUN apt-get install -y wget git-core
RUN apt-get update

#clone latest
RUN git clone https://github.com/kesslerdev/SkimiaOS.git /skimiaos/

RUN mkdir /home/skimiaos/
RUN cp -R /skimiaos/* /home/skimiaos/
#restore nuget
RUN nuget restore /home/skimiaos/SkimiaOS.sln

#build
RUN chmod +x /home/skimiaos/build.sh
RUN (cd /home/skimiaos/ ; sh build.sh)

#run
CMD [ "mono",  "/home/skimiaos/SkimiaOS.CLI.ServerConsole/bin/Release/SkimiaOS.CLI.ServerConsole.exe" ]

#default config 
EXPOSE 8080
