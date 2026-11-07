# Infraestrutura da FCG.Notifications

Este diretório provisiona somente a infraestrutura Azure necessária para executar a `FCG.Notifications` como Azure Function HTTP Trigger em .NET 8 Isolated, usando Flex Consumption.

## Recursos provisionados

- Resource Group exclusivo
- Storage Account Standard LRS
- container Blob privado para pacotes OneDeploy
- App Service Plan Linux Flex Consumption (`FC1`)
- Azure Function App .NET 8 Isolated
- Log Analytics Workspace
- Application Insights

Proteções de custo padrão para o ambiente DEV:

- limite de ingestão do Log Analytics: `0.1 GB/dia`;
- limite de telemetria do Application Insights: `0.1 GB/dia`;
- Flex Consumption: instâncias de `512 MB`, com máximo de `2` instâncias sob demanda;
- retenção de logs: `30 dias`.

Os limites diários são salvaguardas contra picos de ingestão. Quando atingidos, a coleta de telemetria pode ficar indisponível até o próximo reset diário.

Não são provisionados recursos dos demais serviços da solução, AKS, RabbitMQ, bancos de dados, Redis, Kong, Prometheus, Grafana, rede privada ou Container Registry.

## Pré-requisitos

- Terraform 1.10 ou superior
- Azure CLI
- uma assinatura Azure com permissão para criar os recursos
- provider `Microsoft.Web` registrado na assinatura

## Estado remoto

O state usa o mesmo Azure Storage criado para o Terraform do Orchestration, mas
em um blob independente:

```text
Container: tfstate
Key:       fcg-notifications-phase4.tfstate
```

O backend não armazena dados da aplicação. Ele mantém o vínculo entre esta
configuração e os recursos da Function, permitindo execução por outro
colaborador e bloqueio contra alterações simultâneas.

Descubra o SubscriptionId:
```powershell
az account list --output table
```
Autentique-se e selecione a assinatura:

```powershell
az login
az account set --subscription "<SUBSCRIPTION_ID>"
```

## Configuração

Copie os arquivos de exemplo e substitua somente os placeholders:

```powershell
Copy-Item backend.hcl.example backend.hcl
Copy-Item terraform.tfvars.example terraform.tfvars
```

Os nomes do Function App e da Storage Account devem ser globalmente únicos. Não coloque credenciais, Function Keys ou outros secrets em `terraform.tfvars`.

Os limites podem ser ajustados pelas variáveis `log_analytics_daily_quota_gb`, `application_insights_daily_data_cap_in_gb`, `instance_memory_in_mb` e `maximum_instance_count`. Para DEV, mantenha os valores conservadores do arquivo de exemplo.

## Provisionamento

Execute os comandos a partir deste diretório:

```powershell
terraform init -backend-config=backend.hcl
terraform validate
terraform plan -out main.tfplan
terraform apply main.tfplan
```

> `terraform apply` cria recursos reais e gera custos na assinatura Azure. Na
> primeira implantação, revise e aplique o plano localmente. Depois do bootstrap
> OIDC, o workflow executa o mesmo ciclo na `main`, protegido pelo GitHub
> Environment `production`.

Consulte as URLs criadas com:

```powershell
terraform output
```

Os endpoints usam `AuthorizationLevel.Function`. As Function Keys são geradas pela Azure e não são retornadas nos outputs nem armazenadas como variáveis Terraform.

Outputs disponíveis:

- `function_app_name`;
- `function_app_hostname`;
- `function_app_base_url`;
- `user_created_url`;
- `payment_processed_url`;
- `resource_group_name`;
- `location`.

## Publicação do código

Na primeira implantação manual, o Terraform provisiona a infraestrutura, mas não
publica o código. Depois do `terraform apply`, execute na raiz do repositório:

```powershell
dotnet publish `
  .\src\FCG.Notifications.Functions\FCG_Notifications_Functions.csproj `
  --configuration Release `
  --output .\artifacts\publish

Remove-Item .\artifacts\released-package.zip -Force -ErrorAction SilentlyContinue

tar -a -c -f .\artifacts\released-package.zip `
  -C .\artifacts\publish `
  .

az functionapp deployment source config-zip `
  --resource-group "<RESOURCE_GROUP_NAME>" `
  --name "<FUNCTION_APP_NAME>" `
  --src .\artifacts\released-package.zip
```

O comando da Azure CLI usa OneDeploy no Flex Consumption. O `host.json` deve permanecer na raiz do arquivo ZIP.

Após o deployment, obtenha as Function Keys pela Azure CLI ou pelo portal e armazene-as nos secrets dos serviços chamadores. Não as versione no repositório.

## Remoção

Para remover todos os recursos criados por esta configuração:

```powershell
terraform plan -destroy -out destroy.tfplan
terraform apply destroy.tfplan
```

Ou, diretamente:

```powershell
terraform destroy
```

## Arquivos locais sensíveis

O state pode conter valores sensíveis gerados pela Azure, como a chave da
Storage Account. `backend.hcl`, `terraform.tfvars`, planos e arquivos de state são
ignorados pelo Git. O job de validação executa `terraform init -backend=false`. O
job de deploy, restrito à `main`, usa OIDC e o backend remoto para gerar e aplicar
um plano antes da publicação da Function.
