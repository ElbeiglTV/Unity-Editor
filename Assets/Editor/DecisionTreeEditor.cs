using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class DecisionTreeEditor : EditorWindow
{
    #region Variables
    //listas y diccionarios de elementos a agregar-

    public Dictionary<DecisionNode, DecisionNode> nodeParentsAdd = new Dictionary<DecisionNode, DecisionNode>();
    public Dictionary<DecisionNode, NodeType> nodesTypesAdd = new Dictionary<DecisionNode, NodeType>();
    private Dictionary<DecisionNode, Rect> nodeRectsAdd = new Dictionary<DecisionNode, Rect>();
    //-

    //listas de elementos a remover-
    public List<DecisionNode> nodesToRemove = new List<DecisionNode>();
    public Dictionary<DecisionNode, DecisionNode> nodeRemoveParents = new Dictionary<DecisionNode, DecisionNode>();
    //-

    //listas originales -
    public List<DecisionNode> nodes = new List<DecisionNode>();
    public Dictionary<DecisionNode, List<DecisionNode>> nodeParents = new Dictionary<DecisionNode, List<DecisionNode>>();
    public Dictionary<DecisionNode, NodeType> nodesTypes = new Dictionary<DecisionNode, NodeType>();
    private Dictionary<DecisionNode, Rect> nodeRects = new Dictionary<DecisionNode, Rect>();
    //-

    //nodo principal-
    DecisionNode rootNode;
    //-
    //nodos seleccionados en el momento -
    DecisionNode currentConectingNode; // nodo que estas conectando en el momento
    DecisionNode draggedNode; // nodo que estas arrastrando
    //-

    Texture2D nodeTexture;
    //bool de control -
    bool isDraggingNodes; // si estas drageando los nodos
    bool isDraggingNode = false; // si estas drageando un nodo
    bool isConectingNode = false; // si estas conectando un nodo
    //-

    // vectores de control de nodos-
    Vector2 dragStartPosition; // posicion inicial del dragueo
    Vector2 dragOfset = Vector2.zero; // ofset pos del mouse - pos inicial de drag
    Vector2 mousePos; // posicion del mouse
    //Vector2 _zoomPivot;
    //-
    //floats
    float _zoom = 1f;

    //-

    #endregion


    [MenuItem("Window/Decision Tree Editor")] // permite tener el boton en la pestaña window para abrir la ventana editable
    public static void ShowWindow()
    {
        GetWindow(typeof(DecisionTreeEditor)); // abre la ventana editable
    }
    private void OnValidate() // se llama cuando se reimportan los assets y cuando se recompila luego de modificar un script 
    {
        //reinicio de el arbol por la perdida de datos de la reimportacion -
        nodeTexture = Resources.Load<Texture2D>("NodeTexture");
        nodes.Clear();
        nodeParents.Clear();
        nodesTypes.Clear();
        nodeRects.Clear();
        currentConectingNode = null;
        rootNode = null;
        //-
    }

    void Butons() // botones extraidos de la funcion onGUI()
    {
        Handles.color = Color.white; // cambia el color de todo lo que venga de la clase handles (se pone en blanco para que no modifique los colores posteriores)
        Handles.DrawSolidRectangleWithOutline(new Rect(0, 0, position.width, 40), Color.HSVToRGB(0, 0, 0.22f), Color.HSVToRGB(0, 0, 0.22f)); //dibuja un rectangulo de color gris arriva 
        GUILayout.Space(10); // deja un espacio vertical de 10 (creo que pixeles)



        if (rootNode == null)
        {
            // dibujamos el boton solo si no hay un arbol osea solo si no hay un nodo raiz
            if (GUILayout.Button("Create New Decision Tree")) // muestra el boton y el boton devuelve un bool dependiendo de si lo tocaste o no
            {
                rootNode = new DecisionNode(); // crea un nuevo nodo raiz

                AddToTypeQueue(rootNode, NodeType.nonConectedNode); // añade el nodo raiz a la cola
                AddToDicionaryAddQueue(rootNode, new Rect(50, 50, 200, 100)); // Posición inicial del nodo raíz
            }
        }
        else
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Restart DecisionTree"))
            {
                currentConectingNode = null;
                rootNode = new DecisionNode();
                nodesTypes.Clear();
                nodes.Clear();
                nodeRects.Clear();
                AddToTypeQueue(rootNode, NodeType.nonConectedNode);
                AddToDicionaryAddQueue(rootNode, new Rect(50, 50, 200, 100)); // Posición inicial del nodo raíz

            }
            if (GUILayout.Button("Save DecisionTree"))
            {
                SaveDecisionTree();
            }
            GUILayout.EndHorizontal();
        }

    }
    void OnGUI()
    {
        GUIUtility.ScaleAroundPivot(new Vector2(_zoom,_zoom ), Vector2.zero);
        #region Fondo
        // se dibuja el fondo de color gris oscuro-
        Handles.BeginGUI();
        Handles.color = Color.white;
        Handles.DrawSolidRectangleWithOutline(new Rect(0, 0, position.width, position.height), Color.HSVToRGB(0, 0, 0.14f), Color.HSVToRGB(0, 0, 0.14f));
        //-
        //se dibujan las lineas punteadas -
        Handles.color = Color.HSVToRGB(0, 0, 0.25f);
        float _lineDiv = (position.width * 2) / 110;
        for (float i = -position.width; i <= position.width; i += _lineDiv)
        {
            float start = -position.width * 10;
            float end = position.width * 10;

            // Dibujar líneas horizontales
            Handles.DrawDottedLine(new Vector3(start, i, 0), new Vector3(end, i, 0), 0.5f);
            // Dibujar líneas verticales
            Handles.DrawDottedLine(new Vector3(i, start, 0), new Vector3(i, end, 0), 0.5f);
        }
        Handles.EndGUI();
        //-
        #endregion



        // se liquidan las queue de remover y agregar nodos a listas y diccionarios antes de dibujar los nodos -
        RemoveFromParents();
        AddNodesToDictionary();
        AddToTypes();
        AddToParents();
        //-
        //escalado-
        //Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(_zoom, _zoom, 1.0f));
        //Matrix4x4 translationMatrix = Matrix4x4.Translate(new Vector3(_zoomPivot.x, _zoomPivot.y, 0));
        //Matrix4x4 transformMatrix =  scaleMatrix ;

        //Matrix4x4 originalMatrix = GUI.matrix;
        //GUI.matrix = transformMatrix;

        //-
        List<DecisionNode> nodesCopy = nodes; // se crea una copia de la lista a recorrer por si la original se modifica
        // se dibuja cada nodo en la lista copiada-
        foreach (DecisionNode node in nodesCopy)
        {
            DrawNode(node);
        }
        //-
        //escalado
        GUI.matrix = Matrix4x4.identity;
        //-
        //luego se remueven los nodos que no son nesesarios o que el usuario desidio eliminar-
        foreach (DecisionNode node in nodesToRemove)
        {
            RemoveNode(node);
        }
        nodesToRemove.Clear();
        //-




        Butons(); // se llama a los botones de la parte superior se dibuja despues de los nodos para estar por ensima

        // Debugueo de sonas donde el mouse interactua-
        /* foreach(var kvp in nodeRects)
         {
             Handles.DrawSolidRectangleWithOutline(new Rect(kvp.Value.x + 185, kvp.Value.y + 67, 10, 10), Color.white, Color.white);
         }*/
        //-
        #region MouseEvents

        if (isConectingNode && currentConectingNode != null) // si estas intentando conectar un nodo y el nodo que estas intentando conectar no es null
        {
            // se dibuja una linea entre el nodo y la pos del mouse-
            Handles.color = Color.white;
            Handles.DrawLine(mousePos, new Vector3(8, 12, 0) + new Vector3(nodeRects[currentConectingNode].x, nodeRects[currentConectingNode].y));
            //-
            Repaint(); // y se fuersa al repintado
        }
        // Eventos Del Mouse
        Event e = Event.current;
        mousePos = new Vector2(e.mousePosition.x, e.mousePosition.y);
        switch (e.type) //typo de evento 
        {
            case EventType.MouseDown: // cuando algun boton se toca

                if (e.button == 2)// ruedita del mouse
                {
                    foreach (var kvp in nodeRects) // por cada key value pair en node rects
                    {
                        if (kvp.Value.Contains(e.mousePosition)) // si kvp.value (Rect) contiene e.mousePosition (Pocision del mouse)
                        {
                            isDraggingNode = true; // activamos el dragueo de nodo individual
                            dragStartPosition = e.mousePosition - kvp.Value.position; // calculamos la pocicion inicial de dragueo
                            draggedNode = kvp.Key; // volvemos DraggedNode(nodo que estamos moviendo) igual a kvp.key (DecisionNode)
                            break;// salimos del bucle
                        }
                    }
                    if (!isDraggingNode) // si no estamos dragueando un nodo espesifico
                    {
                        isDraggingNodes = true; // Activamos el dragueo de nodos general
                        dragStartPosition = e.mousePosition; // guardamos la posicion actual
                        e.Use(); // desimos que el evento esta en uso
                    }
                }
                if (e.button == 1)// click isquierdo
                {
                    bool contain = false; // bool  que dise si contiene la posicion del mouse o no
                    foreach (var kvp in nodeRects) // recorremos el diccionario
                    {
                        if (kvp.Value.Contains(e.mousePosition)) // vemos si el (Rect) contiene la pos del mouse
                        {
                            contain = true; // volvemos true el bool Contain
                            break;//salimos del bucle
                        }
                        else { contain = false; }// sino la volvemos false 
                    }
                    GenericMenu menu = new GenericMenu();// creamos un nuevo menu (menu contextual)

                    if (!contain)// si ningun nodo contiene la pos del mouse
                    {

                        if (rootNode != null)//si hay nodo raiz
                        {
                            // hace que aparescan estas opciones en el menu contextual-
                            menu.AddItem(new GUIContent("Restart DesicionTree"), false, RestartDecicionTree);
                            menu.AddItem(new GUIContent("Save DecisionTree"), false, SaveDecisionTree);
                            menu.AddItem(new GUIContent("Create new node"), false, CreateNode);
                            //-

                        }
                        else//si no hay un nodo raiz
                        {
                            //agrega esta opcion al menu contextual
                            menu.AddItem(new GUIContent("new DecisionTree"), false, CreateDecisionTree);
                        }
                        //muestra el menu
                        menu.ShowAsContext();

                        e.Use();// dise que el evento esta en uso
                    }
                    else// si algun nodo contiene la pos de el mouse
                    {
                        if (rootNode != null)
                        {
                            menu.AddItem(new GUIContent("RemoveNode"), false, RemoveNode);
                            menu.AddItem(new GUIContent("ConvertInRootNode"), false, ConvertNodeInRootNode);
                            menu.ShowAsContext();
                        }
                    }
                }
                if (e.button == 0)
                {
                    foreach (var kvp in nodeRects)
                    {

                        if (new Rect(kvp.Value.x + 4, kvp.Value.y + 6, 10, 10).Contains(e.mousePosition))
                        {
                            if (kvp.Key != rootNode)
                            {
                                isConectingNode = true;
                                currentConectingNode = kvp.Key;
                            }

                        }

                    }
                }
                break;

            case EventType.MouseDrag:
                if (isDraggingNode && draggedNode != null)
                {
                    nodeRects[draggedNode] = new Rect(e.mousePosition.x - dragStartPosition.x, e.mousePosition.y - dragStartPosition.y, nodeRects[draggedNode].width, nodeRects[draggedNode].height);
                    e.Use();
                }
                else if (isDraggingNodes)
                {

                    dragOfset = e.mousePosition - dragStartPosition;
                    dragStartPosition = e.mousePosition;
                    e.Use();


                }

                break;

            case EventType.MouseUp:

                if (e.button == 0)
                {
                    foreach (var kvp in nodeRects)
                    {

                        if (new Rect(kvp.Value.x + 185, kvp.Value.y + 90, 10, 10).Contains(e.mousePosition))
                        {
                            if (currentConectingNode != null)
                            {
                                if (kvp.Key.falseNode == null)
                                {
                                    kvp.Key.falseNode = currentConectingNode;
                                    kvp.Key.falseNodeID = currentConectingNode.UniqueID;

                                    if (nodeParents.ContainsKey(currentConectingNode))
                                    {
                                        if (!nodeParents[currentConectingNode].Contains(kvp.Key))
                                        {
                                            nodeParents[currentConectingNode].Add(kvp.Key);

                                        }
                                    }
                                    else
                                    {
                                        nodeParents.Add(currentConectingNode, new List<DecisionNode>());
                                        nodeParents[currentConectingNode].Add(kvp.Key);
                                        nodesTypes[currentConectingNode] = NodeType.falseNode; break;
                                    }
                                }
                                else
                                {

                                    AddToRemoveParents(kvp.Key, kvp.Key.falseNode);
                                    kvp.Key.falseNode = currentConectingNode;
                                    kvp.Key.falseNodeID = currentConectingNode.UniqueID;

                                    if (nodeParents.ContainsKey(currentConectingNode))
                                    {
                                        if (!nodeParents[currentConectingNode].Contains(kvp.Key))
                                        {
                                            nodeParents[currentConectingNode].Add(kvp.Key);

                                        }
                                    }
                                    else
                                    {
                                        nodeParents.Add(currentConectingNode, new List<DecisionNode>());
                                        nodeParents[currentConectingNode].Add(kvp.Key);
                                        nodesTypes[currentConectingNode] = NodeType.falseNode; break;
                                    }
                                }
                            }
                        }
                        else if (new Rect(kvp.Value.x + 185, kvp.Value.y + 67, 10, 10).Contains(e.mousePosition))
                        {
                            if (currentConectingNode != null)
                            {
                                if (kvp.Key.trueNode == null)
                                {

                                    kvp.Key.trueNode = currentConectingNode;
                                    kvp.Key.trueNodeID = currentConectingNode.UniqueID;

                                    if (nodeParents.ContainsKey(currentConectingNode))
                                    {
                                        if (!nodeParents[currentConectingNode].Contains(kvp.Key))
                                        {
                                            nodeParents[currentConectingNode].Add(kvp.Key);

                                        }
                                    }
                                    else
                                    {
                                        nodeParents.Add(currentConectingNode, new List<DecisionNode>());
                                        nodeParents[currentConectingNode].Add(kvp.Key);
                                        nodesTypes[currentConectingNode] = NodeType.trueNode; break;
                                    }
                                }
                                else
                                {
                                    AddToRemoveParents(kvp.Key, kvp.Key.trueNode);

                                    kvp.Key.trueNode = currentConectingNode;
                                    kvp.Key.trueNodeID = currentConectingNode.UniqueID;

                                    if (nodeParents.ContainsKey(currentConectingNode))
                                    {
                                        if (!nodeParents[currentConectingNode].Contains(kvp.Key))
                                        {
                                            nodeParents[currentConectingNode].Add(kvp.Key);

                                        }
                                    }
                                    else
                                    {
                                        nodeParents.Add(currentConectingNode, new List<DecisionNode>());
                                        nodeParents[currentConectingNode].Add(kvp.Key);
                                        nodesTypes[currentConectingNode] = NodeType.trueNode; break;
                                    }
                                }
                            }
                        }


                    }
                    isConectingNode = false;
                    currentConectingNode = null;
                }
                isDraggingNode = false;
                isDraggingNodes = false;
                dragOfset = Vector2.zero;
                draggedNode = null;
                break;
            case EventType.ScrollWheel:


               // _zoomPivot = new Vector2(position.width / 2, position.height / 2);
                //_zoomPivot /= _zoom;
                _zoom = Mathf.Clamp(_zoom-e.delta.y * 0.01f,1f, 2.5f);

                Repaint();
                break;
        }
        #endregion
    }
    #region MetodosMenus
    void RestartDecicionTree()
    {

        currentConectingNode = null;
        rootNode = new DecisionNode();
        nodeParents.Clear();
        nodesTypes.Clear();
        nodes.Clear();
        nodeRects.Clear();
        AddToTypeQueue(rootNode, NodeType.nonConectedNode);
        AddToDicionaryAddQueue(rootNode, new Rect(mousePos.x, mousePos.y, 200, 100)); // Posición inicial del nodo raíz
    }
    void CreateDecisionTree()
    {

        rootNode = new DecisionNode();
        AddToDicionaryAddQueue(rootNode, new Rect(mousePos.x, mousePos.y, 200, 100)); // Posición inicial del nodo raíz
        AddToTypeQueue(rootNode, NodeType.nonConectedNode);
    }
    void CreateNode()
    {
        DecisionNode node = new DecisionNode();
        AddToDicionaryAddQueue(node, new Rect(mousePos.x, mousePos.y, 200, 100));
        AddToTypeQueue(node, NodeType.nonConectedNode);
    }
    void ConvertNodeInRootNode()
    {

        foreach (var kvp in nodeRects)
        {
            if (kvp.Value.Contains(mousePos))
            {

                if (nodeParents.ContainsKey(kvp.Key))
                {
                    if (nodesTypes[kvp.Key] == NodeType.trueNode)
                    {
                        foreach (DecisionNode node in nodeParents[kvp.Key])
                        {
                            node.trueNode = null;
                        }
                    }
                    else if (nodesTypes[kvp.Key] == NodeType.falseNode)
                    {
                        foreach (DecisionNode node in nodeParents[kvp.Key])
                        {
                            node.falseNode = null;
                        }
                    }
                }
                nodesTypes[kvp.Key] = NodeType.nonConectedNode;
                rootNode = kvp.Key;

                break;
            }
        }
    }
    void RemoveNode()
    {

        DecisionNode nodeToRemove = null;
        foreach (var kvp in nodeRects)
        {
            if (kvp.Value.Contains(mousePos))
            {
                nodeToRemove = kvp.Key;
                break;
            }
        }
        if (nodeToRemove != null)
        {
            if (nodeParents.ContainsKey(nodeToRemove))
            {
                if (nodeToRemove == rootNode)
                {
                    nodeRects.Remove(nodeToRemove);
                    nodesTypes.Remove(nodeToRemove);
                    nodes.Remove(nodeToRemove);
                    if (nodes.Count >= 1)
                    {
                        rootNode = nodes[0];
                        nodesTypes[rootNode] = NodeType.nonConectedNode;

                    }
                    else
                    {
                        rootNode = null;
                    }
                }
                else
                {

                    foreach (DecisionNode node in nodeParents[nodeToRemove])
                    {
                        if (node.trueNode == nodeToRemove)
                        {
                            node.trueNode = null;
                        }
                        if (node.falseNode == nodeToRemove)
                        {

                            node.falseNode = null;
                        }
                    }
                    nodeParents.Remove(nodeToRemove);
                    nodeRects.Remove(nodeToRemove);
                    nodesTypes.Remove(nodeToRemove);
                    nodes.Remove(nodeToRemove);
                }
            }
            else
            {
                if (nodeToRemove == rootNode)
                {
                    nodeRects.Remove(nodeToRemove);
                    nodesTypes.Remove(nodeToRemove);
                    nodes.Remove(nodeToRemove);
                    if (nodes.Count >= 1)
                    {
                        rootNode = nodes[0];
                        nodesTypes[rootNode] = NodeType.nonConectedNode;

                    }
                    else
                    {
                        rootNode = null;
                    }
                }

            }


        }
    }
    void RemoveNode(DecisionNode nodeToRemove)
    {

        if (nodeToRemove != null)
        {
            if (nodeParents.ContainsKey(nodeToRemove))
            {
                nodeParents.Remove(nodeToRemove);
            }

            nodeRects.Remove(nodeToRemove);
            nodesTypes.Remove(nodeToRemove);
            nodes.Remove(nodeToRemove);

        }
    }
    #endregion
    #region Texture Control
    private Texture2D MakeTex(int width, int height, float cornerRadius, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calcula la distancia al centro de las esquinas redondeadas
                float distanceToCenter = Vector2.Distance(new Vector2(x, y), new Vector2(cornerRadius, cornerRadius));

                // Si está dentro del radio, pinta el píxel con el color; de lo contrario, pinta transparente
                Color pixelColor = (distanceToCenter <= cornerRadius) ? color : Color.clear;
                pixels[y * width + x] = pixelColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }
    private Texture2D MakeTex(int width, int height, Color color)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = color;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    private Texture2D CreateRoundedRectangleTexture(int width, int height, int cornerRadius, Color color)
    {
        Texture2D texture = new Texture2D(width, height);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calcula la distancia al centro de las esquinas redondeadas
                float distanceToCenterX = Mathf.Abs(x - cornerRadius);
                float distanceToCenterY = Mathf.Abs(y - cornerRadius);

                // Si está dentro del radio, pinta el píxel con el color; de lo contrario, pinta transparente
                if (distanceToCenterX > width - cornerRadius * 2 || distanceToCenterY > height - cornerRadius * 2)
                {
                    pixels[y * width + x] = Color.clear;
                }
                else
                {
                    float alpha = 1.0f;
                    if (distanceToCenterX > cornerRadius - 1 && distanceToCenterY > cornerRadius - 1)
                    {
                        alpha = Mathf.Min(distanceToCenterX - cornerRadius + 1, distanceToCenterY - cornerRadius + 1) / cornerRadius;
                    }
                    pixels[y * width + x] = new Color(color.r, color.g, color.b, alpha);
                }
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }
    void DrawLine(Vector2 start, Vector2 end)
    {
        // Calcula la longitud y el ángulo de la línea
        float length = Vector2.Distance(start, end);
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;

        // Dibuja una textura (línea) entre los puntos de inicio y fin
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y, length, 1f), EditorGUIUtility.whiteTexture);
        GUIUtility.RotateAroundPivot(-angle, start);
    }
    #endregion

    Color NodeColor(DecisionNode node)
    {
        if (node != rootNode)
        {

            if (nodesTypes[node] == NodeType.falseNode)
            {
                return Color.red;
            }
            else if (nodesTypes[node] == NodeType.trueNode)
            {
                return Color.green;
            }
            else { return Color.white; }
        }
        else return Color.magenta;
    }
    void DrawNode(DecisionNode node)
    {
        
        GUIStyle areaStyle = new GUIStyle(GUI.skin.button);
        areaStyle.normal.background = MakeTex(1, 1, Color.HSVToRGB(0, 0, 0.19f)); //Color.HSVToRGB(0, 0, 0.19f)
        Rect rect = GetNodeRect(node);
        GUILayout.BeginArea(rect, areaStyle); // se define el area del nodo
        //dibujado de el circulito de arriva a la izquierda-
        Handles.color = Color.gray;
        Handles.DrawSolidDisc(new Vector3(8, 12, 0), Vector3.forward, 5);
        Handles.color = NodeColor(node);
        Handles.DrawSolidDisc(new Vector3(8, 12, 0), Vector3.forward, 3);
        //-
        EditorGUI.BeginChangeCheck();


        Handles.color = Color.white;
        GUILayout.BeginHorizontal();
        GUILayout.Space(10);
        node.isExpanded = EditorGUILayout.Foldout(node.isExpanded, "Node ID: " + node.GetHashCode().ToString()); // flecha que permite la expancion del nodo
        //node.isExpanded = EditorGUILayout.Foldout(node.isExpanded,"hola",true,new GUIStyle(EditorStyles.foldout)); // flecha que permite la expancion del nodo
        GUILayout.EndHorizontal();

        if (node.isExpanded) // se chequea que el nodo este ecpandido
        {
            GUILayout.BeginHorizontal();//inicia el modo horizontal
            GUILayout.Space(30);//espacio horizontal
            GUILayout.Label("Question"); //texto
            GUILayout.EndHorizontal();//termina el espacio horizontal
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            
            node.Questions = (Questions)EditorGUILayout.EnumPopup(node.Questions); // se muestra el enum
            GUILayout.Space(30);
            GUILayout.EndHorizontal();

            if (node.trueNode != null) // si el nodo verdadero no esta vasio
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                if (GUILayout.Button("Restart Node", GUILayout.ExpandWidth(false))) // se dibuja el boton de resetear nodo
                {

                    nodesToRemove.Add(node.trueNode);// se agrega a la lista de nodos a remover el nodo actual
                    node.trueNode = new DecisionNode(); //se crea un nuevo nodo
                    node.trueNodeID = node.trueNode.UniqueID; // se le asigna la id del nuevo nodo a la id del true node
                    //se añade a las queue correspondientes-
                    AddToParentsQueue(node.trueNode, node);
                    AddToTypeQueue(node.trueNode, NodeType.trueNode);
                    AddToDicionaryAddQueue(node.trueNode, new Rect(nodeRects[node].x + 250, nodeRects[node].y, 200, 100));
                    //-

                }
                GUILayout.EndHorizontal();
                Handles.color = Color.gray;
                Handles.DrawSolidDisc(new Vector3(190, 72, 0), Vector3.forward, 5);
                Handles.color = Color.green;
                Handles.DrawSolidDisc(new Vector3(190, 72, 0), Vector3.forward, 3);
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                if (GUILayout.Button("New True Node", GUILayout.ExpandWidth(false)))
                {

                    node.trueNode = new DecisionNode();
                    node.trueNodeID = node.trueNode.UniqueID;
                    AddToDicionaryAddQueue(node.trueNode, new Rect(nodeRects[node].x + 250, nodeRects[node].y, 200, 100));
                    AddToTypeQueue(node.trueNode, NodeType.trueNode);
                    AddToParentsQueue(node.trueNode, node);

                }
                GUILayout.EndHorizontal();
                Handles.color = Color.gray;
                Handles.DrawSolidDisc(new Vector3(190, 72, 0), Vector3.forward, 5);
                Handles.color = Color.white;
                Handles.DrawSolidDisc(new Vector3(190, 72, 0), Vector3.forward, 3);

            }





            if (node.falseNode != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                if (GUILayout.Button("Restart Node", GUILayout.ExpandWidth(false)))
                {

                    nodesToRemove.Add(node.falseNode);
                    node.falseNode = new DecisionNode();
                    node.falseNodeID = node.falseNode.UniqueID;
                    AddToParentsQueue(node.falseNode, node);
                    AddToTypeQueue(node.falseNode, NodeType.falseNode);
                    AddToDicionaryAddQueue(node.falseNode, new Rect(nodeRects[node].x + 250, nodeRects[node].y + 150, 200, 100));

                }
                GUILayout.EndHorizontal();
                Handles.color = Color.gray;
                Handles.DrawSolidDisc(new Vector3(190, 95, 0), Vector3.forward, 5);
                Handles.color = Color.red;
                Handles.DrawSolidDisc(new Vector3(190, 95, 0), Vector3.forward, 3);
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                if (GUILayout.Button("New False Node", GUILayout.ExpandWidth(false)))
                {

                    node.falseNode = new DecisionNode();
                    nodeParents.Add(node.falseNode, new List<DecisionNode>());
                    nodeParents[node.falseNode].Add(node);
                    node.falseNodeID = node.falseNode.UniqueID;
                    nodesTypes.Add(node.falseNode, NodeType.falseNode);
                    AddToDicionaryAddQueue(node.falseNode, new Rect(nodeRects[node].x + 250, nodeRects[node].y + 150, 200, 100));

                }
                GUILayout.EndHorizontal();
                Handles.color = Color.gray;
                Handles.DrawSolidDisc(new Vector3(190, 95, 0), Vector3.forward, 5);
                Handles.color = Color.white;
                Handles.DrawSolidDisc(new Vector3(190, 95, 0), Vector3.forward, 3);
            }



        }
        EditorGUI.EndChangeCheck();

        GUILayout.EndArea();
        // Dibujar líneas de conexión aquí
        Handles.color = Color.green;
        if (node.trueNode != null)
        {
            DibujarLinea(node, node.trueNode);
            // Handles.DrawLine(new Vector2(nodeRects[node].x + nodeRects[node].width, nodeRects[node].y + nodeRects[node].height / 2),
            //            new Vector2(nodeRects[node.trueNode].x, nodeRects[node.trueNode].y + nodeRects[node.trueNode].height / 2));
        }

        Handles.color = Color.red;
        if (node.falseNode != null)
        {
            DibujarLinea(node, node.falseNode);
            // Handles.DrawLine(new Vector2(nodeRects[node].x + nodeRects[node].width, nodeRects[node].y + nodeRects[node].height / 2),
            //                new Vector2(nodeRects[node.falseNode].x, nodeRects[node.falseNode].y + nodeRects[node.falseNode].height / 2));
        }
       

    }
    void AddToRemoveParents(DecisionNode nodeToRemove, DecisionNode InListOfNode)
    {
        nodeRemoveParents.Add(nodeToRemove, InListOfNode);
    }
    void RemoveFromParents()
    {
        foreach (var kvp in nodeRemoveParents)
        {
            if (nodeParents.ContainsKey(kvp.Value))
            {
                if (nodeParents[kvp.Value].Contains(kvp.Key))
                {
                    nodeParents[kvp.Value].Remove(kvp.Key);
                }
                if (nodeParents[kvp.Value].Count == 0)
                {
                    nodesTypes[kvp.Value] = NodeType.nonConectedNode;
                    nodeParents.Remove(kvp.Value);
                }
            }

        }
        nodeRemoveParents.Clear();
    }
    void AddToTypeQueue(DecisionNode node, NodeType nodeType)
    {
        nodesTypesAdd.Add(node, nodeType);
    }
    void AddToTypes()
    {
        foreach (var kvp in nodesTypesAdd)
        {

            nodesTypes.Add(kvp.Key, kvp.Value);
        }
        nodesTypesAdd.Clear();
    }
    void AddToParentsQueue(DecisionNode node, DecisionNode parentNode)
    {
        nodeParentsAdd.Add(node, parentNode);
    }
    void AddToParents()
    {
        foreach (var kvp in nodeParentsAdd)
        {

            if (nodeParents.ContainsKey(kvp.Key))
            {
                nodeParents[kvp.Key].Add(kvp.Value);
            }
            else
            {
                nodeParents.Add(kvp.Key, new List<DecisionNode>());
                nodeParents[kvp.Key].Add(kvp.Value);


            }
        }
        nodeParentsAdd.Clear();
    }
    void AddToDicionaryAddQueue(DecisionNode node, Rect rect)
    {
        nodeRectsAdd.Add(node, rect);
    }
    void AddNodesToDictionary()
    {
        foreach (var kvp in nodeRectsAdd)
        {

            if (!nodeRects.ContainsKey(kvp.Key))
            {
                nodeRects[kvp.Key] = kvp.Value;
                nodes.Add(kvp.Key);
            }
        }
        nodeRectsAdd.Clear();

    }
    Rect GetNodeRect(DecisionNode node)
    {


        float alto = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
        if (node.isExpanded)
        {

            alto = EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 6;
            if (node.trueNode != null)
            {
                alto += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                alto += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            if (node.falseNode != null)
            {
                alto += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            else
            {
                alto += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            alto += EditorGUIUtility.standardVerticalSpacing * 3;
        }



        if (dragOfset != Vector2.zero)
        {

            nodeRects[node] = new Rect(nodeRects[node].x + (dragOfset.x), nodeRects[node].y + (dragOfset.y), nodeRects[node].width, nodeRects[node].height);
        }

        return new Rect(nodeRects[node].x, nodeRects[node].y, 200, alto);
    }
    void DibujarLinea(DecisionNode ActualNode, DecisionNode TargetNode)
    {
        if (ActualNode == null || TargetNode == null) return;
        if (!nodeRects.ContainsKey(ActualNode) || !nodeRects.ContainsKey(TargetNode)) return;
        if (ActualNode.isExpanded && TargetNode.isExpanded)
        {
            Vector2 Actual = new Vector2(nodeRects[ActualNode].x + nodeRects[ActualNode].width, nodeRects[ActualNode].y + nodeRects[ActualNode].height / 2);
            Vector2 Target = new Vector2(nodeRects[TargetNode].x, nodeRects[TargetNode].y + nodeRects[TargetNode].height / 2);
            Handles.DrawLine(Actual, Target);

        }
        else if (ActualNode.isExpanded && !TargetNode.isExpanded)
        {
            Vector2 Actual = new Vector2(nodeRects[ActualNode].x + nodeRects[ActualNode].width, nodeRects[ActualNode].y + nodeRects[ActualNode].height / 2);
            Vector2 Target = new Vector2(nodeRects[TargetNode].x, nodeRects[TargetNode].y + EditorGUIUtility.singleLineHeight / 2);
            Handles.DrawLine(Actual, Target);
        }
        else if (!ActualNode.isExpanded && TargetNode.isExpanded)
        {
            Vector2 Actual = new Vector2(nodeRects[ActualNode].x + nodeRects[ActualNode].width, nodeRects[ActualNode].y + EditorGUIUtility.singleLineHeight / 2);
            Vector2 Target = new Vector2(nodeRects[TargetNode].x, nodeRects[TargetNode].y + nodeRects[TargetNode].height / 2);
            Handles.DrawLine(Actual, Target);

        }
        else
        {
            Vector2 Actual = new Vector2(nodeRects[ActualNode].x + nodeRects[ActualNode].width, nodeRects[ActualNode].y + EditorGUIUtility.singleLineHeight / 2);
            Vector2 Target = new Vector2(nodeRects[TargetNode].x, nodeRects[TargetNode].y + EditorGUIUtility.singleLineHeight / 2);
            Handles.DrawLine(Actual, Target);

        }
    }
    GUIStyle ButonStyle()
    {
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 8; // Tamaño del texto dentro del botón
        buttonStyle.fixedHeight = 15; // Altura fija del botón
        return buttonStyle;
    }
    GUIStyle EnumStyle()
    {
        GUIStyle enumPopupStyle = new GUIStyle();
        enumPopupStyle.fontSize = 8; // Tamaño del texto dentro del EnumPopup
        enumPopupStyle.fixedHeight = 15; // Altura fija del EnumPopup
        return enumPopupStyle;
    }
    void SaveDecisionTree()
    {
        SOdecisionTree DecisionData = CreateInstance<SOdecisionTree>();
        DecisionData.RootNode = rootNode; // Asigna la matriz al ScriptableObject
        DecisionData.TreeData.IgualeTo(nodeRects);

        string filePath = EditorUtility.SaveFilePanel("Save DecisionTree", "Assets", "DecisionTreeData", "asset");
        if (!string.IsNullOrEmpty(filePath))
        {
            // Guarda el ScriptableObject como un archivo asset
            string assetPath = filePath.Substring(Application.dataPath.Length - "Assets".Length);
            AssetDatabase.CreateAsset(DecisionData, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.SetDirty(DecisionData);
            Debug.Log("DecisionTreeData saved at: " + assetPath);
        }
    }

}
public enum NodeType
{
    trueNode, falseNode, nonConectedNode, Mixed
}
