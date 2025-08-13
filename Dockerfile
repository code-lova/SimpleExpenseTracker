FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY publish/wwwroot/ .
EXPOSE 80