using UnityEngine;

public class ExtendBloodstainsDuration : MonoBehaviour
{
    CursorManager _manager_fld;
    CursorManager _manager => _manager_fld ? _manager_fld : _manager_fld = Object.FindAnyObjectByType<CursorManager>();
    public void ExtendBloodstainDuration(float additionalDuration)
    {
        _manager.ExtendBloodstainDuration(additionalDuration);
    }
}
