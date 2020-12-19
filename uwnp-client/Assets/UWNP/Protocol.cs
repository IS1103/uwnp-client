using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using WebSocket4Net;

namespace UWNP
{
    public class Protocol
    {
        Dictionary<string, Action<Package>> packAction = new Dictionary<string, Action<Package>>();
        UniTaskCompletionSource<bool> handshakeTcs;
        Dictionary<uint, UniTaskCompletionSource<Package>> packTcs = new Dictionary<uint, UniTaskCompletionSource<Package>>();
        WebSocket socket;
        HeartBeatService heartBeatService;

        public void SetSocket(WebSocket socket)
        {
            this.socket = socket;
        }

        public UniTask<bool> HandsharkAsync(string token)
        {
            handshakeTcs = new UniTaskCompletionSource<bool>();
            byte[] package = PackageProtocol.Encode<HandShake>(
                PackageType.HANDSHAKE,
                0,
                "SystemController.handShake",
                new HandShake() { token = token });
            socket.Send(package, 0, package.Length);
            return handshakeTcs.Task;
        }

        internal void Notify<T>(string route, T info)
        {
            byte[] packBuff = PackageProtocol.Encode<T>(
                PackageType.NOTIFY,
                0,
                route,
                info);
            socket.Send(packBuff, 0, packBuff.Length);
        }

        public UniTask<Package> RequestAsync<T>(uint packID, string route, T info = default)
        {
            lock (packTcs)
            {
                UniTaskCompletionSource<Package> pack = new UniTaskCompletionSource<Package>();
                byte[] packBuff = PackageProtocol.Encode<T>(
                PackageType.REQUEST,
                packID,
                route,
                info);
           
                packTcs.Add(packID, pack);
                socket.Send(packBuff, 0, packBuff.Length);
                return pack.Task;
            }
        }

        public void CanceledAllUTcs() {
            lock (packTcs)
            {
                foreach (var tcs in packTcs)
                {
                    tcs.Value.TrySetCanceled();
                }
                packTcs.Clear();
            }
        }

        public async void OnReceive(byte[] bytes)
        {
            try
            {
                Package package = PackageProtocol.Decode(bytes);

                //Debug.Log(package.packageType);

                switch ((PackageType)package.packageType)
                {
                    case PackageType.HEARTBEAT:
                        //Debug.Log(PackageType.HEARTBEAT);
                        heartBeatService.HitHole();
                        break;
                    case PackageType.RESPONSE:
                        await UniTask.SwitchToMainThread();
                        ResponseHandler(package);
                        break;
                    case PackageType.PUSH:
                        Debug.Log("PUSH");
                        await UniTask.SwitchToMainThread();
                        PushHandler(package);
                        break;
                    case PackageType.HANDSHAKE:
                        await UniTask.SwitchToMainThread();
                        //Debug.Log(PackageType.HANDSHAKE);
                        HandshakeHandler(package);
                        break;
                    case PackageType.KICK:
                        await UniTask.SwitchToMainThread();
                        //HandleKick(package);
                        break;
                    case PackageType.ERROR:
                        ErrorHandler(package);
                        break;
                    default:
                        Debug.Log("server 有傳東西但沒有相應的 PackageType:" + package.packageType);
                        break;
                }
            }
            catch (Exception e)
            {
                await UniTask.SwitchToMainThread();
                Debug.LogError(e);
                throw e;
            }
        }

        public void StopHeartbeat()
        {
            if (heartBeatService!=null)
            {
                heartBeatService.Stop();
            }
        }

        public void SetOn(string route, Action<Package> ac)
        {
            lock (packAction)
            {
                if (!packAction.ContainsKey(route))
                {
                    packAction.Add(route,ac);
                }
            }
        }

        private void PushHandler(Package pack)
        {
            lock (packAction)
            {
                if (packAction.ContainsKey(pack.route))
                {
#if SOCKET_DEBUG
                    Debug.Log(string.Format("[Push] <<-- [{0}] {1}", pack.route, JsonUtility.ToJson(pack)));
#endif
                    packAction[pack.route]?.Invoke(pack);
                    packAction.Remove(pack.route);
                }
            }
        }

        private void ResponseHandler(Package package)
        {
            lock (packTcs)
            {
                packTcs[package.packID].TrySetResult(package);
                packTcs.Remove(package.packID);
            }
        }

        private void HandshakeHandler(Package package)
        {
            Message<Heartbeat> msg = MessageProtocol.Decode<Heartbeat>(package.buff);
            if (msg.err > 0)
            {
                handshakeTcs.TrySetResult(false);
                Debug.LogError(msg.errMsg);
                return;
            }

            if (heartBeatService != null)
            {
                heartBeatService.ResetTimeout();
            }
            else
            {
                heartBeatService = new HeartBeatService(msg.info.heartbeat, OnSendHeartbeat, socket);
            }//*/
            handshakeTcs.TrySetResult(true);
        }

        private void ErrorHandler(Package package)
        {
            Message<byte[]> msg = MessageProtocol.Decode<byte[]>(package.buff);
            Debug.LogError(string.Format("packType:{2} err:{0} msg:{1}", msg.err, msg.errMsg, package.packageType));
        }

        private void OnSendHeartbeat()
        {
            Debug.Log("伺服器沒有回應心跳");
            socket.Close();
        }
    }
}