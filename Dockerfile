FROM node as client-builder
WORKDIR /build
ADD BudgetTracker.Client/package.json .
RUN npm install
ADD BudgetTracker.Client ./
RUN mkdir out && npm run build

FROM microsoft/dotnet:2.2-sdk as net-builder
RUN dotnet tool install --global dotnet-sonarscanner
RUN apt -y update && \
    apt -y install openjdk-8-jre libnss3 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /build
ADD BudgetTracker.sln .
ADD nuget.config .
ADD BudgetTracker/BudgetTracker.csproj BudgetTracker/
ADD BudgetTracker.JsApiGenerator/BudgetTracker.JsApiGenerator.csproj BudgetTracker.JsApiGenerator/

RUN dotnet restore

ADD BudgetTracker BudgetTracker
ADD BudgetTracker.JsApiGenerator BudgetTracker.JsApiGenerator
COPY --from=client-builder /build/out/*.js* BudgetTracker/wwwroot/js/
COPY --from=client-builder /build/out/*.css* BudgetTracker/wwwroot/css/

ARG SONAR_TOKEN=test
RUN echo $SONAR_TOKEN && /root/.dotnet/tools/dotnet-sonarscanner begin /k:"DiverOfDark_BudgetTracker" /o:"diverofdark-github" /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login=$SONAR_TOKEN && \
    dotnet build BudgetTracker.sln && \
    /root/.dotnet/tools/dotnet-sonarscanner end /d:sonar.login="$SONAR_TOKEN"

RUN dotnet publish --output ../out/ --configuration Release --runtime linux-x64 BudgetTracker

FROM microsoft/dotnet:2.2-aspnetcore-runtime

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

ARG IsProduction=false
ARG CiCommitName=local
ARG CiCommitHash=sha

ENV Properties__IsProduction=$IsProduction
ENV Properties__CiCommitName=$CiCommitName
ENV Properties__CiCommitHash=$CiCommitHash
ENV ASPNETCORE_ENVIRONMENT=Production

ENV TZ=Europe/Moscow
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

ENTRYPOINT ["/app/run.sh"]
