# FCG.Notifications

Microsserviço responsável pelo processamento de notificações da plataforma FIAP Cloud Games.

## Responsabilidades

- Consumo de eventos da plataforma
- Envio de notificações assíncronas
- Envio de e-mail de boas-vindas
- Envio de confirmação de pagamento
- Integração desacoplada via RabbitMQ

---

# Arquitetura

```
FCG.Notifications
├── src
│   ├── FCG.Notifications.Application
│   ├── FCG.Notifications.Domain
│   ├── FCG.Notifications.Infrastructure
│   └── FCG.Notifications.Worker
├── tests
├── k8s
├── Dockerfile
├── docker-compose.yml
├── docker-compose.full.yml
├── NuGet.config
└── README.md
```

---

# Camadas

## Worker

- Background Worker
- Consumo de filas RabbitMQ
- Processamento assíncrono

## Application

- Casos de uso
- Interfaces
- DTOs

## Domain

- Regras de negócio
- Objetos de domínio

## Infrastructure

- RabbitMQ
- MassTransit
- Configurações
- Publicação e consumo de mensagens

---

# Tecnologias

- .NET 8
- Worker Service
- RabbitMQ
- MassTransit
- Docker
- Kubernetes
- xUnit

---

# Dependência Compartilhada

```xml
<PackageReference Include="FCG.BuildingBlocks" Version="1.0.1" />
```

---

# Execução Local

## Restore

```powershell
dotnet restore --configfile .\NuGet.config
```

## Build

```powershell
dotnet build
```

## Testes

```powershell
dotnet test
```

## Executar Worker

```powershell
dotnet run --project .\src\FCG.Notifications.Worker
```

---

# Docker

## Build

```powershell
docker build -f Dockerfile -t brnmatos/fcg-notifications-worker:1.0.0 .
```

## Executar

```powershell
docker run brnmatos/fcg-notifications-worker:1.0.0
```

---

# Docker Hub

```powershell
docker push brnmatos/fcg-notifications-worker:1.0.0
```

---

# Docker Compose

## RabbitMQ

```powershell
docker compose up -d
```

## Ambiente Completo

```powershell
docker compose -f docker-compose.full.yml up -d --build
```

---

# Kubernetes

## Estrutura

```
k8s
├── namespace.yaml
├── configmap.yaml
├── secret.yaml
├── worker-deployment.yaml
└── rabbitmq.yaml
```

---

## Deploy

```powershell
kubectl apply -f .\k8s\
```

---

## Verificar Pods

```powershell
kubectl get pods -n fcg
```

---

## Verificar Deployments

```powershell
kubectl get deployments -n fcg
```

---

## Logs

```powershell
kubectl logs -f deployment/fcg-notifications-worker -n fcg
```

---

# RabbitMQ

Caso esteja executando localmente via Kubernetes:

```powershell
kubectl port-forward service/rabbitmq 15672:15672 -n fcg
```

Acesso:

```
http://localhost:15672
```

Usuário:

```
rabbitmquser
```

Senha:

```
********
```

---

# Eventos

## Consumidos

- UserCreatedEvent
- PaymentProcessedEvent

## Publicados

Este microsserviço não publica eventos.

---

# Fluxo

```
Users API
      │
      ▼
UserCreatedEvent
      │
      ▼
RabbitMQ
      │
      ▼
Notifications Worker
      │
      ▼
Envio de e-mail de boas-vindas
```

```
Catalog API
      │
      ▼
OrderPlacedEvent
      │
      ▼
Payments Worker
      │
      ▼
PaymentProcessedEvent
      │
      ▼
RabbitMQ
      │
      ▼
Notifications Worker
      │
      ▼
Confirmação de pagamento
```

---

# Comunicação

```
RabbitMQ
        │
        ▼
Notifications Worker
```

---

# Configuração

## RabbitMQ

As configurações podem ser fornecidas por:

- appsettings.Local.json (desenvolvimento)
- Docker Compose
- ConfigMap (Kubernetes)
- Secret (Kubernetes)

---

# Segurança

- ConfigMap para configurações
- Secret para credenciais
- RabbitMQ ClusterIP
- Worker sem exposição HTTP

---

# CI/CD

```
Restore
    ↓
Build
    ↓
Tests
    ↓
Docker Build
    ↓
Docker Push
    ↓
Kubernetes
```

---

# Troubleshooting

## Verificar Pods

```powershell
kubectl get pods -n fcg
```

## Verificar Deployments

```powershell
kubectl get deployments -n fcg
```

## Verificar Services

```powershell
kubectl get services -n fcg
```

## Descrever Pod

```powershell
kubectl describe pod <pod-name> -n fcg
```

## Logs

```powershell
kubectl logs -f deployment/fcg-notifications-worker -n fcg
```

---

# Estrutura do Projeto

```
FCG.Notifications
│
├── src
│   ├── FCG.Notifications.Application
│   ├── FCG.Notifications.Domain
│   ├── FCG.Notifications.Infrastructure
│   └── FCG.Notifications.Worker
│
├── tests
│
├── k8s
│   ├── namespace.yaml
│   ├── configmap.yaml
│   ├── secret.yaml
│   ├── rabbitmq.yaml
│   └── worker-deployment.yaml
│
├── Dockerfile
├── docker-compose.yml
├── docker-compose.full.yml
├── NuGet.config
└── README.md
```

---

# Autor

**Bruno Matos**

Pós-graduação FIAP - Arquitetura de Sistemas .NET / Tech Challenge