# ==============================================================================
# Multi-stage Dockerfile for Gestion_Ope Frontend (Angular 18 Standalone)
# ==============================================================================

# Stage 1: Build the Angular SPA
FROM node:20-alpine AS build
WORKDIR /app

# Copy dependency manifests
COPY package*.json ./
RUN npm ci

# Copy application source code
COPY . .

# Build for production
RUN npm run build -- --configuration production

# Stage 2: Serve with Nginx Alpine
FROM nginx:1.27-alpine AS runtime

# Copy custom Nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy build artifacts to Nginx html directory
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
