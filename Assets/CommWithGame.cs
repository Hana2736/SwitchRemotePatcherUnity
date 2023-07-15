using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommWithGame : MonoBehaviour
{
    public static TcpClient c;
    public static NetworkStream s;
    public static Dictionary<string, string> patchTextRaw;
    public static Dictionary<string, List<string[]>> patchCode;
    public static Dictionary<string, List<string[]>> oldCode;
    public static Dictionary<string, int> groupOfPatch;
    public static HashSet<string> defaultOnCheats;
    public static string titleId;
    public static HashSet<string> patchesOn;
    private static float timeWriter;
    private static float timeReader;
    public GameObject waitGamePopup, waitNetPopup, restartGamePopup;
    public GameObject patchPane;
    public GameObject addPchBtn;
    public GameObject saveBtn;
    public GameObject defToggle;
    public GameObject selLabel;
    public Button confirmPatch;
    public TMP_InputField patchName;
    public TMP_Dropdown groupSel;
    public GameObject groupViewSet;
    private bool gameNotFound;
    private bool gameReady;
    private float gameWatcherCd;
    private bool waitForGame;
    private float waitGameCd;

    private void Start()
    {
        c = new TcpClient();
        new Thread(_ =>
        {
            c.Connect(File.ReadAllText("ipAddr.txt"), 6000);
            s = c.GetStream();
            waitForGame = true;
        }).Start();
        patchTextRaw = new Dictionary<string, string>();
        oldCode = new Dictionary<string, List<string[]>>();
        patchCode = new Dictionary<string, List<string[]>>();
        defaultOnCheats = new HashSet<string>();
        groupOfPatch = new Dictionary<string, int>();
        patchesOn = new HashSet<string>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (c.Connected)
        {
            timeWriter += Time.deltaTime;
            timeReader += Time.deltaTime;
            gameWatcherCd += Time.deltaTime;
            if (gameReady && gameWatcherCd >= 30f && timeWriter > 30 && timeReader > 30)
            {
                gameWatcherCd = 0;
                sendStr("getTitleID");
                var msgs = readNow();
                if (msgs is not null)
                    foreach (var s in msgs)
                        if (s != titleId)
                        {
                            Debug.LogError("new game found: old " + titleId + " new: " + s);
                            restartGamePopup.SetActive(true);
                            addPchBtn.SetActive(false);
                            saveBtn.SetActive(false);
                            return;
                        }
            }


            waitGameCd += Time.deltaTime;
            if (waitGameCd >= 1 && waitForGame)
            {
                waitGameCd = 0;
                sendStr("getTitleID");
            }

            if (waitForGame)
            {
                var msgs = readNow();
                if (msgs is not null)
                    foreach (var s in msgs)
                        if (!s.Contains("0000000000000"))
                        {
                            waitNetPopup.SetActive(false);
                            waitForGame = false;
                            if (!gameNotFound)
                            {
                                restartGamePopup.SetActive(true);
                                return;
                            }

                            waitGamePopup.SetActive(false);
                            addPchBtn.SetActive(true);
                            patchPane.SetActive(true);
                            saveBtn.SetActive(true);
                            selLabel.SetActive(true);
                            defToggle.SetActive(true);
                            groupViewSet.SetActive(true);
                            titleId = s.Trim();

                            gameReady = true;

                            if (File.Exists("cheatsDataBase" + titleId + ".json"))
                            {
                                var loadedDB =
                                    JsonConvert.DeserializeObject<List<string>>(
                                        File.ReadAllText("cheatsDataBase" + titleId + ".json"));

                                patchTextRaw = JsonConvert.DeserializeObject<Dictionary<string, string>>(loadedDB[0]);

                                patchCode =
                                    JsonConvert.DeserializeObject<Dictionary<string, List<string[]>>>(loadedDB[1]);

                                oldCode = JsonConvert
                                    .DeserializeObject<Dictionary<string, List<string[]>>>(loadedDB[2]);


                                defaultOnCheats = JsonConvert.DeserializeObject<HashSet<string>>(loadedDB[3]);

                                groupOfPatch = JsonConvert.DeserializeObject<Dictionary<string, int>>(loadedDB[4]);

                                foreach (var pair in patchTextRaw)
                                {
                                    patchName.text = pair.Key;
                                    groupSel.value = groupOfPatch[pair.Key];
                                    confirmPatch.onClick.Invoke();
                                    if (defaultOnCheats.Contains(pair.Key)) AddPchButton.lastBtn.onClick.Invoke();
                                }
                            }
                        }
                        else
                        {
                            waitNetPopup.SetActive(false);
                            gameNotFound = true;
                        }
            }
        }
    }

    public static void sendStr(string st)
    {
        timeWriter = 0;
        s.Write(Encoding.UTF8.GetBytes(st + "\r\n"));
        s.Flush();
        Debug.Log("wrote to console: " + st);
        if (st.StartsWith("peek") || st.StartsWith("get"))
            Thread.Sleep(20);
    }

    public static List<string> readNow()
    {
        if (s is null)
            return null;
        if (s.DataAvailable)
        {
            var ret = new List<string>();
            var myReadBuffer = new byte[1024];
            var myCompleteMessage = new StringBuilder();
            var numberOfBytesRead = 0;
            do
            {
                numberOfBytesRead = s.Read(myReadBuffer, 0, myReadBuffer.Length);
                myCompleteMessage.AppendFormat("{0}", Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead));
            } while (s.DataAvailable);

            var msgsIn = myCompleteMessage.ToString().Split("\n");
            foreach (var msg in msgsIn)
            {
                if (msg.Trim().Length == 0)
                    continue;
                ret.Add(msg);
                Debug.Log("msg: " + msg);
            }

            return ret;
        }

        return null;
    }
}