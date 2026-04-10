using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIAudioInyectora : MonoBehaviour
{
    void Start()
    {
        Button[] todosLosBotones = GetComponentsInChildren<Button>(true);

        foreach (Button btn in todosLosBotones)
        {
            // Clic de Ratón
            btn.onClick.AddListener(() => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
            });

            EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();

            // Hover de Ratón
            EventTrigger.Entry entryHoverRaton = new EventTrigger.Entry();
            entryHoverRaton.eventID = EventTriggerType.PointerEnter;
            entryHoverRaton.callback.AddListener((data) => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayUIHover();
            });
            trigger.triggers.Add(entryHoverRaton);

            // Hover de Mando (Select)
            EventTrigger.Entry entrySeleccionControl = new EventTrigger.Entry();
            entrySeleccionControl.eventID = EventTriggerType.Select;
            entrySeleccionControl.callback.AddListener((data) => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayUIHover();
            });
            trigger.triggers.Add(entrySeleccionControl);

            // Clic de Mando (Submit)
            EventTrigger.Entry entrySubmitControl = new EventTrigger.Entry();
            entrySubmitControl.eventID = EventTriggerType.Submit;
            entrySubmitControl.callback.AddListener((data) => {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick();
            });
            trigger.triggers.Add(entrySubmitControl);
        }
    }
}