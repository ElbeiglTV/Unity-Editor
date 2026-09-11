using UnityEngine;

public static class EditorStylesManager
{
    public static GUIStyle ButtonStyle;
    public static GUIStyle ToggleStyle;
    public static GUIStyle LabelStyle;

    static EditorStylesManager()
    {
        // Inicializar estilos
        InitializeStyles();
    }

    private static void InitializeStyles()
    {
        // Estilo para botones cuadrados con ícono
        ButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fixedWidth = 32, // Ancho del botón
            fixedHeight = 32, // Alto del botón
            /*
             imagePosition = ImagePosition.ImageOnly, // Mostrar solo imagen
             alignment = TextAnchor.MiddleCenter, // Centrar la imagen en el botón
             normal = { background = CreateTextureFromColor(Color.white) } // Fondo blanco
            */
        };

        // Estilo para toggles
        ToggleStyle = new GUIStyle(GUI.skin.toggle)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft
        };

        // Estilo para etiquetas
        LabelStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
    }
    public static GUIStyle MakeButtonStyle(float fixedWidth, float fixedHeight) => new GUIStyle(GUI.skin.button)
    {
        fixedWidth = fixedWidth, // Ancho del botón
        fixedHeight = fixedHeight, // Alto del botón
        /*
         imagePosition = ImagePosition.ImageOnly, // Mostrar solo imagen
         alignment = TextAnchor.MiddleCenter, // Centrar la imagen en el botón
         normal = { background = CreateTextureFromColor(Color.white) } // Fondo blanco
        */
    };


    private static Texture2D CreateTextureFromColor(Color color)
    {
        // Crear una textura simple de un solo color
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }
}
