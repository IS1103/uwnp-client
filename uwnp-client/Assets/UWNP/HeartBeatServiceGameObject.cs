using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocket4Net;

namespace UWNP
{
    public class HeartBeatServiceGameObject : MonoBehaviour
    {
        public Action OnServerTimeout;
        private WebSocket socket;
        public float interval = 0;

        public long lastReceiveHeartbeatTime;

        void Start()
        {
            
        }

        public static long GetTimestamp()
        {
            TimeSpan ts = DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1);//ToUniversalTime()转换为标准时区的时间,去掉的话直接就用北京时间
            //return (long)ts.TotalMilliseconds; //精确到毫秒
            return (long)ts.TotalSeconds;//获取10位
        }

        public float t;

        void Update()
        {
            t += Time.deltaTime;
            if (t >= interval)
            {
                CheckAndSendHearbeat();
                t = 0;
            }
        }

        private void CheckAndSendHearbeat()
        {
            //檢查最後一次取得心跳包的時間是否小於客戶端心跳間隔時間
            long s1 = GetTimestamp();
            long s = (s1 - lastReceiveHeartbeatTime);
            if (s > interval)
            {
                Debug.Log(string.Format("CheckAndSendHearbeat：s1:{0} l:{1} s:{2} s > interval:{3}", s1, lastReceiveHeartbeatTime, s, s > interval));
                this.enabled = false;
                OnServerTimeout?.Invoke();
            }
            else
            {
                this.enabled = true;
                SendHeartbeatPack();
            }
        }

        public void HitHole()
        {
            lastReceiveHeartbeatTime = GetTimestamp();
        }

        private void SendHeartbeatPack()
        {
            //lastSendHeartbeatPackTime = DateTime.Now;
            byte[] package = PackageProtocol.Encode(
                PackageType.HEARTBEAT);
            socket.Send(package, 0, package.Length);//*/
        }

        internal void Setup(uint interval, Action onServerTimeout, WebSocket socket)
        {
            this.socket = socket;
            this.interval = (interval / 1000 )/2;
            this.OnServerTimeout = onServerTimeout;
            this.enabled = true;
            SendHeartbeatPack();
        }

        internal void ResetTimeout(uint interval)
        {
            this.enabled = true;
            this.interval = (interval / 1000) / 2;
            t = 0;
            long s1 = GetTimestamp();
            long s = (s1 - lastReceiveHeartbeatTime);
            Debug.Log(string.Format("ResetTimeout： s1:{0} l:{1} s:{2} s > interval:{3}", s1, lastReceiveHeartbeatTime, s, s > interval));
            lastReceiveHeartbeatTime = GetTimestamp();
            SendHeartbeatPack();
        }

        internal void Stop()
        {
            this.enabled = false;
            t = 0;
        }
    }
}