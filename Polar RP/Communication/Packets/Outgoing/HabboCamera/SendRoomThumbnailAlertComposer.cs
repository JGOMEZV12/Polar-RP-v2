using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polar.Communication.Packets.Outgoing.HabboCamera
{
    internal class SendRoomThumbnailAlertComposer : ServerPacket
    {
        public SendRoomThumbnailAlertComposer()
            : base(ServerPacketHeader.SendRoomThumbnailAlertMessageComposer)
        {
            Compose(this);
        }

        public void Compose(ServerPacket packet)
        { 
        }
    }
}
