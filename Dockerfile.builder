FROM mcr.microsoft.com/dotnet/core/sdk:3.1 as net-builder

RUN dotnet tool install --global dotnet-sonarscanner
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - && \
    apt -y update && \
    apt -y install nodejs openjdk-11-jre libnss3 && \
    rm -rf /var/lib/apt/lists/*