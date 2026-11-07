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

O envio atual é simulado por `ConsoleEmailSender`: a Function valida e processa a
notificação, mas ainda não está integrada a um provedor externo de e-mail, como
Azure Communication Services, SendGrid ou SMTP.

## Papel na Fase 4

Notifications amplia a solução entregue na Fase 4, mas não faz parte do escopo
mínimo de pipeline, que exige Users e Catalog. O componente permanece serverless,
com código e infraestrutura no próprio repositório.

A integração aprovada usa HTTP Trigger: Users e Payments enviam os contratos para
os endpoints da Function. RabbitMQ continua existindo somente no fluxo assíncrono
de compra entre Catalog e Payments.

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
|-- .github/workflows/   # validação da aplicação e do Terraform
|-- k8s/README.md        # esclarece que não existe Deployment no AKS
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

O container existe para desenvolvimento e testes locais da Function, não como destino oficial de produção da Fase 4.

```powershell
docker build -f Dockerfile -t fcg-notifications-functions:latest .
docker run -p 7071:80 fcg-notifications-functions:latest
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
Copy-Item backend.hcl.example backend.hcl
Copy-Item terraform.tfvars.example terraform.tfvars
terraform init -backend-config=backend.hcl
terraform validate
terraform plan -out=main.tfplan
terraform apply main.tfplan
```

`terraform apply` cria recursos reais na assinatura Azure e nunca deve ser executado automaticamente sem revisão/autorização. Para remoção:

```powershell
terraform destroy
```

O provisionamento da infraestrutura e a publicação do código são etapas separadas. Consulte [terraform/README.md](./terraform/README.md) para o comando de deployment.

## Kubernetes

A pasta `k8s/` contém somente um README. Os manifests antigos foram removidos
para evitar dois destinos de produção concorrentes.

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

Application Insights e Log Analytics recebem os logs e telemetria da Function.
Como o envio ainda é simulado, o `ConsoleEmailSender` registra destinatário,
assunto e corpo para permitir a conferência da notificação durante a apresentação.
Esses dados também ficam disponíveis no Application Insights; em um ambiente real,
esse log deve ser reduzido ou protegido por políticas apropriadas. Prometheus e
Grafana continuam responsáveis por Users e Catalog no AKS.

## Segurança

- endpoints Azure protegidos por Function Key;
- secrets reais fora do Git;
- Storage Account com acesso seguro configurado pelo Terraform;
- logs sem exposição de credenciais;
- conteúdo completo do e-mail somente no log de simulação do `ConsoleEmailSender`;
- infraestrutura serverless isolada dos demais serviços.

## CI/CD da Fase 4

O workflow `.github/workflows/notifications-ci.yml` executa em pushes e pull
requests para `fase4` e `main`:

- restore e testes automatizados;
- `dotnet publish` da Azure Function;
- verificação de formatação e validação do Terraform sem acessar o backend.

Em pushes para `main`, depois dessas validações, o job de deploy:

- autentica na Azure por OIDC, sem senha de Service Principal;
- gera e aplica um plano usando o state remoto;
- publica o pacote validado na Azure Function Flex Consumption;
- confirma que as duas Functions foram descobertas pelo runtime.

O job usa o GitHub Environment `production`, que deve exigir aprovação manual, e
os secrets `AZURE_CLIENT_ID`, `AZURE_TENANT_ID` e `AZURE_SUBSCRIPTION_ID`. A
identidade OIDC e suas permissões limitadas são provisionadas pelo Terraform do
repositório `FCG.Orchestration`.

## Fluxos de integração

```text
Cadastro:
Users -> HTTP POST -> UserCreatedFunction -> serviço de notificação

Pagamento:
Payments -> HTTP POST -> PaymentProcessedFunction -> serviço de notificação

Fluxo independente preservado:
Payments -> RabbitMQ -> Catalog Worker
```
