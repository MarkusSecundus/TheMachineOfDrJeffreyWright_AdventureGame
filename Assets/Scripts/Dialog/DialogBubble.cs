using Assets.Scripts.Dialog;
using MarkusSecundus.Utils.Randomness;
using MarkusSecundus.TinyDialog;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogBubble : MonoBehaviour, ITextBox
{
    [SerializeField] float perCharacter_seconds = 0.01f;
    [SerializeField] float endWait_seconds = 0.4f;
    //[SerializeField] float destroyFadeout_seconds = 1f;

    [SerializeField] AudioClip[] typingSounds;
    [SerializeField] int typingSoundsInterval = 5;

    [SerializeField] KeyCode[] skipPrintoutKeys = new KeyCode[] {KeyCode.Space, KeyCode.Mouse0 };

    [SerializeField] TMP_Text _textField_fld;
    TMP_Text _textField => _textField_fld ??= GetComponentInChildren<TMP_Text>();
    AudioSource _audioSource_fld;
    AudioSource _audioSource => _audioSource_fld ??= GetComponent<AudioSource>();


    event Action InterruptHandler;

    public void StartPrintout(string text, Action onClosed, ITextBox.DisplayMode displayMode)
    {
        _textField.text = "";
        gameObject.SetActive(true);

        StartCoroutine(printoutCoroutine());
        IEnumerator printoutCoroutine()
        {
            var cameraTag = new object();
            UICamera.TurnOn(cameraTag);
            yield return null;
            Debug.Log($"Initiating printout '{text}'");

            bool printoutFinished = false;
            bool displayFinished = false;
            bool endReguested = false;
            InterruptHandler += Interrupt;
            _textField.text = "";
            Debug.Log($"Starting printout", this);
            try
            {
                for (int t = 1; t <= text.Length; ++t)
                {
                    yield return new WaitForSeconds(perCharacter_seconds);
                    if (printoutFinished) t = text.Length;

                    _textField.text = $@"{text[0..t]}<color=#00000000>{text[t..]}</color>";
                    if (_audioSource && typingSounds.Length > 0 && typingSoundsInterval > 0 && t % typingSoundsInterval == 0)
                        _audioSource.PlayOneShot(typingSounds.RandomElement());
                }
                printoutFinished = true;
                if (endWait_seconds > 0f)
                    yield return new WaitForSeconds(endWait_seconds);
            }
            finally
            {
                displayFinished = true;
            }
            while (!endReguested) yield return null;

            InterruptHandler -= Interrupt;
            if (displayMode == ITextBox.DisplayMode.Normal)
                gameObject.SetActive(false);
            else if (displayMode == ITextBox.DisplayMode.Stay) { }
            else if(displayMode == ITextBox.DisplayMode.FadeOut)
            {
                Debug.LogError($"{nameof(ITextBox.DisplayMode.FadeOut)} is not implemented!", this);
                gameObject.SetActive(false);
            }
            UICamera.TurnOff(cameraTag);
            onClosed?.Invoke();

            void Interrupt()
            {
                printoutFinished = true;
                if (displayFinished)
                    endReguested = true;
            }
        }
    }

    void Update()
    {
        foreach(var k in skipPrintoutKeys)
            if (Input.GetKeyDown(k))
            {
                Debug.Log($"Requesting end by keyboard!", this);
                InterruptHandler?.Invoke();
            }
    }

    public void InterruptRequest()
    {
        Debug.Log($"Requesting end!", this);
        InterruptHandler?.Invoke();
    }



}
