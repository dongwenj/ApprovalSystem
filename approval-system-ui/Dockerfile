FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
# 接收來自 compose 的變數
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL
RUN echo "Checking ARG: $VITE_API_URL"
RUN npm run build

# 使用 Nginx 執行
FROM nginx:stable-alpine
# 將編譯好的檔案複製到 Nginx 預設目錄
COPY --from=build /app/dist /usr/share/nginx/html
# React Router，防止重新整理 404
COPY nginx.conf /etc/nginx/conf.d/default.conf 
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]