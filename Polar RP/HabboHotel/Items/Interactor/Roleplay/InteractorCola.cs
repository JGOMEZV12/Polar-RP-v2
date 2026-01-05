using System.Drawing;
using Polar.HabboHotel.GameClients;
using Polar.Communication.Packets.Outgoing.Polls;
using Polar.HabboHotel.Pathfinding;
using Polar.HabboRoleplay.Misc;

namespace Polar.HabboHotel.Items.Interactor
{
    public class InteractorCola : IFurniInteractor
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

            if (Session.GetRoleplay().CurEnergy >= Session.GetRoleplay().MaxEnergy)
            { 
                Session.Shout("* " + Session.GetHabbo().Username + " ¡La maquina se ha comido el billete pero no ha dado la Pepsi!", 15);
                Session.SendWhisper("Al parecer no tienes más sed (Tu barra de energía esta llena)");
                Session.GetHabbo().Credits -= 70;
                Session.GetHabbo().UpdateCreditsBalance();

                #region Bank Company Balance
                RoleplayManager.GiveMoneyToCompany(12, Session, "supermarket", true, 70);
                #endregion Bank Company Balance

                return;
            }

            if (Session.GetHabbo().Credits < 70)
            {
                Session.Shout("* Uff olvidé guardar dinero en mi billetera, no puedo comprar una Pepsi *", 15);

                return;
            }

            #endregion

            if (Session.GetRoomUser() == null)
                return;

            var User = Session.GetRoomUser();

            if (Rooms.Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
            {
                 Session.GetRoleplay().UsingCola = true;
                    Session.Shout("* Toma una Pepsi y gracias a los 35,0 Gr de azúcar, se siente más enérgico [-70$] *", 11);
                    Session.GetRoleplay().CurEnergy += 50;
                    Session.GetHabbo().Credits -= 70;
                    Session.GetHabbo().UpdateCreditsBalance();
                    User.MoveTo(Item.SquareInFront);
                    Session.GetRoleplay().IsWorking = false;
                    User.CarryItem(55);

                    #region Bank Company Balance
                    RoleplayManager.GiveMoneyToCompany(12, Session, "supermarket", true, 70);
                    #endregion Bank Company Balance
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