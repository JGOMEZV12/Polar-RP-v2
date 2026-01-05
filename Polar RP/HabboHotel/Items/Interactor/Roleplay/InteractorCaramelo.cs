using System.Drawing;
using Polar.HabboHotel.GameClients;
using Polar.Communication.Packets.Outgoing.Polls;
using Polar.HabboHotel.Pathfinding;
using Polar.HabboRoleplay.Misc;

namespace Polar.HabboHotel.Items.Interactor
{
    public class InteractorCaramelo : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }


        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            #region Conditions
            if (Session.GetHabbo().Credits < 10000)
            {
                Session.SendWhisper("*Necesitas 10000$ para comprar los caramelos que hay en esta máquina*", 1);

                return;
            }

            #endregion

            if (Session.GetRoomUser() == null)
                return;

            var User = Session.GetRoomUser();

            if (Rooms.Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                 Session.GetRoleplay().UsingCola = true;
                    Session.Shout("* Mete 10.000$ en la máquina y le da 15 caramelos importados [-10.000$] *", 11);
                    Session.GetRoleplay().Caramelos += 15;
                    Session.GetHabbo().Credits -= 10000;
                    Session.GetHabbo().UpdateCreditsBalance();
                    #region Bank Company Balance
                    RoleplayManager.GiveMoneyToCompany(12, Session, "supermarket", true, 10000);
                    #endregion Bank Company Balance
                    User.MoveTo(Item.SquareInFront);
                    User.CarryItem(67);
            }
            else
            {
               
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }
    }
}