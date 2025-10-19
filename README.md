# 🏦 KRT Bank Accounts System

---

<details>
  <summary><strong>⚠️CLIQUE AQUI ⚠️ PARA: Configuração inicial da aplicação</strong></summary>

  <br/>

### ✅ **Pré-requisitos**

* [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
* [Docker Desktop](https://www.docker.com/products/docker-desktop/)
* [Visual Studio 2022](https://visualstudio.microsoft.com/) (ou VS Code)

---

  <details>
    <summary><strong>1️⃣ Subir os serviços necessários</strong></summary>

  <br/>

A aplicação depende dos seguintes serviços:

* 🏦 **SQL Server** → banco de dados principal
* 🧰 **Redis** → cache de contas consultadas
* 🐇 **RabbitMQ** → mensageria para eventos de integração

Para subir todos de uma vez, execute na raiz do projeto:

```bash
docker compose up -d
```

Isso criará e iniciará automaticamente **SQL Server**, **Redis** e **RabbitMQ**.

Para verificar se estão rodando corretamente:

```bash
docker ps
```

  </details>

---

  <details>
    <summary><strong>2️⃣ Executar migrações (Entity Framework Core)</strong></summary>

  <br/>

#### ⚙️ Instale a ferramenta do Entity Framework (caso não tenha):

```bash
dotnet tool install --global dotnet-ef --version 8.0.21
```

---

#### 1️⃣ Acesse a pasta da API:

```bash
cd backend/KRT.BankAccounts.Api
```

---

#### 2️⃣ (Opcional) Criar a primeira migration:

> ⚠️ Só necessário se você quiser gerar a migration do zero.
> Caso ela já exista, pule para o passo 3.

```bash
dotnet ef migrations add InitialCreate --project ../KRT.BankAccounts.Api --startup-project ../KRT.BankAccounts.Api --output-dir ../KRT.BankAccounts.Api/_04_Infrastructure/Migrations
```

---

#### 3️⃣ Aplicar as migrações ao banco:

```bash
dotnet ef database update --project ../KRT.BankAccounts.Api --startup-project ../KRT.BankAccounts.Api
```
  </details>

---

  <details>
    <summary><strong>3️⃣ Rodando o projeto</strong></summary>

  <br/>

* 🧱 **Backend (API .NET 8)**
  Acesse a pasta `backend/KRT.BankAccounts.Api` e rode o projeto:

  * Via **Visual Studio** → selecione *KRT.BankAccounts.Api* como projeto de inicialização e pressione ▶️ *Executar*
  * Ou via CLI:
  * Vá até a pasta ./backend/KRT.BankAccounts.Api
    ```bash
    dotnet run --urls "https://localhost:7020"
    ```

* 💻 **Frontend (ASP.NET MVC)**
    
    Acesse a pasta do frontend MVC:
    
  
    ```bash
    cd frontend/KRT.BankAccounts.Web
    ```
    
    * **Via Visual Studio:**
      Selecione o projeto **KRT.BankAccounts.Web** como *Startup Project* e pressione ▶️ *Executar*

    Vá até a pasta ./frontend/krt-bank-accounts-web
  
    * **Ou via CLI:**
    
      ```bash
      dotnet run --urls "https://localhost:7286"
      ```
    
    🌐 **Aplicação web disponível em:**
    [https://localhost:7286](http://localhost:7286) *(ou conforme a porta configurada no launchSettings.json)*


  </details>

</details>

---

<img width="1246" height="897" alt="KRT Bank Accounts System Architecture" src="https://github.com/user-attachments/assets/dcbaf102-d01b-4d85-94ff-c2249a0f397e" />

# 🏦 KRT Bank Accounts API — Descrição Técnica

## 🎯 **Objetivo da Aplicação**

A **KRT Bank Accounts API** é uma solução desenvolvida em **.NET 8**, cujo propósito é **gerenciar as contas de clientes do banco KRT**.
Ela permite realizar o CRUD completo das contas — **criação, consulta, atualização, ativação/inativação e exclusão** — garantindo uma arquitetura escalável, organizada e pronta para integração com outros sistemas internos do banco.

Cada conta possui:

* `Id`
* `Name` (nome do titular)
* `Cpf`
* `Status` (Ativa / Inativa)
* `CreatedAt` e `UpdatedAt` (controle temporal)

---

## ⚙️ **Arquitetura e Boas Práticas Aplicadas**

A aplicação foi construída seguindo **DDD (Domain-Driven Design)**, **Clean Architecture** e **SOLID Principles**, com camadas bem definidas:

```
├── _01_Presentation      → Controllers e DTOs (entrada/saída de dados)
├── _02_Application       → Regras de negócio, validações e orquestração de serviços
├── _03_Domain            → Entidades e regras de domínio puro (ex: Account)
├── _04_Infrastructure    → Persistência, cache e mensageria (SQL, Redis, RabbitMQ)
```

### ✅ **Principais práticas aplicadas**

| Conceito                         | Implementação                                                                                                    |
| -------------------------------- | ---------------------------------------------------------------------------------------------------------------- |
| **SOLID**                        | Classes coesas, injeção de dependência, interfaces claras e responsabilidade única.                              |
| **DDD**                          | Separação clara entre domínio (entidades), aplicação (casos de uso) e infraestrutura (implementações concretas). |
| **Clean Code**                   | Métodos curtos, nomes expressivos, retorno padronizado (`Result<T>` e `ResponseDto>`).                           |
| **Padrão MVC**                   | Controllers isolam lógica de negócio, chamando serviços de aplicação.                                            |
| **Padrão Repository**            | Acesso a dados centralizado via `IAccountRepository`.                                                            |
| **Injeção de Dependência (IoC)** | Usando `IServiceCollection` para registrar todos os serviços, repositórios, cache e mensageria.                  |
| **DTOs de Request/Response**     | Separação total entre modelos de entrada/saída e entidades do domínio.                                           |

---

## 🧠 **Tecnologias Utilizadas**

| Tecnologia                   | Finalidade                                                                     |
| ---------------------------- | ------------------------------------------------------------------------------ |
| **.NET 8 (C#)**              | Linguagem e framework principal da aplicação.                                  |
| **Entity Framework Core 8**  | ORM para persistência no banco de dados SQL Server.                            |
| **SQL Server 2022 (Docker)** | Armazenamento principal das contas dos clientes.                               |
| **Redis 7 (Docker)**         | Cache de contas consultadas, reduzindo custo de consultas recorrentes.         |
| **RabbitMQ 3 (Docker)**      | Mensageria para publicar eventos de criação, atualização e exclusão de contas. |
| **xUnit + Moq**              | Testes unitários cobrindo todos os cenários críticos dos serviços.             |
| **Swagger**                  | Documentação automática e interação com endpoints.                             |
| **Docker Compose**           | Orquestração dos containers (SQL, Redis, RabbitMQ, API).                       |

---

## 🚀 **Fluxo de Funcionamento**

### 🔹 **1. Criação de Conta**

1. O cliente envia `POST /api/account`.
2. O serviço valida CPF e nome.
3. Cria a entidade `Account` e salva no SQL Server.
4. Publica evento `account.created` no RabbitMQ.
5. Retorna `ResponseDto` padronizado.

### 🔹 **2. Consulta de Conta**

1. O serviço tenta buscar a conta no **Redis Cache**.
2. Se não encontrar, busca no **SQL Server**, armazena no cache e retorna.
3. Evita custos desnecessários de consultas repetidas.

### 🔹 **3. Atualização / Exclusão**

1. Atualiza dados e status da conta.
2. Publica o evento correspondente no RabbitMQ.
3. Remove o cache da conta afetada para manter consistência.

---

## 🧩 **Testes Unitários**

Os testes foram criados com **xUnit + Moq**, cobrindo:

* Cenários de sucesso (Create, Update, GetById, Delete).
* Validações de CPF duplicado.
* Erros de banco (simulados com exceções).
* Validação de cache e publicação de eventos.

Esses testes garantem **confiabilidade** e **resiliência**, validando os comportamentos de negócio independentemente da infraestrutura.

### 🚀 100% de cobertura do core da aplicação
<img width="1546" height="832" alt="image" src="https://github.com/user-attachments/assets/024ddf25-d923-4d60-9e4a-e6845e51fe20" />

---

## 🧱 **Camada de Infraestrutura**

| Componente                   | Função                                                                          | Implementação                             |
| ---------------------------- | ------------------------------------------------------------------------------- | ----------------------------------------- |
| **RedisCacheService**        | Armazena contas consultadas por ID com TTL configurável via `appsettings.json`. | Implementado via `StackExchange.Redis`.   |
| **RabbitMqMessagePublisher** | Publica eventos JSON para outros serviços internos.                             | Baseado em `RabbitMQ.Client`.             |
| **AccountRepository**        | Executa queries no banco via EF Core.                                           | Repositório isolado da lógica de negócio. |

---

## 🧰 **DevOps**

Os serviços externos necessários para a aplicação (banco, cache e mensageria) podem ser levantados com um único comando usando o **Docker Compose**:

```bash
docker compose up -d
```

Com isso, sobem automaticamente os seguintes containers:

| Serviço           | Descrição                                                         | Endereço                                         |
| ----------------- | ----------------------------------------------------------------- | ------------------------------------------------ |
| 🏦 **SQL Server** | Banco de dados principal (armazenamento das contas)               | `localhost:1433`                                 |
| 🧰 **Redis**      | Cache para otimizar consultas repetidas                           | `localhost:6379`                                 |
| 🐇 **RabbitMQ**   | Broker de mensageria (eventos de criação, atualização e exclusão) | [http://localhost:15672](http://localhost:15672) |

---

## 📊 **Conclusão**

A **KRT Bank Accounts API** é um projeto que demonstra:

* Estrutura profissional em **DDD + Clean Architecture**;
* Aplicação prática de **SOLID, Clean Code e boas práticas REST**;
* Uso de **cache inteligente** (Redis) para reduzir custos e latência;
* **Mensageria** para comunicação entre sistemas e consistência eventual;
* **Testes unitários** assegurando robustez do domínio;
* **Documentação e Docker Compose** para facilitar execução e avaliação.

💡 Essa abordagem reflete um **ambiente real de produção bancária**, com foco em **performance, extensibilidade e manutenção**.

