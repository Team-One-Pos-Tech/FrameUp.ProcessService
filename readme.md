# FrameUp Processing Service

The Processing Service is designed to process a video and extract thumbnails based on a given parameters. The video should be previously stored at a Bucket/BlobStorage and it expected to be done by [FrameUp.OrderService](https://github.com/Team-One-Pos-Tech/FrameUp.OrderService)

## Running the application

***This project makes use of Shared Workflows***
In order to have a standard environment and containers, this project makes use of [shared workflows](https://github.com/Team-One-Pos-Tech/FrameUp.OrderService), it means that this repository won't have a docker file, in order to run it, we recommend to execute the docker compose file under the folder '/deploy'.
It will get the [latest images published at our repository registry](https://github.com/orgs/Team-One-Pos-Tech/packages?repo_name=FrameUp.ProcessService) and will set up all the necessary dependencies.

In short, navigate to the ./deploy folder in your terminal and execute:

```sh
   docker-compose up -d
```

## External dependencies 

## Sequence diagrams


