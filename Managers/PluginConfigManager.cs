using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using FTKAPI.Managers.PluginConfig;
using FTKAPI.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace FTKAPI.Managers;

public class PluginConfigManager : BaseManager<PluginConfigManager>
{
    internal List<IConfigurableEntry> Bindings = new();
    internal GameObject CheckboxPrefab;
    
    [HarmonyPatch(typeof(uiOptionsMenu), nameof(uiOptionsMenu.Show))]
    class uiOptionsMenu_Show_Patch {
        static void Postfix(uiOptionsMenu __instance) {
            if (IsInited) Instance.CreatePluginOptions(__instance);
        }
    }

    internal override void Init()
    {
        Plugin.Instance.Harmony.PatchNested<PluginConfigManager>();
    }

    public static void RegisterBinding(ConfigEntryBase binding)
    {
        Instance.RegisterSingleBinding(new BepinexConfigurableEntry(binding));
    }
    
    public void RegisterSingleBinding(IConfigurableEntry entry)
    {
        Instance.Bindings.Add(entry);
    }
    
    private void CreatePluginOptions(uiOptionsMenu menu)
    {
        if (!menu.gameObject.transform.Find("MainWindow/plugin_options") && this.Bindings.Any())
        {
            Plugin.Log.LogInfo($"Creating plugin options button");
            var button = CreateMenuButton("Plugin Options", OpenPluginOptions);
            button.name = "plugin_options";
            button.transform.SetParent(menu.gameObject.transform.Find("MainWindow"));
            button.transform.SetSiblingIndex(7);
            button.transform.Scale1();
        }
    }
    
    internal static GameObject CreateMenuButton(string label, UnityAction callback, GameObject prefab = null)
    {
        prefab ??= uiOptionsMenu.Instance.m_ControlButton.gameObject;
        
        var btnObject = Object.Instantiate(prefab);
        btnObject.name = "PluginOptionsButton";
        var transform = (RectTransform)btnObject.transform;
        transform.Scale1();

        var textComponent = transform.Find("Text").GetComponent<Text>();
        textComponent.text = label;

        var buttonComponent = btnObject.GetComponent<Button>();
        buttonComponent.onClick = new Button.ButtonClickedEvent();
        buttonComponent.onClick.AddListener(callback);

        return btnObject;
    }
    

    private void OpenPluginOptions()
    {
        var existing = FTKHub.Instance.m_uiRoot.Find("SystemUI/OptionTarget/OptionMenu/PluginOptionsPanel");
        if (existing)
        {
            Plugin.Log.LogInfo($"Option exists, updating..");
            UpdatePanelContent((RectTransform)existing.Find("VisibleArea/Options"));
            existing.gameObject.SetActive(true);
            return;
        }
        
        Plugin.Log.LogInfo($"Opening options..");
        var panelTransform = (RectTransform)Object.Instantiate(FTKHub.Instance.m_uiRoot.Find("SystemUI/OptionTarget/OptionMenu/GamePanel").transform);
        panelTransform.name = "PluginOptionsPanel";
        var panelGo = panelTransform.gameObject;

        panelTransform.Find("DisplayHeader/HeaderText").GetComponent<Text>().text = "Plugin Options";
        
        var optionsTransform = (RectTransform)panelTransform.Find("Options");
        CheckboxPrefab = Object.Instantiate(optionsTransform.Find("LockCursor")).gameObject;

        CreateScrollableArea(panelTransform, optionsTransform);
        UpdatePanelContent(optionsTransform);
        
        panelTransform.SetParent(FTKHub.Instance.m_uiRoot.Find("SystemUI/OptionTarget/OptionMenu").transform);
        panelGo.SetActive(true);
        
        panelTransform.ScaleResolutionBased();
        panelTransform.anchoredPosition = new Vector2(0, -200);
        panelTransform.anchorMin = new Vector2(0.5f, 1);
        panelTransform.anchorMax = new Vector2(0.5f, 1);
    }
    
    private void UpdatePanelContent(RectTransform optionsTransform)
    {
        // remove existing elements
        foreach (Transform child in optionsTransform)
        {
            Object.Destroy(child.gameObject);
        }
        
        foreach (var sectionGrouping in this.Bindings
                     .GroupBy(b => !string.IsNullOrEmpty(b.OwnerName) ? $"{b.OwnerName} - {b.Section}" : b.Section))
        {
            var sectionName = sectionGrouping.Key;
            var boolEntries = sectionGrouping.ToArray();
            if (boolEntries.Any())
            {
                AddSectionGrouping(sectionName, optionsTransform);
            }
            foreach (var entry in sectionGrouping)
            {
                AddCheckbox(entry, optionsTransform);
            }
        }
    }
    

