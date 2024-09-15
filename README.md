# AspNetDockerDemo

## Simple ASP.NET Core project using Docker

## Set up step:

###1. Edit the docker-compose.yaml file. 
- You may want to edit the sa account's password and volume location on host machine to match yours

###2. Run "docker-compose -f .\docker-compose.yaml up" from project directory

###3. Run "docker build -t dockerdemo.api:1.0 ." from project directory to create the image for the app

###4. Run "docker run --name dockerdemo.api -p 8080:8080 dockerdemo.api:1.0" to run the created image

Now you can access http://localhost:8080/swagger/index.html to see the API definitions
