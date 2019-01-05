FROM microsoft/dotnet:2.1-sdk as net-builder
WORKDIR /build
ADD BudgetTracker.sln .
ADD nuget.config .
ADD BudgetTracker/BudgetTracker.csproj BudgetTracker/

RUN dotnet restore

ADD BudgetTracker BudgetTracker
RUN dotnet publish --output ../out/ --configuration Release --runtime linux-x64 BudgetTracker

FROM microsoft/dotnet:2.1-aspnetcore-runtime

# Install Chrome WebDriver
RUN apt-get -yqq update && \
    apt-get -yqq install unzip gnupg2 procps htop && \
    rm -rf /var/lib/apt/lists/*

RUN CHROMEDRIVER_VERSION=2.37 && \
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
