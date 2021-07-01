# Microsservicos
Projeto de estudos de criação de uma arquitetura de microsserviços em .Net Core 3.1
Projeto de laboratório pessoal para aplicar os conhecimentos que estou adquirindo a respeito de microsserviços.
De inicio será feito 5 API's Identidade(gerenciamento de usuários), produtos, clientes, pedidos e estoque, a ideia é implementar esses serviços onde cada um seja responsável somente pela sua regra
e cada API terá seu banco de dados, que de início será o SQL Server, mas pretendo utilzar o MongoDB para algum serviço onde os dados não são tão sensíveis por exemplo carrinho de compra
A comunicação entre as mesmas será através do rabbitMQ que será o orquestrador das mensagens. Todos os serviços irá rodar encima do docker
