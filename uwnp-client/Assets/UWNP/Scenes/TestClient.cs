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
        public string host,token;
        Client client;
        public Image img;

        [Obsolete]
        void Start()
        {
            client = new Client();
            CreateConeccetion().Forget();
        }

        private async UniTaskVoid CreateConeccetion()
        {
            Debug.Log("開始連線...");
            int count = 3;
            bool isConeccet = false;
            while (count-->0 && !isConeccet)
            {
                isConeccet = await client.ConnectAsync(host, token);
            }
            
            if (isConeccet)
            {
                Debug.Log("已連線");

                // On
                client.On("testOn",(Package pack) => {
                    TestPush info = MessageProtocol.DecodeInfo<TestPush>(pack.buff);
                    Debug.Log(JsonUtility.ToJson(info));
                    img.gameObject.SetActive(true);
                });

                //請求/響應
                TestRq testRq = new TestRq();
                Message<TestRp> a = await client.RequestAsync<TestRq, TestRp>("TestController.testA", testRq);
                Debug.Log("a:"+ a.info.packageType);

                //通知
                TestNotify testRq2 = new TestNotify() { name="小叮噹" };
                client.Notify("TestController.testB", testRq2);
            }
            else
                Debug.Log("多次嘗試連線但依然未連線");
        }

        private void OnDestroy()
        {
            client.Destroy();
        }
    }
}

