
using Polar.Core;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Camera;
using Newtonsoft.Json;
using System;
using Polar.Communication.Packets.Outgoing.HabboCamera;
using Polar.HabboRoleplay.Misc;

namespace Polar.Communication.Packets.Incoming.HabboCamera
{
    public class HabboCameraPictureDataEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            int count = Packet.PopInt();

            byte[] data = Packet.ReadBytes(count);

            try
            {
                string base64 = Convert.ToBase64String(data);

                //Check if is an PNG valid image
                if (!base64.Substring(0, 5).ToUpper().Equals("IVBOR"))
                {
                    Console.WriteLine("¡Alguien intentó tomar una fotografía con un tipo mime no válido! (Username: " + Session.GetHabbo().Username + ")");
                    return;
                }

                string result = CameraHelper.request("camera", Session.GetHabbo().Id, Session.GetHabbo().CurrentRoom.Id, base64);

                JSONCamera jsonCamera = JsonConvert.DeserializeObject<JSONCamera>(result);
                if (!jsonCamera.status)
                {
                    Session.SendNotification("¡Se produjo un error al intentar guardar esta imagen! ¡Inténtalo de nuevo!");
                    return;
                }

                //Logging.LogException("New photo camera: " + jsonCamera.preview);

                Session.GetHabbo().lastPhotoPreview = jsonCamera;
                Session.SendMessage(new CameraSendImageUrlComposer(jsonCamera.preview));
            }
            catch (Exception ex)
            {
                Logging.LogException(ex.ToString());
            }
        }
    }
}