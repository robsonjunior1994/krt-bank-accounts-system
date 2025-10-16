# krt-bank-accounts-system

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