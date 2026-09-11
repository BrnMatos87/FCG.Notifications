# FCG.Notifications

Componente serverless da FIAP Cloud Games responsável por notificações de cadastro e pagamento.

## Arquitetura atual

```text
FCG.Users
  -> HTTP POST /api/notifications/user-created
  -> Azure Function
  -> notificação de boas-vindas

FCG.Payments
  -> HTTP POST /api/notifications/payment-processed
  -> Azure Function
  -> confirmação de pagamento
```

A aplicação usa Azure Functions v4, .NET 8 Isolated e HTTP Trigger. Não possui RabbitMQ Trigger, consumer MassTransit nem processo Worker consumindo filas.

Os contratos `UserCreatedEvent` e `PaymentProcessedEvent` continuam sendo usados como payloads JSON para preservar os dados necessários, mas o transporte até Notifications é HTTP.

## Decisão em relação à Fase 3

O requisito central de executar Notifications como solução serverless e manter seu código/IaC no próprio repositório é atendido com Azure Functions e Terraform.

O documento da Fase 3 sugere disparo direto pela mensageria. A implementação aprovada para este projeto substitui especificamente esse disparo por HTTP Trigger: Users e Payments possuem métodos POST equivalentes aos eventos anteriores. RabbitMQ continua existindo somente nos demais fluxos da solução.

## Endpoints

| Método | Rota | Contrato |
|---|---|---|
| POST | `/api/notifications/user-created` | `UserCreatedEvent` |
| POST | `/api/notifications/payment-processed` | `PaymentProcessedEvent` |

As Functions usam `AuthorizationLevel.Function`. Na Azure, os chamadores devem enviar:

```http
x-functions-key: <FUNCTION_KEY>
```

Requisições inválidas retornam erro HTTP apropriado; falhas de processamento são registradas e não devem ser tratadas como sucesso.

## Estrutura

```text
FCG.Notifications/
|-- src/
|   |-- FCG.Notifications.Application/
|   |-- FCG.Notifications.Domain/
|   |-- FCG.Notifications.Infrastructure/
|   `-- FCG.Notifications.Functions/
|       |-- Functions/
|       |-- host.json
|       `-- FCG_Notifications_Functions.csproj
|-- tests/
|-- terraform/
|-- k8s/                 # alternativa local, não é o deploy oficial da Fase 3
|-- Dockerfile
|-- docker-compose.full.yml
`-- README.md
```

## Execução local

Pré-requisitos:

- .NET SDK 8;
- Azure Functions Core Tools v4;
- Azurite ou uma Storage Account válida;
- configurações locais não versionadas.

```powershell
dotnet restore --configfile .\NuGet.config
dotnet build
dotnet test
func start --dotnet-isolated --script-root .\src\FCG.Notifications.Functions
```

Use `src/FCG.Notifications.Functions/local.settings.json` para configurações locais. Não versione credenciais ou chaves reais.

Endpoints locais:

```text
POST http://localhost:7071/api/notifications/user-created
POST http://localhost:7071/api/notifications/payment-processed
```

No ambiente local, a Function Key pode estar vazia conforme o runtime/configuração usados. Em Azure, obtenha a chave gerada depois do deploy.

## Docker

O container existe para desenvolvimento e testes locais da Function, não como destino oficial de produção da Fase 3.

```powershell
docker build -f Dockerfile -t brnmatos/fcg-notifications-functions:1.0.0 .
docker run -p 7071:80 brnmatos/fcg-notifications-functions:1.0.0
```

Ou:

```powershell
docker compose -f docker-compose.full.yml up -d --build
```

O Compose local inclui Azurite para o runtime da Function.

## Infraestrutura Azure com Terraform

A configuração em [`terraform/`](./terraform/README.md) provisiona somente a infraestrutura serverless de Notifications:

- Resource Group;
- Storage Account e container privado de deployment;
- plano Flex Consumption `FC1`;
- Azure Function App Linux em .NET 8 Isolated;
- Log Analytics Workspace;
- Application Insights.

O Terraform não provisiona AKS, RabbitMQ, SQL Server, MongoDB, Redis, Kong, Prometheus, Grafana ou componentes dos outros microsserviços.

Fluxo manual:

```powershell
Set-Location terraform
Copy-Item terraform.tfvars.example terraform.tfvars
terraform init
terraform validate
terraform plan
terraform apply
```

`terraform apply` cria recursos reais na assinatura Azure e nunca deve ser executado automaticamente sem revisão/autorização. Para remoção:

```powershell
terraform destroy
```

O provisionamento da infraestrutura e a publicação do código são etapas separadas. Consulte [terraform/README.md](./terraform/README.md) para o comando de deployment.

## Kubernetes

Os arquivos em `k8s/` executam a imagem da Function em container e podem ser usados apenas como alternativa local ou demonstração técnica. Eles não representam o destino serverless oficial da Fase 3 e não são incluídos pelo `FCG.Orchestration`.

Na arquitetura oficial:

- não existe Deployment da Notifications no cluster;
- não existe Service `fcg-notifications-functions` no cluster;
- Users e Payments chamam a URL configurada da Azure Function;
- a Function Key permanece em Secret dos chamadores.

## Configurações dos chamadores

Users e Payments precisam fornecer:

| Variável | Uso |
|---|---|
| `Notifications__BaseUrl` | URL base retornada pelo Terraform, sem rota específica |
| `Notifications__FunctionKey` | chave enviada no header `x-functions-key` |

Exemplo de URL base:

```text
https://<function-app-name>.azurewebsites.net
```

Não coloque Function Keys em ConfigMaps, arquivos `appsettings` versionados ou `terraform.tfvars`.

## Observabilidade

Application Insights e Log Analytics recebem os logs e telemetria da Function. A stack Prometheus/Grafana exigida na Opção A da Fase 3 coleta obrigatoriamente Users e Catalog e é provisionada pelo Orchestration.

## Segurança

- endpoints Azure protegidos por Function Key;
- secrets reais fora do Git;
- Storage Account com acesso seguro configurado pelo Terraform;
- logs sem exposição de credenciais;
- infraestrutura serverless isolada dos demais serviços.

## Fluxos de integração

```text
Cadastro:
Users -> HTTP POST -> UserCreatedFunction -> serviço de notificação

Pagamento:
Payments -> HTTP POST -> PaymentProcessedFunction -> serviço de notificação

Fluxo independente preservado:
Payments -> RabbitMQ -> Catalog Worker
```
