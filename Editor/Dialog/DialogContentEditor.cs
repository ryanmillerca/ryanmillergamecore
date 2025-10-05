namespace RyanMillerGameCore.Dialog
{
    using UnityEditor;
    using UnityEditor.UIElements;
    using UnityEngine;
    using UnityEngine.UIElements;

    [CustomEditor(typeof(DialogContent))]
    public class DialogContentEditor : Editor
    {
        private VisualElement root;
        private VisualElement linesUIContainer;

        private void OnEnable()
        {
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
        }

        private void OnUndoRedoPerformed()
        {
            if (root == null) return;
            serializedObject.Update();
            BuildDialogLinesUI();
            root.Bind(serializedObject);
        }

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement();
            linesUIContainer = new VisualElement();
            var so = serializedObject;

            // === Dialog Settings Foldout ===
            var settingsFoldout = new Foldout { text = "Dialog Settings", value = true };

            string[] fieldNames = new[]
            {
                "freezeInputs", "freezeTime", "autoAdvance",
                "delay", "autoAdvanceCharTime", "charRevealRate"
            };

            foreach (var fieldName in fieldNames)
            {
                var prop = so.FindProperty(fieldName);
                settingsFoldout.Add(new PropertyField(prop));
            }

            root.Add(settingsFoldout);
            root.Add(linesUIContainer);

            BuildDialogLinesUI();
            root.Bind(serializedObject);
            return root;
        }

        private void BuildDialogLinesUI()
        {
            linesUIContainer.Clear();
            serializedObject.Update();

            var linesProp = serializedObject.FindProperty("lines");

            linesUIContainer.Add(new Label("Dialog Lines")
            {
                style = { unityFontStyleAndWeight = FontStyle.Bold }
            });

            for (int i = 0; i < linesProp.arraySize; i++)
            {
                int currentIndex = i;
                var lineProp = linesProp.GetArrayElementAtIndex(i);

                var lineBox = new Box();
                lineBox.style.marginTop = 6;
                lineBox.style.paddingBottom = 6;
                lineBox.style.paddingLeft = 6;
                lineBox.style.paddingRight = 6;

                var textProp    = lineProp.FindPropertyRelative("text");
                var speakerProp = lineProp.FindPropertyRelative("speaker");
                var lookAtProp  = lineProp.FindPropertyRelative("lookAt");

                // Always-visible text field with dynamic label
                var textField = new PropertyField(textProp, string.Empty);
                lineBox.Add(textField);

                void RefreshTextLabel()
                {
                    var spkObj = speakerProp?.objectReferenceValue;
                    var latObj = lookAtProp?.objectReferenceValue;

                    string speakerName = spkObj != null ? spkObj.name : "";
                    string lookAtName  = latObj != null ? latObj.name : "";

                    if (!string.IsNullOrEmpty(speakerName) && string.IsNullOrEmpty(lookAtName))
                        textField.label = speakerName + " says:";
                    else if (!string.IsNullOrEmpty(speakerName) && !string.IsNullOrEmpty(lookAtName))
                        textField.label = speakerName + " → looking at " + lookAtName;
                    else
                        textField.label = "Dialog text:";
                }

                textField.RegisterCallback<GeometryChangedEvent>(_ => RefreshTextLabel());
                RefreshTextLabel();

                // === Line Options Foldout ===
                var lineOptionsFoldout = new Foldout { text = "Line Options", value = false };

                var voicePF     = new PropertyField(lineProp.FindPropertyRelative("voiceOver"));
                var centeredPF  = new PropertyField(lineProp.FindPropertyRelative("centered"));
                var speakerPF   = new PropertyField(speakerProp);
                var lookAtPF    = new PropertyField(lookAtProp);
                var focusPF     = new PropertyField(lineProp.FindPropertyRelative("focusCameraOnSpeaker"));
                var offsetPF    = new PropertyField(lineProp.FindPropertyRelative("cameraOffsetRotation"));
                var speakAnimPF = new PropertyField(lineProp.FindPropertyRelative("speakerAnimation"));
                var lookAnimPF  = new PropertyField(lineProp.FindPropertyRelative("lookAtAnimation"));

                // Remove if not present in your DialogLine anymore
                var speakCmdProp = lineProp.FindPropertyRelative("speakerCommands");
                var targCmdProp  = lineProp.FindPropertyRelative("targetCommands");
                if (speakCmdProp != null) lineOptionsFoldout.Add(new PropertyField(speakCmdProp));
                if (targCmdProp  != null) lineOptionsFoldout.Add(new PropertyField(targCmdProp));

                speakerPF.RegisterValueChangeCallback((SerializedPropertyChangeEvent _) => RefreshTextLabel());
                lookAtPF.RegisterValueChangeCallback((SerializedPropertyChangeEvent _) => RefreshTextLabel());

                lineOptionsFoldout.Add(voicePF);
                lineOptionsFoldout.Add(centeredPF);
                lineOptionsFoldout.Add(speakerPF);
                lineOptionsFoldout.Add(lookAtPF);
                lineOptionsFoldout.Add(focusPF);
                lineOptionsFoldout.Add(offsetPF);
                lineOptionsFoldout.Add(speakAnimPF);
                lineOptionsFoldout.Add(lookAnimPF);

                lineBox.Add(lineOptionsFoldout);

                // === Navigation (IDs only) ===
                var navProp = lineProp.FindPropertyRelative("navigation");
                if (navProp != null)
                {
                    var navFoldout = new Foldout { text = "Navigation", value = false };
                    var navEnabledProp = navProp.FindPropertyRelative("enabled");

                    var navEnabledPF = new PropertyField(navEnabledProp, "Enabled");
                    var navTargetIDPF = new PropertyField(navProp.FindPropertyRelative("targetID"), "Target (ID)");
                    var navOffsetPF   = new PropertyField(navProp.FindPropertyRelative("offset"), "Offset");
                    var navStopPF     = new PropertyField(navProp.FindPropertyRelative("stopDistance"), "Stop Distance");
                    var navFaceIDPF   = new PropertyField(navProp.FindPropertyRelative("faceID"), "Face (ID)");
                    var navTimeoutPF  = new PropertyField(navProp.FindPropertyRelative("timeoutSeconds"), "Timeout (s)");

                    //var navBody = new VisualElement();
                    lineOptionsFoldout.style.marginLeft = 12;
                    lineOptionsFoldout.Add(navTargetIDPF);
                    lineOptionsFoldout.Add(navOffsetPF);
                    lineOptionsFoldout.Add(navStopPF);
                    lineOptionsFoldout.Add(navFaceIDPF);
                    lineOptionsFoldout.Add(navTimeoutPF);

                    void RefreshNavEnabled()
                    {
                     //   navBody.SetEnabled(navEnabledProp.boolValue);
                    }

                    navEnabledPF.RegisterValueChangeCallback((SerializedPropertyChangeEvent _) => RefreshNavEnabled());

                    lineOptionsFoldout.Add(navEnabledPF);
                   // navFoldout.Add(navBody);
                    lineBox.Add(lineOptionsFoldout);

                    lineOptionsFoldout.RegisterCallback<GeometryChangedEvent>(_ => RefreshNavEnabled());
                    RefreshNavEnabled();
                }

                // === Remove Button ===
                var removeButton = new Button(() =>
                {
                    var updatedLinesProp = serializedObject.FindProperty("lines");
                    updatedLinesProp.DeleteArrayElementAtIndex(currentIndex);
                    serializedObject.ApplyModifiedProperties();
                    BuildDialogLinesUI();
                    root.Bind(serializedObject);
                })
                { text = "Remove" };
                removeButton.style.marginTop = 6;
                lineBox.Add(removeButton);

                // === Controls Row: Move Up / Move Down / Add Line / Add Opposite ===
                var controlsRow = new VisualElement();
                controlsRow.style.flexDirection = FlexDirection.Row;
                controlsRow.style.marginTop = 4;
                controlsRow.style.justifyContent = Justify.FlexStart;

                void StyleButton(Button b, bool last = false)
                {
                    b.style.marginRight = last ? 0 : 4;
                    b.style.flexGrow = 1;
                    b.style.flexBasis = 0; // even distribution
                }

                // Move Up
                var moveUpButton = new Button(() =>
                {
                    var updated = serializedObject.FindProperty("lines");
                    if (currentIndex > 0)
                    {
                        updated.MoveArrayElement(currentIndex, currentIndex - 1);
                        serializedObject.ApplyModifiedProperties();
                        BuildDialogLinesUI();
                        root.Bind(serializedObject);
                    }
                })
                { text = "⬆" };
                if (currentIndex == 0) moveUpButton.SetEnabled(false);
                StyleButton(moveUpButton);

                // Move Down
                var moveDownButton = new Button(() =>
                {
                    var updated = serializedObject.FindProperty("lines");
                    if (currentIndex < updated.arraySize - 1)
                    {
                        updated.MoveArrayElement(currentIndex, currentIndex + 1);
                        serializedObject.ApplyModifiedProperties();
                        BuildDialogLinesUI();
                        root.Bind(serializedObject);
                    }
                })
                { text = "⬇" };
                if (currentIndex >= linesProp.arraySize - 1) moveDownButton.SetEnabled(false);
                StyleButton(moveDownButton);

                // Add Line (insert below)
                var addBelowButton = new Button(() =>
                {
                    var updated = serializedObject.FindProperty("lines");
                    int insertIndex = Mathf.Clamp(currentIndex + 1, 0, updated.arraySize);
                    updated.InsertArrayElementAtIndex(insertIndex);

                    var newLine = updated.GetArrayElementAtIndex(insertIndex);
                    if (newLine != null)
                    {
                        var text = newLine.FindPropertyRelative("text");
                        if (text != null) text.stringValue = string.Empty;

                        var nav = newLine.FindPropertyRelative("navigation");
                        if (nav != null)
                        {
                            var enabled = nav.FindPropertyRelative("enabled");
                            if (enabled != null) enabled.boolValue = false;
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                    BuildDialogLinesUI();
                    root.Bind(serializedObject);
                })
                { text = "+ Line" };
                StyleButton(addBelowButton);

                // Add Opposite (insert below, swap speaker/lookAt)
                var addOppositeButton = new Button(() =>
                {
                    var updated = serializedObject.FindProperty("lines");

                    // Capture current line's speaker/lookAt
                    Object curSpeaker = lineProp.FindPropertyRelative("speaker")?.objectReferenceValue;
                    Object curLookAt  = lineProp.FindPropertyRelative("lookAt")?.objectReferenceValue;

                    int insertIndex = Mathf.Clamp(currentIndex + 1, 0, updated.arraySize);
                    updated.InsertArrayElementAtIndex(insertIndex);

                    var newLine = updated.GetArrayElementAtIndex(insertIndex);
                    if (newLine != null)
                    {
                        var newText = newLine.FindPropertyRelative("text");
                        if (newText != null) newText.stringValue = string.Empty;

                        var newSpeakerProp = newLine.FindPropertyRelative("speaker");
                        var newLookAtProp  = newLine.FindPropertyRelative("lookAt");

                        if (newSpeakerProp != null) newSpeakerProp.objectReferenceValue = curLookAt;
                        if (newLookAtProp  != null) newLookAtProp.objectReferenceValue  = curSpeaker;

                        var nav = newLine.FindPropertyRelative("navigation");
                        if (nav != null)
                        {
                            var enabled = nav.FindPropertyRelative("enabled");
                            if (enabled != null) enabled.boolValue = false;
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                    BuildDialogLinesUI();
                    root.Bind(serializedObject);
                })
                { text = "+ Flip" };
                StyleButton(addOppositeButton, last: true);

                controlsRow.Add(moveUpButton);
                controlsRow.Add(moveDownButton);
                controlsRow.Add(addBelowButton);
                controlsRow.Add(addOppositeButton);

                lineBox.Add(controlsRow);

                linesUIContainer.Add(lineBox);
            }
        }
    }
}