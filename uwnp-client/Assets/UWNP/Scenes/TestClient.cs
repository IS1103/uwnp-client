using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UWNP{
    public class TestClient : MonoBehaviour
    {
        public ToggleGroup toggleGroup;
        public InputField ip, port;
        public string token, version;
        private string host;
        Client client;
        public Image img;

        [Obsolete]
        void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public void CreateConeccetionBtn() {

            string host = string.Format("ws://{0}:{1}/={2}", this.host, port.text, version);

            client = new Client(host);
            client.OnDisconnect = OnDisconnect;
            client.OnReconected = OnReconected;
            client.OnError = OnError;
            client.OnConnected = OnConnected;
            CreateConeccetion().Forget();
        }

        private void OnConnected()
        {
            Debug.Log("OnConnected");
        }

        private void OnError(string msg)
        {
            Debug.LogError(string.Format("err msg:{0}",msg));
        }

        private void OnReconected()
        {
            Debug.Log("OnReconect");
            img.gameObject.SetActive(true);
        }

        private void OnDisconnect()
        {
            Debug.Log("OnDisconnect");
            img.gameObject.SetActive(false);
        }

        public void pushBtn() {

            if (client != null) client.Cancel();

            IEnumerable<Toggle> toggles = toggleGroup.ActiveToggles();

            foreach (Toggle toggle in toggles)
            {
                if (toggle.isOn)
                {
                    host = toggle.GetComponentInChildren<Text>().text;
                    CreateConeccetionBtn();
                    return;
                }
            }
        }

        private async UniTaskVoid CreateConeccetion()
        {
            Debug.Log("開始連線..."+ host);

            int count = 3;
            bool isConeccet = false;
            while (count-->0 && !isConeccet)
            {
                Debug.Log(host);
                isConeccet = await client.ConnectAsync("jon");
            }
            
            if (isConeccet)
            {
                img.gameObject.SetActive(true);
                
                // On
                client.On("testOn",(Package pack) => {
                    TestPush info = MessageProtocol.DecodeInfo<TestPush>(pack.buff);
                    Debug.Log(JsonUtility.ToJson(info));
                    //img.gameObject.SetActive(false);
                });

                //請求/響應
                TestRq testRq = new TestRq();
                Message<TestRp> a = await client.RequestAsync<TestRq, TestRp>("TestController.testA", testRq);
                if (a.err>0)
                {
                    Debug.LogWarning("err:" + a.err);
                    Debug.LogWarning("err msg:" + a.errMsg);
                }
                else
                {
                    Debug.Log("a:" + a.info.packageType);
                }

                //請求/響應
                Message<TestRp2> a3 = await client.RequestAsync<TestRq, TestRp2>("TestController.testC",null,"custom1");
                if (a3.err > 0)
                {
                    Debug.LogWarning("err:" + a3.err);
                    Debug.LogWarning("err msg:" + a3.errMsg);
                }
                else
                {
                    Debug.Log("a:" + a3.info.info);
                }

                //通知
                //TestNotify testRq2 = new TestNotify() { name="小叮噹" };
                //client.Notify("TestController.testB", testRq2);
                //*/
            }
            else
                Debug.Log("多次嘗試連線但依然未連線");
        }

        public async void SendAPI()
        {
            //請求/響應
            TestRq testRq = new TestRq();
            Message<TestRp> a = await client.RequestAsync<TestRq, TestRp>("TestController.testA", testRq);
            if (a.err > 0)
            {
                Debug.LogWarning("err:" + a.err);
                Debug.LogWarning("err msg:" + a.errMsg);
            }
            else
            {
                Debug.Log("a:" + a.info.packageType);
            }
        }

        private void OnDestroy()
        {
            if (client!=null)
            {
                client.Cancel();
            }
        }
    }
}

