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
        public HeartBeatServiceGameObject heartBeatServiceGo;
        public Action OnReconect;
        public Action<string> OnError;

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
                handshakeTcs.TrySetCanceled();
            }
        }

        public async void OnReceive(byte[] bytes)
        {
            try
            {
                await UniTask.SwitchToMainThread();
                Package package = PackageProtocol.Decode(bytes);

                //Debug.Log(package.packageType);

                switch ((PackageType)package.packageType)
                {
                    case PackageType.HEARTBEAT:
                        //Debug.LogWarning("get HEARTBEAT");
                        heartBeatServiceGo.HitHole();
                        break;
                    case PackageType.RESPONSE:
                        ResponseHandler(package);
                        break;
                    case PackageType.PUSH:
                        PushHandler(package);
                        break;
                    case PackageType.HANDSHAKE:
                        HandshakeHandler(package);
                        break;
                    case PackageType.KICK:

                        //HandleKick(package);
                        break;
                    case PackageType.ERROR:
                        ErrorHandler(package);
                        break;
                    default:
                        Debug.Log("No match packageType::" + package.packageType);
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
            if (heartBeatServiceGo != null)
            {
                Debug.Log("Stop Heartbeat");
                heartBeatServiceGo.Stop();
                //heartBeatServiceGo = null;
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
                if (packTcs.ContainsKey(package.packID))
                {
                    packTcs.Remove(package.packID);
                }
            }
        }

        private void HandshakeHandler(Package package)
        {
            Message<Heartbeat> msg = MessageProtocol.Decode<Heartbeat>(package.buff);
            if (msg.err > 0)
            {
                handshakeTcs.TrySetResult(false);
                OnError?.Invoke(msg.errMsg);
                return;
            }

            if (heartBeatServiceGo == null)
            {
                GameObject go = new GameObject();
                go.name = "heartBeatServiceGo";
                heartBeatServiceGo = go.AddComponent(typeof(HeartBeatServiceGameObject)) as HeartBeatServiceGameObject;
                heartBeatServiceGo.Setup(msg.info.heartbeat, OnServerTimeout, socket);
            }
            else
            {
                OnReconect?.Invoke();
                heartBeatServiceGo.ResetTimeout(msg.info.heartbeat);
            }//*/
            handshakeTcs.TrySetResult(true);
        }

        private void ErrorHandler(Package package)
        {
            Message<byte[]> msg = MessageProtocol.Decode<byte[]>(package.buff);
            Debug.LogError(string.Format("packType:{2} err:{0} msg:{1}", msg.err, msg.errMsg, package.packageType));
        }

        private void OnServerTimeout()
        {
            if (socket.State == WebSocketState.Connecting)
            {
                socket.Close();
            }
            if (heartBeatServiceGo != null && socket.State != WebSocketState.Connecting && socket.State != WebSocketState.Open)
            {
                heartBeatServiceGo.Stop();
            }
        }
    }
}