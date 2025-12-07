using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class InputFieldInteraction : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    TMP_InputField inputField;
    GameObject input;
    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();
        input = inputField.placeholder.gameObject;
    }
    public void OnSelect(BaseEventData eventData)
    {
        input.SetActive(false);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            input.SetActive(true);
        }
    }
}
