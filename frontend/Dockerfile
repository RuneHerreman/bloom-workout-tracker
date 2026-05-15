FROM node:20-alpine AS builder
WORKDIR /app
COPY bloom/package.json ./
COPY bloom/package-lock.json ./
RUN npm install --legacy-peer-deps
COPY bloom/ .
RUN npm run build

FROM nginx:alpine AS final
COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
