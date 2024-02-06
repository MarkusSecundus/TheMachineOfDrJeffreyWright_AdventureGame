using UnityEngine;

public class ChangeMusicAction : MonoBehaviour
{
    public void ChangeToTrack(int index)
    {
        MusicManager.Instance.SwitchTrack(index);
    }
}
