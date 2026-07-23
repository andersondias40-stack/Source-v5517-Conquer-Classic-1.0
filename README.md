# Source-v5517-Conquer-Classic-1.0 #
Projeto Source Conquer v5517 reduzido para 5095 - Mantido nostálgico Classico 1.0
- Baseado na versão 5517
- Código escrito em C#
- Contém AccServer (autenticação) e GameServer (lógica do jogo)
- Repleto de correções, otimizações e melhorias de estabilidade
- Sistema de eventos automáticos, shop, PvP, guilds, etc.
- Banco de dados MySQL incluso no formato `.zq`

## Estrutura do Projeto

```
AccServer/     - Servidor de autenticação (login, conexão com DB)
GameServer/    - Toda lógica do jogo, eventos, controle de players
Database/      - Arquivo .zq com estrutura MySQL para uso direto
My channel/https://www.youtube.com/@BlackoutlFKZ
```
## Como Ligar o servidor

1. Necessário o Visual Studio

2. No `AccServer`, edite a string de conexão:
   - Exemplo: `Database=zq;Uid=root;Password=anderson
`
3. No `GameServer`, altere a senha `anderson` para a mesma senha do banco sua caso houver

4. Importe o banco de dados `.zq` no seu (Navicat)

5. Você precisa colocar o seu ipv4 no arquivo servers do (Navicat) - Não mude mais nada dos IPs da Source e Nome.

6. Encontre na source ( DataHolderTable.cs , MySqlCommand.cs , MySqlReader.cs ) adicione seu Password e data base.

7. Compile os projetos no Visual Studio, compilar o `AccServer.exe` e `GameServer.exe

8. Execute `AccServer.exe` e `GameServer.exe` que foram compilados por você.

 Seu (AccServer.exe) compilado por você está aqui, exemplo > C:\Conquer_Classic\Source_5517\AccServer\bin\Debug

 Seu (GameServer.exe) compilado por você está aqui, exemplo > C:\Conquer_Classic\Source_5517\GameServer\bin\Debug

## Observações útilitários Necessários ##

- ** Instale o ( appserve )

- ** Instale o ( Navicat )

- ** Instale o ( Flash Player6 ) - Ele é necessário para você e seus jogadores conseguir entrar no servidor.

- ** Lembrando o ( DatCryptor ) está incluido na source e na cliente
          caso deseja editar o monstertpe.dat e itemtype.dat é só arrastar e soltar.
    Seu anti-virús pode acusar como um falso dispositivo colocando ele como uma ameaça e deleta-lo.


- ** ( Siga os passos corretos e não irá ter problema ao ligar o servidor, use o que se encontra na pastas )
