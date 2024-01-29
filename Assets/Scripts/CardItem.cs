using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CardItem : MonoBehaviour
{
    Toggle toggle; 

    // Start is called before the first frame update
    public void Init(Sprite sprite, UnityAction<CardItem, bool> callback)
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener((on) => callback?.Invoke(this, on));
        toggle.image.sprite = sprite;
    }

    public bool IsOn
    {
        get => toggle.isOn;
        set => toggle.SetIsOnWithoutNotify(value);
    }
}
