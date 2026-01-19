using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

namespace AIAction.UI
{
    public class InputDebugger : MonoBehaviour, IPointerClickHandler, ISubmitHandler, ISelectHandler, IDeselectHandler
    {
        private TMP_InputField input;

        private void Awake()
        {
            input = GetComponent<TMP_InputField>();
            if (input != null)
            {
                input.onValueChanged.AddListener((val) => Debug.Log($"[InputDebugger] Value Changed: {val}"));
                input.onSubmit.AddListener((val) => Debug.Log($"[InputDebugger] Submit: {val}"));
                input.onEndEdit.AddListener((val) => Debug.Log($"[InputDebugger] EndEdit: {val}"));
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log($"[InputDebugger] Clicked on {name}");
        }

        public void OnSubmit(BaseEventData eventData)
        {
            Debug.Log($"[InputDebugger] Submit Event on {name}");
        }

        public void OnSelect(BaseEventData eventData)
        {
            Debug.Log($"[InputDebugger] Selected {name}");
        }

        public void OnDeselect(BaseEventData eventData)
        {
            Debug.Log($"[InputDebugger] Deselected {name}");
        }
    }
}