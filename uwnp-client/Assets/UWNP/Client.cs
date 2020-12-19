using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using WebSocket4Net;
using System.Net;
using System.Threading;
using SuperSocket.ClientEngine;

namespace UWNP
{
    public enum NetWorkState
    {
        CONNECTING,
        CONNECTED,
        DISCONNECTED,
        TIMEOUT,
        ERROR,
        KICK
    }

    public class Client
    {
        private static int RqID = 0;

        public NetWorkState state;

        public Action OnReconect;

        Protocol protocol;

        WebSocket socket;

        public Client()
        {
            ServicePointManager.SecurityProtocol =
                    SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
                    SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;//*/
        }

        public UniTask<bool> ConnectAsync(string host, string token)
        {
            UniTaskCompletionSource<bool> utcs = new UniTaskCompletionSource<bool>();
            socket = new WebSocket(host);
            EventHandler<SuperSocket.ClientEngine.ErrorEventArgs> onErr = (sender, e) =>
            {
                Debug.LogError(e.Exception.Message);
                state = NetWorkState.ERROR;
                utcs.TrySetResult(false);
            };
            socket.Error += onErr;
            socket.Opened += async (sender, e) =>
            {
                socket.Error -= onErr;
                socket.Error += OnErr;
                socket.Closed += OnClose;
                protocol = new Protocol();
                protocol.SetSocket(socket);
                socket.DataReceived += OnReceived;
                bool isOK = await protocol.HandsharkAsync(token);
                //Debug.Log("open:" + e);
                socket.Closed += OnDisconect;
                state = NetWorkState.CONNECTED;
                utcs.TrySetResult(isOK);
            };

            state = NetWorkState.CONNECTING;
            socket.Open();
            return utcs.Task;
        }

        private void OnClose(object sender, EventArgs e)
        {
            Debug.LogError(e.ToString());
            state = NetWorkState.DISCONNECTED;
        }

        private void OnErr(object sender, ErrorEventArgs e)
        {
            Debug.LogError(e.Exception.Message);
            state = NetWorkState.ERROR;
            Destroy();
        }

        private void OnDisconect(object sender, EventArgs e)
        {
            state = NetWorkState.DISCONNECTED;
            protocol.StopHeartbeat();
            Debug.Log("close:" + e);
        }

        private void OnReceived(object sender, DataReceivedEventArgs e)
        {
            protocol.OnReceive(e.Data);
        }

        public void On(string route, Action<Package> cb)
        {
            protocol.SetOn(route, cb);
        }

        public void Notify<T>(string route, T info = default)
        {
            try
            {
#if SOCKET_DEBUG
                Debug.Log(string.Format("[Notify] -->> [{0}] {1}", route, JsonUtility.ToJson(info)));
#endif
                protocol.Notify<T>(route, info);
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("[Notify Exception]{0}", e.Message));
                throw e;
            }
        }

        public async UniTask<Message<S>> RequestAsync<T,S>(string route, T info = default)
        {
            uint rqID = (uint)Interlocked.Increment(ref RqID);
            try
            {
#if SOCKET_DEBUG
                Debug.Log(string.Format("[{0}][Request] -->> [{1}] {2}", rqID, route, JsonUtility.ToJson(info)));
#endif
                Package pack = await protocol.RequestAsync<T>(rqID, route, info);
                Message<S> msg = MessageProtocol.Decode<S>(pack.buff);
#if SOCKET_DEBUG
                Debug.Log(string.Format("[{0}][Request] <<-- [{1}] {2} {3}", rqID, route, JsonUtility.ToJson(msg), JsonUtility.ToJson(msg.info)));
#endif
                return msg;
            }
            catch (Exception e)
            {
                Debug.Log(string.Format("[{0}][RequestAsync Exception]{1}", rqID, e.Message));
                throw e;
            }
        }

        public void Destroy() {
            state = NetWorkState.DISCONNECTED;
            socket.Close();
            if (protocol!=null)
            {
                protocol.StopHeartbeat();
                protocol.CanceledAllUTcs();
            }
        }
    }
}