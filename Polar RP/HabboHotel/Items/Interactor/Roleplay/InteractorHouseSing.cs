using System.Drawing;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Pathfinding;
using System;
using Polar.HabboRoleplay.Houses;
using Polar.HabboHotel.Rooms;
using Polar.HabboRoleplay.Misc;

namespace Polar.HabboHotel.Items.Interactor
{
    public class InteractorHouseSing : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            // Verifica si el jugador está en la habitación y tiene acceso a los datos
            if (Session.GetRoomUser() == null)
                return;

            var User = Session.GetRoomUser();

            // Verifica si el WebSocket está conectado
            if (Session.GetRoleplay().WebSocketConnection != null)
            {
                // Verifica si el usuario está en la posición adecuada para interactuar con el ítem
                if (Rooms.Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
                {
                    // Obtiene la casa asociada al ítem (cartel)
                    var House = PolarEnvironment.GetGame().GetHouseManager().GetHouseBySignItem(Item.Id);

                    if (House != null)
                    {
                        // Obtiene el propietario de la casa
                        string Owner = GetHouseOwner(House.OwnerId);

                        if (Owner != null)
                        {
                            // Actualiza los datos de la casa en el rol del jugador
                            Session.GetRoleplay().HouseSignId = House.ItemId;
                            Session.GetRoleplay().HouseOwner = Owner;
                            Session.GetRoleplay().ViewHouse = true;

                            // Ejecuta el evento web para abrir la casa
                            PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_house", "open");
                        }
                        else
                        {
                            // Si no se encuentra al propietario, muestra un mensaje de error
                            Session.SendNotification("((Ha ocurrido un problema al buscar la información de la casa o terreno. Contacte con un Administrador))");
                        }
                    }
                    else
                    {
                        // Si no se encuentra la casa, notifica un error
                        Session.SendNotification("((No se pudo encontrar la casa asociada a este cartel. Contacte con un Administrador))");
                    }
                }
                else
                {
                    // Si el jugador no está en la posición correcta, lo mueve
                    User.MoveTo(Item.SquareBehind);
                }
            }
            else
            {
                // Si el WebSocket está desconectado, notifica al usuario
                Session.SendWhisper("((El WebSocket del servidor está Offline. Por favor contacte con un Administrador))", 1);
            }
        }

        // Método auxiliar para obtener el propietario de la casa
        private string GetHouseOwner(int ownerId)
        {
            var habbo = PolarEnvironment.GetHabboById(ownerId);

            // Si el jugador está online
            if (habbo != null)
                return habbo.Username;

            // Si el jugador no está online, intenta buscar por nombre de usuario en la base de datos
            return PolarEnvironment.GetUsernameById(ownerId);
        }

        public void OnWiredTrigger(Item Item)
        {
        }
    }
}
