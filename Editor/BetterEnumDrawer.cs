using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BetterEnumAttribute))]
public class BetterEnumDrawer : PropertyDrawer
{
    private List<string> enumList;

    private int currentElementSelected = -2;

    private SerializedProperty localProperty = null;

    /******************** Override ********************/

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        const float BUTTON_WIDTH = 30f;
        const float SPACE_BETWEEN_ELEMENTS = 10f;

        // Open window button
        Rect buttonPosition = new Rect(position.x, position.y, BUTTON_WIDTH, position.height);
        if (GUI.Button(buttonPosition, new GUIContent("\u25CE", "Open enum selector window"), EditorStyles.miniButton))
        {
            // Update the global references
            localProperty = property;
            enumList = new List<string>(property.enumDisplayNames);

            // Init the window
            BetterEnumWindow window = (BetterEnumWindow)EditorWindow.GetWindow(typeof(BetterEnumWindow), utility: true, title: $"{property.displayName}", focus: true);
            window.UpdateEnumValue(enumList);
            window.UpdateCurrentSelection(property.enumValueIndex);

            // Subscribe to his events
            window.OnItemChosen += BetterEnumWindow_OnItemChosen;
            window.OnWindowCloses += BetterEnumWindow_OnWindowCloses;

            // Show the window
            window.Focus();
            window.Show();
        }

        // Make sure to always update the enum
        EditorGUI.BeginChangeCheck();
        {
            Rect fieldPosition = new Rect(position.x + BUTTON_WIDTH + SPACE_BETWEEN_ELEMENTS, position.y, position.width, position.height);
            EditorGUI.PropertyField(fieldPosition, property, label);

            if (EditorGUI.EndChangeCheck())
            {
                currentElementSelected = property.enumValueIndex;
            }
            else
            {
                if (currentElementSelected < 0)
                {
                    currentElementSelected = property.enumValueIndex;
                }
                // ? Maybe you don't need this
                else if (property.enumValueIndex != currentElementSelected)
                {
                    property.enumValueIndex = currentElementSelected;
                }
            }
        }
    }

    /******************** Events ********************/

    private void BetterEnumWindow_OnWindowCloses(BetterEnumWindow windowClosed)
    {
        windowClosed.OnWindowCloses -= BetterEnumWindow_OnWindowCloses;
        windowClosed.OnItemChosen -= BetterEnumWindow_OnItemChosen;
    }

    /// <summary>
    /// Used to update the enum in the inspector
    /// </summary>
    private void BetterEnumWindow_OnItemChosen(string enumItem)
    {
        currentElementSelected = enumList.FindIndex(x => x == enumItem);

        if (localProperty == null || localProperty.Equals(null) || currentElementSelected < 0)
            return;

        localProperty.enumValueIndex = currentElementSelected;
        localProperty.serializedObject.ApplyModifiedProperties();
        localProperty.serializedObject.Update();
    }
}
