# 🧰 RabbitMQ Setup with Docker

This guide will help you run **RabbitMQ** locally using **Docker Desktop**, complete with the **management plugin** for web-based UI access. It's ideal for development and testing purposes.

---

## 📦 Prerequisites

- **Docker Desktop** installed and running  
  [Download Docker](https://www.docker.com/products/docker-desktop)

- (Optional) A .NET Core SDK if you plan to integrate RabbitMQ into a C# application  
  [Download .NET SDK](https://dotnet.microsoft.com/download)

---

## 🚀 Run RabbitMQ in Docker

Use the following command to start RabbitMQ with the management plugin:

```bash
docker run -d --hostname my-rabbit --name some-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management
