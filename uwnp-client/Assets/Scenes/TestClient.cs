using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace UWNP{
    public class TestClient : MonoBehaviour
    {
        public ToggleGroup toggleGroup;
        public InputField ip, port;
        public string host,token;
        Client client;
        public Image img;

        [Obsolete]
        void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public void CreateConeccetionBtn() {

            string host = string.Format("ws://{0}:{1}/=1.0.0", this.host, port.text);

            client = new Client(host, token, 3);
            client.OnDisconnect = OnDisconnect;
            CreateConeccetion().Forget();
        }

        private void OnDisconnect()
        {
            Debug.Log("已斷線");
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
                isConeccet = await client.ConnectAsync(token);
            }
            
            if (isConeccet)
            {
                img.gameObject.SetActive(true);
                /*
                // On
                client.On("testOn",(Package pack) => {
                    TestPush info = MessageProtocol.DecodeInfo<TestPush>(pack.buff);
                    Debug.Log(JsonUtility.ToJson(info));
                    img.gameObject.SetActive(false);
                });

                //請求/響應
                TestRq testRq = new TestRq();
                Message<TestRp> a = await client.RequestAsync<TestRq, TestRp>("TestController.testA", testRq);
                Debug.Log("a:"+ a.info.packageType);

                //通知
                TestNotify testRq2 = new TestNotify() { name="小叮噹" };
                client.Notify("TestController.testB", testRq2);
                //*/
            }
            else
                Debug.Log("多次嘗試連線但依然未連線");
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

