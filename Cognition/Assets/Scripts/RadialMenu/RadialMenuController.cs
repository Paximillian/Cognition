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
    
    [Tooltip("The minimum distance you need to move the finger from center to make an item highlighted")]
    [SerializeField]
    private float m_MinLineDrawDistance;

    private bool m_isChoosing = false;
    private Vector2 m_mouseStartPos;

    private RadialMenuSegment[] m_menuSegments;

    private int m_numOfItems;
    private float[] m_cutAngles;
    private Vector2[] m_segmentStartEndAngles;
    private Cog m_CurrentlySelectedCog;
    private HexTile m_CurrentlySelectedTile;

    /// <summary>
    /// The array index of the menu item that was last hovered upon.
    /// </summary>
    private int? m_HoveredItemIndex;
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
        setupItems();

        NetworkPlayer.LocalPlayer.ResourcesChanged += LocalPlayer_OnResourcesChanged;
        NetworkPlayer.LocalPlayer.CogBuilt += LocalPlayer_OnCogBuilt;
    }
    #endregion UnityMethods

    #region PrivateMethods
    /// <summary>
    /// Sets up the items represented by the radial menu for selection and building.
    /// </summary>
    private void setupItems()
    {
        turnOffAllItemsHightlight();
        m_isChoosing = false;
        m_MenuParent.SetActive(false);
        m_numOfItems = m_Items.Length;
        m_segmentStartEndAngles = new Vector2[m_numOfItems];
        m_menuSegments = new RadialMenuSegment[m_numOfItems];

        m_cutAngles = new float[m_numOfItems];
        float oneSegment = 360 / m_numOfItems;

        for (int i = 0; i < m_numOfItems; i++)
        {
            GameObject newSegment = Instantiate(m_CircleImage, m_MenuParent.transform);
            newSegment.transform.localPosition = new Vector2(0, 0);
            RadialMenuSegment newSegImg = newSegment.GetComponent<RadialMenuSegment>();
            Image icon = newSegImg.ItemIcon;

            m_cutAngles[i] = oneSegment * i;
            newSegImg.BackgroundImage.fillAmount = 1 / (360 / (oneSegment - (m_SpaceBetweenSegments / 2)));
            newSegImg.BackgroundImage.fillMethod = Image.FillMethod.Radial360;
            icon.sprite = m_Items[i].Sprite;

            float startPos = 0;
            float endPos = 0;

            if (m_Offset == -1)
            {
                newSegment.transform.rotation = Quaternion.Euler(0, 0, m_cutAngles[i]);
            }
            else if (m_Offset == 0)
            {
                newSegment.transform.rotation = Quaternion.Euler(0, 0, m_cutAngles[i] + (oneSegment / 2));
                startPos += oneSegment / 2;
                endPos += oneSegment / 2;
            }
            else
            {
                newSegment.transform.rotation = Quaternion.Euler(0, 0, m_cutAngles[i] + m_Offset);
                startPos = m_Offset;
                endPos = m_Offset;
            }

            m_menuSegments[i] = newSegImg;

            startPos = (startPos + m_cutAngles[i] < 0) ? startPos + m_cutAngles[i] + 360 : startPos + m_cutAngles[i];
            endPos = (startPos + m_cutAngles[i] + oneSegment < 0) ? endPos + m_cutAngles[i] + oneSegment + 360 : endPos + m_cutAngles[i] + oneSegment;

            m_segmentStartEndAngles[i] = new Vector2(startPos, (endPos > 360) ? endPos - 360 : endPos);

            icon.transform.RotateAround(transform.position, Vector3.forward, -oneSegment);
        }
    }

    private void turnOffAllItemsHightlight()
    {
        if (m_menuSegments == null) return;
        foreach (RadialMenuSegment image in m_menuSegments)
        {
            image.CurrentState = image.CurrentState.RemoveState(RadialMenuSegment.eSegmentState.Highlighted);
        }

        m_CurrentlySelectedCog = null;
    }

    private void setPosition()
    {
        m_MenuParent.transform.position = m_mouseStartPos;
    }

    /// <summary>
    /// A method that will be called when the resource count available to the player updates.
    /// </summary>
    private void LocalPlayer_OnResourcesChanged(int i_NewResourceCount)
    {
        checkForSufficientResources(i_NewResourceCount);
        SetItemHighlight(m_HoveredItemIndex);
    }

    /// <summary>
    /// A method that will be called when a new cog has been built by the player.
    /// </summary>
    private void LocalPlayer_OnCogBuilt(Cog i_NewlyBuiltCog)
    {
        checkForBuildRange();
        SetItemHighlight(m_HoveredItemIndex);
    }

    /// <summary>
    /// Checks for each item in the menu if it's in range to be built on this tile.
    /// </summary>
    private void checkForBuildRange()
    {
        for (int i = 0; i < m_menuSegments.Length; ++i)
        {
            if (!NetworkPlayer.LocalPlayer.CanBuildCog(m_CurrentlySelectedTile, m_Items[i]))
            {
                m_menuSegments[i].CurrentState = m_menuSegments[i].CurrentState.AddState(RadialMenuSegment.eSegmentState.OutOfRange);
            }
            else
            {
                m_menuSegments[i].CurrentState = m_menuSegments[i].CurrentState.RemoveState(RadialMenuSegment.eSegmentState.OutOfRange);
            }
        }
    }

    /// <summary>
    /// Checks for each item in the menu if we have enough resources to build it, and changes its interactivity state accordingly.
    /// </summary>
    private void checkForSufficientResources(int i_NewResourceCount)
    {
        for (int i = 0; i < m_menuSegments.Length; ++i)
        {
            if (m_Items[i].Cost > i_NewResourceCount)
            {
                m_menuSegments[i].CurrentState = m_menuSegments[i].CurrentState.AddState(RadialMenuSegment.eSegmentState.NotEnoughResources);
            }
            else
            {
                m_menuSegments[i].CurrentState = m_menuSegments[i].CurrentState.RemoveState(RadialMenuSegment.eSegmentState.NotEnoughResources);
            }
        }
    }

    /// <summary>
    /// Checks the building limitations of all buildable cogs in the menu to update their state.
    /// </summary>
    private void checkAllCogBuildingLimitations()
    {
        checkForBuildRange();
        checkForSufficientResources(NetworkPlayer.LocalPlayer.Resources);
    }

    /// <summary>
    /// Cancels the active build menu.
    /// </summary>
    private void cancelBuild()
    {
        m_CurrentlySelectedTile = null;
        m_isChoosing = false;
        m_CurrentlySelectedCog = null;
        m_MenuParent.SetActive(false);
    }
    #endregion PrivateMethods

    #region PublicMethods
    public void SetItemHighlight(int? index)
    {
        if (index.HasValue)
        {
            for (int i = 0; i < m_menuSegments.Length; i++)
            {
                if (i == index)
                {
                    if (m_menuSegments[i].CurrentState.CheckState(RadialMenuSegment.eSegmentState.Available))
                    {
                        m_menuSegments[i].CurrentState = m_menuSegments[i].CurrentState.AddState(RadialMenuSegment.eSegmentState.Highlighted);
                        m_CurrentlySelectedCog = m_Items[i];
                    }
                }
                else
                {
                    m_menuSegments[i].CurrentState = m_menuSegments[i].CurrentState.RemoveState(RadialMenuSegment.eSegmentState.Highlighted);
                }
            }
        }
    }

    public void OnPointerUp(PointerEventData i_pointerData, HexTile i_SelectedTile)
    {
        if (Input.touchCount > 1)
        {
            cancelBuild();
        }
        else
        {
            if (i_pointerData.button == PointerEventData.InputButton.Left)
            {
                m_CurrentlySelectedTile = null;
                m_isChoosing = false;
                m_MenuParent.SetActive(false);

                if (m_CurrentlySelectedCog)
                {
                    NetworkPlayer.LocalPlayer.BuildCog(i_SelectedTile, m_CurrentlySelectedCog);
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData i_pointerData, HexTile i_SelectedTile)
    {
        if (Input.touchCount > 1)
        {
            cancelBuild();
        }
        else
        {
            StartCoroutine(delayedShowMenu(i_pointerData, i_SelectedTile));
        }
    }

    private IEnumerator delayedShowMenu(PointerEventData i_pointerData, HexTile i_SelectedTile)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        if (i_pointerData.button == PointerEventData.InputButton.Left && Input.touchCount <= 1)
        {
            m_CurrentlySelectedTile = i_SelectedTile;
            m_isChoosing = true;
            m_MenuParent.SetActive(true);
            turnOffAllItemsHightlight();
            m_mouseStartPos = i_pointerData.position;
            m_MenuParent.transform.localPosition = m_mouseStartPos - new Vector2(Screen.width / 2, Screen.height / 2);

            checkAllCogBuildingLimitations();
        }
    }

    public void OnDrag(PointerEventData i_pointerData, HexTile i_SelectedTile)
    {
        if (Input.touchCount > 1)
        {
            cancelBuild();
        }
        else
        {
            if (i_pointerData.button == PointerEventData.InputButton.Left)
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
                                    m_HoveredItemIndex = i;
                                    SetItemHighlight(i);
                                }
                                else
                                {
                                    m_HoveredItemIndex = null;
                                    turnOffAllItemsHightlight();
                                }
                            }
                        }
                        else if ((angleDegrees > m_segmentStartEndAngles[i].x && angleDegrees <= 360) || (angleDegrees < m_segmentStartEndAngles[i].y && angleDegrees >= 0))
                        {
                            if (distance > m_MinLineDrawDistance)
                            {
                                m_HoveredItemIndex = i;
                                SetItemHighlight(i);
                            }
                            else
                            {
                                m_HoveredItemIndex = null;
                                turnOffAllItemsHightlight();
                            }
                        }
                    }
                }
            }
        }
    }
    #endregion PublicMethods
}
