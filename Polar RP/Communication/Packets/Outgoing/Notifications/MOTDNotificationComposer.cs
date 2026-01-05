using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polar.Communication.Packets.Outgoing.Notifications
{
    internal class MOTDNotificationComposer : ServerPacket
    {
        public string Message { get; }
        public MOTDNotificationComposer(string message)
            : base(ServerPacketHeader.MOTDNotificationMessageComposer)
        {
            this.Message = message;
            Compose(this);
        }

        public void Compose(ServerPacket packet)
        {
            packet.WriteInteger(1);
            packet.WriteString(Message);
        }
    }
}
