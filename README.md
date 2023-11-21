# huisbot
An osu! related discord bot for pp.huismetbenen.nl, used on the pp Discord server.

# Read before usage

The bot uses an onion-key in order to gain onion-level access to [Huismetbenen](https://pp.huismetbenen.nl/). However, this key is not necessary for the basic functionality of the bot and only provides an extended access to more reworks.

# Setup for deployment

The current release of this bot (found in the release branch) is automatically being deployed into a Docker image which can be found [here](https://hub.docker.com/repository/docker/minisbett/huisbot/general). Therefore, you'll need to install the Docker Engine onto your system.

In order to setup the bot, follow these steps:

1. Copy the `huisbot/.env.example` file onto your system. You will then pass a path to that file when running the docker container.

2. In order to persist the database outside of the container environment, create an empty database file, preferably where you also store your .env file so everything is in one place.

3. Run the following commands to install the bot & update it whenever the Docker image was updated:
```sh
# Pulls the latest Huisbot Docker image from Docker Hub.
docker pull minisbett/huisbot:latest

# Cleans up the old container if you want to update the bot.
docker stop huisbot
docker rm huisbot

# Runs the docker container.
docker run -d --env-file "/path/to/.env" --mount type=bind,source="/path/to/your/database/file.db",target="/app/database.db" --name huisbot minisbett/huisbot:latest
```

If you wish to access the logs of the container, you can do that with `docker logs huisbot`.  
To enter the container environment, do `docker exec -it huisbot bash`.

# Setup for development

If you'd like to perform development on the bot, follow these steps:

1. Clone the repository.
2. Copy the `.env.example` file and rename it to `.env`.
3. Set the output mode of the .env file to `Copy If Newer`.
4. Fill out the required environment variables.
5. Run the application.

If you'd like to contribute, **please make sure to Pull Request onto the master branch**, and not the release branch. The release branch is set up with a CI/CD pipeline, which automatically builds a new Docker image, pushes it to Docker Hub and deploys it on the production server.