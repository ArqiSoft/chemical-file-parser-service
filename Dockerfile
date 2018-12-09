FROM maven:3.5.4-jdk-8-slim AS builder
COPY . /usr/src/chemical-parser
WORKDIR /usr/src/chemical-parser
RUN mvn -Pdocker clean package

FROM openjdk:8-jre
#VOLUME /logs
RUN mkdir /temps
ENV SPRING_BOOT_APP chemical-parser.jar
ENV SPRING_BOOT_APP_JAVA_OPTS -XX:NativeMemoryTracking=summary
WORKDIR /app
EXPOSE 8083
RUN apt-get update && apt-get install -y curl
RUN curl https://raw.githubusercontent.com/vishnubob/wait-for-it/master/wait-for-it.sh > /app/wait-for-it.sh && chmod 777 /app/wait-for-it.sh
COPY --from=builder /usr/src/chemical-parser/target/$SPRING_BOOT_APP ./
ENTRYPOINT java $SPRING_BOOT_APP_JAVA_OPTS -jar $SPRING_BOOT_APP