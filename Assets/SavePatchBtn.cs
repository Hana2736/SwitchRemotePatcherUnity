using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SavePatchBtn : MonoBehaviour
{
    public Button saveBtn;
    public TMP_InputField codeField;
    public GameObject errorBox;
    public GameObject addPane;

    private void Start()
    {
        saveBtn.onClick.AddListener(() =>
        {
            saveBtn.GetComponent<Button>().interactable = false;
            try
            {
                CommWithGame.patchTextRaw[AddPchButton.selectedPatch] = codeField.text;
            }
            catch (Exception e)
            {
                StartCoroutine(showError("Select a patch first!"));
                return;
            }

            if (CommWithGame.patchesOn.Contains(AddPchButton.selectedPatch))
            {
                StartCoroutine(showError("Can't save: Patch is on. Turn it off first."));
                return;
            }

            var lines = codeField.text.Split("\n");
            var cheat = new List<string[]>();
            var oldCheat = new List<string[]>();
            foreach (var line in lines)
            {
                var commRemove = line;
                if (commRemove.Contains("//")) commRemove = commRemove.Substring(0, commRemove.IndexOf("//"));

                commRemove = commRemove.Trim();

                if (commRemove.Trim().Length == 0)
                    continue;
                if (commRemove[8] != ' ')
                {
                    StartCoroutine(showError("Can't save: Patch offset must be exactly 8 characters."));
                    return;
                }

                int addr;
                try
                {
                    addr = int.Parse(commRemove.Substring(0, 8), NumberStyles.HexNumber);
                }
                catch (Exception e)
                {
                    StartCoroutine(showError("Can't save: Patch offset not valid hex number"));
                    return;
                }


                cheat.Add(commRemove[9] != '\"'
                    ? new[] { addr.ToString(), "0x" + commRemove[9..] }
                    : new[] { addr.ToString(), commRemove.Substring(10, commRemove.Length - 11) });

                CommWithGame.sendStr("peekMain 0x" + commRemove[..8] + " " +
                                     (commRemove[9] != '\"' ? cheat[^1][1].Length / 2 - 1 : cheat[^1][1].Length + 1));
                List<string> read;
                do
                {
                    read = CommWithGame.readNow();
                } while (read is null);

                oldCheat.Add(new[] { addr.ToString(), read[0] });
            }

            CommWithGame.patchCode[AddPchButton.selectedPatch] = cheat;
            CommWithGame.oldCode[AddPchButton.selectedPatch] = oldCheat;
            saveToDisk();
        });
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public IEnumerator showError(string reason)
    {
        errorBox.SetActive(true);
        errorBox.transform.GetChild(0).GetComponent<TMP_Text>().text = reason;
        yield return new WaitForSeconds(3);
        errorBox.SetActive(false);
    }

    public static void saveToDisk()
    {
        var data = new List<string>
        {
            JsonConvert.SerializeObject(CommWithGame.patchTextRaw, Formatting.Indented),
            JsonConvert.SerializeObject(CommWithGame.patchCode, Formatting.Indented),
            JsonConvert.SerializeObject(CommWithGame.oldCode, Formatting.Indented),
            JsonConvert.SerializeObject(CommWithGame.defaultOnCheats, Formatting.Indented),
            JsonConvert.SerializeObject(CommWithGame.groupOfPatch, Formatting.Indented)
        };
        var outRaw = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText("cheatsDataBase" + CommWithGame.titleId + ".json", outRaw);
    }

    public void cancelBtn()
    {
        var parent = saveBtn.transform.parent;
        addPane.SetActive(false);
        parent.GetChild(2).GetComponent<Button>().interactable = true;
    }
}