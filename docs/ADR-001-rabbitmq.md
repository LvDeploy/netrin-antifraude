# Architecture Decision Record

## ADR-001: Implementação do Rabbitmq para mensagens assíncronas

**Status:** Aceito

**Data:** 01-08-2026

### Context

A ausência de um recurso de mensageria dificulta a escalabilidade da aplicação, 
pois acaba criando alto acoplamento entre serviços que implementam regras de negócio, e um risco maior de falhas críticas que geram interrupções, devido ao fato de uma única aplicação ser 
responsável por todo processo.

O recurso mensageria é uma forma de entregar performace e manter a solução escalável para encorporar 
novos processos de negócio e tecnologias.

### Decisão

O **RabbitMQ** foi escolhido como a tecnologia utilizada.

A solução usará:

- Exchanges diretos com rota estabelecida
- Filas duráveis
- Mensagens persistentes
- Dead Letter Queues (DLQ) para erros imprevistos
- Politicas de retentativa com backoff em minutos configurado na aplicação
- Modo de garantia (At Least Once) com reconhecimento ativo (ACK/NACK)

Cada serviço de consumo de evento terá sua própria fila.

### Alternativas Consideradas

#### Opção 1 – Amazon SQS/SNS 

**Pros**

- Auto-gerenciado pela infra da AWS
- Roteamento simples
- Retém mensagens por um número limitado de dias

**Cons**

- Não é gratuita
- Tecnologia presa à infraestrutura da AWS
- Não é possível testar localmente

#### Opção 2 – RabbitMQ (Selected)

**Pros**

- Ecosistema maduro
- Controle de roteamento flexível
- Capacidade de rodar localmente
- Baixo custo operacional

**Cons**

- Dificuldade de operar com milhões de mensagens por segundo
- Requer um maior trabalho para gerenciar o fluxo de mensagens

#### Opção 3 – Apache Kafka

**Pros**

- Melhor escalabilidade
- Capaz de trabalhar com streaming de dados
- Retenção de mensagens armazenados em logs 

**Cons**

- Responsabilidade do roteamento fica a cargo das aplicações envolvidas
- Custo de infraestrutura maior
- Maior complexidade para gerenciar partições e os offsets

### Consequências

#### Positivas

- Maior resiliência
- Baixo acoplamento entre serviços
- Escalabilidade aprimorada
- Respostas de API mais rápidas graças ao processamento assíncrono
- Integração mais fácil de novos consumidores

#### Negativas

- Maior complexidade para depurar 
- Consistência eventual
- Segregação de monitoramento
- Necessidade de implementar idempotentKey

### Estratégia de Implementação

1. Utilizar o `main.exchange` para envio das operações.

2. Configurar uma chave para as filas que receberão as mensagens

3. Configurar chave para fila de mensagens de erro (DLQ).

4. Estabelecer tempo de retry para reprocessar registros na fila de mensagens de erro.

5. Estabelecer um número máximo de tentativas para reprocessar baseando-se no atributo `x-death`.