    private GameObject AddSectionGrouping(string name, RectTransform parent)
    {
        var go = Object.Instantiate(CheckboxPrefab);
        Object.Destroy(go.GetComponent<Toggle>());
        var transform = (RectTransform)go.transform;
        Object.Destroy(transform.GetChild(0).gameObject);
        var text = transform.Find("Label").GetComponent<Text>();
        text.text = $"  [{name}]";
        text.fontStyle = FontStyle.Bold;
        transform.SetParent(parent);
        transform.ScaleResolutionBased();
        
        
        return go;
    }
    
    private void AddCheckbox(IConfigurableEntry entry, RectTransform parent)
    {
        if (entry.SettingType == typeof(bool))
        {
            var checkbox = CreateCheckbox(
                entry.Key,
                (bool)entry.Value,
                v => entry.Value = v,
                entry.Description);
            checkbox.transform.SetParent(parent);
            checkbox.transform.ScaleResolutionBased();
        }
    }
    
    private GameObject CreateScrollableArea(RectTransform panelTransform, RectTransform optionsTransform)
    {
        // create scrollable options
        var visibleArea = new GameObject { name = "VisibleArea" };
        
        // va/transform
        var vaTransform = visibleArea.AddComponent<RectTransform>();
        vaTransform.SetParent(panelTransform);
        vaTransform.anchorMin = Vector2.zero;
        vaTransform.anchorMax = Vector2.one;
        vaTransform.anchoredPosition = new Vector2(15, 15f);
        vaTransform.sizeDelta = new Vector2(-45, -115);
        
        // va/image
        visibleArea.AddComponent<Image>().color = new Color32(0x20, 0x1f, 0x1f, 0xFF);

        //  va/mask
        visibleArea.AddComponent<Mask>().showMaskGraphic = false;
        
        // scroll rect
        var scrollRect = visibleArea.AddComponent<ScrollRect>();
        scrollRect.scrollSensitivity = 15;
        scrollRect.content = optionsTransform;
        scrollRect.horizontal = false;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        
        // set content
        optionsTransform.SetParent(vaTransform);
        var optionsFitter = optionsTransform.gameObject.AddComponent<ContentSizeFitter>();
        optionsFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // scrollbar
        var scrollbarGo = new GameObject
        {
            name = "Scrollbar"
        };
        var scrollbarTransform = scrollbarGo.AddComponent<RectTransform>();
        scrollbarTransform.SetParent(vaTransform);
        scrollbarTransform.anchorMin = new Vector2(1, 0);
        scrollbarTransform.anchorMax = Vector2.one;
        scrollbarTransform.sizeDelta = new Vector2(10, 0);
        scrollbarTransform.anchoredPosition = new Vector2(-30, 0);
        var scrollbar = scrollbarGo.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollRect.verticalScrollbar = scrollbar;
        
        // scrollbar handle
        var scrollbarHandleGo = new GameObject
        {
            name = "Handle"
        };
        var scrollbarHandleTransform = scrollbarHandleGo.AddComponent<RectTransform>();
        scrollbarHandleTransform.SetParent(scrollbarTransform);
        scrollbarHandleTransform.sizeDelta = new Vector2(0, 0);
        scrollbarHandleTransform.anchoredPosition = new Vector2(0, 0);
        scrollbarHandleTransform.anchorMin = new Vector2(0, 0);
        scrollbarHandleTransform.anchorMax = new Vector2(1, 1);
        var scrollbarHandleImg = scrollbarHandleGo.AddComponent<Image>();
        
        // assign handle -> scrollbar
        scrollbar.handleRect = scrollbarHandleTransform;
        scrollbar.targetGraphic = scrollbarHandleImg;
        
        // adjust options position
        optionsTransform.anchoredPosition = new Vector2(0, optionsTransform.anchoredPosition.y);

        return visibleArea;
    }
    
    private GameObject CreateCheckbox(string label, bool isOn, UnityAction<bool> callback, string tooltipText = null)
    {
        var checkboxGo = Object.Instantiate(this.CheckboxPrefab);
        var toggle = checkboxGo.GetComponent<Toggle>();
        toggle.isOn = isOn;
        toggle.onValueChanged = new Toggle.ToggleEvent();
        toggle.onValueChanged.AddListener(callback);
        
        checkboxGo.transform.Find("Label").GetComponent<Text>().text = label;

        // tooltips are located in layer below menu, so they will not display correctly
        // if (tooltipText != null)
        // {
        //     var tooltip = checkboxGo.AddComponent<uiToolTipGeneral>();
        //     tooltip.m_Info = label;
        //     tooltip.m_ReturnRawInfo = true;
        //     tooltip.m_DetailInfo = tooltipText;
        //     tooltip.m_IsFollowHoriz = true;
        //     tooltip.m_ToolTipOffset = new Vector2(0, -50);
        // }

        return checkboxGo;
    }
}