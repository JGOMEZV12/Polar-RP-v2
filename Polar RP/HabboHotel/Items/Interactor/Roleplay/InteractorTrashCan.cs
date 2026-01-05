using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Polar.Utilities;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Rooms;
using Polar.HabboRoleplay.Misc;
using Polar.HabboHotel.Rooms.Pathfinding;
using Polar.HabboHotel.Users.Effects;
using MoreLinq;
using Polar.HabboHotel.Groups;

namespace Polar.HabboHotel.Items.Interactor
{
    public class InteractorTrashCan : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                if (Item.ExtraData == "" || Item.ExtraData == "0")
                    if (User.CanWalk)
                        User.MoveTo(Item.SquareInFront);
            }
            else
            {
                #region Conditions                

                #region Group Conditions
                List<Group> Groups = PolarEnvironment.GetGame().GetGroupManager().GetJobsForUser(Session.GetHabbo().Id);

                if (Groups.Count <= 0)
                {
                    Session.SendWhisper("No tienes ningún trabajo para hacer eso.", 1);
                    return;
                }

                int GroupNumber = -1;

                if (Groups[0].GType != 2)
                {
                    if (Groups.Count > 1)
                    {
                        if (Groups[1].GType != 2)
                        {
                            Session.SendWhisper("((No perteneces a ningún trabajo usar ese comando))", 1);
                            return;
                        }
                        GroupNumber = 1; // Segundo indicie de variable
                    }
                    else
                    {
                        Session.SendWhisper("((No perteneces a ningún trabajo para usar ese comando))", 1);
                        return;
                    }
                }
                else
                {
                    GroupNumber = 0; // Primer indice de Variable Group
                }

                Session.GetRoleplay().JobId = Groups[GroupNumber].Id;
                Session.GetRoleplay().JobRank = Groups[GroupNumber].Members[Session.GetHabbo().Id].UserRank;
                #endregion

                #region Extra Conditions            
                // Existe el trabajo?
                if (!GroupManager.JobExists(Session.GetRoleplay().JobId, Session.GetRoleplay().JobRank))
                {
                    Session.GetRoleplay().TimeWorked = 0;
                    Session.GetRoleplay().JobId = 0; // Desempleado
                    Session.GetRoleplay().JobRank = 0;

                    //Room.Group.DeleteMember(Session.GetHabbo().Id);// OJO ACÁ

                    Session.SendWhisper("Lo sentimos, ese trabajo no existe. Te hemos removido ese trabajo.", 1);
                    return;
                }

                if (!GroupManager.HasJobCommand(Session, "basurero"))
                {
                    Session.SendWhisper("Debes tener el trabajo de Basuero recoger contenedores de basura.", 1);
                    return;
                }

                #endregion

                #region Basic Conditions
                if (Session.GetRoleplay().Cuffed)
                {
                    Session.SendWhisper("No puedes hacer eso mientras estás esposad@", 1);
                    return;
                }
                if (!Session.GetRoomUser().CanWalk)
                {
                    Session.SendWhisper("Al parecer no puedes ni moverte para hacer eso.", 1);
                    return;
                }
                if (Session.GetRoleplay().Pasajero)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras vas de Pasajer@!", 1);
                    return;
                }
                if (Session.GetRoleplay().IsDead)
                {
                    Session.SendWhisper("¡No puedes hacer esto mientras estás muert@!", 1);
                    return;
                }
                if (Session.GetRoleplay().IsJailed)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras estás muert@!", 1);
                    return;
                }
                if (Session.GetRoleplay().DrivingCar)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras conduces!", 1);
                    return;
                }
                if (Session.GetRoleplay().EquippedWeapon != null)
                {
                    Session.SendWhisper("¡No puedes hacer eso mientras equipas un arma!", 1);
                    return;
                }
                if (!Session.GetRoleplay().IsWorking)
                {
                    Session.SendWhisper("¡Debes trabajar de recolector de basura para hacer eso!", 1);
                    return;
                }
                if (Session.GetRoleplay().BasuTrashCount >= 15)
                {
                    Session.SendWhisper("¡Ya tienen 15 contenedores recogidos! Vuelvan al Basurero a ':descargarcamion' para recibir su paga.", 1);
                    return;
                }
                #endregion

                #region Basurero Conditions
               /* if (Session.GetRoleplay().BasuTeamId <= 0)
                {
                    Session.SendWhisper("Para recolectar basura necesitas un Compañero que conduzca el Camión.", 1);
                    return;
                }*/
               /* GameClient TeamChofer = PolarEnvironment.GetGame().GetClientManager().GetClientByUserID(Session.GetRoleplay().BasuTeamId);
                if (TeamChofer == null)
                {
                    Session.SendWhisper("Al parecer tu compañero de Basurero se ha ido y han Fracasado el Recorrido.", 1);
                    #region Retornamos Vars
                    Session.GetRoleplay().BasuTeamId = 0;
                    Session.GetRoleplay().BasuTeamName = "";
                    Session.GetRoleplay().BasuTrashCount = 0;
                    Session.GetRoleplay().IsBasuPasaj = false;
                    Session.GetRoleplay().IsBasuChofer = false;
                    #endregion
                    return;
                }
                if (!TeamChofer.GetRoleplay().IsBasuChofer)
                {
                    Session.SendWhisper("Tu compañero de Basurero debe estar conduciendo el Camión de Basura. Recuerda que puedes usar ':nobasurero' para cancelar la misión.", 1);
                    return;
                }*/
                #endregion
                #endregion

                if (Item.ExtraData == "")
                    Item.ExtraData = "0";

                if (Item.ExtraData == "0")
                {
                    int Minutes = 5;

                    User.ClearMovement(true);
                    User.SetRot(Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

                    // 135 Cycles approximately 1 minute
                    Item.ExtraData = "1";
                    Item.UpdateState(false, true);
                    Item.RequestUpdate(135 * Minutes, true);
                    RoleplayManager.Shout(Session, "*Comienza a recoger la basura del contenedor*", 5);

                    new Thread(() =>
                    {
                        User.CanWalk = false;

                        if (User.CurrentEffect != 4 && Session.GetRoleplay().EquippedWeapon == null)
                            User.ApplyEffect(EffectsList.Twinkle);

                        Thread.Sleep(RoleplayManager.GetTimerByMyJob(Session, "basurero") * 1000);

                        if (User.CurrentEffect != 0 && Session.GetRoleplay().EquippedWeapon == null)
                            User.ApplyEffect(0);

                        if (Session != null && Session.GetRoleplay() != null && Session.GetHabbo() != null)
                            ChooseReward(Session);
                        if (User != null)
                            User.CanWalk = true;

                        Session.GetRoleplay().BasuTrashCount++;
                        //TeamChofer.GetRoleplay().BasuTrashCount++;

                        Session.SendWhisper("Contenedores: " + Session.GetRoleplay().BasuTrashCount + " / 15", 1);
                        //TeamChofer.SendWhisper("Contenedores: " + TeamChofer.GetRoleplay().BasuTrashCount + " / 15", 1);

                        PolarEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session,
                            "compose_basurero|" +
                            "basucount|" +
                            Session.GetRoleplay().BasuTrashCount + "/15");

                        /*PolarEnvironment.GetGame().GetWebEventManager().SendDataDirect(TeamChofer,
                            "compose_basurero|" +
                            "basucount|" +
                            TeamChofer.GetRoleplay().BasuTrashCount + "/15");*/

                        if (Session.GetRoleplay().BasuTrashCount >= 15)
                        {
                            Session.SendWhisper("¡Han llegado a recolectar 15 Contenedores! Ahora vuelvan al Basurero para ':descargarcamion' y recibir su paga.", 1);
                            //TeamChofer.SendWhisper("¡Han llegado a recolectar 15 Contenedores! Ahora vuelvan al Basurero para ':descargarcamion' y recibir su paga.", 1);
                        }

                    }).Start();
                }
                else
                    Session.SendWhisper("¡Al parecer este contenedor de basura ya ha sido recogido!", 1);
            }
        }
        /*public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
            {
                if (Item.ExtraData == "" || Item.ExtraData == "0")
                    if (User.CanWalk)
                        User.MoveTo(Item.SquareInFront);
            }
            else
            {
                if (Item.ExtraData == "")
                    Item.ExtraData = "0";

                if (Item.ExtraData == "0")
                {
                    int Minutes = 5;

                    User.ClearMovement(true);
                    User.SetRot(Pathfinding.Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

                    // 135 Cycles approximately 1 minute
                    Item.ExtraData = "1";
                    Item.UpdateState(false, true);
                    Item.RequestUpdate(135 * Minutes, true);
                    Session.Shout("*Se tira de cabeza en la basura buscando algo de valor*", 4);

                    new Thread(() =>
                    {
                        User.CanWalk = false;

                        if (User.CurrentEffect != 10 && Session.GetRoleplay().EquippedWeapon == null)
                            User.ApplyEffect(EffectsList.Twinkle);

                        Thread.Sleep(5000);

                        if (User.CurrentEffect != 10 && Session.GetRoleplay().EquippedWeapon == null)
                            User.ApplyEffect(0);

                        if (Session != null && Session.GetRoleplay() != null && Session.GetHabbo() != null)
                            ChooseReward(Session);
                        if (User != null)
                            User.CanWalk = true;
                    }).Start();
                }
                else
                    Session.SendWhisper("¡Vaya, parece que alguien ya hurgó en este bote de basura!", 1);
            }
        }
        */
        public void OnWiredTrigger(Item Item)
        {

        }

        public void ChooseReward(GameClient Session)
        {
            var Random = new CryptoRandom();
            //int TotalCraftingItems = CraftingManager.CraftableItems.Count;
            int Chance = Random.Next(1, 101);
            int SecondChance = Random.Next(1, 101);

            //if (SecondChance < 4 && Chance > TotalCraftingItems)
            //  Chance = Random.Next(1, TotalCraftingItems + 1);

            #region Crafting Materials (OFF)
            /*
            if (Chance <= TotalCraftingItems)
            {
                var CraftingItemName = CraftingManager.CraftableItems[Chance - 1];

                ItemData Data = null;
                foreach (var itemdata in PolarEnvironment.GetGame().GetItemManager()._items.Values)
                {
                    if (itemdata.ItemName != CraftingItemName)
                        continue;

                    Data = itemdata;
                    break;
                }

                var Item = ItemFactory.CreateSingleItemNullable(Data, Session.GetHabbo(), "", "");
                Session.GetHabbo().GetInventoryComponent().TryAddItem(Item);

                ICollection<Item> FloorItems = Session.GetHabbo().GetInventoryComponent().GetFloorItems();
                ICollection<Item> WallItems = Session.GetHabbo().GetInventoryComponent().GetWallItems();

                Session.GetRoleplay().CraftingCheck = true;
                Session.SendMessage(new FurniListComposer(FloorItems.ToList(), WallItems, Session.GetRoleplay().CraftingCheck));
                RoleplayManager.Shout(Session, "*After rummaging the trash can, they pull out what appears to be one " + Item.GetBaseItem().PublicName +"*", 4);
            }
            */
            #endregion

            #region Drugs
            if (Chance <= 40)
            {
                int Amount;

                // Cocaine
                if (Chance > 30)
                {
                    Amount = Random.Next(1, 3);
                    Session.GetRoleplay().Cocaine += Amount;
                    RoleplayManager.Shout(Session, "*Encuentra " + Amount + "g de Crack dentro de la basura*", 5);
                }

                // Medicamentos
                else if (Chance <= 30 && Chance > 16)
                {
                    Amount = Random.Next(1, 3);
                    Session.GetRoleplay().Medicina += Amount;
                    RoleplayManager.Shout(Session, "*Encuentra " + Amount + " medicamento(s) dentro de la basura*", 5);
                }
                /*// Weed
                else
                {
                    Amount = Random.Next(1, 4);
                    Session.GetRoleplay().Weed += Amount;
                    RoleplayManager.Shout(Session, "*After rummaging the trash can, they pull out a small bag containing " + Amount + "g of weed*", 4);
                }
                */
            }
            #endregion

            #region Money
            else if (Chance > 40 && Chance <= 65)
            {
                int Amount = Random.Next(3, 9);

                Session.GetHabbo().Credits += Amount;
                Session.GetHabbo().UpdateCreditsBalance();
                //Session.GetRoleplay().MoneyEarned += Amount;
                RoleplayManager.Shout(Session, "*Ha encontrado una billetera con $" + Amount + " dentro de la basura*", 5);
            }
            #endregion

            #region Special Bot
            /*else if (Chance > 75 && Chance <= 78)
            {

            }*/
            #endregion

            #region No Reward
            else
            {
                Session.SendWhisper("Esta vez no has encontrado nada en el contenedor.", 1);
            }
            #endregion
        }
        /*public void ChooseReward(GameClient Session)
        {
            var Random = new CryptoRandom();
            int TotalCraftingItems = CraftingManager.CraftableItems.Count;
            int Chance = Random.Next(1, 101);
            int SecondChance = Random.Next(1, 101);

            if (SecondChance < 4 && Chance > TotalCraftingItems)
                Chance = Random.Next(1, TotalCraftingItems + 1);

            #region Crafting Materials
            if (Chance <= TotalCraftingItems)
            {
                var CraftingItemName = CraftingManager.CraftableItems[Chance - 1];

                ItemData Data = null;
                foreach (var itemdata in PolarEnvironment.GetGame().GetItemManager()._items.Values)
                {
                    if (itemdata.ItemName != CraftingItemName)
                        continue;

                    Data = itemdata;
                    break;
                }

                var Item = ItemFactory.CreateSingleItemNullable(Data, Session.GetHabbo(), "", "");
                Session.GetHabbo().GetInventoryComponent().TryAddItem(Item);

                IEnumerable<Item> Items = Session.GetHabbo().GetInventoryComponent().GetWallAndFloor;

                Session.GetRoleplay().CraftingCheck = true;

                 int page = 0;
                int pages = ((Items.Count() - 1) / 700) + 1;

                if (!Items.Any())
                {
                    Session.SendMessage(new FurniListComposer(Items.ToList(), 1, 0, Session.GetRoleplay().CraftingCheck));
                }
                else
                {
                    foreach (ICollection<Item> batch in Items.Batch(700))
                    {
                        Session.SendMessage(new FurniListComposer(batch.ToList(), pages, page, Session.GetRoleplay().CraftingCheck));

                        page++;
                    }
                };
                Session.Shout("*Después de hurgar en la basura, saca lo que parece ser un " + Item.GetBaseItem().PublicName +"*", 4);
            }
            #endregion

            #region Drugs
            else if (Chance > TotalCraftingItems && Chance <= 20)
            {
                int Amount;

                // Cocaine
                if (Chance > 25)
                {
                    Amount = Random.Next(1, 5);
                    Session.GetRoleplay().Cocaine += Amount;
                    Session.GetRoleplay().Hygiene -= 90;
                    Session.GetRoleplay().CurHealth-= 50;
                    Session.Shout("*Buscando en la basura consiguió: " + Amount + "g de cocaina pero se cortó con un vidrio, (-50) vida y (-90) Higiene*", 4);
                }

                // Cigarettes
                else if (Chance <= 10 && Chance > 5)
                {
                    Amount = Random.Next(1, 10);
                    Session.GetRoleplay().Cigarettes += Amount;
                    Session.GetRoleplay().Hygiene -= 90;
                    Session.Shout("*Después de buscar en la basura encuentra: " + Amount + " cigarrillos  (-90) Higiene*", 4);
                }

                // Weed
                else
                {
                    Amount = Random.Next(1, 5);
                    Session.GetRoleplay().Weed += Amount;
                    Session.GetRoleplay().Hygiene -= 100;
                    Session.Shout("*Buscando en la basura se encontró " + Amount + "g de marihuana  (-90) Higiene*", 4);
                }
            }
            #endregion

            #region Money
            else if (Chance > 2 && Chance <= 1)
            {
                int Amount = Random.Next(90, 120);

                Session.GetHabbo().Credits += Amount;
                Session.GetHabbo().UpdateCreditsBalance();
                Session.GetRoleplay().Hygiene -= 90;
                Session.GetRoomUser().ApplyEffect(10);
                Session.GetRoleplay().CurHealth -= 50;
                Session.Shout("*Ha conseguido: $" + Amount + " pero había un clavo oxidado que se enterró en el pie. (-50) vida y (-90) Higiene*", 4);
            }
            #endregion

            #region Special Bot
           /* else if (Chance > 75 && Chance <= 78)
            {

            }
            #endregion

            #region No Reward
            else
            {
                Session.GetRoleplay().Hygiene -= 90;
                Session.GetRoleplay().CurHealth -= 20;
                Session.Shout("*Habia un alambre de púas y te cortaste con el. (-20) de vida y (-90) Higiene*", 4);
            }
            #endregion
        }*/
    }
}