FROM microsoft/dotnet:2.2-sdk as net-builder
ARG IsProduction=false
ARG CiCommitName=local
ARG CiCommitHash=sha
ARG SONAR_TOKEN=test

RUN dotnet tool install --global dotnet-sonarscanner
RUN curl -sL https://deb.nodesource.com/setup_12.x | bash - && \
    apt -y update && \
    apt -y install nodejs openjdk-8-jre libnss3 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /build
ADD . .
RUN dotnet restore
RUN dotnet run --project BudgetTracker.JsApiGenerator/BudgetTracker.JsApiGenerator.csproj

WORKDIR /build/BudgetTracker.Client
RUN npm install
RUN npm run build

WORKDIR /build/

ENV CiCommitName=$CiCommitName
RUN /root/.dotnet/tools/dotnet-sonarscanner begin \
        /k:"DiverOfDark_BudgetTracker" \
        /o:"diverofdark-github" \
        /d:sonar.host.url="https://sonarcloud.io" \
        /d:sonar.login=$SONAR_TOKEN \
        /d:sonar.branch.name=$CiCommitName /d:sonar.sources=/build/BudgetTracker.Client && \
    dotnet build BudgetTracker.sln && \
    /root/.dotnet/tools/dotnet-sonarscanner end /d:sonar.login="$SONAR_TOKEN"

RUN dotnet publish --output ../out/ --configuration Release --runtime linux-x64 BudgetTracker

FROM microsoft/dotnet:2.2-aspnetcore-runtime
ENV TZ=Europe/Moscow
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

# Install Chrome WebDriver
RUN apt-get -yqq update && \
    apt-get -yqq install unzip gnupg2 procps htop && \
    rm -rf /var/lib/apt/lists/*

RUN CHROMEDRIVER_VERSION=74.0.3729.6 && \
    mkdir -p /opt/chromedriver-$CHROMEDRIVER_VERSION && \
    curl -sS -o /tmp/chromedriver_linux64.zip http://chromedriver.storage.googleapis.com/$CHROMEDRIVER_VERSION/chromedriver_linux64.zip && \
    unzip -qq /tmp/chromedriver_linux64.zip -d /opt/chromedriver-$CHROMEDRIVER_VERSION && \
    rm /tmp/chromedriver_linux64.zip && \
    chmod +x /opt/chromedriver-$CHROMEDRIVER_VERSION/chromedriver && \
    ln -fs /opt/chromedriver-$CHROMEDRIVER_VERSION/chromedriver /usr/local/bin/chromedriver

# Install Google Chrome
RUN curl -sS -o - https://dl-ssl.google.com/linux/linux_signing_key.pub | apt-key add - && \
    echo "deb http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google-chrome.list && \
    apt-get -yqq update && \
    apt-get -yqq install google-chrome-stable && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=net-builder /build/out ./net

RUN ln -fs /usr/local/bin/chromedriver /app/net/chromedriver
RUN ln -fs /opt/google/chrome/chrome /usr/bin/chrome

ADD run.sh .
RUN chmod +x run.sh

ENV Properties__IsProduction=$IsProduction
ENV Properties__CiCommitName=$CiCommitName
ENV Properties__CiCommitHash=$CiCommitHash
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["/app/run.sh"]
