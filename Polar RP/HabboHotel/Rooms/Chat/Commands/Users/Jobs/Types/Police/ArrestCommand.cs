using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Collections.Generic;

using Polar.HabboHotel.Rooms;
using Polar.HabboHotel.GameClients;
using Polar.HabboHotel.Groups;
using Polar.HabboRoleplay.Misc;
using Polar.Communication.Packets.Outgoing.Rooms.Notifications;
using Polar.HabboRoleplay.RPRoom;


namespace Polar.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class ArrestCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_arrest"; }
        }

        public string Parameters
        {
            get { return "%user%"; }
        }

        public string Description
        {
            get { return "Arresta a una persona según el nivel de búsqueda."; }
        }

        public async Task Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            string MyCity = Room.City;
            RPRoom Data;
            RPRoom Data2;
            int JailRID = PolarEnvironment.GetGame().GetRPRoomManager().TryToGetJail(MyCity, out Data);//prision de la cd.
            int PolStationID = PolarEnvironment.GetGame().GetRPRoomManager().TryToGetPolStation(MyCity, out Data2);//prision

            if (Session.GetHabbo().CurrentRoomId != JailRID && Session.GetHabbo().CurrentRoomId != PolStationID)
            {
                Session.SendWhisper("Debes llevar a la persona dentro de la Prisión o Comisaría para encarcelarla. ((Usa :escoltar [nombre] para llevarlo hasta allá)).", 1);
                return;
            }
            
            if (Params.Length == 1)
            {
                Session.SendWhisper("Debes ingresar el nombre de la persona.", 1);
                return;
            }

            GameClient TargetClient = PolarEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada.", 1);
                return;
            }

            RoomUser RoomUser = Session.GetRoomUser();
            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Username);
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                return;
            }
            if (TargetClient.GetRoleplay().PassiveMode)
            {
                Session.SendWhisper("¡No puedes hacerle eso a una persona que está en modo pasivo!", 1);
                return;
            }
            if (TargetUser == null)
            {
                Session.SendWhisper("Ha ocurrido un error al buscar a la persona, probablemente esté desconectada o no está en esta zona.", 1);
                return;
            }
            if (!Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                if (TargetClient.GetHabbo().Id == Session.GetHabbo().Id)
                {
                    Session.SendWhisper("No puedes hacerte eso a ti mism@.", 1);
                    return;
                }
                if (GroupManager.HasJobCommand(TargetClient, "law"))
                {
                    Session.SendWhisper("¡No puedes hacer eso entre compañeros de trabajo!", 1);
                    return;
                }
            }

            if (!GroupManager.HasJobCommand(Session, "arrest") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("¡Solo un oficial de policía puede hacer eso!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
            {
                Session.SendWhisper("Debes estar trabajando para hacer eso.", 1);
                return;
            }
            /*if (Session.GetHabbo().Escorting != TargetClient.GetHabbo().Id)
            {
                Session.SendWhisper("Debes tener escoltando a tu convicto a arrestar.", 1);
                return;
            }
            if (Session != TargetClient && TargetClient.GetHabbo().Rank > 3)
            {
                Session.SendWhisper("((No puedes hacerle eso a un miembro de la administración))", 1);
                return;
            }*/
            if (TargetClient.GetRoleplay().IsDead)
            {
                Session.SendWhisper("¡No puedes arrestar a una persona muerta!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().IsJailed)
            {
                Session.SendWhisper("¡No puedes arrestar a una persona encarcelada!", 1);
                return;
            }

            if (!TargetClient.GetRoleplay().Cuffed)
            {
                Session.SendWhisper("¡Primero debes esposar a la persona!", 1);
                return;
            }
            if (TargetUser.IsAsleep)
            {
                Session.SendWhisper("¡No puedes arrestar a un usuario ausente!", 1);
                return;
            }
            if (Session.GetRoleplay().TryGetCooldown("arrest", true))
            {
                Session.SendWhisper("Por favor espera un poco para hacer eso nuevamente.", 1);
                return;
            }
            #endregion

            #region Execute
            Point ClientPos = new Point(RoomUser.X, RoomUser.Y);
            Point TargetClientPos = new Point(TargetUser.X, TargetUser.Y);
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);
            Wanted Wanted = RoleplayManager.WantedList.ContainsKey(TargetClient.GetHabbo().Id) ? RoleplayManager.WantedList[TargetClient.GetHabbo().Id] : null;
            int WantedTime = Wanted == null ? RoleplayManager.DefaultJailTime : Wanted.WantedLevel * RoleplayManager.StarsJailTime;
            int ReduceTime = 0;
            string ExtraMsg = "";

            if (TargetClient.GetHabbo().VIPRank == 1)
            {
                ExtraMsg = " con una reducción del 10% de tiempo por ser VIP.";
                ReduceTime = WantedTime / 10;
            }
            if (TargetClient.GetHabbo().VIPRank == 2)
            {
                ExtraMsg = " con una reducción del 25% de tiempo por ser VIP2.";
                ReduceTime = WantedTime / 4;
            }

            if (Distance <= 1)
            {
                if (TargetClient.GetRoleplay().IsWorking)
                {
                    WorkManager.RemoveWorkerFromList(TargetClient);
                    TargetClient.GetRoleplay().IsWorking = false;
                    TargetClient.GetHabbo().Poof();
                }

                RoleplayManager.Shout(Session, "*Libera las manos de " + TargetClient.GetHabbo().Username + " y lo encierra en una celda durante " + WantedTime + " minuto(s)" + ExtraMsg, 37);
                TargetClient.GetRoleplay().Cuffed = false;
                TargetClient.GetRoomUser().ApplyEffect(0);

                if (TargetClient.GetHabbo().Look.Contains("lg-78322"))
                {
                    if (!TargetClient.GetRoleplay().WantedFor.Contains("exposición indecente"))
                        TargetClient.GetRoleplay().WantedFor = TargetClient.GetRoleplay().WantedFor + "exposicion indecente, ";
                }

                if (TargetUser.Frozen)
                    TargetUser.Frozen = false;

                if (!TargetClient.GetRoleplay().IsJailed)
                {
                    TargetClient.GetRoleplay().IsJailed = true;
                    TargetClient.GetRoleplay().JailedTimeLeft = WantedTime - ReduceTime;
                    TargetClient.GetRoleplay().TimerManager.CreateTimer("jail", 1000, false);
                }

                if (TargetClient.GetHabbo().CurrentRoomId == JailRID)
                {
                    RoleplayManager.GetLookAndMotto(TargetClient);
                    RoleplayManager.SpawnBeds(TargetClient, "bed_silo_one");
                    TargetClient.SendMessage(new RoomNotificationComposer("room_jail_prison", "message", "Has sido arrestad@ por " + Session.GetHabbo().Username + " por " + WantedTime + " minuto(s)" + ExtraMsg));
                }
                else
                {
                    TargetClient.SendMessage(new RoomNotificationComposer("room_jail_prison", "message", "Has sido arrestad@ por " + Session.GetHabbo().Username + " por " + WantedTime + " minuto(s)" + ExtraMsg));
                    RoleplayManager.SendUserOld2(TargetClient, JailRID);
                }
                if (RoleplayManager.WantedList.ContainsKey(TargetClient.GetHabbo().Id))
                {
                    Wanted Junk;
                    RoleplayManager.WantedList.TryRemove(TargetClient.GetHabbo().Id, out Junk);
                }

                PolarEnvironment.GetGame().GetClientManager().JailAlert("[RADIO] ¡" + TargetClient.GetHabbo().Username + " ha sido arrestad@ por " + Session.GetHabbo().Username + "! Buen trabajo chicos.");
                //PolarEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_Arrests", 1);
                Session.GetRoleplay().Arrests++;
                //PolarEnvironment.GetGame().GetAchievementManager().ProgressAchievement(TargetClient, "ACH_Arrested", 1);
                TargetClient.GetRoleplay().Arrested++;
                if (TargetClient.GetRoomUser() != null)
                    TargetClient.GetRoomUser().CanWalk = true;

                // UnEscort
                if (TargetClient.GetRoomUser() != null)
                {
                    TargetClient.GetRoomUser().ClearMovement(true);
                    TargetClient.GetRoomUser().CanWalk = true;
                }

                #region Quitar Drogas
                if (Session.GetRoleplay().Weed <= 0 && Session.GetRoleplay().Cocaine <= 0 && Session.GetRoleplay().Heroina <= 0)
                {
                    Session.SendWhisper("¡No tienes drogas para eliminar!", 1);
                }
                else
                {
                    bool HasWeed = TargetClient.GetRoleplay().Weed > 0;
                    bool HasCocaine = TargetClient.GetRoleplay().Cocaine > 0;
                    bool HasHeroine = TargetClient.GetRoleplay().Heroina > 0;

                    if (!HasWeed && !HasCocaine && !HasHeroine)
                    {
                        Session.Shout("*Revisa a " + TargetClient.GetHabbo().Username + " Tratando de encontrar alguna droga, pero parece que no puede encontrar ninguna*", 37);
                    }
                    else if (HasWeed && !HasCocaine && !HasHeroine)
                    {
                        Session.Shout("*Le encuentra a " + TargetClient.GetHabbo().Username + ", " + String.Format("{0:N0}", TargetClient.GetRoleplay().Weed) + "g de marihuana [LEGAL SON: 10g]*", 37);
                        TargetClient.GetRoleplay().Weed = 0;
                    }
                    else if (HasCocaine && !HasWeed && !HasHeroine)
                    {
                        Session.Shout("*Le encuentra a " + TargetClient.GetHabbo().Username + ", " + String.Format("{0:N0}", TargetClient.GetRoleplay().Cocaine) + "g de cocaina [LEGAL SON: 8g] *", 37);
                        TargetClient.GetRoleplay().Cocaine = 0;
                    }
                    else if (HasHeroine && !HasCocaine && !HasWeed)
                    {
                        Session.Shout("*Le encuentra a " + TargetClient.GetHabbo().Username + ", " + String.Format("{0:N0}", TargetClient.GetRoleplay().Heroina) + "g de heroina [LEGAL SON: 20g] *", 37);
                        TargetClient.GetRoleplay().Heroina = 0;
                    }
                   /* int Weed = Session.GetRoleplay().Weed;
                    int Cocaine = Session.GetRoleplay().Cocaine;
                    int Heroine = Session.GetRoleplay().Heroina;
                    RoleplayManager.Shout(Session, "Le encuentra " + Weed +"g de", 37);
                    Session.GetRoleplay().Weed = 0;
                    Session.GetRoleplay().Cocaine = 0;
                    Session.GetRoleplay().Heroina = 0;*/
                }
                #endregion
                #region Desequipar al Convicto
                if (TargetClient.GetRoleplay().EquippedWeapon != null)
                {
                    string UnEquipMessage = TargetClient.GetRoleplay().EquippedWeapon.UnEquipText;
                    UnEquipMessage = UnEquipMessage.Replace("[NAME]", TargetClient.GetRoleplay().EquippedWeapon.PublicName);

                    RoleplayManager.Shout(TargetClient, UnEquipMessage, 5);

                    if (TargetClient.GetRoomUser().CurrentEffect == TargetClient.GetRoleplay().EquippedWeapon.EffectID)
                        TargetClient.GetRoomUser().ApplyEffect(0);

                    if (TargetClient.GetRoomUser().CarryItemID == TargetClient.GetRoleplay().EquippedWeapon.HandItem)
                        TargetClient.GetRoomUser().CarryItem(0);

                    TargetClient.GetRoleplay().CooldownManager.CreateCooldown("unequip", 1000, 3);
                    TargetClient.GetRoleplay().EquippedWeapon = null;

                    TargetClient.GetRoleplay().WLife = 0;
                    TargetClient.GetRoleplay().Bullets = 0;
                    TargetClient.GetRoleplay().Cocaine = 0;
                    TargetClient.GetRoleplay().Weed = 0;
                }
                else
                {
                    TargetClient.GetRoleplay().Cocaine = 0;
                    TargetClient.GetRoleplay().Weed = 0;
                }
                #endregion


                PolarEnvironment.SendMs("**__¡LiveFeed!__** `|` **" + TargetClient.GetHabbo().Username + "** Ha sido arrestad@ por **" + Session.GetHabbo().Username + "**");

                #region Live Feed
                foreach (GameClient client in PolarEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null)
                        continue;

                    PolarEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(client, "event_feedcomposer", "alert|" + Session.GetHabbo().Username + "|" + TargetClient.GetHabbo().Username + "|" + "Arresto a");
                }
                #endregion

                Session.GetRoleplay().CooldownManager.CreateCooldown("arrest", 1000, 3);
                return;
            }
            else
            {
                Session.SendWhisper("Debes estar más cerca de la persona para hacer eso.", 1);
                return;
            }
            #endregion
        }

    }
}