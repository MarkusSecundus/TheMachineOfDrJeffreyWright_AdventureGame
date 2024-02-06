using MarkusSecundus.TinyDialog;
using UnityEngine;

public class DialogRunner : MonoBehaviour
{
    DialogManager _manager_fld;
    DialogManager _manager => _manager_fld ? _manager_fld : _manager_fld = Object.FindAnyObjectByType<DialogManager>();
    public void RunDialog(string dialogName)
    {
        _manager.RunDialog(dialogName);
    }
}
