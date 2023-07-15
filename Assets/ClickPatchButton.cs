using UnityEngine;
using UnityEngine.UI;

public class ClickPatchButton : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        var cheatToggle = transform.GetChild(0).GetComponent<Button>();
        var cheatName = transform.GetChild(1).GetComponent<Button>();
    }

    // Update is called once per frame
    private void Update()
    {
    }
}