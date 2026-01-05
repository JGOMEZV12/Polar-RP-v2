using System.Drawing;
using Polar.HabboHotel.GameClients;
using Polar.Communication.Packets.Outgoing.Polls;
using Polar.HabboHotel.Pathfinding;
using Polar.HabboRoleplay.Misc;
using Polar.HabboHotel.Rooms;

namespace Polar.HabboHotel.Items.Interactor
{
    public class InteractorAgua : IFurniInteractor
    {
        private const int CostoBebida = 150;
        private const int EnergiaRestaurada = 30;

        public void OnPlace(GameClient session, Item item) { }

        public void OnRemove(GameClient session, Item item) { }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            #region Validaciones

            // Verificar si el jugador tiene la energía completa
            if (session.GetRoleplay().CurEnergy >= session.GetRoleplay().MaxEnergy)
            {
                session.SendWhisper("Al parecer no tienes más sed (Tu barra de energía está llena).");
                return;
            }

            // Verificar si el jugador tiene suficientes créditos
            if (session.GetHabbo().Credits < CostoBebida)
            {
                session.Shout("* Uff, olvidé guardar dinero en mi billetera para el bebedero *", 15);
                return;
            }

            // Verificar si el jugador está en una sala válida
            if (session.GetRoomUser() == null)
            {
                session.SendWhisper("No puedes interactuar sin estar en una sala.");
                return;
            }

            #endregion

            var user = session.GetRoomUser();

            // Verificar si el jugador está en la misma coordenada que el bebedero
            if (Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y))
            {
                // Actualizar el estado del jugador y descontar el dinero
                session.GetRoleplay().UsingAgua = true;
                session.Shout("* Se inclina en el bebedero y bebe agua [-150$] *", 11);
                session.GetRoleplay().CurEnergy += EnergiaRestaurada;
                session.GetHabbo().Credits -= CostoBebida;
                session.GetHabbo().UpdateCreditsBalance();

                // Mover al usuario frente al bebedero
                user.MoveTo(item.SquareInFront);

                // Marcar que el jugador no está trabajando
                session.GetRoleplay().IsWorking = false;

                #region Bank Company Balance
                // Descontar el dinero de la empresa (ejemplo: supermercado)
                RoleplayManager.GiveMoneyToCompany(12, session, "supermarket", true, CostoBebida);
                #endregion Bank Company Balance
            }
            else
            {
                session.SendWhisper("¡Debes estar más cerca del bebedero para beber agua!");
            }
        }

        public void OnWiredTrigger(Item item) { }
    }
}
