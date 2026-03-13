FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
# 接收來自 compose 的變數
ARG REACT_APP_API_URL
ENV REACT_APP_API_URL=$REACT_APP_API_URL
RUN npm run build

# 使用 Nginx 執行
FROM nginx:stable-alpine
# 將編譯好的檔案複製到 Nginx 預設目錄
COPY --from=build /app/build /usr/share/nginx/html
# React Router，防止重新整理 404
COPY nginx.conf /etc/nginx/conf.d/default.conf 
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]