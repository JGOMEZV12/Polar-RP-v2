using System.Drawing;
using Polar.HabboHotel.GameClients;
using Polar.Communication.Packets.Outgoing.Polls;
using Polar.HabboHotel.Pathfinding;
using Polar.HabboRoleplay.Misc;

namespace Polar.HabboHotel.Items.Interactor
{
    public class InteractorCajero : IFurniInteractor
    {
        // Método que se ejecuta cuando el item es colocado en la sala
        public void OnPlace(GameClient session, Item item)
        {
        }

        // Método que se ejecuta cuando el item es removido de la sala
        public void OnRemove(GameClient session, Item item)
        {
        }

        // Método que se ejecuta cuando el jugador interactúa con el item
        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            #region Verificaciones de condiciones

            // Verificar si el jugador tiene cooldown para usar el cajero
            if (session.GetRoleplay().TryGetCooldown("cajerodiamonds", true))
            {
                session.SendWhisper("Debe esperar hasta que puedas hacer esto nuevamente!", 1);
                return;
            }

            // Verificar si el jugador tiene al menos 20 diamantes
            if (session.GetHabbo().Diamonds < 20)
            {
                session.SendWhisper("Necesitas 20 rubies para usar este cajero. ¡Cómpralos en la web!", 34);
                return;
            }

            #endregion

            // Verificar si el jugador está en la misma coordenada que el cajero
            if (session.GetRoomUser() == null)
                return;

            var user = session.GetRoomUser();

            // Verificar si el jugador está tocando el cajero
            if (Rooms.Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y))
            {
                // Activar la interacción con el cajero
                session.GetRoleplay().UsingCajero = true;

                // Informar al jugador que ha realizado la transacción
                session.Shout("* Introduce 20 rubies en la ranura y los cambia por: $100.000 *", 11);

                // Descontar los diamantes y agregar los créditos
                session.GetHabbo().Diamonds -= 20;
                session.GetHabbo().UpdateDiamondsBalance();
                session.GetHabbo().Credits += 100000;
                session.GetHabbo().UpdateCreditsBalance();

                // Mover al jugador a la posición del cajero
                user.MoveTo(item.SquareInFront);

                #region Banco - Balance de la compañía
                // Actualizar el balance del banco de la empresa
                RoleplayManager.GiveMoneyToCompany(9, session, "bank", true, 150);
                #endregion

                // Crear cooldown para evitar abusos
                session.GetRoleplay().CooldownManager.CreateCooldown("cajerodiamonds", 1000, 5); // 5 segundos de cooldown
            }
            else
            {
                session.SendWhisper("¡Debes estar frente al cajero para usarlo!", 1);
            }
        }

        // Método que se ejecuta cuando el item es activado por un trigger (no se está utilizando aquí)
        public void OnWiredTrigger(Item item)
        {
        }
    }
}
