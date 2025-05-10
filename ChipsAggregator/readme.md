Exercise 3: FindChips Distributor Data Aggregation Tool
Objective:
Assess your ability to integrate external platforms, handle asynchronous programming, and export structured data into Excel.
Task:
Use https://www.findchips.com to search for the part number “2N222”.

From the search results:
- Identify any 5 distributors listed on the site
- Retrieve up to 5 offers per distributor, extracting the following:
  - Distributor Name
  - Seller Name (if shown)
  - MOQ
  - SPQ
  - Unit Price
  - Currency
  - Offer URL (if available)


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
