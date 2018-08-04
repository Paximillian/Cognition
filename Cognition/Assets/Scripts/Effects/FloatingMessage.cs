using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingMessage : MonoBehaviour
{
    [SerializeField]
    private Material m_LocalPlayerMaterial, m_EnemyPlayerMaterial;

    [SerializeField]
    private float m_FloatVelocity;

    [SerializeField]
    private float m_FloatDuration;

    public TextMesh Text { get; set; }
    public SpriteRenderer Sprite { get; set; }

    private void Awake()
    {
        Text = GetComponentInChildren<TextMesh>();
        Sprite = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine(floatText());
    }

    private IEnumerator floatText()
    {
        for (float t = 0; t < m_FloatDuration; t += Time.deltaTime)
        {
            transform.position += transform.up * m_FloatVelocity;
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void SetInvokingPlayerId(int i_InvokerId)
    {
        Sprite.material = i_InvokerId == NetworkPlayer.LocalPlayer.PlayerId ? m_LocalPlayerMaterial : m_EnemyPlayerMaterial;
    }
}
