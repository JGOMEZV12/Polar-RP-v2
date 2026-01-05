using System;

namespace Polar.Communication.Packets.Outgoing.Rooms.Chat
{
    public class ChatComposer : ServerPacket
    {
        public int VirtualId { get; }
        public string Message { get; }
        public int Emotion { get; }
        public int Colour { get; }

        public ChatComposer(int VirtualId, string Message, int Emotion, int Colour)
            : base(ServerPacketHeader.ChatMessageComposer)
        {
            this.VirtualId = VirtualId;
            this.Message = Message;
            this.Emotion = Emotion;
            this.Colour = Colour;
            Compose(this);
        }

        public void Compose(ServerPacket packet)
        {
            packet.WriteInteger(VirtualId);
            packet.WriteString(Message);
            packet.WriteInteger(Emotion);
            packet.WriteInteger(Colour);
            packet.WriteInteger(0);
            packet.WriteInteger(-1);
        }
    }
}