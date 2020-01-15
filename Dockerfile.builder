FROM mcr.microsoft.com/dotnet/core/sdk:3.0.102-disco as net-builder

RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm -f packages-microsoft-prod.deb

RUN dotnet tool install --global dotnet-sonarscanner
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - && \
    apt -y update && \
    apt -y install nodejs openjdk-8-jre libnss3 dotnet-sdk-2.2 && \
    rm -rf /var/lib/apt/lists/*