# Architecture Decision Record

## ADR-002: Adoção do SQL como banco de dados primário

**Status:** Aceito

**Data:** 01-08-2026

### Contexto

A aplicação gerencia dados transacionais críticos para o negócio. Essas entidades possuem relacionamentos bem definidos e exigem forte consistência para garantir a integridade dos dados.
O sistema deve oferecer suporte a atomicidade, integridade referencial e recuperação confiável de dados.

### Decisão

Será adotado o banco de dados relacional **Microsoft SQL Server** como a camada de persistência principal.

A solução seguirá estes princípios:

- Normalizar dados transacionais 
- Utilizar chaves primárias e estrangeiras para garantir a integridade dos relacionamentos
- Criar índices com base nos padrões de consulta
- Garantir atomicidade durante a aplicação das regras de negócio 
- Acessar o banco de dados por meio do padrão *Repository*
- Gerenciar a evolução do sistema utilizando migrações de banco de dados

Bancos de dados NoSQL poderão ser introduzidos posteriormente para casos de uso especializados, como
cache, registro de logs ou análise de dados, mas o SQL Server permanecerá como a fonte oficial de dados.

### Alternativas Consideradas

#### Opção 1 – SQL Server (Selecionada)

**Prós**

- Fortes garantias ACID
- Integridade referencial
- Excelente suporte para juntar dados entre tabelas
- Ecossistema maduro
- Bem adequado para sistemas transacionais
- Suporte nativo para backups

**Contras**

- Alterações de esquema exigem migrações
- Escalabilidade horizontal é mais complexa
- As operações em bancos SQL são lentas comparadas a bancos NoSQL

---

#### Opção 2 – MongoDB

**Prós**

- Esquema flexível
- Recuperação rápida de documentos
- Escalabilidade horizontal fácil
- Bem adequado para dados semiestruturados

**Contras**

- Garantia de integridade é eventual
- Alta comlexidade ao manter relacionamento entre documentos
- Lógica de aplicação adicional necessária para manter a consistência ao modificar a estrutura
de um documento

### Consequências

#### Positivas

- Forte consistência de dados
- Gerenciamento de transações confiável
- Aplicação mais fácil de regras de negócio
- Risco reduzido de anomalias nos dados

#### Negativas

- Evolução do esquema exige planejamento
- Escalar operações de escrita pode exigir otimização do banco de dados

### Estratégia de Implementação

1. Utilizar Entity Framework Core ou Dapper para acesso a dados.

2. Manter a lógica de negócios fora do banco de dados sempre que possível.

3. Migrações devem ser aplicadas fora do ORM.

4. Usar o padrão Unit of Work para manter integridade nas operações de persistência.

5. Acesso as operações de banco via padrão *Repository*.
