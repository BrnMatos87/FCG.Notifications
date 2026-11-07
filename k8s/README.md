# Kubernetes

FCG.Notifications não possui Deployment Kubernetes na arquitetura oficial.

O serviço é publicado como Azure Function no plano Flex Consumption. Users e
Payments chamam os HTTP Triggers usando a URL e a Function Key injetadas pelo
Secret `fcg-shared-secret` do cluster.

Os manifests compartilhados da solução ficam no repositório
`FCG.Orchestration`. Eles não criam Pod ou Service para Notifications.

Consulte [`../terraform/README.md`](../terraform/README.md) para provisionar a
infraestrutura serverless e [`../README.md`](../README.md) para publicar e
integrar a Function.
