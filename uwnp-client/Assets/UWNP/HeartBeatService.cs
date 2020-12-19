using System;
using System.Timers;
using UnityEngine;
using WebSocket4Net;

namespace UWNP
{
    public class HeartBeatService
    {
        public Action OnServerTimeout;
        private WebSocket socket;
        uint interval = 0;
        Timer timer = null;

        DateTime lastHeartbeatTime;
        TimeSpan span;

        public HeartBeatService(uint interval, Action onServerTimeout, WebSocket socket)
        {
            this.socket = socket;
            this.interval = interval;
            this.OnServerTimeout = onServerTimeout;

            timer = new Timer();
            timer.Interval = this.interval;
            timer.Elapsed += this.CheckAndSendHearbeat;
            ResetTimeout();
        }

        private void CheckAndSendHearbeat(object source, ElapsedEventArgs e)
        {
            //檢查最後一次取得心跳包的時間是否小於心跳間隔時間
            span = DateTime.Now - lastHeartbeatTime;
            if ((int)span.TotalMilliseconds <= interval)
            {
                //伺服器準時送回心跳包
                Start();
            }
            else
            {
                //過了心跳間隔的時間伺服器沒有回應心跳
                Stop();
                OnServerTimeout?.Invoke();
            }
        }

        public void ResetTimeout()
        {
            lastHeartbeatTime = DateTime.Now;
            Stop();
            Start();
        }

        private void SendHeartbeatPack()
        {
            Debug.Log("送出心跳包");
            byte[] package = PackageProtocol.Encode(
                PackageType.HEARTBEAT);
            socket.Send(package, 0, package.Length);
        }

        private void onServerTimeout(object source, ElapsedEventArgs e)
        {
            Stop();
            OnServerTimeout?.Invoke();
        }

        public void HitHole()
        {
            lastHeartbeatTime = DateTime.Now;
        }

        public void Start()
        {
            if (timer != null)
            {
                timer.Enabled = true;
                timer.Start();

                SendHeartbeatPack();
            }
        }

        public void Stop()
        {
            if (timer!=null)
            {
                timer.Enabled = false;
                timer.Stop();
            }
        }
    }
}