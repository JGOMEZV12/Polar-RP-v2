using Polar.Communication.Packets.Outgoing;
using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.Users;
using Polar.Communication.Packets.Incoming;

namespace Polar.HabboHotel.Items.Wired.Boxes.Effects
{
    class BotFollowsUserBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type { get { return WiredBoxType.EffectBotFollowsUserBox; } }
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public BotFollowsUserBox(Room Instance, Item Item)
        {
            this.Instance = Instance;
            this.Item = Item;
            this.SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket Packet)
        {
            int Unknown = Packet.PopInt();
            int FollowMode = Packet.PopInt();
            string BotConfiguration = Packet.PopString();

            if (this.SetItems.Count > 0)
                this.SetItems.Clear();

            this.StringData = FollowMode + ";" + BotConfiguration;
        }


        public void Serialize(ServerPacket Packet)
        {
            Packet.WriteBoolean(false);
            Packet.WriteInteger(100);
            Packet.WriteInteger(SetItems.Count);
            foreach (Item Item in SetItems.Values.ToList())
            {
                Packet.WriteInteger(Item.Id);
            }
            Packet.WriteInteger(Item.GetBaseItem().SpriteId);
            Packet.WriteInteger(Item.Id);
            if (String.IsNullOrEmpty(StringData)) StringData = "0;Bot name";
            Packet.WriteString(StringData.Split(';')[1]);
            Packet.WriteInteger(1);
            Packet.WriteInteger(int.Parse(StringData.Split(';')[0]));
            if (this is IWiredCycle)
            {
                Packet.WriteInteger(WiredBoxTypeUtility.GetWiredId(Type));
                Packet.WriteInteger(0);
                Packet.WriteInteger(((IWiredCycle)this).Delay);
            }
            else
            {
                Packet.WriteInteger(0);
                Packet.WriteInteger(WiredBoxTypeUtility.GetWiredId(Type));
                Packet.WriteInteger(0);
            }
        }
        public bool Execute(params object[] Params)
        {
            if (Params == null || Params.Length == 0)
                return false;

            if (String.IsNullOrEmpty(this.StringData))
                return false;

            Habbo Player = (Habbo)Params[0];
            if (Player == null)
                return false;

            RoomUser Human = Instance.GetRoomUserManager().GetRoomUserByHabbo(Player.Id);
            if (Human == null)
                return false;

            string[] Stuff = this.StringData.Split(';');
            if (Stuff.Length != 2)
                return false;

            string Username = Stuff[1];

            RoomUser User = this.Instance.GetRoomUserManager().GetBotByName(Username);
            if (User == null)
                return false;

            int FollowMode = 0;
            if (!int.TryParse(Stuff[0], out FollowMode))
                return false;

            if (FollowMode == 0)
            {
                User.BotData.ForcedUserTargetMovement = 0;

                if (User.IsWalking)
                    User.ClearMovement(true);
            }
            else if (FollowMode == 1)
            {
                User.BotData.ForcedUserTargetMovement = Player.Id;

                if (User.IsWalking)
                    User.ClearMovement(true);

                User.MoveTo(Human.X, Human.Y);
            }

            return true;
        }
    }
}
