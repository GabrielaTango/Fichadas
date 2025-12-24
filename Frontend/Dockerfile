# Etapa de build
FROM node:20-alpine AS build
WORKDIR /app

# Copiar package.json y package-lock.json
COPY package*.json ./

# Instalar dependencias
RUN npm ci

# Copiar el resto del codigo
COPY . .

# Argumento para la URL de la API (se pasa en build time)
ARG VITE_API_URL=http://localhost:5210/api
ENV VITE_API_URL=$VITE_API_URL

# Build de produccion
RUN npm run build

# Etapa de produccion con nginx
FROM nginx:alpine AS production
WORKDIR /usr/share/nginx/html

# Copiar configuracion de nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copiar build de la etapa anterior
COPY --from=build /app/dist .

# Exponer puerto 5173
EXPOSE 5173

CMD ["nginx", "-g", "daemon off;"]
