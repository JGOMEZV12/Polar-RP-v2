using System;
using System.Linq;
using Polar.HabboHotel.Rooms.Pathfinding;
using Polar.Utilities;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Rooms;
using Polar.HabboRoleplay.Misc;
using Polar.HabboHotel.Items.Crafting;
using MoreLinq;
using Polar.HabboRoleplay.Turfs;
using Polar.HabboHotel.Groups;

namespace Polar.HabboHotel.Items.Interactor
{
    public class InteractorPorro : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            // Verificación básica de la sesión y el estado del usuario
            if (Session == null || Session.GetHabbo() == null || Session.GetRoleplay() == null)
            {
                Session.SendWhisper("Error: sesión inválida", 6);
                return;
            }

            if (!Session.GetHabbo().CurrentRoom.TurfEnabled)
            {
                Session.SendWhisper("¡Esta sala no es un territorio!", 6);
                return;
            }

            // Recuperar la información del Turf, Pandilla, y mesa de marihuana
            Room Room = Session.GetHabbo().CurrentRoom;
            Group Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);
            Turf Turf = PolarEnvironment.GetGame().GetGangTurfsManager().GetTurfById(Session.GetHabbo().CurrentRoomId);
            Item? BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "table_weed_wed" && x.Coordinate == Session.GetRoomUser().SquareInFront);

            if (Turf == null)
            {
                Session.SendWhisper("¡No estás en ningún territorio!", 6);
                return;
            }

            // Verificar que el usuario está en la posición correcta
            RoomUser? User = Item.GetRoom()?.GetRoomUserManager()?.GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null) return;

            // Verificar si el jugador está tocando el ítem o no
            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                if (Item.ExtraData == "0" || Item.ExtraData == "1")
                {
                    if (User.CanWalk)
                        User.MoveTo(Item.SquareInFront);
                }
                return;
            }

            // Validar créditos, estado del jugador, y pertenencia a pandilla
            if (Session.GetHabbo().Credits < 500)
            {
                Session.SendWhisper("¡No tienes 500$ para invertir!", 6);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer esto mientras estás muerto!", 6);
                return;
            }

            if (Session.GetRoleplay().IsWorkingOut)
            {
                Session.SendWhisper("Deja de trabajar para poder fabricar los porros de marihuana", 6);
                return;
            }

            if (Session.GetRoleplay().GangId <= 0)
            {
                Session.SendWhisper("¡No perteneces a ninguna pandilla para fabricar porros!", 1);
                return;
            }

            if (Session.GetRoleplay().Weedmateria < 10)
            {
                Session.SendWhisper("¡Necesitas 10 semillas de marihuana para hacer porros de marihuana!", 1);
                return;
            }

            // Validar que esté en su territorio
            if (Turf.GangId < Session.GetRoleplay().GangId)
            {
                Session.SendWhisper("¡No estás en tu territorio para vender drogas!", 1);
                return;
            }

            if (Session.GetRoleplay().GangId < Turf.GangId)
            {
                Session.SendWhisper("¡Tu pandilla debe ser dueña del laboratorio para cosechar en él!", 1);
                return;
            }

            // Verificar si está frente a la mesa de marihuana
            if (BTile == null)
            {
                Session.SendWhisper("¡Debes estar frente a la mesa de marihuana para hacer los porros!", 1);
                return;
            }

            // Iniciar la fabricación de los porros
            if (Item.ExtraData == "")
                Item.ExtraData = "0";

            if (Item.ExtraData == "0")
            {
                int Minutes = 1;

                User.ClearMovement(true);
                User.SetRot(Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

                // Indicar que la mesa está ocupada y comenzar la fabricación
                Item.ExtraData = "1";
                Item.UpdateState(false, true);
                Item.RequestUpdate(100 * Minutes, true);
                Session.Shout("* Saca un papel y comienza a hacer el porro [-500$ Inversión] [-10 Semillas de marihuana]*", 6);
                Session.GetHabbo().Credits -= 500;
                Session.GetRoleplay().Weedmateria -= 10;
                Session.GetHabbo().UpdateCreditsBalance();

                // Crear un hilo para el proceso de fabricación
                new Thread(() =>
                {
                    User.CanWalk = false;

                    if (User.CurrentEffect != 595 && Session.GetRoleplay().EquippedWeapon == null)
                        User.ApplyEffect(595);

                    Thread.Sleep(10000); // Tiempo de fabricación de 10 segundos

                    if (User.CurrentEffect != 595 && Session.GetRoleplay().EquippedWeapon == null)
                        User.ApplyEffect(0);

                    // Verificar estado de la sesión y proceder con la recompensa
                    if (Session != null && Session.GetRoleplay() != null && Session.GetHabbo() != null)
                    {
                        ChooseReward(Session);
                        Item.ExtraData = "0"; // Restablecer el estado de la mesa
                    }

                    User.CanWalk = true; // Permitir el movimiento nuevamente
                }).Start();
            }
            else
            {
                Session.SendWhisper("¡ESPERE POR FAVOR, LA MESA ESTÁ OCUPADA...!", 6);
            }
        }

        // Método no utilizado (OnWiredTrigger) 
        public void OnWiredTrigger(Item Item)
        {
        }

        // Método para elegir la recompensa después de la fabricación
        public void ChooseReward(GameClient Session)
        {
            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("¡No puedes hacer esto mientras estás muerto!", 6);
                return;
            }

            var Random = new CryptoRandom();
            int TotalCraftingItems = CraftingManager.CraftableItems.Count;
            int Chance = Random.Next(1, 5);
            int SecondChance = Random.Next(1, 5);

            // Ajuste de probabilidades para la recompensa
            if (SecondChance < 4 && Chance > TotalCraftingItems)
                Chance = Random.Next(1, TotalCraftingItems + 1);

            #region Weed
            else if (Chance > 1 && Chance <= 6)
            {
                int Amount = Random.Next(10, 15);

                // Recompensa para el jugador
                Session.GetRoleplay().Weed += Amount;
                Session.Shout("*¡Felicidades fabricaste! " + Amount + "g de marihuana *", 6);
                Session.SendWhisper("*¡Ve a tu casa y guárdala en el baúl! [TODO ESTO ES ILEGAL]*", 1);
            }
            #endregion

            #region No Reward
            else
            {
                Session.SendWhisper("*¡Las semillas están podridas, no sirven para la producción de porros!*", 1);
            }
            #endregion
        }
    }
}
