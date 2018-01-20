using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadialMenuController : MonoBehaviour
{
    #region Variables
    public static RadialMenuController Instance { get; private set; }

    /// <summary>
    /// The items to choose from in the radial menu
    /// </summary>
    [SerializeField]
    private Cog[] m_Items;

    [Tooltip("How much empty space will be between each segment")]
    [SerializeField]
    private float m_SpaceBetweenSegments;

    [Tooltip("If 0, then cut straight from top, if -1, it is symmetrically offseted, otherwise offseted Counter-clockwise")]
    [SerializeField]
    private float m_Offset;

    /// <summary>
    /// The GameObject that will hold all the segment clones
    /// </summary>
    [SerializeField]
    private GameObject m_MenuParent;

    /// <summary>
    /// This is the background, might not use this at all
    /// </summary>
    [SerializeField]
    private GameObject m_CircleImage;

    /// <summary>
    /// The 2 colors for regular or highlighted items
    /// </summary>
    [SerializeField]
    private Color m_RegularColor;
    [SerializeField]
    private Color m_HighlightedColor;

    [Tooltip("The minimum distance you need to move the finger from center to make an item highlighted")]
    [SerializeField]
    private float m_MinLineDrawDistance;

    private bool m_isChoosing = false;
    private Vector2 m_mouseStartPos;

    private Image[] m_menuSegments;

    private int m_numOfItems;
    private float[] m_cutAngles;
    private Vector2[] m_segmentStartEndAngles;
    private Cog m_CurrentlySelectedCog;
    #endregion Variables

    #region UnityMethods
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start ()
    {
        turnOffAllItemsHightlight();
        m_isChoosing = false;
        m_MenuParent.SetActive(false);
        m_numOfItems            = m_Items.Length;
        m_segmentStartEndAngles = new Vector2[m_numOfItems];
        m_menuSegments          = new Image[m_numOfItems];

        m_cutAngles         = new float[m_numOfItems];
        float oneSegment    = 360 / m_numOfItems;

        for(int i = 0; i < m_numOfItems; i++)
        {
            GameObject newSegment               = Instantiate(m_CircleImage, m_MenuParent.transform);
            newSegment.transform.localPosition  = new Vector2(0, 0);
            Image newSegImg                     = newSegment.GetComponent<Image>();
            Image icon                          = newSegment.transform.Find("Icon").GetComponent<Image>();
            newSegImg.color                     = m_RegularColor;

            m_cutAngles[i]       = oneSegment * i;
            newSegImg.fillAmount = 1 / (360 / (oneSegment - (m_SpaceBetweenSegments / 2)));
            newSegImg.fillMethod = Image.FillMethod.Radial360;
            icon.sprite = m_Items[i].Sprite;

            float startPos = 0;
            float endPos   = 0;

            if (m_Offset == -1)
            {
                newSegment.transform.rotation = Quaternion.Euler(0,0, m_cutAngles[i]);
            }
            else if (m_Offset == 0)
            {
                newSegment.transform.rotation = Quaternion.Euler(0, 0, m_cutAngles[i] + (oneSegment / 2));
                startPos += oneSegment / 2;
                endPos   += oneSegment / 2;
            }
            else
            {
                newSegment.transform.rotation = Quaternion.Euler(0, 0, m_cutAngles[i] + m_Offset);
                startPos = m_Offset;
                endPos   = m_Offset;
            }

            m_menuSegments[i] = newSegImg;

            startPos = (startPos + m_cutAngles[i] < 0)              ? startPos + m_cutAngles[i] + 360 : startPos + m_cutAngles[i];
            endPos   = (startPos + m_cutAngles[i] + oneSegment < 0) ? endPos + m_cutAngles[i] + oneSegment + 360 : endPos + m_cutAngles[i] + oneSegment;

            m_segmentStartEndAngles[i] = new Vector2(startPos, (endPos > 360) ? endPos - 360 : endPos);

            icon.transform.RotateAround(transform.position, Vector3.forward, -oneSegment);
        }
    }
    #endregion UnityMethods

    #region PrivateMethods
    private void turnOffAllItemsHightlight()
    {
        if (m_menuSegments == null) return;
        foreach (Image image in m_menuSegments)
        {
            image.color = m_RegularColor;
        }

        m_CurrentlySelectedCog = null;
    }

    private void setPosition()
    {
        m_MenuParent.transform.position = m_mouseStartPos;
    }
    #endregion PrivateMethods

    #region PublicMethods
    public void SetItemHighlight(int index)
    {
        for(int i = 0; i < m_menuSegments.Length; i++)
        {
            if(i == index)
            {
                m_menuSegments[i].color = m_HighlightedColor;
                m_CurrentlySelectedCog = m_Items[i];
            }
            else
            {
                m_menuSegments[i].color = m_RegularColor;
            }
        }
    }

    public void OnPointerUp(PointerEventData i_pointerData, HexTile i_SelectedTile)
    {
        m_isChoosing = false;
        m_MenuParent.SetActive(false);

        if(m_CurrentlySelectedCog)
        {
            NetworkPlayer.LocalPlayer.BuildCog(i_SelectedTile, m_CurrentlySelectedCog);
        }
    }

    public void OnPointerDown(PointerEventData eventData, HexTile i_SelectedTile)
    {
        m_isChoosing = true;
        m_MenuParent.SetActive(true);
        turnOffAllItemsHightlight();
        m_mouseStartPos = eventData.position;
        m_MenuParent.transform.localPosition = m_mouseStartPos - new Vector2(Screen.width / 2, Screen.height / 2);
    }

    public void OnDrag(PointerEventData i_pointerData, HexTile i_SelectedTile)
    {
        if (m_isChoosing)
        {
            Vector2 v = i_pointerData.position - m_mouseStartPos;
            float angleRadians = Mathf.Atan2(v.y, v.x);
            float angleDegrees = angleRadians * Mathf.Rad2Deg;

            if (angleDegrees < 0) angleDegrees += 360;

            float distance = Vector2.Distance(m_mouseStartPos, i_pointerData.position);

            for (int i = 0; i < m_numOfItems; i++)
            {
                if (m_segmentStartEndAngles[i].x < m_segmentStartEndAngles[i].y)
                {
                    if (angleDegrees > m_segmentStartEndAngles[i].x && angleDegrees < m_segmentStartEndAngles[i].y)
                    {
                        if (distance > m_MinLineDrawDistance)
                        {
                            SetItemHighlight(i);
                        }
                        else
                        {
                            turnOffAllItemsHightlight();
                        }
                    }
                }
                else if ((angleDegrees > m_segmentStartEndAngles[i].x && angleDegrees <= 360) || (angleDegrees < m_segmentStartEndAngles[i].y && angleDegrees >= 0))
                {
                    if (distance > m_MinLineDrawDistance)
                    {
                        SetItemHighlight(i);
                    }
                    else
                    {
                        turnOffAllItemsHightlight();
                    }
                }
            }
        }
    }
    #endregion PublicMethods
}
