using UnityEditor;
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace I2.Loc
{
	public partial class LocalizationEditor
	{
		#region Variables

		const string ValidChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_";
		const string NumberChars = "0123456789";

		int Script_Tool_MaxVariableLength = 50;

		#endregion
		
		#region GUI Generate Script
		
		void OnGUI_Tools_Script()
		{
			OnGUI_KeysList (false, 200, false);

			//GUILayout.Space (5);
			
			GUI.backgroundColor = Color.Lerp (Color.gray, Color.white, 0.2f);
			GUILayout.BeginVertical("AS TextArea", GUILayout.Height(1));
			GUI.backgroundColor = Color.white;
			
			EditorGUILayout.HelpBox("This tool creates the ScriptLocalization.cs with the selected terms.\nThis allows for Compile Time Checking on the used Terms referenced in scripts", MessageType.Info);
			
			GUILayout.Space (5);

			GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace();
				EditorGUIUtility.labelWidth = 240;
				EditorGUILayout.IntField("Max Length of the Generated Term IDs:", Script_Tool_MaxVariableLength);
				EditorGUIUtility.labelWidth = 0;
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.Space (10);
			
			GUILayout.BeginHorizontal ();
				GUILayout.FlexibleSpace();
                if (GUILayout.Button(new GUIContent("Select Baked Terms", "Selects all the terms previously built in ScriptLocalization.cs")))
                    SelectTermsFromScriptLocalization();

				if (GUILayout.Button("Build Script with Selected Terms"))
					EditorApplication.update += BuildScriptWithSelectedTerms;
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
		}

        void SelectTermsFromScriptLocalization()
        {
            var ScriptFile = GetPathToGeneratedScriptLocalization();
            var text = System.IO.File.ReadAllText(ScriptFile, Encoding.UTF8);

            mSelectedKeys.Clear();
            foreach (Match match in Regex.Matches(text, "\".+\"") )
            {
                var term = match.Value.Substring(1, match.Value.Length-2);

                mSelectedKeys.Add(term);
            }
        }

		#endregion

		#region Generate Script File
		public static string mScriptLocalizationHeader = 
@"// This class is Auto-Generated by the Script Tool in the Language Source
using UnityEngine;

namespace I2.Loc
{
	public static class ScriptLocalization
	{
		public static string Get(string Term) { return LocalizationManager.GetTermTranslation(Term, LocalizationManager.IsRight2Left, 0, false); }
		public static string Get(string Term, bool FixForRTL) { return LocalizationManager.GetTermTranslation(Term, FixForRTL, 0, false); }
		public static string Get(string Term, bool FixForRTL, int maxLineLengthForRTL) { return LocalizationManager.GetTermTranslation(Term, FixForRTL, maxLineLengthForRTL, false); }
		public static string Get(string Term, bool FixForRTL, int maxLineLengthForRTL, bool ignoreNumbers) { return LocalizationManager.GetTermTranslation(Term, FixForRTL, maxLineLengthForRTL, ignoreNumbers); }

";


		void BuildScriptWithSelectedTerms()
		{
			EditorApplication.update -= BuildScriptWithSelectedTerms;
			var sb = new StringBuilder();
			sb.Append(mScriptLocalizationHeader);

			BuildScriptWithSelectedTerms( sb );

			sb.AppendLine();
			sb.AppendLine("	}");
			sb.AppendLine("}");


			string ScriptFile = GetPathToGeneratedScriptLocalization ();
			Debug.Log ("Generating: " + ScriptFile);

			System.IO.File.WriteAllText(ScriptFile, sb.ToString(), Encoding.UTF8);
			AssetDatabase.ImportAsset(ScriptFile);
		}

		static string GetPathToGeneratedScriptLocalization()
		{
			string[] assets = AssetDatabase.FindAssets("ScriptLocalization");
			if (assets.Length==0)
				return UpgradeManager.GetI2Path() + "/ScriptLocalization.cs";

			string FilePath = AssetDatabase.GUIDToAssetPath(assets[0]);
			return FilePath;
		}

		void BuildScriptWithSelectedTerms( StringBuilder sb )
		{
			List<string> Categories = LocalizationManager.GetCategories();
			foreach (string Category in Categories)
			{
				List<string> CategoryTerms = ScriptTool_GetSelectedTermsInCategory(Category);
				if (CategoryTerms.Count<=0)
					continue;

				List<string> AdjustedCategoryTerms = new List<string>(CategoryTerms);
				for (int i=0, imax=AdjustedCategoryTerms.Count; i<imax; ++i)
					AdjustedCategoryTerms[i] = ScriptTool_AdjustTerm( AdjustedCategoryTerms[i] );
				ScriptTool_EnumerateDuplicatedTerms(AdjustedCategoryTerms);
				
				sb.AppendLine();
				if (Category != LanguageSource.EmptyCategory)
				{
					sb.AppendLine("		public static class " + ScriptTool_AdjustTerm(Category,true));
					sb.AppendLine("		{");
				}

				BuildScriptCategory( sb, Category, AdjustedCategoryTerms, CategoryTerms );

				if (Category != LanguageSource.EmptyCategory)
				{
					sb.AppendLine("		}");
				}
			}
		}

		List<string> ScriptTool_GetSelectedTermsInCategory( string Category )
		{
			List<string> list = new List<string>();
			foreach (string FullKey in mSelectedKeys)
			{
				string categ =  LanguageSource.GetCategoryFromFullTerm(FullKey);
				if (categ == Category && ShouldShowTerm(FullKey))
				{
					list.Add(  LanguageSource.GetKeyFromFullTerm(FullKey) );
				}
			}

			return list;
		}

		void BuildScriptCategory( StringBuilder sb, string Category, List<string> AdjustedTerms, List<string> Terms )
		{
			if (Category==LanguageSource.EmptyCategory)
			{
				for (int i=0; i<Terms.Count; ++i)
					sb.AppendLine("		public static string "+AdjustedTerms[i]+" \t\t{ get{ return Get (\""+Terms[i]+"\"); } }");
			}
			else
			for (int i=0; i<Terms.Count; ++i)
			{
				sb.AppendLine("			public static string "+AdjustedTerms[i]+" \t\t{ get{ return Get (\""+Category+"/"+Terms[i]+"\"); } }");
			}
		}

		string ScriptTool_AdjustTerm( string Term, bool allowFullLength = false )
		{
			// C# IDs can't start with a number
			if (NumberChars.IndexOf(Term[0])>=0)
				Term = "_"+Term;
			
			if (!allowFullLength && Term.Length>Script_Tool_MaxVariableLength)
				Term = Term.Substring(0, Script_Tool_MaxVariableLength);
			
			// Remove invalid characters
			char[] chars = Term.ToCharArray();
			for (int i=0, imax=chars.Length; i<imax; ++i)
				if (ValidChars.IndexOf(chars[i])<0)
					chars[i]='_';
			return new string(chars);
		}

		void ScriptTool_EnumerateDuplicatedTerms(List<string> AdjustedTerms)
		{
			string lastTerm = "$";
			int Counter = 1;
			for (int i=0, imax=AdjustedTerms.Count; i<imax; ++i)
			{
				string currentTerm = AdjustedTerms[i];
				if (lastTerm == currentTerm || (i<imax-1 && currentTerm==AdjustedTerms[i+1]))
				{
					AdjustedTerms[i] = AdjustedTerms[i] + "_" + Counter;
					Counter++;
				}
				else
					Counter = 1;

				lastTerm = currentTerm;
			}
		}

		#endregion
	}
}