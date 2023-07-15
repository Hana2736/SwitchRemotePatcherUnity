using UnityEngine;
using UnityEngine.UI;

public class ShowSaveBtn : MonoBehaviour
{
    public GameObject saveBtn;

    private void Start()
    {
        saveBtn.GetComponent<Button>().interactable = false;
    }

    public void textChange()
    {
        saveBtn.GetComponent<Button>().interactable = true;
    }
}