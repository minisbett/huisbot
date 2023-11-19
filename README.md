# huisbot
An osu! related discord bot for pp.huismetbenen.nl, used on the pp Discord server.

# Read before usage

The bot uses an onion-key in order to gain onion-level access to [Huismetbenen](https://pp.huismetbenen.nl/). However, this key is not necessary for the basic functionality of the bot and only provides an extended access to more reworks.

# Setup for deployment

The current release of this bot (found in the release branch) is automatically being deployed into a Docker image which can be found [here](https://hub.docker.com/repository/docker/minisbett/huisbot/general). Therefore, you'll need to install the Docker Engine onto your system.

In order to setup the bot, you will need to copy the `huisbot/.env.example` file onto your system. You will then pass a path to that file when running the docker container. Consider creating a script to automate the following commands.

Here's how you can download and run the bot:
```sh
# Pulls the latest Huisbot Docker image from Docker Hub.
docker pull minisbett/huisbot:latest

# Cleans up the old container if you want to update the bot.
docker stop huisbot
docker rm huisbot

# Runs the docker container.
docker run -d --env-file "/path/to/.env" --name huisbot minisbett/huisbot:latest
```

If you wish to access the logs of the container, you can do that with `docker logs huisbot`.

# Setup for development

If you'd like to perform development on the bot, follow these steps:

1. Clone the repository.
2. Copy the `.env.example` file and rename it to `.env`.
3. Set the output mode of the .env file to `Copy If Newer`.
4. Fill out the required environment variables.
5. Start up the application.

If you'd like to contribute, please make sure to Pull Request onto the master branch, and not the release branch. The release branch is set up with a CI/CD pipeline, which automatically builds a new Docker image, pushes it to Docker Hub and deploys it on the production server.