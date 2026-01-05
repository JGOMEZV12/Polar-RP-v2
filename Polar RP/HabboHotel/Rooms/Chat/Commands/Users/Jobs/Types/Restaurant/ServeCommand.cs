using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Pathfinding;
using Polar.HabboHotel.Rooms.Chat.Styles;
using Polar.HabboRoleplay.Food;
using Polar.HabboHotel.Groups;
using Polar.HabboRoleplay.Misc;

namespace Polar.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Restaurant
{
    class ServeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_restaurant_serve"; }
        }

        public string Parameters
        {
            get { return "%name%"; }
        }

        public string Description
        {
            get { return "Sirve la comida o la bebida deseada delante de usted."; }
        }

        public async Task Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            RoomUser User = Session.GetRoomUser();
            Group Group = GroupManager.GetJob(Session.GetRoleplay().JobId);
            GroupRank GroupRank = GroupManager.GetJobRank(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank);
            #endregion

            #region Conditions
            if (User == null)
                return;

            if (Params.Length == 1)
            {
                Session.SendWhisper("Por favor escriba: serve (item) Sólo puede servir los siguientes elementos: " + FoodManager.GetServableItems(Session) + "!", 1);
                return;
            }

            string FoodName = Params[1].ToString();
            Food Food = FoodManager.GetFoodAndDrink(FoodName);

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Usted debe estar trabajando para hacer esto trabajando", 1);
                return;
            }

            if (Session.GetRoleplay().IsDead)
            {
                Session.SendWhisper("¡No puedes servir comida o bebidas mientras estés muerto!", 1);
                return;
            }

            if (Session.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("¡No puedes servir comida o bebidas mientras estás encarcelado!", 1);
                return;
            }

            if (!GroupManager.HasJobCommand(Session, "serve"))
            {
                Session.SendWhisper("¡No tienes permiso para usar este comando!", 1);
                return;
            }

            if (!GroupRank.CanWorkHere(Session.GetHabbo().CurrentRoomId))
            {
                Session.SendWhisper("Esta no es una de sus salas de trabajo, Sólo puede trabajar en RoomID (s): " + String.Join(",", GroupRank.WorkRooms) + ".", 1);
                return;
            }

            if (Food == null)
            {
                Session.SendWhisper("¡Esto no es un tipo válido de comida o bebida! Sólo puede servir: " + FoodManager.GetServableItems(Session) + "!", 1);
                return;
            }

            if (!FoodManager.CanServe(User))
            {
                Session.SendWhisper("¡Encuentra una buena mesa para servir!", 1);
                return;
            }

            if (Food.Type == "food" && !GroupManager.HasJobCommand(Session, "food"))
            {
                Session.SendWhisper("¡Lo siento! Sólo puede servir: " + FoodManager.GetServableItems(Session) + "!", 1);
                return;
            }

            if (Food.Type == "drink" && !GroupManager.HasJobCommand(Session, "drinks"))
            {
                Session.SendWhisper("¡Lo siento! Sólo puede servir: " + FoodManager.GetServableItems(Session) + "!", 1);
                return;
            }

            /*if (!Food.Servable)
            {
                if (Food.Type == "drink")
                    Session.SendWhisper("¡Lo siento! Sólo puede servir: " + FoodManager.GetServableItems(Session) + "!", 1);
                else
                    Session.SendWhisper("¡Lo siento! Sólo puedes servir: " + FoodManager.GetServableItems(Session) + "!", 1);
                return;
            }*/
            #endregion

            #region Execute
            double MaxHeight = 0.0;
            Item ItemInFront;
            if (Room.GetGameMap().GetHighestItemForSquare(User.SquareInFront, out ItemInFront))
            {
                if (ItemInFront != null)
                    MaxHeight = ItemInFront.TotalHeight;
            }

            Session.Shout(Food.ServeText, 4);
            RoleplayManager.PlaceItemToRoom(Session, Food.ItemId, 0, User.SquareInFront.X, User.SquareInFront.Y, MaxHeight, User.RotBody, false, Room.Id, false, Food.ExtraData, true);
            #endregion
        }
    }
}