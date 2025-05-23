﻿using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace I2
{	
	public class GUITools
	{
		public static Color White = Color.white;
		public static Color LightGray = Color.Lerp(Color.gray, Color.white, 0.5f);
		public static Color DarkGray = Color.Lerp(Color.gray, Color.white, 0.2f);
		public static Color LightYellow = Color.Lerp(Color.yellow, Color.white, 0.5f);

        public static GUILayoutOption DontExpandWidth = GUILayout.ExpandWidth(false);
        public static GUIContent EmptyContent = new GUIContent ();

         #region Header

        public static bool DrawHeader (string text, string key, bool ShowToggle=false, bool ToggleState=false, System.Action<bool> OnToggle= null, string HelpURL=default(string), Color disabledColor = default(Color))
		{
			bool state = EditorPrefs.GetBool(key, false);

			bool newState = DrawHeader (text, state, ShowToggle, ToggleState, OnToggle, HelpURL, disabledColor);

			if (state!=newState) EditorPrefs.SetBool(key, newState);
			return newState;
		}

		public static bool DrawHeader (string text, bool state, bool ShowToggle=false, bool ToggleState=false, System.Action<bool> OnToggle= null, string HelpURL=default(string), Color disabledColor = default(Color), bool allowCollapsing = true)
		{
			GUIStyle Style = new GUIStyle(EditorStyles.foldout) {richText = true};
			EditorStyles.foldout.richText = true;
			if (state)
			{
				GUI.backgroundColor=DarkGray;
				GUILayout.BeginVertical("AS TextArea", GUILayout.Height(1));
				GUILayout.BeginHorizontal();
                if (!string.IsNullOrEmpty(text))
                {
                    if (allowCollapsing)
                        state = GUILayout.Toggle(state, text, Style, GUILayout.ExpandWidth(true));
                    else
                        GUILayout.Label(text, GUILayout.ExpandWidth(true));
                }

				if (!string.IsNullOrEmpty(HelpURL))
				{
					if (GUILayout.Button (Icon_Help, EditorStyles.label, GUILayout.ExpandWidth(false)))
						Application.OpenURL(HelpURL);
				}
				if (ShowToggle)
				{
					GUI.changed = false;
					bool newBool = GUILayout.Toggle(ToggleState, "", "OL Toggle", GUILayout.ExpandWidth(false));
					if (GUI.changed && OnToggle!=null)
						OnToggle(newBool);
				}
				GUILayout.EndHorizontal();
				GUILayout.Space(2);
				
				GUI.backgroundColor = Color.white;
			}
			else
			{
				if (ShowToggle && !ToggleState) 
					GUI.color = disabledColor;

				GUILayout.BeginHorizontal("Box");
				//GUILayout.BeginHorizontal(EditorStyles.toolbar);
				state = GUILayout.Toggle(state, text, Style, GUILayout.ExpandWidth(true));
				if (ShowToggle)
				{
					GUI.changed = false;
					bool newBool = GUILayout.Toggle(ToggleState, "", "OL Toggle", GUILayout.ExpandWidth(false));
					if (GUI.changed && OnToggle!=null)
						OnToggle(newBool);
				}
				GUILayout.EndHorizontal();
				GUI.color = White;
			}
			return state;
		}

		public static void CloseHeader()
		{
			GUILayout.EndHorizontal();
		}


		#endregion

		#region Content
	
		public static void BeginContents ()
		{
			EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
			GUILayout.Space(2f);
			EditorGUILayout.BeginVertical();
			GUILayout.Space(2f);
		}
	
		public static void EndContents () { EndContents(true); }
		public static void EndContents ( bool closeHeader )
		{
			GUILayout.Space(2f);
			EditorGUILayout.EndVertical();
			GUILayout.Space(3f);
			GUILayout.EndHorizontal();

			if (closeHeader) CloseHeader();
		}

		#endregion

		#region Tabs

		public static void DrawTabs( SerializedProperty mProperty, GUIStyle Style=null, int height=25 )
		{
			int curIndex = mProperty.enumValueIndex;
			int newIndex = DrawTabs( curIndex, mProperty.enumNames, Style, height);

			if (curIndex!=newIndex)
				mProperty.enumValueIndex = newIndex;
		}

		public static int DrawTabs( int Index, string[] Tabs, GUIStyle Style=null, int height=25, bool expand = true)
		{
			GUIStyle MyStyle = new GUIStyle(Style != null ? Style : GUI.skin.FindStyle("dragtab")) {fixedHeight = 0};

			GUILayout.BeginHorizontal();
			for (int i=0; i<Tabs.Length; ++i)
			{
				int idx = Tabs[i].IndexOf('|');
				if (idx>0)
				{
					string text = Tabs[i].Substring(0, idx);
					string tooltip = Tabs[i].Substring(idx+1);
					if ( GUILayout.Toggle(Index==i, new GUIContent(text, tooltip), MyStyle, GUILayout.Height(height), GUILayout.ExpandWidth(expand)) && Index!=i) 
					{
							Index=i;
							GUI.FocusControl(string.Empty);
					}
				}
				else
				{
					if ( GUILayout.Toggle(Index==i, Tabs[i], MyStyle, GUILayout.Height(height), GUILayout.ExpandWidth(expand)) && Index!=i) 
					{
						Index=i;
						GUI.FocusControl(string.Empty);
					}
				}
			}
			GUILayout.EndHorizontal();
			return Index;
		}

		public static void DrawShadowedTabs( SerializedProperty mProperty, GUIStyle Style=null, int height=25, bool expand=true )
		{
			int curIndex = mProperty.enumValueIndex;
			int newIndex = DrawShadowedTabs( curIndex, mProperty.enumNames, height, expand);

			if (curIndex!=newIndex)
				mProperty.enumValueIndex = newIndex;
		}

		public static int DrawShadowedTabs( int Index, string[] Tabs, int height = 25, bool expand=true )
		{
			GUI.backgroundColor=Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical("AS TextArea", GUILayout.Height(1));
				GUI.backgroundColor=Color.white;
				GUILayout.Space(2);
				Index = DrawTabs( Index, Tabs, height: height, expand:expand );
			GUILayout.EndVertical();
			return Index;
		}

		public static int DrawTabs( int Index, Texture2D[] Tabs, GUIStyle Style, int height )
		{
			GUIStyle MyStyle = new GUIStyle(Style!=null?Style:GUI.skin.FindStyle("dragtab"));
			MyStyle.fixedHeight=0;

			//width = Mathf.Max (width, height * Tabs[0].width/(float)Tabs[0].height);

			GUILayout.BeginHorizontal();
			float width = (Screen.width-(MyStyle.border.left+MyStyle.border.right)*(Tabs.Length-1)) / (float)Tabs.Length;
			for (int i=0; i<Tabs.Length; ++i)
			{
				if ( GUILayout.Toggle(Index==i, Tabs[i], MyStyle, GUILayout.Height(height), GUILayout.Width(width)) && Index!=i) 
				{
					Index=i;
					GUI.changed = true;
				}
			}
			GUILayout.EndHorizontal();
			return Index;
		}

		#endregion

		#region Object Array

		public static void DrawObjectsArray( SerializedProperty PropArray )
		{
			GUILayout.BeginVertical();

				int DeleteElement = -1, MoveUpElement = -1;

				for (int i=0, imax=PropArray.arraySize; i<imax; ++i)
				{
					SerializedProperty Prop = PropArray.GetArrayElementAtIndex(i);
					GUILayout.BeginHorizontal();

						//--[ Delete Button ]-------------------
						if (GUILayout.Button("X", EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
				    		DeleteElement = i;

						GUILayout.Space(2);
				    	//--[ Object ]--------------------------
						GUILayout.BeginHorizontal(EditorStyles.toolbar);
							GUI.changed = false;
							Object Obj = EditorGUILayout.ObjectField( Prop.objectReferenceValue, typeof(Object), true, GUILayout.ExpandWidth(true));
							if (Obj==null)
								DeleteElement = i;
							else
							if (GUI.changed)
								Prop.objectReferenceValue = Obj;
						GUILayout.EndHorizontal();

						//--[ MoveUp Button ]-------------------
						if (i==0)
						{
							if (imax>1)
								GUILayout.Space (18);
						}
						else
						{
							if (GUILayout.Button( "\u25B2", EditorStyles.toolbarButton, GUILayout.Width(18)))
								MoveUpElement = i;
						}

					GUILayout.EndHorizontal();
				}

				GUILayout.BeginHorizontal(EditorStyles.toolbar);
					Object NewObj = EditorGUILayout.ObjectField( null, typeof(Object), true, GUILayout.ExpandWidth(true));
					if (NewObj) 
					{
						int Index = PropArray.arraySize;
						PropArray.InsertArrayElementAtIndex( Index );
						PropArray.GetArrayElementAtIndex(Index).objectReferenceValue = NewObj;
					}
				GUILayout.EndHorizontal();

				if (DeleteElement>=0)
				{
					PropArray.DeleteArrayElementAtIndex( DeleteElement );
					//PropArray.DeleteArrayElementAtIndex( DeleteElement );
				}

				if (MoveUpElement>=0)
					PropArray.MoveArrayElement(MoveUpElement, MoveUpElement-1);

			GUILayout.EndVertical();
		}

		#endregion

		#region Toggle

		public static int ToggleToolbar( int Index, string[] Options )
		{
			GUILayout.BeginHorizontal();
			for (int i=0; i<Options.Length; ++i)
			{
				if ( GUILayout.Toggle(Index==i, Options[i], EditorStyles.toolbarButton)) 
					Index=i;
			}
			GUILayout.EndHorizontal();
			return Index;
		}

		public static void ToggleToolbar( SerializedProperty EnumVar )
		{
			int index = ToggleToolbar( EnumVar.enumValueIndex, EnumVar.enumNames);
			if (EnumVar.enumValueIndex != index)
				EnumVar.enumValueIndex = index;
		}

		#endregion

		#region Misc
		
		public static bool ObjectExistInScene( GameObject Obj )
		{
			//if (Obj.transform.root != Obj.transform)
			//	continue;
			
			// We are only interested in GameObjects that are visible in the Hierachy panel and are persitent 
			if ((Obj.hideFlags & (HideFlags.DontSave|HideFlags.HideInHierarchy)) > 0)
				return false;
			
			// We are not interested in Prefab, unless they are Prefab Instances
			PrefabType pfType = PrefabUtility.GetPrefabType(Obj);
			if(pfType == PrefabType.Prefab || pfType == PrefabType.ModelPrefab)
				return false;
			
			// If the database contains the object then its not an scene object, 
			// but the previous test should get rid of them, so I will just comment this 
			// unless an false positive object is found in the future
			/*if (AssetDatabase.Contains(Obj))
					return false;*/
			
			return true;
		}

		public static IEnumerable<GameObject> SceneRoots()
		{
			var prop = new HierarchyProperty(HierarchyType.GameObjects);
			var expanded = new int[0];
			while (prop.Next(expanded)) {
				yield return prop.pptrValue as GameObject;
			}
		}
		
		public static List<GameObject> SceneRootsList()
		{
			return new List<GameObject>(SceneRoots());
		}
		
		public static IEnumerable<Transform> AllSceneObjects()
		{
			var queue = new Queue<Transform>();
			
			foreach (var root in SceneRoots()) {
				var tf = root.transform;
				yield return tf;
				queue.Enqueue(tf);
			}
			
			while (queue.Count > 0) {
				foreach (Transform child in queue.Dequeue()) {
					yield return child;
					queue.Enqueue(child);
				}
			}
		}

		public static string GetScenePath(Transform tr)
		{
			if (tr==null)
				return string.Empty;
			
			string path = tr.name;
			while (tr.parent != null)
			{
				tr = tr.parent;
				path = tr.name + "/" + path;
			}
			return path;
		}

		public static Transform FindObjectInEditor( string scenePath )
		{
			if (string.IsNullOrEmpty(scenePath))
				return null;

			int index = scenePath.IndexOfAny("/\\".ToCharArray());
			string first = index<0 ? scenePath : scenePath.Substring(0, index);

			foreach (var root in AllSceneObjects())
				if (root.name==first)
				{
					if (index<0) 
						return root;

					return root.Find(scenePath.Substring(index+1));
				}
			return null;
		}


		public static GUIContent Icon_Help { 
			get{
				if (mIconHelp == null)
					mIconHelp = EditorGUIUtility.IconContent("_Help");
				return mIconHelp;
			}
		}
		static GUIContent mIconHelp;

		#endregion

		#region Angle Drawer
		private static Vector2 mAngle_lastMousePosition;
		static Texture mAngle_TextureCircle;
		static Texture pAngle_TextureCircle { 
			get{ 
				if (mAngle_TextureCircle) return mAngle_TextureCircle;  
				mAngle_TextureCircle = GUI.skin.GetStyle("CN CountBadge").normal.background;
				return mAngle_TextureCircle;
			}
		}
		
		public static float FloatAngle(Rect rect, float value)
		{
			return FloatAngle(rect, value, -1, -1, -1);
		}
		
		public static float FloatAngle(Rect rect, float value, float snap)
		{
			return FloatAngle(rect, value, snap, -1, -1);
		}
		
		public static float FloatAngle(Rect rect, float value, float snap, float min, float max)
		{
			int id = GUIUtility.GetControlID(FocusType.Passive, rect);
			
			Rect knobRect = new Rect(rect.x, rect.y, rect.height, rect.height);
			
			float delta;
			if (min != max)
				delta = ((max - min) / 360);
			else
				delta = 1;
			
			if (Event.current != null)
			{
				if (Event.current.type == EventType.MouseDown && knobRect.Contains(Event.current.mousePosition))
				{
					GUIUtility.hotControl = id;
					mAngle_lastMousePosition = Event.current.mousePosition;
				}
				else if (Event.current.type == EventType.MouseUp && GUIUtility.hotControl == id)
					GUIUtility.hotControl = -1;
				else if (Event.current.type == EventType.MouseDrag && GUIUtility.hotControl == id)
				{
					Vector2 move = mAngle_lastMousePosition - Event.current.mousePosition;
					value += delta * (-move.x - move.y);
					
					if (snap > 0)
					{
						float mod = value % snap;
						
						if (mod < (delta * 3) || Mathf.Abs(mod - snap) < (delta * 3))
							value = Mathf.Round(value / snap) * snap;
					}
					
					mAngle_lastMousePosition = Event.current.mousePosition;
					GUI.changed = true;
				}
			}

			GUI.DrawTexture(knobRect, pAngle_TextureCircle);
			Matrix4x4 matrix = GUI.matrix;
			
			if (min != max)
				GUIUtility.RotateAroundPivot(value * (360 / (max - min)), knobRect.center);
			else
				GUIUtility.RotateAroundPivot(value, knobRect.center);

			knobRect.height = 5;
			knobRect.width = 5;
			GUI.DrawTexture(knobRect, pAngle_TextureCircle);
			GUI.matrix = matrix;
			
			Rect label = new Rect(rect.x + rect.height, rect.y + (rect.height / 2) - 9, rect.height, 18);
			value = EditorGUI.FloatField(label, value);
			
			if (min != max)
				value = Mathf.Clamp(value, min, max);
			
			return value;
		}

		public static float AngleCircle(Rect rect, float angle, float snap, float min, float max, Texture background=null, Texture knobLine=null)
		{
			Rect knobRect = new Rect(rect.x, rect.y, rect.height, rect.height);
			
			float delta;
			if (min != max)
				delta = ((max - min) / 360);
			else
				delta = 1;

			if (Event.current != null && GUIUtility.hotControl<=0 &&  (Event.current.type==EventType.MouseDown || Event.current.type==EventType.MouseDrag) && knobRect.Contains(Event.current.mousePosition))
			{
				angle = Vector2.Angle( Vector2.right, Event.current.mousePosition-knobRect.center);
				if (Event.current.mousePosition.y<knobRect.center.y) angle = 360-angle;
				if (Event.current.alt || Event.current.control)
					snap = 5;
				if (snap > 0)
				{
					float mod = Mathf.Repeat(angle, snap);
					
					if (mod < (delta * 3) || Mathf.Abs(mod - snap) < (delta * 3))
						angle = Mathf.Round(angle / snap) * snap;
				}
				
				GUI.changed = true;
			}

			if (background==null) background = pAngle_TextureCircle;
			GUI.DrawTexture (knobRect, background);

			Matrix4x4 matrix = GUI.matrix;
			
			if (min != max)
				GUIUtility.RotateAroundPivot(angle * (360 / (max - min))+90, knobRect.center);
			else
				GUIUtility.RotateAroundPivot(angle+90, knobRect.center);

			float Radius = Mathf.Min (knobRect.width, knobRect.height) * 0.5f;
			knobRect.x = knobRect.x + 0.5f * knobRect.width - 4;
			knobRect.y += 2;
			knobRect.width = 8;
			knobRect.height = Radius+2;
			if (knobLine == null)
				knobLine = GUI.skin.FindStyle ("MeBlendPosition").normal.background;
			GUI.DrawTexture(knobRect, knobLine);
			GUI.matrix = matrix;
			
			return Mathf.Repeat(angle, 360);
		}
        #endregion

		#region Unity Version branching

		public static string Editor_GetCurrentScene()
		{
			#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
				return EditorApplication.currentScene;
			#else 
				return UnityEngine.SceneManagement.SceneManager.GetActiveScene().path;
			#endif
		}

        public static void Editor_MarkSceneDirty()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
#else
			EditorApplication.MarkSceneDirty();
            #endif
        }

        public static void Editor_SaveScene()
		{
			#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			EditorApplication.SaveScene ();
			#else
			UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
			#endif
		}

		public static void Editor_OpenScene( string sceneName )
		{
			#if UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
			EditorApplication.OpenScene( sceneName );
			#else
			UnityEditor.SceneManagement.EditorSceneManager.OpenScene(sceneName);
			#endif
		}

		#endregion

    }
}
