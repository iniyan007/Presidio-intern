# Stage 1: Build the Angular application
FROM node:20-alpine AS build
WORKDIR /app

# Copy package.json and package-lock.json
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy the rest of the application code
COPY . .

# Build the Angular application for production
RUN npm run build --configuration=production

# Stage 2: Serve the application using Nginx
FROM nginx:alpine

# Copy the build output to replace the default nginx contents.
# Note: Ensure the build output path matches your angular.json (e.g. dist/frontend)
COPY --from=build /app/dist/frontend/browser /usr/share/nginx/html

# Copy custom nginx configuration if needed (optional)
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
