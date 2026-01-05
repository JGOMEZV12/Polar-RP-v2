using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using log4net;

using Polar.Communication.Packets.Outgoing.Inventory.Furni;
using Polar.Communication.Packets.Outgoing.Catalog;
using Polar.HabboHotel.Items;
using Polar.Communication.Packets.Outgoing.Handshake;
using Polar.Database.Interfaces;

namespace Polar.HabboHotel.Users.Process
{
    sealed class ProcessComponent
    {
        private static readonly ILog log = LogManager.GetLogger("Polar.HabboHotel.Users.Process.ProcessComponent");

        private Habbo _player = null;
        private Timer _timer = null;
        private bool _timerRunning = false;
        private bool _timerLagging = false;
        private bool _disabled = false;
        private AutoResetEvent _resetEvent = new AutoResetEvent(true);
        private static int _runtimeInSec = 1;

        public bool Init(Habbo Player)
        {
            if (Player == null)
                return false;
            else if (this._player != null)
                return false;

            this._player = Player;
            this._timer = new Timer(new TimerCallback(Run), null, _runtimeInSec * 1000, _runtimeInSec * 1000);
            return true;
        }

        /// <summary>
        /// Called for each time the timer ticks.
        /// </summary>
        /// <param name="State"></param>
        public void Run(object State)
        {
            try
            {
                if (this._disabled)
                    return;

                if (this._timerRunning)
                {
                    this._timerLagging = true;
                    log.Warn("<Player " + this._player.Id + "> Server can't keep up, Player timer is lagging behind.");
                    return;
                }

                this._resetEvent.Reset();

                // BEGIN CODE

                #region Muted Checks
                if (this._player.TimeMuted > 0.0)
                    this._player.TimeMuted -= 1.0;
                if (this._player.GetClient() != null && this._player.GetClient().GetRoleplay() != null && this._player.GetClient().GetRoleplay().VIPBanned > 0)
                    this._player.GetClient().GetRoleplay().VIPBanned--;
                #endregion

                #region Console Checks
                if (this._player.MessengerSpamTime > 0)
                    this._player.MessengerSpamTime -= 60.0;
                if (this._player.MessengerSpamTime <= 0)
                    this._player.MessengerSpamCount = 0;
                #endregion

                this._player.TimeAFK += 1;

                #region Respect checking
                if (this._player.GetStats().RespectsTimestamp != DateTime.Today.ToString("MM/dd"))
                {
                    this._player.GetStats().RespectsTimestamp = DateTime.Today.ToString("MM/dd");
                    using (IQueryAdapter dbClient = PolarEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE `user_stats` SET `dailyRespectPoints` = '3', `respectsTimestamp` = '" + DateTime.Today.ToString("MM/dd") + "' WHERE `id` = '" + this._player.Id + "' LIMIT 1");
                    }

                    this._player.GetStats().DailyRespectPoints = 3;
                    this._player.GetStats().DailyPetRespectPoints = 3;

                    if (this._player.GetClient() != null)
                    {
                        this._player.GetClient().SendMessage(new UserObjectComposer(this._player));
                    }
                }
                #endregion

                #region Reset Scripting Warnings
                if (this._player.GiftPurchasingWarnings < 15)
                    this._player.GiftPurchasingWarnings = 0;

                if (this._player.MottoUpdateWarnings < 15)
                    this._player.MottoUpdateWarnings = 0;

                if (this._player.ClothingUpdateWarnings < 15)
                    this._player.ClothingUpdateWarnings = 0;
                #endregion


                if (this._player.GetClient() != null)
                    PolarEnvironment.GetGame().GetAchievementManager().ProgressAchievement(this._player.GetClient(), "ACH_AllTimeHotelPresence", 1);

                this._player.Effects().CheckEffectExpiry(this._player);

                // END CODE

                // Reset the values
                this._timerRunning = false;
                this._timerLagging = false;

                this._resetEvent.Set();
            }
            catch { }
        }

        /// <summary>
        /// Stops the timer and disposes everything.
        /// </summary>
        public void Dispose()
        {
            // Wait until any processing is complete first.
            try
            {
                this._resetEvent.WaitOne(TimeSpan.FromMinutes(5.0));
            }
            catch { } // give up

            // Set the timer to disabled
            this._disabled = true;

            // Dispose the timer to disable it.
            try
            {
                if (this._timer != null)
                    this._timer.Dispose();
            }
            catch { }

            // Remove reference to the timer.
            this._timer = null;

            // Null the player so we don't reference it here anymore
            this._player = null;
        }
    }
}