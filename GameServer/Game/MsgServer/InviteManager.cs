using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COServer.Game.MsgServer
{
    public static class InviteManager
    {
 
        // Lista estática que armazena os jogadores interessados
        private static List<Client.GameClient> playersInterestedInInvite = new List<Client.GameClient>();

        // Método para adicionar um jogador à lista de interessados
        public static void AddToInviteList(Client.GameClient client)
        {
            if (!playersInterestedInInvite.Contains(client))
            {
                Console.WriteLine(playersInterestedInInvite);
                playersInterestedInInvite.Add(client);
                client.SendSysMesage("Você foi adicionado à lista de convite!");
                Console.WriteLine(playersInterestedInInvite);
            }
            else
            {
                client.SendSysMesage("Você já está na lista de convite.");
            }
        }

        // Método para mover todos os jogadores da lista para as coordenadas de um jogador específico
        public static void MoveAllToPlayer(Client.GameClient client)
        {
            ushort x = (ushort)client.Player.X;
            ushort y = (ushort)client.Player.Y;
            ushort map = (ushort)client.Player.Map;

            foreach (var user in playersInterestedInInvite)
            {
                user.Teleport(x, y, map);
            }

            // Limpa a lista de jogadores após o movimento
            playersInterestedInInvite.Clear();
        }

        // Método para verificar se o jogador está na lista de interessados
        public static bool IsInInviteList(Client.GameClient client)
        {
            return playersInterestedInInvite.Contains(client);
        }
    }
}
