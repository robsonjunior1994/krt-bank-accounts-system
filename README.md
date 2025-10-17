# krt-bank-accounts-system

<details>

<summary>SQL Server</summary>

## 🏦 **Banco de Dados — Configuração (SQL Server via Docker)**

### 🧩 **Passo a passo**

#### 1️⃣ Baixar a imagem oficial do SQL Server

```bash
docker pull mcr.microsoft.com/mssql/server:2022-latest
```

---

#### 2️⃣ Rodar o container do SQL Server

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Root@12345" -p 1433:1433 --name sqlserver2022 -d mcr.microsoft.com/mssql/server:2022-latest
```

🔹 Esse comando vai:

* Aceitar o contrato da Microsoft (`ACCEPT_EULA=Y`);
* Definir a senha do administrador (`Root@12345`);
* Expor a porta **1433** (padrão do SQL Server);
* Criar o container chamado **sqlserver2022**;
* Usar a imagem **2022-latest**.

---

## 🧱 **Executando Migrações (Entity Framework Core)**

### ⚠️ Instale a ferramenta se ainda não tiver:

```bash
dotnet tool install --global dotnet-ef --version 8.0.21
```

---

### 1️⃣ Acesse a pasta da API

```bash
cd backend/KRT.BankAccounts.Api
```

---

### 2️⃣ (Opcional) Criar a primeira migração

> ⚠️ Esse passo só é necessário se quiser **gerar a migration do zero**.
> Caso ela já exista no repositório, pule para o passo 3.

```bash
dotnet ef migrations add InitialCreate --project ../KRT.BankAccounts.Api --startup-project ../KRT.BankAccounts.Api --output-dir ../KRT.BankAccounts.Api/_04_Infrastructure/Migrations
```

---

### 3️⃣ Aplicar as migrações ao banco

```bash
dotnet ef database update --project ../KRT.BankAccounts.Api --startup-project ../KRT.BankAccounts.Api
```

---

### ✅ Resultado esperado

Após o comando acima:

* O banco `KRTBankAccounts` será criado no container SQL Server;
* As tabelas (`Accounts`, etc.) serão aplicadas automaticamente;
* Você poderá conectar ao banco usando o SQL Server Management Studio (SSMS), Azure Data Studio ou DBeaver.

🧠 **Credenciais padrão:**

* **Servidor:** `localhost,1433`
* **Usuário:** `sa`
* **Senha:** `Root@12345`

---

## 🧪 **Testando a conexão rapidamente**

Você pode testar a conexão diretamente com:

```bash
sqlcmd -S localhost,1433 -U sa -P Root@12345 -d KRTBankAccounts
```

Se aparecer o prompt `1>`, o banco está conectado corretamente

ctrl + c para sair.

</details>

<details>
<summary>REDIS para cache</summary>


# 🧰 Cache (Redis) — Setup local com Docker

## 1) Baixar a imagem do Redis

```bash
docker pull redis:7-alpine
```

> `alpine` é levinho e ótimo para desenvolvimento.

---

## 2) Subir o container do Redis

### COM senha (recomendado)

```bash
docker run -d --name redis-cache -p 6379:6379  -e REDIS_PASSWORD=devpassword123 redis:7-alpine  redis-server --requirepass devpassword123
```

> **Dica:** quer persistência entre reinícios? Adicione `-v redisdata:/data` ao comando.

---

## 3) Testar o Redis (opcional)

### Com senha:

```bash
docker exec -it redis-cache redis-cli -a devpassword123 ping
# PONG
```

---

## 5) Subir a API

```bash
dotnet restore
dotnet build
dotnet run
```

Endpoint de saúde rápido (exemplo):
`GET http://localhost:5000/api/account` (ajuste conforme sua porta)

---

## 6) (Opcional) Docker Compose

Se preferir **subir tudo com um arquivo**:

```yaml
# docker-compose.yml
services:
  redis:
    image: redis:7-alpine
    container_name: redis-cache
    command: ["redis-server", "--requirepass", "devpassword123"]
    ports:
      - "6379:6379"
    volumes:
      - redisdata:/data
    restart: unless-stopped

volumes:
  redisdata:
```

Subir:

```bash
docker compose up -d
```

Use em `appsettings.json`:

```json
"RedisConnection": "localhost:6379,password=devpassword123"
```

---

## 7) Troubleshooting rápido

* **`ECONNREFUSED`**: verifique se a porta `6379` está livre e o container está rodando: `docker ps`.
* **`NOAUTH Authentication required`**: você subiu o Redis **com senha**; adicione `password=...` no connection string.
* **TimeOut**: adicione `abortConnect=false` no connection string para evitar abortar na primeira tentativa:

  ```json
  "RedisConnection": "localhost:6379,password=devpassword123,abortConnect=false"
  ```

---
</details>

<details>
<summary> RabbitMQ </summary

Perfeito, Robson 🔥 — hora de configurar a **mensageria** pra completar o desafio com chave de ouro!
Você já preparou tudo certinho pra isso: arquitetura em camadas, injeção de dependência, e até um publisher mockado.
Agora vamos fazer o RabbitMQ rodar **de verdade**, mas mantendo o projeto limpo e desacoplado.

---

## 🧩 1️⃣ Criar container RabbitMQ (com painel de controle)

No seu passo a passo do projeto (tipo o que você fez pro SQL e Redis), adiciona esta parte 👇

```bash
# 🐇 Baixar a imagem do RabbitMQ com o painel de administração
docker pull rabbitmq:3-management

# 🚀 Rodar o container com painel web habilitado
docker run -d --name rabbitmq  -p 5672:5672  -p 15672:15672  -e RABBITMQ_DEFAULT_USER=guest -e RABBITMQ_DEFAULT_PASS=guest rabbitmq:3-management
```

📍 **Acesso ao painel web:**
👉 [http://localhost:15672](http://localhost:15672)
Usuário: `guest`
Senha: `guest`

---

## 🧩 7️⃣ Publicar eventos na aplicação

Agora em qualquer serviço (ex: `AccountService`), você pode publicar eventos como:

```csharp
await _publisher.PublishAsync("account.created", new
{
    account.Id,
    account.Name,
    account.Cpf,
    Status = account.Status.ToString()
});
```

ou

```csharp
await _publisher.PublishAsync("account.deleted", new { account.Id });
```

---

## 🧩 8️⃣ Verificar publicação

Acesse o painel RabbitMQ:
🔗 [http://localhost:15672](http://localhost:15672)
→ Vá em **Exchanges → krt.bank.exchange**
→ Clique em **Queues → krt.account.events**
→ Clique em **Get messages**

Você verá a mensagem JSON chegando!

---

</details>
