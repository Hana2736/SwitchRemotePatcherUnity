using UnityEngine;

public class OnByDefaultToggle : MonoBehaviour
{
    public void toggleDefault(bool on)
    {
        if (on)
        {
            CommWithGame.defaultOnCheats.Add(AddPchButton.selectedPatch);
            SavePatchBtn.saveToDisk();
        }
        else
        {
            if (CommWithGame.defaultOnCheats.Contains(AddPchButton.selectedPatch))
            {
                CommWithGame.defaultOnCheats.Remove(AddPchButton.selectedPatch);
                SavePatchBtn.saveToDisk();
            }
        }
    }
}