[![License](https://img.shields.io/github/license/wsmd/ws-multipath.svg)](https://github.com/wsmd/ws-multipath/blob/master/LICENSE)

# 說明
![image](https://raw.githubusercontent.com/IS1103/uwnp-client/34d27c46dffea4b1788f10d578ba49ca7c9a410e/logo.png)
- [UWNP由來](#UWNP由來)
- [客戶端功能](#客戶端功能)
# UWNP由來
UWNP 全名是 unity+websocket+nodejs+protobuf 輕量級單線程連線框架，目的是讓開發者只專注在開發商業邏輯 API 。
# 客戶端功能
* 斷線重連
* 幾乎零配置，只撰寫 API。
* 客戶端、服務端有四種溝通方法
  * request 客戶端發出請求
  * response 服務端回復請求
  * notify 客戶端通知，服務端不必回復
  * push 服務端主動發送訊息給客戶端
* 同一個token無法重複登入，會把舊的連線關閉。
* 服務端從[這裡](https://github.com/IS1103/uwnp-server)點擊.
## 啟動
1. 執行 server
2. 選擇 127.0.0.1，點擊
2. 執行 uwnp-client/UWNP/Semple

![image](https://raw.githubusercontent.com/IS1103/uwnp-client/main/0.png)
# 與服務端建立連接
```C#
public class TestClient : MonoBehaviour
{
    public async void Start(){

        Client client = new Client("ws://127.0.0.1:3013/=1.0.0", "jon");
        client.OnDisconnect = OnDisconnect;
        client.OnReconect = OnReconect;
        client.OnError = OnError;

        bool isConeccet = await client.ConnectAsync(token);

        if(isConeccet){
            // listen
            client.On("testOn",(Package pack) => {
                TestPush info = MessageProtocol.DecodeInfo<TestPush>(pack.buff);
                Debug.Log(JsonUtility.ToJson(info));
            });

            //request/response
            TestRq testRq = new TestRq();
            Message<TestRp> a = await client.RequestAsync<TestRq, TestRp>("TestController.testA", testRq);
            if (a.err>0)
                Debug.LogWarning("err msg:" + a.errMsg);
            else
                Debug.Log("a:" + a.info.packageType);
            
            //notify mesage
            TestNotify testRq2 = new TestNotify() { name="Lesa" };
            client.Notify("TestController.testB", testRq2);
        }
    }

    private void OnError(uint err,string msg)
    {
        //do something...
    }

    private void OnReconect()
    {
        //do something...
    }

    private void OnDisconnect()
    {
        //do something...
    }
}

```