using Polar.HabboHotel.GameClients;
using Polar.HabboRoleplay.Weapons;

namespace Polar.Communication.Packets.Outgoing.Inventory.Weapons
{
    internal class WeaponsComposer : ServerPacket
    {
        public WeaponsComposer( GameClient Session)
            : base(ServerPacketHeader.WeaponsMessageComposer)
        {
            base.WriteInteger(Session.GetRoleplay().OwnedWeapons.Count);
            foreach (Weapon Weapon in Session.GetRoleplay().OwnedWeapons.Values)
            {
                base.WriteInteger(1);
                base.WriteString(Weapon.Name);
            }
        }
    }
}
