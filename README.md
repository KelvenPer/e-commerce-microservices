# Sistema de E-commerce com Arquitetura de Microserviços

Este projeto demonstra uma arquitetura de microserviços para uma plataforma de e-commerce, focada em gerenciar estoque e vendas. A solução é construída com .NET Core (C#) e utiliza padrões como API Gateway, comunicação assíncrona com RabbitMQ e autenticação JWT.

## Visão Geral da Arquitetura

A aplicação é dividida em três serviços principais e um projeto de contratos compartilhados:
* **API Gateway**: Roteia as requisições e gerencia a autenticação.
* **EstoqueService**: Responsável pelo catálogo de produtos e controle de inventário.
* **VendasService**: Gerencia a criação de pedidos e a validação de estoque.
* **Shared**: Contratos de mensagens e modelos compartilhados entre os microserviços.

A comunicação síncrona (verificação de estoque) é feita via HTTP, enquanto a comunicação assíncrona (notificação de venda) é realizada através do **RabbitMQ**.

## Pré-requisitos

Para rodar a aplicação, você precisa ter o seguinte instalado:
* [.NET SDK 6.0 ou superior](https://dotnet.microsoft.com/download)
* [Docker Desktop](https://www.docker.com/products/docker-desktop) (para rodar o RabbitMQ e o banco de dados)
* [Visual Studio Code](https://code.visualstudio.com/) ou [Visual Studio](https://visualstudio.microsoft.com/)
* Um cliente REST para testes (ex: [Postman](https://www.postman.com/) ou [Insomnia](https://insomnia.rest/))

## Passo a Passo para Execução

Siga os passos abaixo em sequência para configurar e rodar a aplicação.

### 1. Configurar o Ambiente Local

Abra o terminal e execute os seguintes comandos para iniciar as dependências externas:

**a. Iniciar o RabbitMQ com Docker**
```bash
docker run -d --name my-rabbit -p 5672:5672 -p 15672:15672 rabbitmq:3-management

Você pode verificar o painel de administração em http://localhost:15672 (usuário/senha: guest/guest).

b. Iniciar o SQL Server LocalDB
Os projetos estão configurados para usar o (localdb)\\mssqllocaldb. Não é necessário iniciar manualmente. As migrações criarão os bancos de dados automaticamente.

2. Rodar as Migrações do Banco de Dados
Navegue até os diretórios de cada microserviço e execute as migrações para criar as tabelas no banco de dados.

a. EstoqueService
cd src/EstoqueService
dotnet ef database update

b. VendasService
cd src/VendasService
dotnet ef database update

3. Executar os Microserviços e o API Gateway
Você pode iniciar os projetos de três maneiras:

Opção 1: Terminal (Recomendado)
Abra três janelas de terminal separadas e execute um comando em cada uma:

Terminal 1 (EstoqueService)
cd src/EstoqueService
dotnet run --urls "http://localhost:5001"

Terminal 2 (VendasService)
cd src/VendasService
dotnet run --urls "http://localhost:5002"

Terminal 3 (API Gateway)
cd src/ApiGateway
dotnet run --urls "http://localhost:5000"

Opção 2: Visual Studio
Abra a solução (.sln) no Visual Studio, clique com o botão direito na solução no Solution Explorer e selecione "Set Startup Projects...". Escolha a opção "Multiple startup projects" e marque os três projetos (ApiGateway, EstoqueService, VendasService) para iniciar.

4. Executar os Testes Unitários
Navegue até a pasta raiz do projeto e execute o comando:
dotnet test

Este comando irá descobrir e executar todos os testes nos projetos EstoqueService.Tests e VendasService.Tests.

5. Testar as Funcionalidades via API
Use seu cliente REST para interagir com os endpoints.

Passo 5.1: Obter o Token de Autenticação
Todas as rotas do API Gateway estão protegidas. Primeiro, obtenha um token JWT.

Método: POST

URL: http://localhost:5000/login

Corpo da Requisição: (vazio)

Resultado: Copie o token retornado na resposta.

Passo 5.2: Cadastrar um Produto (EstoqueService)
Use o token para autenticar a requisição.

Método: POST

URL: http://localhost:5000/estoque/produtos

Headers:

Authorization: Bearer [SEU_TOKEN]

Corpo da Requisição (JSON):
{
  "nome": "Laptop Gamer",
  "descricao": "Laptop de alta performance para jogos.",
  "preco": 7500.00,
  "quantidade": 15
}

Resultado: Resposta 201 Created com os dados do produto.

Passo 5.3: Criar um Pedido de Venda (VendasService)
Este passo acionará a comunicação entre os serviços.

Método: POST

URL: http://localhost:5000/vendas/pedidos

Headers:

Authorization: Bearer [SEU_TOKEN]

Corpo da Requisição (JSON):
{
  "produtoId": 1, 
  "quantidade": 2
}

Nota: O produtoId deve ser o ID do produto que você cadastrou no passo anterior.

Fluxo de Eventos:

O VendasService receberá a requisição.

Ele fará uma chamada síncrona (via HTTP) ao EstoqueService para verificar a quantidade disponível.

Se o estoque for suficiente, o pedido será salvo no banco de dados do VendasService.

Uma mensagem será publicada no RabbitMQ para notificar o EstoqueService.

O EstoqueService, agindo como um consumidor, receberá a mensagem e atualizará a quantidade do produto no seu próprio banco de dados.

Você pode verificar a redução da quantidade do produto consultando-o novamente em http://localhost:5000/estoque/produtos/{id}.