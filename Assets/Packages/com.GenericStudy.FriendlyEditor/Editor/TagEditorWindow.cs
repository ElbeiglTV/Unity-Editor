using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class TagEditorWindow : EditorWindow
{
    #region Funcionalidad
    private List<string> availableTags = new List<string>(); // Lista de tags disponibles
    private List<string> selectedTags = new List<string>();  // Lista de tags seleccionadas
    private string selectedTag = ""; // Tag seleccionada en el popup

    private bool isAdditive = false; // Modo de adición de etiquetas (true) o restrictivo (false)

    private Vector2 scrollPos;
    #endregion
    #region Agrupación por objeto
    private bool groupByObject = true; // Modo de agrupación por objeto (true) o no (false)
    private Dictionary<string, bool> foldouts = new Dictionary<string, bool>(); // Diccionario para almacenar el estado del foldout por objeto
    #endregion

    #region Metodos de inicio y muestra de la ventana
    [MenuItem("Window/Tag Filter")]
    public static void ShowWindow()
    {
        GetWindow<TagEditorWindow>("Tag Filter");
    }

    private void OnEnable()
    {
        // Cargar todas las etiquetas disponibles cuando se abre la ventana
        FindAvailableTags();

    }
    #endregion

    private void OnGUI()
    {
        GUILayout.Space(10);
        //GUILayout.Label("Tag Filter", EditorStyles.boldLabel);
        #region Bakground
        EditorGUI.DrawRect(new Rect(0, 0, position.width, selectedTags.Count > 0 ? EditorGUIUtility.singleLineHeight * 6.1f : EditorGUIUtility.singleLineHeight * 5.1f), new Color32(71, 71, 71, 236));

        #endregion
        // Mostrar popup para seleccionar una etiqueta
        #region Popup de selección de etiqueta
        if (availableTags.Count > 0)
        {
            if (selectedTag == "") selectedTag = availableTags[0];

            int selectedTagIndex = availableTags.IndexOf(selectedTag);
            GUILayout.BeginHorizontal();
            selectedTagIndex = EditorGUILayout.Popup("Select Tag to Add:", selectedTagIndex, availableTags.ToArray());

            // Actualizar la etiqueta seleccionada
            selectedTag = availableTags[selectedTagIndex];
            if (GUILayout.Button("+", EditorStylesManager.MakeButtonStyle(EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight)))
            {
                // Agregar la etiqueta seleccionada a la lista de etiquetas seleccionadas
                if (!selectedTags.Contains(selectedTag) && selectedTag != "None")
                {
                    selectedTags.Add(selectedTag);
                }
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("No tags available");
        }
        #endregion
        // Mostrar las tags seleccionadas como botones que se pueden eliminar
        #region Mostrar las tags seleccionadas
        EditorGUILayout.LabelField("Selected Tags:");
        EditorGUILayout.BeginHorizontal();
        foreach (var tag in selectedTags.ToList())  // Copiar lista para evitar modificación durante iteración
        {
            if (GUILayout.Button(tag, GUILayout.Width(tag.Length * 10), GUILayout.ExpandWidth(false)))
            {
                // Eliminar tag al hacer clic
                selectedTags.Remove(tag);
            }
        }
        EditorGUILayout.EndHorizontal();
        #endregion
        GUILayout.Space(5);
        // Botón para alternar el modo de agrupación
        #region Botón de alternar modo de agrupación
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(groupByObject ? "¬" : "|=", EditorStylesManager.ButtonStyle)) // Cambiar el texto del botón según el modo de agrupación y muestra el boton
        {
            groupByObject = !groupByObject; // Alternar el modo de agrupación
            Repaint(); //repintar la ventana
        }
        #endregion


        #region Botón de actualización
        if (GUILayout.Button("R", EditorStylesManager.ButtonStyle))
        {
            FindAvailableTags(); // Actualizar las etiquetas disponibles
            Repaint(); // Actualizar la ventana
        }
        #endregion
        #region Botón de Aditivo o Restrictivo
        if (GUILayout.Button("A", EditorStylesManager.ButtonStyle)) isAdditive = !isAdditive;
        #endregion


        EditorGUILayout.EndHorizontal();
        #region Scrollview con los campos DebugTag filtrados
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        if (selectedTags.Count > 0)
        {
            // Filtrar y mostrar los campos con las etiquetas seleccionadas 
            FindTaggedFields(selectedTags, groupByObject); // Llama a la función que busca los campos etiquetados y mostrarlos
        }
        #endregion
        GUILayout.EndScrollView();
        Repaint();
    }


    private void FindAvailableTags()
    {
        availableTags.Clear();
        var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();

        foreach (var monoBehaviour in allMonoBehaviours)
        {
            var fields = monoBehaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                var tagAttributes = field.GetCustomAttributes(typeof(FriendlyEditor.UtilityAttributes.DebugTagAttribute), true);
                foreach (FriendlyEditor.UtilityAttributes.DebugTagAttribute tagAttribute in tagAttributes)
                {
                    if (!availableTags.Contains(tagAttribute.Label))
                    {
                        availableTags.Add(tagAttribute.Label);
                    }
                }
            }
        }

        availableTags.Sort();
        availableTags.Insert(0, "None");
    }


    private void FindTaggedFields(List<string> tagFilters, bool groupByObject)
    {
        var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
        if (groupByObject)
        {
            // Agrupar por objeto con foldout
            foreach (var monoBehaviour in allMonoBehaviours)
            {
                var fields = monoBehaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);

                IEnumerable<FieldInfo> groupedFields;
                if (isAdditive)
                {
                    groupedFields = fields
                    .Where(field => tagFilters.Any(tag => field.GetCustomAttributes(typeof(FriendlyEditor.UtilityAttributes.DebugTagAttribute), true)
                        .Cast<FriendlyEditor.UtilityAttributes.DebugTagAttribute>()
                        .Any(attr => attr.Label == tag)))
                    .ToList();
                }
                else
                {
                    groupedFields = fields
                    .Where(field => tagFilters.All(tag => field.GetCustomAttributes(typeof(FriendlyEditor.UtilityAttributes.DebugTagAttribute), true)
                        .Cast<FriendlyEditor.UtilityAttributes.DebugTagAttribute>()
                        .Any(attr => attr.Label == tag)))
                    .ToList();
                }



                if (groupedFields.Count() > 0)
                {
                    // Estado del foldout para el objeto
                    bool foldoutState = foldouts.ContainsKey(monoBehaviour.name) ? foldouts[monoBehaviour.name] : true;

                    // Mostrar foldout para el objeto
                    foldoutState = EditorGUILayout.Foldout(foldoutState, monoBehaviour.name, true);
                    foldouts[monoBehaviour.name] = foldoutState;

                    if (foldoutState)
                    {
                        foreach (var field in groupedFields)
                        {
                            object fieldValue = field.GetValue(monoBehaviour);

                            // Dibuja el campo
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(monoBehaviour.name, EditorStyles.linkLabel))
                            {
                                Selection.activeObject = monoBehaviour.gameObject;
                                EditorGUIUtility.PingObject(monoBehaviour.gameObject);
                            }
                            GUILayout.Label($".{field.Name}: {fieldValue}");
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }
        else
        {
            // No agrupar por objeto
            foreach (var monoBehaviour in allMonoBehaviours)
            {
                var fields = monoBehaviour.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy);



                foreach (var field in fields)
                {
                    var tagAttributes = field.GetCustomAttributes(typeof(FriendlyEditor.UtilityAttributes.DebugTagAttribute), true);
                    var fieldTags = tagAttributes.Select(a => ((FriendlyEditor.UtilityAttributes.DebugTagAttribute)a).Label).ToList();


                    // Verificar que el campo tenga todas las etiquetas seleccionadas
                    if (isAdditive)
                    {

                        if (tagFilters.All(fieldTags.Contains))
                        {
                            object fieldValue = field.GetValue(monoBehaviour);

                            // Dibuja el campo con la funcionalidad de selección
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(monoBehaviour.name, EditorStyles.linkLabel))
                            {
                                Selection.activeObject = monoBehaviour.gameObject;
                                EditorGUIUtility.PingObject(monoBehaviour.gameObject);
                            }
                            GUILayout.Label($".{field.Name}: {fieldValue}");
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        if (tagFilters.Any(fieldTags.Contains))
                        {
                            object fieldValue = field.GetValue(monoBehaviour);

                            // Dibuja el campo con la funcionalidad de selección
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button(monoBehaviour.name, EditorStyles.linkLabel))
                            {
                                Selection.activeObject = monoBehaviour.gameObject;
                                EditorGUIUtility.PingObject(monoBehaviour.gameObject);
                            }
                            GUILayout.Label($".{field.Name}: {fieldValue}");
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }
    }
}

