using Polar.Communication.Packets.Outgoing.HabboCamera;
using Polar.Database.Interfaces;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Items;
using Polar.HabboHotel.Camera;
using Polar.HabboRoleplay.Misc;


namespace Polar.Communication.Packets.Incoming.HabboCamera
{
    class PurchaseCameraPictureEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            string imagen1 = PolarEnvironment.GetConfig().data["Camera_img_1"];
            string imagen2 = PolarEnvironment.GetConfig().data["Camera_img_2"];
            int imagenint1 = Int32.Parse(imagen1);
            int imagenint2 = Int32.Parse(imagen2);

            ItemData ItemDataSmall;
            if (!PolarEnvironment.GetGame().GetItemManager().GetItem(imagenint2, out ItemDataSmall))
            {
                //Session.SendNotification("Invalid id 02");
                return;
            }

            JSONCamera jsonInfo = Session.GetHabbo().lastPhotoPreview;

            string roomId = jsonInfo.room_id;
            double timestamp = jsonInfo.timestamp;
            string md5image = jsonInfo.encrypted_id;
            string username = Session.GetHabbo().Username;

            string ExtraData = "{\"w\":\"" + CameraHelper.BASE_URL2 + "photos/" + md5image + ".png" + "\", \"n\":\"" + username + "\", \"s\":\"" + Session.GetHabbo().Id + "\", \"u\":\"" + "0" + "\", \"t\":\"" + timestamp + "" + "\"}";
            PolarEnvironment.SendMs2("", RoleplayManager.CDNSWF + "/newfoto/" + jsonInfo.preview, "¡Mira está foto que ha tomada por " + Session.GetHabbo().Username + "!", "En " + Session.GetHabbo().CurrentRoom.Name, Session.GetHabbo().Look, true);

            Session.GetHabbo().GetInventoryComponent().AddNewItem(0, ItemDataSmall.Id, ExtraData, 0, true, false, 0, 0);
            Session.GetHabbo().GetInventoryComponent().UpdateItems(false);
            Session.SendMessage(new CamereFinishPurchaseComposer());



            /*using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("INSERT INTO server_pictures (id, picture_id, creator_id, creator_name, room_id, time) VALUES ('" + md5image + "', '" + Session.GetHabbo().Id + "', '" + Session.GetHabbo().Username + "', '" + roomId + "', '" + timestamp + "')");
            }*/

            //Session.SendNotification(PolarEnvironment.GetLanguageManager().TryGetValue("notif.buyphoto.valide", Session.Langue));
        }
    }
}
