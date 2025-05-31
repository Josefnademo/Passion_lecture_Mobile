# Passion_lecture_Mobile
Passion_lecture_Mobile: The company "Read4All" aims to popularize reading by offering a mobile application for reading e-books called "ReadME."

## Installation - Deploy

1. **Clone the Repository:**

   Start by cloning the project repository to your local machine and after on server(if it wasn't done yet):

   ```bash
   git clone https://github.com/Josefnademo/Passion_lecture_Mobile
   ```

2. **Installation of dependencies**

   Installation of dependencies for backend

   ```bash
   cd Passion_lecture_Mobile\src\helpers\api
   npm install
   ```

3. **Compose docker container:**

   First, build thedocker container with (MySQL database, Backend API, PHPMyAdmin).

   ```bash
   cd Passion_lecture_Mobile\src\helpers\api
   ```
   ```bash
   docker compose up --build
   ```
4. **Deploy application**

   After this you can start the application on you mobile device, for this run the **.sln** file named "**PLMobile.sln**".

   ```bash
   cd Passion_lecture_Mobile\src\code\PLMobile
   ```
      
