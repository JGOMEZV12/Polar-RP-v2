using Newtonsoft.Json;
using Polar.Communication.Packets.Incoming;
using Polar.Communication.Packets;
using Polar.Core;
using Polar.HabboHotel.Camera;
using Polar.HabboHotel.GameClients;
using System;
using Polar.Communication.Packets.Outgoing.HabboCamera;

namespace Polar.Communication.Packets.Incoming.HabboCamera
{
    class SetRoomThumbnailEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            int count = Packet.PopInt();

            byte[] data = Packet.ReadBytes(count);

            try
            {
                string base64 = Convert.ToBase64String(data);

                //Check if is an PNG valid image
                if (!base64.Substring(0, 5).ToUpper().Equals("IVBOR"))
                {
                    Logging.WriteLine("Someone tried take a picture with an invalid mime type! (Username: " + Session.GetHabbo().Username + ")");
                    Session.SendMessage(new SendRoomThumbnailAlertComposer());
                    return;
                }

                string result = CameraHelper.request("thumbnail", Session.GetHabbo().Id, Session.GetHabbo().CurrentRoom.Id, base64);

                JSONCamera jsonCamera = JsonConvert.DeserializeObject<JSONCamera>(result);
                if (!jsonCamera.status)
                {
                    Session.SendNotification("It happened some error while trying save this thumbnail! Try again!");
                    return;
                }

                //Logging.WriteLine("New photo camera: " + jsonCamera.preview);

                Session.SendMessage(new SendRoomThumbnailAlertComposer());
            }
            catch (Exception ex)
            {
                Logging.LogException(ex.ToString());
                Session.SendMessage(new SendRoomThumbnailAlertComposer());
            }
        }
    }
}
