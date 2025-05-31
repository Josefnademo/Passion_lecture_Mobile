FROM node:20

WORKDIR /app

# Copy package files first for better caching
COPY package*.json ./

# Install both production and dev dependencies
RUN npm install

# Copy the rest of the application
COPY . .

CMD ["node", "app.js"] 