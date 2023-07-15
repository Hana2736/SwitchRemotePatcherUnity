using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AddPchButton : MonoBehaviour
{
    public static string selectedPatch;
    public static Button lastBtn;
    public GameObject patchAddPopup;
    public TMP_InputField patchName;
    public Button confirmPch;
    public Button addPchBtn;
    public GameObject templateCard;
    public TMP_InputField patchData;
    public SavePatchBtn sav;
    public TMP_Text selLabel;
    public Toggle defOn;
    public TMP_Dropdown groupSel;
    public GameObject paneA, paneB, paneC, paneD, paneE, paneF;
    public ScrollRect pane;
    private GameObject instParent;

    private void Start()
    {
        try
        {
            addPchBtn.onClick.AddListener(() =>
            {
                patchAddPopup.SetActive(true);
                addPchBtn.interactable = false;
            });
        }
        catch (Exception ignored)
        {
            //this works
        }

        try
        {
            confirmPch.onClick.AddListener(() =>
            {
                patchAddPopup.SetActive(false);
                addPchBtn.enabled = true;
                instParent = groupSel.value switch
                {
                    0 => paneA,
                    1 => paneB,
                    2 => paneC,
                    3 => paneD,
                    4 => paneE,
                    5 => paneF,
                    _ => instParent
                };
                var c = Instantiate(templateCard, instParent.transform, false);
                c.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = patchName.text;
                var cName = patchName.text;
                CommWithGame.groupOfPatch[cName] = groupSel.value;
                c.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
                {
                    if (CommWithGame.patchTextRaw.ContainsKey(cName))
                        patchData.text = CommWithGame.patchTextRaw[cName];
                    else
                        patchData.text = "//placeholder";
                    selectedPatch = cName;
                    selLabel.text = "Selected: " + cName;
                    defOn.isOn = CommWithGame.defaultOnCheats.Contains(cName);
                });
                var toggleOn = c.transform.GetChild(0).GetComponent<Button>();
                lastBtn = toggleOn;
                toggleOn.onClick.AddListener(() =>
                {
                    var label = c.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
                    if (label.text == "OFF")
                    {
                        foreach (var line in CommWithGame.patchCode[cName])
                        {
                            CommWithGame.sendStr("pokeMain " + line[0] + " " + line[1]);
                            if (!line[1].StartsWith("0x"))
                                CommWithGame.sendStr("pokeMain " + (int.Parse(line[0]) + line[1].Length) + " 0x0");
                        }

                        CommWithGame.patchesOn.Add(cName);
                        label.text = "ON";
                    }
                    else
                    {
                        foreach (var line in CommWithGame.oldCode[cName])
                            CommWithGame.sendStr("pokeMain " + line[0] + " 0x" + line[1]);
                        CommWithGame.patchesOn.Remove(cName);
                        label.text = "OFF";
                    }
                });
                c.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() =>
                {
                    var label2 = c.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
                    if (label2.text == "ON")
                    {
                        StartCoroutine(sav.showError("Can't delete: Patch is still on."));
                        return;
                    }

                    var label = c.transform.GetChild(1).GetChild(0).GetComponent<TMP_Text>();
                    try
                    {
                        CommWithGame.patchCode.Remove(label.text);
                        CommWithGame.oldCode.Remove(label.text);
                        CommWithGame.patchTextRaw.Remove(label.text);
                        CommWithGame.defaultOnCheats.Remove(label.text);
                    }
                    catch (Exception e)
                    {
                        //its ok bro
                    }

                    if (label.text == selectedPatch)
                    {
                        selectedPatch = null;
                        patchData.text = "//select a patch :)";
                        selLabel.text = "Selected: none";
                    }

                    SavePatchBtn.saveToDisk();
                    Destroy(c.gameObject);
                });
                c.transform.GetChild(1).GetComponent<Button>().onClick.Invoke();
                addPchBtn.interactable = true;
            });
        }
        catch (Exception ignored)
        {
            //this also works
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void changeShownGroup(int val)
    {
        var view = val switch
        {
            0 => paneA,
            1 => paneB,
            2 => paneC,
            3 => paneD,
            4 => paneE,
            5 => paneF,
            _ => null
        };
        paneA.SetActive(false);
        paneB.SetActive(false);
        paneC.SetActive(false);
        paneD.SetActive(false);
        paneE.SetActive(false);
        paneF.SetActive(false);
        view.SetActive(true);
        pane.content = view.GetComponent<RectTransform>();
    }
}