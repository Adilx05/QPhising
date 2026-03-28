FROM node:22.13.1-alpine3.21 AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci --no-audit --no-fund

COPY . .
RUN npm run build

FROM nginx:1.27.5-alpine3.21 AS runtime
COPY --from=build /app/dist/qphising-frontend /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
