FROM node:20-alpine AS builder
WORKDIR /app
COPY bloom/package.json ./
COPY bloom/package-lock.json ./
RUN npm install
COPY bloom/ .
ARG VITE_API_BASE_URL=/api/
RUN npm run build

FROM nginx:alpine AS final
COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
