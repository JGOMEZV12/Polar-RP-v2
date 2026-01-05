using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Polar.Communication.Packets.Outgoing.Rooms.Engine;
using Polar.Database.Interfaces;

namespace Polar.HabboHotel.Rooms.Chat.Commands.Developers
{
    class RoomCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_room"; }
        }

        public string Parameters
        {
            get { return "%type%"; }
        }

        public string Description
        {
            get { return "Gives you the ability to enable or disable basic room commands."; }
        }

        public async Task Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Oops, you must choose a room option to disable.", 1);
                return;
            }

            string Option = Params[1];
            switch (Option)
            {
                case "list":
                    {
                        StringBuilder List = new StringBuilder();
                        List.Append("---------- Room Settings ----------\n\n");
                        List.Append("Pet Morphs: " + (Room.PetMorphsAllowed == true ? "enabled" : "disabled") + "\n");
                        List.Append("Pull: " + (Room.PullEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Push: " + (Room.PushEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Super Pull: " + (Room.SPullEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Super Push: " + (Room.SPushEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Respect: " + (Room.RespectNotificationsEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Enables: " + (Room.EnablesEnabled == true ? "enabled" : "disabled") + "\n\n");
                        List.Append("---------- Roleplay Settings ----------\n\n");
                        List.Append("Bank: " + (Room.BankEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Shooting: " + (Room.ShootEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Hitting: " + (Room.HitEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Safezone: " + (Room.SafeZoneEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Robbing: " + (Room.RobEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Sex Commands: " + (Room.SexCommandsEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Turf: " + (Room.TurfEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Gym: " + (Room.GymEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Delivery: " + (Room.DeliveryEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Tutorial: " + (Room.TutorialEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Drive: " + (Room.DriveEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Taxi To: " + (Room.TaxiToEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Bus To: " + (Room.BusToEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Taxi From: " + (Room.TaxiFromEnabled == true ? "enabled" : "disabled") + "\n");
                        List.Append("Entrance Message: " + Room.EnterRoomMessage + "\n");
                        List.Append("Zona Hospital: " + (Room.IsHospital == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Prisión: " + (Room.IsPrison == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Comisaría: " + (Room.IsPolStation == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Camioneros: " + (Room.IsCamionero == true ? "activado" : "desactivado") + "\n");
                        List.Append("Zona Basureros: " + (Room.IsBasurero == true ? "activado" : "desactivado") + "\n");
                        Session.SendNotification(List.ToString());
                        break;
                    }

                case "push":
                    {
                        Room.PushEnabled = !Room.PushEnabled;
                        Room.RoomData.PushEnabled = Room.PushEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `push_enabled` = @PushEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PushEnabled", PolarEnvironment.BoolToEnum(Room.PushEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Push mode is now " + (Room.PushEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "spush":
                    {
                        Room.SPushEnabled = !Room.SPushEnabled;
                        Room.RoomData.SPushEnabled = Room.SPushEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `spush_enabled` = @PushEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PushEnabled", PolarEnvironment.BoolToEnum(Room.SPushEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Super Push mode is now " + (Room.SPushEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "spull":
                    {
                        Room.SPullEnabled = !Room.SPullEnabled;
                        Room.RoomData.SPullEnabled = Room.SPullEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `spull_enabled` = @PullEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PullEnabled", PolarEnvironment.BoolToEnum(Room.SPullEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Super Pull mode is now " + (Room.SPullEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "pull":
                    {
                        Room.PullEnabled = !Room.PullEnabled;
                        Room.RoomData.PullEnabled = Room.PullEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `pull_enabled` = @PullEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PullEnabled", PolarEnvironment.BoolToEnum(Room.PullEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Pull mode is now " + (Room.PullEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "enable":
                case "enables":
                    {
                        Room.EnablesEnabled = !Room.EnablesEnabled;
                        Room.RoomData.EnablesEnabled = Room.EnablesEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `enables_enabled` = @EnablesEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("EnablesEnabled", PolarEnvironment.BoolToEnum(Room.EnablesEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Enables mode set to " + (Room.EnablesEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "respect":
                    {
                        Room.RespectNotificationsEnabled = !Room.RespectNotificationsEnabled;
                        Room.RoomData.RespectNotificationsEnabled = Room.RespectNotificationsEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `respect_notifications_enabled` = @RespectNotificationsEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("RespectNotificationsEnabled", PolarEnvironment.BoolToEnum(Room.RespectNotificationsEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Respect notifications mode set to " + (Room.RespectNotificationsEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "bank":
                    {
                        Room.BankEnabled = !Room.BankEnabled;
                        Room.RoomData.BankEnabled = Room.BankEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `bank_enabled` = @BankEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("BankEnabled", PolarEnvironment.BoolToEnum(Room.BankEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Bank settings for the room set to " + (Room.BankEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "taxito":
                    {
                        Room.TaxiToEnabled = !Room.TaxiToEnabled;
                        Room.RoomData.TaxiToEnabled = Room.TaxiToEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `taxi_to_enabled` = @TaxiToEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("TaxiToEnabled", PolarEnvironment.BoolToEnum(Room.TaxiToEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Taxi To settings for the room set to " + (Room.TaxiToEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "busto":
                    {
                        Room.BusToEnabled = !Room.BusToEnabled;
                        Room.RoomData.BusToEnabled = Room.BusToEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `bus_to_enabled` = @BusToEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("BusToEnabled", PolarEnvironment.BoolToEnum(Room.BusToEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Se puede ir en BUS: " + (Room.BusToEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "taxifrom":
                    {
                        Room.TaxiFromEnabled = !Room.TaxiFromEnabled;
                        Room.RoomData.TaxiFromEnabled = Room.TaxiFromEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `taxi_from_enabled` = @TaxiFromEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("TaxiFromEnabled", PolarEnvironment.BoolToEnum(Room.TaxiFromEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Taxi From settings for the room set to " + (Room.TaxiFromEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "shoot":
                    {
                        Room.ShootEnabled = !Room.ShootEnabled;
                        Room.RoomData.ShootEnabled = Room.ShootEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `shoot_enabled` = @ShootEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("ShootEnabled", PolarEnvironment.BoolToEnum(Room.ShootEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Shoot settings for the room set to " + (Room.ShootEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }
                case "buycar":
                    {
                        Room.BuyCarEnabled = !Room.BuyCarEnabled;
                        Room.RoomData.BuyCarEnabled = Room.BuyCarEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `buycar_enabled` = @BuyCarEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("BuyCarEnabled", PolarEnvironment.BoolToEnum(Room.BuyCarEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("El modo 'Concesionario' está ahora " + (Room.BuyCarEnabled == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "hospital":
                    {
                        Room.IsHospital = !Room.IsHospital;
                        Room.RoomData.IsHospital = Room.IsHospital;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `is_hospital` = @IsHospital WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsHospital", PolarEnvironment.BoolToEnum(Room.IsHospital));
                            dbClient.RunQuery();
                        }
                        Room.IsPrison = false;
                        Room.IsPrison2 = false;
                        Room.IsPolStation = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        PolarEnvironment.GetGame()._rproomManager.Init();
                        Session.SendWhisper("El modo 'Hospital' está ahora " + (Room.IsHospital == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "prision":
                    {
                        Room.IsPrison = !Room.IsPrison;
                        Room.RoomData.IsPrison = Room.IsPrison;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `is_prison` = @IsPrison WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsPrison", PolarEnvironment.BoolToEnum(Room.IsPrison));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPolStation = false;
                        Room.IsPrison2 = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        PolarEnvironment.GetGame()._rproomManager.Init();
                        Session.SendWhisper("El modo 'Prisión' está ahora " + (Room.IsPrison == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "prisionback":
                    {
                        Room.IsPrison2 = !Room.IsPrison2;
                        Room.RoomData.IsPrison2 = Room.IsPrison2;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `is_prisonback` = @IsPrisonBack WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsPrisonBack", PolarEnvironment.BoolToEnum(Room.IsPrison2));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPolStation = false;
                        Room.IsPrison = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        PolarEnvironment.GetGame()._rproomManager.Init();
                        Session.SendWhisper("El modo 'Jailbreak' está ahora " + (Room.IsPrison == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "comisaria":
                case "polstation":
                    {
                        Room.IsPolStation = !Room.IsPolStation;
                        Room.RoomData.IsPolStation = Room.IsPolStation;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `is_polstation` = @IsPolStation WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsPolStation", PolarEnvironment.BoolToEnum(Room.IsPolStation));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPrison = false;
                        Room.IsPrison2 = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        //PolarEnvironment.GetGame()._rproomManager.Init();
                        Session.SendWhisper("El modo 'Estación de Policía' está ahora " + (Room.IsPolStation == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "camionero":
                    {
                        Room.IsCamionero = !Room.IsCamionero;
                        Room.RoomData.IsCamionero = Room.IsCamionero;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `is_camionero` = @IsCamionero WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsCamionero", PolarEnvironment.BoolToEnum(Room.IsCamionero));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPrison = false;
                        Room.IsPrison2 = false;
                        Room.IsMecanico = false;
                        Room.IsBasurero = false;
                        Room.IsPolStation = false;
                        //PolarEnvironment.GetGame()._rproomManager.Init();
                        Session.SendWhisper("El modo 'Camionero' está ahora " + (Room.IsCamionero == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                
                case "basurero":
                    {
                        Room.IsBasurero = !Room.IsBasurero;
                        Room.RoomData.IsBasurero = Room.IsBasurero;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `is_basurero` = @IsBasurero WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("IsBasurero", PolarEnvironment.BoolToEnum(Room.IsBasurero));
                            dbClient.RunQuery();
                        }
                        Room.IsHospital = false;
                        Room.IsPrison = false;
                        Room.IsPrison2 = false;
                        Room.IsCamionero = false;
                        Room.IsMecanico = false;
                        Room.IsPolStation = false;
                        //PolarEnvironment.GetGame()._rproomManager.Init();
                        Session.SendWhisper("El modo 'Basurero' está ahora " + (Room.IsBasurero == true ? "activado." : "desactivado."), 1);
                        break;
                    }
                case "hit":
                    {
                        Room.HitEnabled = !Room.HitEnabled;
                        Room.RoomData.HitEnabled = Room.HitEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `hit_enabled` = @HitEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("HitEnabled", PolarEnvironment.BoolToEnum(Room.HitEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Hit settings for the room set to " + (Room.HitEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "drive":
                case "car":
                    {
                        Room.DriveEnabled = !Room.DriveEnabled;
                        Room.RoomData.DriveEnabled = Room.DriveEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `drive_enabled` = @DriveEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("DriveEnabled", PolarEnvironment.BoolToEnum(Room.DriveEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Drive settings for the room set to " + (Room.DriveEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "tutorial":
                    {
                        Room.TutorialEnabled = !Room.TutorialEnabled;
                        Room.RoomData.TutorialEnabled = Room.TutorialEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `tutorial_enabled` = @TutorialEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("TutorialEnabled", PolarEnvironment.BoolToEnum(Room.TutorialEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Tutorial settings for the room set to " + (Room.TutorialEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "rob":
                    {
                        Room.RobEnabled = !Room.RobEnabled;
                        Room.RoomData.RobEnabled = Room.RobEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `rob_enabled` = @RobEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("RobEnabled", PolarEnvironment.BoolToEnum(Room.RobEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Rob settings for the room set to " + (Room.RobEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "sex":
                case "sexcommands":
                    {
                        Room.SexCommandsEnabled = !Room.SexCommandsEnabled;
                        Room.RoomData.SexCommandsEnabled = Room.SexCommandsEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `sexcommands_enabled` = @SexCommandsEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("SexCommandsEnabled", PolarEnvironment.BoolToEnum(Room.SexCommandsEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Sex Commands settings for the room set to " + (Room.SexCommandsEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "turf":
                    {
                        Room.TurfEnabled = !Room.TurfEnabled;
                        Room.RoomData.TurfEnabled = Room.TurfEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `turf_enabled` = @TurfEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("TurfEnabled", PolarEnvironment.BoolToEnum(Room.TurfEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Turf settings for the room set to " + (Room.TurfEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "gym":
                    {
                        Room.GymEnabled = !Room.GymEnabled;
                        Room.RoomData.GymEnabled = Room.GymEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `gym_enabled` = @GymEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("GymEnabled", PolarEnvironment.BoolToEnum(Room.GymEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Gym settings for the room set to " + (Room.GymEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "delivery":
                    {
                        Room.DeliveryEnabled = !Room.DeliveryEnabled;
                        Room.RoomData.DeliveryEnabled = Room.DeliveryEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `delivery_enabled` = @DeliveryEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("DeliveryEnabled", PolarEnvironment.BoolToEnum(Room.DeliveryEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Delivery settings for the room set to " + (Room.DeliveryEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "message":
                    {
                        if (Params.Length < 3)
                        {
                            Session.SendWhisper("You need to type a room entrance message!", 1);
                            break;
                        }

                        string Message = CommandManager.MergeParams(Params, 2);

                        Room.EnterRoomMessage = Message;
                        Room.RoomData.EnterRoomMessage = Room.EnterRoomMessage;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);

                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `enter_message` = @EnterMessage WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("EnterMessage", Room.EnterRoomMessage);
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("The Rooms enter message is now: " + Room.EnterRoomMessage, 1);
                        break;
                    }

                case "safezone":
                    {
                        Room.SafeZoneEnabled = !Room.SafeZoneEnabled;
                        Room.RoomData.SafeZoneEnabled = Room.SafeZoneEnabled;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rp_rooms` SET `safezone_enabled` = @SafeZoneEnabled WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("SafeZoneEnabled", PolarEnvironment.BoolToEnum(Room.SafeZoneEnabled));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Safezone mode is now " + (Room.SafeZoneEnabled == true ? "enabled!" : "disabled!"), 1);
                        break;
                    }

                case "pets":
                case "morphs":
                    {
                        Room.PetMorphsAllowed = !Room.PetMorphsAllowed;
                        Room.RoomData.PetMorphsAllowed = Room.PetMorphsAllowed;
                        PolarEnvironment.GetGame().GetRoomManager().UpdateRoom(Room);
                        using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `rooms` SET `pet_morphs_allowed` = @PetMorphsAllowed WHERE `id` = '" + Room.Id + "' LIMIT 1");
                            dbClient.AddParameter("PetMorphsAllowed", PolarEnvironment.BoolToEnum(Room.PetMorphsAllowed));
                            dbClient.RunQuery();
                        }

                        Session.SendWhisper("Human pet morphs notifications mode set to " + (Room.PetMorphsAllowed == true ? "enabled!" : "disabled!"), 1);

                        if (!Room.PetMorphsAllowed)
                        {
                            foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
                            {
                                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                                    continue;

                                User.GetClient().SendWhisper("The room owner has disabled the ability to use a pet morph in this room.", 1);
                                if (User.GetClient().GetHabbo().PetId > 0)
                                {
                                    // Tell the user what is going on.
                                    User.GetClient().SendWhisper("Oops, the room owner has just disabled pet-morphs, un-morphing you.", 1);

                                    // Change the users Pet Id.
                                    User.GetClient().GetHabbo().PetId = 0;

                                    // Quickly remove the old user instance.
                                    Room.SendMessage(new UserRemoveComposer(User.VirtualId));

                                    // Add the new one, they won't even notice a thing.
                                    Room.SendMessage(new UsersComposer(User));
                                }
                            }
                        }
                        break;
                    }
            }
        }
    }
}
