# Architecture Decision Record

## ADR-003: Implementação de Idempotência nos Commands

**Status:** Aceito

**Data:** 01/08/2026

### Contexto

O sistema processa operações de negócio, em um modulo de avaliação de antifraude, por meio de APIs REST e tráfego de mensagens assíncronas. Falhas de rede, tentativas de reenvio pelo cliente, reenvios de mensagens, podem fazer com que a mesma requisição ou mensagem seja processada múltiplas vezes.
Isso pode resultar em processamento duplicado e inconsistências nos registros.
A aplicação deve garantir que a repetição da mesma operação produza o mesmo resultado de negócio.

### Decisão

Implementaremos uma estratégia de idempotência em nível de aplicação para todos os Commands do negócios.

A solução consiste em:

- Clientes fornecendo uma **Chave de Idempotência** para ações de persistência.
- Persistir a chave juntamente com o resultado da operação.
- Enviar a chave nas mensagens assíncronas para verificação de duplicidade no serviço específico.
- Rejeitar solicitações duplicadas usando a resposta armazenada.

A chave de idempotência será persistida no SQL Server e compartilhada entre todas as instâncias do aplicativo.

### Alternativas Consideradas

#### Opção 1 – Sem Idempotência

**Prós**

- Implementação simples
- Sem necessidade de armazenamento adicional

**Contras**

- Operações de negócios duplicadas
- Inconsistências nos dados
- Recuperação difícil

---

#### Opção 2 – Somente Restrições de dado único no Banco de Dados

**Prós**

- Impede registros duplicados
- Implementação simples

**Contras**

- Não impede o processamento duplicado
- Risco de elevar o número exceções no sistema 
- O erro é mais difícil de monitorar

---

#### Opção 3 – Idempotência em Nível de Aplicação (Selecionada)

**Prós**

- Impede a execução duplicada de operações de negócios
- Funciona em várias instâncias da aplicação
- Suporta novas tentativas de API
- Suporta reenvio de mensagens

**Contras**

- Armazenamento de dado adicional no banco
- Aumenta complexidade na aplicação das regras de negócio
- Aumento na latência das requisições

### Consequências

#### Positivas

- Persistências seguras
- Resiliência aprimorada
- Melhor compatibilidade com sistemas distribuídos
- Verificação mais fácil de falhas relacionadas a duplicidade

#### Negativas

- Necessidade de armazenamento adicional
- Necessidade de uma regra adicional para evitar duplicidade

### Estratégia de Implementação

1. O cliente envia no cabeçalho da requisição a `Idempotency-Key`.

2. A API inicialmente persiste a informação.

3. A API envia o evento com sobre a nova transação.

4. Após ler a mensagem o Worker aciona o serviço de verificação.

5. O Serviço verifica se a mensagem tem um registro persistido na base.

6. Ignora o processamento se o registro já estiver sido processado.

7. Se encontrado mais de um registro para a `Idempotency-Key` informada:

- Obtém o último registro para processar.

- Deleta o registro caso já tenha outro com mesmo `Idempotency-Key` que foi processado.

8. Caso contrário:

- Executa a operação de negócio.

- Persiste a resposta.

- Encerra a operação.