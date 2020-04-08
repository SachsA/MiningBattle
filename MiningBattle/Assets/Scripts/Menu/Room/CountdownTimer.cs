// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CountdownTimer.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Utilities,
// </copyright>
// <summary>
// This is a basic CountdownTimer. In order to start the timer, the MasterClient can add a certain entry to the Custom Room Properties,
// which contains the property's name 'StartTime' and the actual start time describing the moment, the timer has been started.
// To have a synchronized timer, the best practice is to use PhotonNetwork.Time.
// In order to subscribe to the CountdownTimerHasExpired event you can call CountdownTimer.OnCountdownTimerHasExpired += OnCountdownTimerIsExpired;
// from Unity's OnEnable function for example. For unsubscribing simply call CountdownTimer.OnCountdownTimerHasExpired -= OnCountdownTimerIsExpired;.
// You can do this from Unity's OnDisable function for example.
// </summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.UI;

using ExitGames.Client.Photon;
using Photon.Pun;

namespace Photon.Pun.UtilityScripts
{
    /// <summary>
    /// This is a basic CountdownTimer. In order to start the timer, the MasterClient can add a certain entry to the Custom Room Properties,
    /// which contains the property's name 'StartTime' and the actual start time describing the moment, the timer has been started.
    /// To have a synchronized timer, the best practice is to use PhotonNetwork.Time.
    /// In order to subscribe to the CountdownTimerHasExpired event you can call CountdownTimer.OnCountdownTimerHasExpired += OnCountdownTimerIsExpired;
    /// from Unity's OnEnable function for example. For unsubscribing simply call CountdownTimer.OnCountdownTimerHasExpired -= OnCountdownTimerIsExpired;.
    /// You can do this from Unity's OnDisable function for example.
    /// </summary>
    public class CountdownTimer : MonoBehaviourPunCallbacks, IPunObservable
    {
        public const string CountdownStartTime = "StartTime";
        public const string CountdownStop = "Stop";

        /// <summary>
        /// OnCountdownTimerHasExpired delegate.
        /// </summary>
        public delegate void CountdownTimerHasExpired();

        /// <summary>
        /// Called when the timer has expired.
        /// </summary>
        public static event CountdownTimerHasExpired OnCountdownTimerHasExpired;

        private bool isTimerRunning;

        private float startTime;

        [Header("Countdown time in seconds")]
        public float Countdown = 5.0f;


        public void Update()
        {
            if (!isTimerRunning)
            {
                return;
            }

            float timer = (float)PhotonNetwork.Time - startTime;
            float countdown = Countdown - timer;

            if (countdown > 0.0f)
            {
                return;
            }

            isTimerRunning = false;

            RoomManager.Instance.LoadGame();

            if (OnCountdownTimerHasExpired != null)
            {
                OnCountdownTimerHasExpired();
            }
        }

        private string FormatCountdown(float countdown)
        {
            var seconds = (int)countdown % 60;
            var fraction = countdown * 1000;
            fraction = fraction % 1000;
            string timeText = string.Format("{0:00}:{1:000}", seconds, fraction);
            return timeText;
        }

        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            object startTimeFromProps;

            if (propertiesThatChanged.TryGetValue(CountdownStartTime, out startTimeFromProps))
            {
                isTimerRunning = true;
                startTime = (float)startTimeFromProps;
                RoomManager.Instance.timerHasStarted = true;
            }
            if (propertiesThatChanged.TryGetValue(CountdownStop, out startTimeFromProps))
            {
                isTimerRunning = false;
                RoomManager.Instance.timerHasStarted = false;
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
        }
    }
}