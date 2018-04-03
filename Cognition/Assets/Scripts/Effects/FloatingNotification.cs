using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class FloatingNotification : MonoBehaviour
{
    [SerializeField]
    private Image m_CogIcon;

    [SerializeField]
    private float m_FloatDuration;

    /// <summary>
    /// The object that this notification is attached to.
    /// </summary>
    private Transform m_Target;
    
    private RectTransform m_RectTransform;
    public Image[] Sprites { get; set; }

    private void Awake()
    {
        m_RectTransform = GetComponent<RectTransform>();
        Sprites = GetComponentsInChildren<Image>();
    }
    
    /// <summary>
    /// Fades out the notification while sticking to the object that it's attached to.
    /// </summary>
    private IEnumerator floatText()
    {
        for (float t = 0; t < m_FloatDuration; t += Time.deltaTime)
        {
            Vector2 screenPos = Camera.main.WorldToScreenPoint(m_Target.position);
            screenPos = new Vector2(Mathf.Clamp(screenPos.x, m_RectTransform.rect.width / 2, Screen.width - m_RectTransform.rect.width / 2),
                                    Mathf.Clamp(screenPos.y, m_RectTransform.rect.height / 2, Screen.height - m_RectTransform.rect.height / 2));

            m_RectTransform.position = screenPos;

            for (int i = 0; i < Sprites.Length; ++i)
            {
                Sprites[i].color = new Color(Sprites[i].color.r, Sprites[i].color.g, Sprites[i].color.b, 1 - (t / m_FloatDuration));
            }

            yield return null;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Sets the object that this notification is attached to, this notification will be displayed over it or in its direction if that object is offscreen.
    /// </summary>
    public void SetTarget(Cog i_Target)
    {
        m_Target = i_Target.transform;
        m_CogIcon.sprite = i_Target.Sprite;

        StartCoroutine(floatText());
    }
}
