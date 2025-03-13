# WebSockets + MVC Chat App

This project is a simple web application designed to explore and understand how WebSockets work. The goal is to demonstrate the implementation of WebSockets using a minimal approach, with a custom connection manager class.

In this application, WebSockets are implemented using `app.UseWebSockets(webSocketOptions)`. The connection manager is a custom class that leverages Dependency Injection (DI) in singleton scope to manage WebSocket connections. The application tracks active connections in memory, ensuring that messages are routed correctly to the intended recipients.

## What is WebSocket?

WebSocket is a communication protocol that provides full-duplex communication channels over a single TCP connection. It enables real-time, bidirectional communication between the server and the client. Unlike HTTP, which is request-response based, WebSocket allows data to flow continuously, making it ideal for applications like chat apps, live notifications, and real-time data updates.

## Application Screenshot

![Chat App Screenshot](https://raw.githubusercontent.com/Igor-Vicente/Chatapp.Mvc.Websockets/refs/heads/main/img/chat-example.png)  
_Image of the application (2 chats)_

## How to Test

This application is fully containerized using Docker Compose. If you have Docker installed, you can easily spin up the entire environment by running the following command from the directory where the `docker-compose.yml` file is located:

```bash
docker compose up -d
```

Then you can user your browser to navigate to: http://localhost:8082/

## Refs

- https://learn.microsoft.com/en-us/aspnet/core/fundamentals/websockets?view=aspnetcore-9.0
- https://www.rfc-editor.org/rfc/rfc6455
