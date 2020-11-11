FROM mcr.microsoft.com/dotnet/sdk:5.0 as net-builder

RUN apt -y update && \
    apt -y install apt-utils lsb-release gnupg && \
    rm -rf /var/lib/apt/lists/*

RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - &&  \
    apt -y update && \
    apt -y install nodejs && \
    rm -rf /var/lib/apt/lists/*

RUN dotnet tool install --global dotnet-sonarscanner
RUN mkdir -p /usr/share/man/man1
RUN apt -y update && \
    apt -y install openjdk-11-jre-headless libnss3 && \
    rm -rf /var/lib/apt/lists/*