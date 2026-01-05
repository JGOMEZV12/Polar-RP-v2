using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polar.Communication.Packets.Outgoing.BuildersClub
{
    internal class BCBorrowedItemsComposer : ServerPacket
    {
        public BCBorrowedItemsComposer()
            : base(ServerPacketHeader.BCBorrowedItemsMessageComposer)
        {
            base.WriteInteger(0);
        }
    }
}
