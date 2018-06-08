using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RadialMenuController : Singleton<RadialMenuController>
{
    #region Variables
    /// <summary>
    /// If floating over a menu item for a greater amount of seconds than this, a tooltip would show up to give details of the selected cog.
    /// </summary>
    private const float k_TooltipShowDelay = 1;

    /// <summary>
    /// A short delay between the time a tile is selected and the time when the radial menu opens over it.
    /// This is to avoid conflict with camera related gestures.
    /// </summary>
    private const float k_RadialMenuOpenDelay = 0.2f;

    /// <summary>
    /// The items to choose from in the radial menu
    /// </summary>
    [Tooltip("The items to choose from in the radial menu")]
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
    [Tooltip("The GameObject that will hold all the segment clones")]
    [SerializeField]
    private GameObject m_MenuParent;

    /// <summary>
    /// This is the background
    /// </summary>
    [Tooltip("A prefab of the segment of the circle that will be drawn for each object")]
    [SerializeField]
    private GameObject m_SegmentPrefab;
    
    [Tooltip("The minimum distance you need to move the finger from center to make an item highlighted")]
    [SerializeField]
    private float m_MinItemSelectionDistance = 110;

    [Tooltip("The line renderer object used to point from the menu to the tile it's about to build on")]
    [SerializeField]
    private LineRenderer m_LineRenderer;

    private bool m_isChoosingCogType = false;

    private Vector2 m_PointerStartPos;

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

    /// <summary>
    /// Should we cancel the tooltip timer that's currently ongoing?
    /// </summary>
    private bool m_CancelTooltip;
    private bool m_MenuStartCancelled;

    /// <summary>
    /// Is the radial menu currently being used.
    /// </summary>
    public bool IsActive { get { return m_isChoosingCogType; } }
    #endregion Variables

    #region UnityMethods
    private void Start ()
    {
        setupItems();
    }

    private void OnEnable()
    {
        NetworkPlayer.LocalPlayer.ResourcesChanged += LocalPlayer_OnResourcesChanged;
        NetworkPlayer.LocalPlayer.CogBuilt += LocalPlayer_OnCogBuilt;
    }

    private void OnDisable()
    {
        m_HoveredItemIndex = null;
        m_CurrentlySelectedCog = null;
        NetworkPlayer.LocalPlayer.ResourcesChanged -= LocalPlayer_OnResourcesChanged;
        NetworkPlayer.LocalPlayer.CogBuilt -= LocalPlayer_OnCogBuilt;
    }
    #endregion UnityMethods

    #region PrivateMethods
    /// <summary>
    /// Counts the cooldown for the given item.
    /// </summary>
    private IEnumerator cooldownCountFor(int? m_HoveredItemIndex)
    {
        if (m_HoveredItemIndex.HasValue)
        {
            float time = Time.time;
            float timeAtLastFrame = time;
            float fullFillAmount = m_menuSegments[m_HoveredItemIndex.Value].DisabledBackgroundImage.fillAmount = m_menuSegments[m_HoveredItemIndex.Value].BackgroundImage.fillAmount;
            m_menuSegments[m_HoveredItemIndex.Value].CurrentState = m_menuSegments[m_HoveredItemIndex.Value].CurrentState.AddState(RadialMenuSegment.eSegmentState.Coooldown);
            m_menuSegments[m_HoveredItemIndex.Value].CurrentState = m_menuSegments[m_HoveredItemIndex.Value].CurrentState.RemoveState(RadialMenuSegment.eSegmentState.Highlighted);

            for (float t = 0; t < m_Items[m_HoveredItemIndex.Value].Cooldown; t += Time.time - timeAtLastFrame)
            {
                timeAtLastFrame = Time.time;
                m_menuSegments[m_HoveredItemIndex.Value].DisabledBackgroundImage.fillAmount = fullFillAmount * (1 - (t / m_Items[m_HoveredItemIndex.Value].Cooldown));
                yield return null;
            }

            m_menuSegments[m_HoveredItemIndex.Value].CurrentState = m_menuSegments[m_HoveredItemIndex.Value].CurrentState.RemoveState(RadialMenuSegment.eSegmentState.Coooldown);
        }
    }

    /// <summary>
    /// Sets up the items represented by the radial menu for selection and building.
    /// </summary>
    private void setupItems()
    {
        turnOffAllItemsHightlight();
        m_isChoosingCogType = false;
        m_MenuParent.SetActive(false);
        m_numOfItems = m_Items.Length;
        m_segmentStartEndAngles = new Vector2[m_numOfItems];
        m_menuSegments = new RadialMenuSegment[m_numOfItems];

        m_cutAngles = new float[m_numOfItems];
        float oneSegment = 360 / m_numOfItems;

        for (int i = 0; i < m_numOfItems; i++)
        {
            GameObject newSegment = Instantiate(m_SegmentPrefab, m_MenuParent.transform);
            newSegment.transform.SetAsFirstSibling();
            newSegment.transform.localPosition = new Vector2(0, 0);
            RadialMenuSegment newSegImg = newSegment.GetComponent<RadialMenuSegment>();
            Image icon = newSegImg.ItemIcon;
            TMP_Text cost = newSegImg.CostText;

            m_cutAngles[i] = oneSegment * i;
            newSegImg.DisabledBackgroundImage.fillMethod = newSegImg.BackgroundImage.fillMethod = Image.FillMethod.Radial360;
            newSegImg.BackgroundImage.fillAmount = 1 / (360 / (oneSegment - (m_SpaceBetweenSegments / 2)));
            newSegImg.DisabledBackgroundImage.fillAmount = 0;
            icon.sprite = m_Items[i].Sprite;
            cost.text = m_Items[i].Cost.ToString();

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

            icon.transform.eulerAngles = Vector3.zero;
            cost.transform.eulerAngles = Vector3.zero;
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
        m_MenuParent.transform.position = m_PointerStartPos;
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
        m_isChoosingCogType = false;
        m_CurrentlySelectedCog = null;
        m_MenuParent.SetActive(false);
    }

    /// <summary>
    /// If the item is hovered over for a short while, then we'll display a tooltip detailing its functionality.
    /// </summary>
    private IEnumerator showTooltipDelayed()
    {
        int? selectedCogIndex = m_HoveredItemIndex;
        m_CancelTooltip = false;

        yield return new WaitForSeconds(k_TooltipShowDelay);

        if (!m_CancelTooltip)
        {
            if (selectedCogIndex == m_HoveredItemIndex && selectedCogIndex.HasValue)
            {
                Tooltip.Instance.Show();
                Tooltip.Instance.SetText(m_Items[selectedCogIndex.Value].Description);
            }
        }
    }

    /// <summary>
    /// Removes highlight from all items.
    /// </summary>
    private void unlightAll()
    {
        Tooltip.Instance.Hide();
        m_CancelTooltip = true;
        m_HoveredItemIndex = null;
        turnOffAllItemsHightlight();
    }

    /// <summary>
    /// Highlights the item at the given index.
    /// </summary>
    private void highlightItem(int i)
    {
        int? lastSelectedIndex = m_HoveredItemIndex;

        m_HoveredItemIndex = i;
        SetItemHighlight(i);

        if (m_HoveredItemIndex != lastSelectedIndex)
        {
            Tooltip.Instance.Hide();
            m_CancelTooltip = true;
            StartCoroutine(showTooltipDelayed());
        }
    }
    #endregion PrivateMethods

    #region PublicMethods
    public void SetItemHighlight(int? index)
    {
        if (IsActive)
        {
            if (index.HasValue)
            {
                for (int i = 0; i < m_menuSegments.Length; i++)
                {
                    if (i == index)
                    {
                        if (m_menuSegments[i].CurrentState.CheckState(RadialMenuSegment.eSegmentState.Available) ||
                            m_menuSegments[i].CurrentState.CheckState(RadialMenuSegment.eSegmentState.Coooldown))
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
            else
            {
                Tooltip.Instance.Hide();
                m_CancelTooltip = true;
            }
        }
    }

    public void OnPointerUp(PointerEventData i_pointerData, HexTile i_SelectedTile)
    {
        Tooltip.Instance.Hide();
        m_CancelTooltip = true;
        m_MenuStartCancelled = true;

        if (Input.touchCount > 1)
        {
            cancelBuild();
        }
        else
        {
            if (i_pointerData.button == PointerEventData.InputButton.Left)
            {
                //TODO: Change to actual code once Amir adds an outline to the tile's shader.
                m_CurrentlySelectedTile?.transform.GetComponentsInChildren<Transform>(true).First(trans => trans.gameObject.name.Equals("Outline")).gameObject.SetActive(false);

                m_CurrentlySelectedTile = null;
                m_isChoosingCogType = false;
                m_MenuParent.SetActive(false);

                if (m_CurrentlySelectedCog &&
                    !m_menuSegments[m_HoveredItemIndex.Value].CurrentState.CheckState(RadialMenuSegment.eSegmentState.Coooldown))
                {
                    NetworkPlayer.LocalPlayer.BuildCog(i_SelectedTile, m_CurrentlySelectedCog);
                    
                    StartCoroutine(cooldownCountFor(m_HoveredItemIndex));
                }
            }
        }

        m_CurrentlySelectedCog = null;
    }

    public void OnPointerDown(PointerEventData i_pointerData, HexTile i_SelectedTile)
    {
        if (Input.touchCount > 1)
        {
            cancelBuild();
            Tooltip.Instance.Hide();
            m_CancelTooltip = true;
        }
        else
        {
            StartCoroutine(delayedShowMenu(i_pointerData, i_SelectedTile));
        }
    }

    private IEnumerator delayedShowMenu(PointerEventData i_pointerData, HexTile i_SelectedTile)
    {
        m_MenuStartCancelled = false;
        m_PointerStartPos = i_pointerData.position;
        yield return new WaitForSeconds(k_RadialMenuOpenDelay);

        //Check if we're still trying to show the menu, meaning still only 1 touch is made and it was fairly static during the delay before showing the menu.
        if (i_pointerData.button == PointerEventData.InputButton.Left && Input.touchCount <= 1 && !m_MenuStartCancelled)
        {
            m_CurrentlySelectedTile = i_SelectedTile;
            m_isChoosingCogType = true;
            m_MenuParent.SetActive(true);
            turnOffAllItemsHightlight();
            m_MenuParent.transform.localPosition = Camera.main.WorldToScreenPoint(m_CurrentlySelectedTile.transform.position) - new Vector3(Screen.width / 2, Screen.height / 2);
            Vector3 tilePosition = m_MenuParent.transform.position;

            //Fixes the menu position to fit inside the screen.
            Rect normalizedRectRange = new Rect(0, 0, 1, 1);
            Vector3[] corners = new Vector3[4];
            (m_MenuParent.transform as RectTransform).GetWorldCorners(corners);
            corners = corners.Select(corner => Camera.main.ScreenToViewportPoint(corner)).ToArray();

            while (corners.Any(corner => !normalizedRectRange.Contains(corner)))
            {
                if (corners.Any(corner => corner.x < 0)) { m_MenuParent.transform.position += Vector3.right * 10; }
                if (corners.Any(corner => corner.x > 1)) { m_MenuParent.transform.position += Vector3.left * 10; }
                if (corners.Any(corner => corner.y < 0)) { m_MenuParent.transform.position += Vector3.up * 10; }
                if (corners.Any(corner => corner.y > 1)) { m_MenuParent.transform.position += Vector3.down * 10; }

                (m_MenuParent.transform as RectTransform).GetWorldCorners(corners);
                corners = corners.Select(corner => Camera.main.ScreenToViewportPoint(corner)).ToArray();
            }
            
            //Draws a line that shows us which tile is affected by the menu.
            m_LineRenderer.SetPositions(new Vector3[] {
                Camera.main.ScreenToWorldPoint(tilePosition),
                Camera.main.ScreenToWorldPoint(m_MenuParent.transform.position)
            });

            m_PointerStartPos = new Vector3(m_MenuParent.transform.position.x, m_MenuParent.transform.position.y);
            Debug.Log(m_PointerStartPos + " " + i_pointerData.position);

            //TODO: Change to actual code once Amir adds an outline to the tile's shader.
            m_CurrentlySelectedTile.transform.GetComponentsInChildren<Transform>(true).First(trans => trans.gameObject.name.Equals("Outline")).gameObject.SetActive(true);

            checkAllCogBuildingLimitations();
        }
    }

    public void OnDrag(PointerEventData i_pointerData, HexTile i_SelectedTile)
    {
        if (Input.touchCount > 1)
        {
            cancelBuild();
            Tooltip.Instance.Hide();
            m_CancelTooltip = true;
        }
        else
        {
            if (i_pointerData.button == PointerEventData.InputButton.Left)
            {
                if (m_isChoosingCogType)
                {
                    Vector2 v = i_pointerData.position - m_PointerStartPos;
                    float angleRadians = Mathf.Atan2(v.y, v.x);
                    float angleDegrees = angleRadians * Mathf.Rad2Deg;

                    if (angleDegrees < 0) angleDegrees += 360;

                    float distance = Vector2.Distance(m_PointerStartPos, i_pointerData.position);

                    for (int i = 0; i < m_numOfItems; i++)
                    {
                        if (m_segmentStartEndAngles[i].x < m_segmentStartEndAngles[i].y)
                        {
                            if (angleDegrees > m_segmentStartEndAngles[i].x && angleDegrees < m_segmentStartEndAngles[i].y)
                            {
                                if (distance > m_MinItemSelectionDistance)
                                {
                                    highlightItem(i);
                                }
                                else
                                {
                                    unlightAll();
                                }
                            }
                        }
                        else if ((angleDegrees > m_segmentStartEndAngles[i].x && angleDegrees <= 360) || (angleDegrees < m_segmentStartEndAngles[i].y && angleDegrees >= 0))
                        {
                            if (distance > m_MinItemSelectionDistance)
                            {
                                highlightItem(i);
                            }
                            else
                            {
                                unlightAll();
                            }
                        }
                    }
                }
                else
                {
                    if (Vector3.Distance(m_PointerStartPos, i_pointerData.position) >= m_MinItemSelectionDistance)
                    {
                        m_MenuStartCancelled = true;
                    }
                }
            }
        }
    }
    #endregion PublicMethods
}
