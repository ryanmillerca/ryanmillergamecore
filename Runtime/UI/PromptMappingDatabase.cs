namespace RyanMillerGameCore.UI
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    [CreateAssetMenu(menuName = "RyanMillerGameCore/Input/Prompt Mapping Database")]
    public class PromptMappingDatabase : ScriptableObject
    {
        [SerializeField] private InputSearch[] inputMappings;
        [SerializeField] private PromptGraphics defaultGraphics;

        public PromptGraphics GetGraphicsForDevice(InputDevice device)
        {
            if (device == null)
            {
                // Keyboard/Mouse fallback
                if (Keyboard.current != null || Mouse.current != null)
                {
                    return GetGraphicsForStrings("Keyboard", "Keyboard", "Keyboard");
                }

                return defaultGraphics;
            }

            var description = device.description;
            return GetGraphicsForStrings(
                description.deviceClass,
                description.product,
                device.name
            );
        }

        private PromptGraphics GetGraphicsForStrings(string deviceClass, string productName, string deviceName)
        {
            if (inputMappings == null) return defaultGraphics;

            foreach (var mapping in inputMappings)
            {
                if (ContainsAny(deviceClass, mapping.deviceClassKeywords) ||
                    ContainsAny(productName, mapping.productNameKeywords) ||
                    ContainsAny(deviceName, mapping.deviceNameKeywords))
                {
                    return mapping.graphics;
                }
            }

            return defaultGraphics;
        }

        private bool ContainsAny(string input, string[] keywords)
        {
            if (string.IsNullOrEmpty(input) || keywords == null || keywords.Length == 0)
                return false;

            string lowerInput = input.ToLower();
            foreach (string keyword in keywords)
            {
                if (!string.IsNullOrEmpty(keyword) &&
                    lowerInput.Contains(keyword.ToLower()))
                {
                    return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            // Default mappings for new assets
            inputMappings = new InputSearch[]
            {
                new InputSearch // PlayStation
                {
                    deviceClassKeywords = new[] { "sony" },
                    productNameKeywords = new[] { "dualsense", "dualshock", "wireless controller" },
                    deviceNameKeywords = new[] { "playstation" }
                },
                new InputSearch // Nintendo Switch
                {
                    productNameKeywords = new[] { "switch", "joy-con", "nintendo" }
                },
                new InputSearch // Xbox
                {
                    deviceClassKeywords = new[] { "xinput" },
                    productNameKeywords = new[] { "xbox", "series x" }
                }
            };
        }
#endif
    }
}