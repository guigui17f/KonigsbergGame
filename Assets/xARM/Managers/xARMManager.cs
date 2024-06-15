#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using xARM;


public static class xARMManager {
	
	#region Fields
	private static xARMConfig config;
	
	// Ref to the xARMProxy
	private static GameObject myxARMProxyGO;
	private static xARMProxy myxARMProxy;
	
	// Refs to xARM windows
	private static xARMPreviewWindow _myxARMPreviewWindow;
	private static xARMGalleryWindow _myxARMGalleryWindow;
	// Window states
	public static bool PreviewWindowIsAlive = false;
	public static bool GalleryWindowIsAlive = false;
	// helper
	private static double previewHeartbeat;
	private static double galleryHeartbeat;
	private static double checkedPreviewHeartbeat;
	private static double checkedGalleryHeartbeat;
	
	// Ref to GameView
	private static EditorWindow _gameView;
	public static Vector2 GameViewToolbarOffset = new Vector2(0,17);
	public static int YScrollOffset = 5;
	private static Vector2 defaultGameViewResolution;
	
	// was the list of SCs changed?
	public static bool AvailScreenCapsChanged = false;
	public static xARMScreenCap UpdatingScreenCap;

	// time of last change in editor
	private static double lastChangeInEditorTime = 0;
	private static double lastAllScreenCapsUpdatedTime = 0;

	// states
	public static bool GalleryIsUpdating = false;
	public static bool PreviewIsUpdating = false;
	public static bool ScreenCapUpdateInProgress = false; // true from trigger SC-update, wait x frames, until read SC from screen
	public static bool FinalizeScreenCapInProgress = false; // true after all SCs are updated, while GV changes to default resolution
	private  static bool skipNextUpdate = false;
	private static string currentScene = GetCurrentScene();
	public static bool HideGameViewToggle = false;



	// Delegate to run code before/after ScreenCap update
	public static OnStartScreenCapUpdateDelegate OnStartScreenCapUpdate;
	public static OnPreScreenCapUpdateDelegate OnPreScreenCapUpdate;
	public static OnPostScreenCapUpdateDelegate OnPostScreenCapUpdate;
	public static OnFinalizeScreenCapUpdateDelegate OnFinalizeScreenCapUpdate;
	
	// Delegates
	public delegate void OnStartScreenCapUpdateDelegate();
	public delegate void OnPreScreenCapUpdateDelegate();
	public delegate void OnPostScreenCapUpdateDelegate();
	public delegate void OnFinalizeScreenCapUpdateDelegate();
	#endregion
	
	#region Properties
	public static xARMConfig Config{
		get {
			if(config == null){
				config = xARMConfig.InitOrLoad ();
			}
			return config;
		}
	}
	
	public static List<xARMScreenCap> AvailScreenCaps{
		get {return Config.AvailScreenCaps;}
		set {Config.AvailScreenCaps = value;}
	}

	public static List<xARMScreenCap> ActiveScreenCaps{
		get {
			List<xARMScreenCap> activeScreenCaps = new List<xARMScreenCap>();
			
			foreach(xARMScreenCap currScreenCap in AvailScreenCaps){
				if(currScreenCap.OrientationIsActive && currScreenCap.Enabled) activeScreenCaps.Add (currScreenCap);
			}
			
			return activeScreenCaps;
		}
	}
	
	public static string[] ScreenCapList{
		get {
			string[] longNameList = new string[ActiveScreenCaps.Count];
			for(int x = 0; x < ActiveScreenCaps.Count; x++){
				longNameList[x] = ActiveScreenCaps[x].LongName;
			}
			return longNameList;
		}
	}
	
	public static xARMProxy Proxy{
		get {return myxARMProxy;}
	}
	
	public static GameObject ProxyGO{
		get {return myxARMProxyGO;}
	}
	
	public static EditorWindow GameView{
		get {
			// get Ref if necessary
			if(!_gameView){
				foreach(EditorWindow curr in Resources.FindObjectsOfTypeAll(typeof(EditorWindow))){
#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3|| UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0
					if(curr.title == "UnityEditor.GameView") _gameView = curr;
#else
					if(curr.titleContent.text == "Game") _gameView = curr;
#endif
				}
			}
			
			return _gameView;
		}
	}
		
	private static xARMPreviewWindow myxARMPreviewWindow{
		get {
			// get Ref if necessary
			if(!_myxARMPreviewWindow){
				foreach(xARMPreviewWindow curr in Resources.FindObjectsOfTypeAll(typeof(xARMPreviewWindow))){
					_myxARMPreviewWindow = curr;
				}
			}
			
			return _myxARMPreviewWindow;
		}
	}
	
	private static xARMGalleryWindow myxARMGalleryWindow{
		get {
			// get Ref if necessary
			if(!_myxARMGalleryWindow){
				foreach(xARMGalleryWindow curr in Resources.FindObjectsOfTypeAll(typeof(xARMGalleryWindow))){
					_myxARMGalleryWindow = curr;
				}
			}
			
			return _myxARMGalleryWindow;
		}
	}
	
	private static Rect currentGameViewRect{
		get {
			return GameView.position;
		}
		set {
			GameView.position = value;
		}
	}

	public static Vector2 CurrentGameViewPosition{
		get{
			return new Vector2(currentGameViewRect.xMin, currentGameViewRect.yMin);
		}
	}

	public static Vector2 DefaultGameViewResolution{
		get {
			if(xARMManager.Config.GameViewInheritsPreviewSize && defaultGameViewResolution.x >= 100 && defaultGameViewResolution.y >= 100){
				return defaultGameViewResolution;
			} else {
				return xARMManager.Config.FallbackGameViewSize;
			}
		}
		set {defaultGameViewResolution = value;}
	}

	// skip Update()-Events caused by xARM (+ GameView has focus)
	public static bool ExecuteUpdate{
		get{
			// execute this update?
			if(
				!skipNextUpdate &&
				!ScreenCapUpdateInProgress &&
				!FinalizeScreenCapInProgress &&
				(!GameViewHasFocus || (GameViewHasFocus && xARMManager.Config.UpdatePreviewWhileGameViewHasFocus))
				)
			{ // use update
				skipNextUpdate = false;
				return true;

			} else { // skip scene change
				skipNextUpdate = false;
				return false;
			}
		}
	}

	// used by other assets
	public static bool UpdateInProgress{
		get{
			if(GalleryIsUpdating == false && PreviewIsUpdating == false && ScreenCapUpdateInProgress == false && FinalizeScreenCapInProgress == false){
				return false;
			} else {
				return true;
			}
		}
	}

	// used to check if other assets are updating
	public static bool OtherUpdateInProgress{
		get{
			// what asset exist and is it updating?
			bool xCBMIsUpdating = GetUpdateInProgress("xCBMManager", "UpdateInProgress");

			if(xCBMIsUpdating){
				skipNextUpdate = true; // skip updates caused by other assets

				return true;
			} else {
				return false;
			}
		}

	}


	public static bool GameViewHasFocus{
		get {
			if(GameView){
				return GameView.Equals(EditorWindow.focusedWindow);
			} else {
				return false;
			}
		}
	}

	// returns Editor's current mode
	public static EditorMode CurrEditorMode{
		get {
			if(!EditorApplication.isPlaying){ // Edit
				return EditorMode.Edit;

			} else if(EditorApplication.isPlaying && EditorApplication.isPaused){ // Pause
				return EditorMode.Pause;

			} else if(EditorApplication.isPlaying && !EditorApplication.isPaused){ // Play
				return EditorMode.Play;

			} else {
				return EditorMode.Other;

			}
		}
	}
	#endregion
	
	#region Functions
	static xARMManager() {
		InitAvailScreenCaps ();
	}

	#region Init
	private static void InitAvailScreenCaps(){
		
		// List of all default Resolutions/Aspect Ratios (ScreenCaps)
		// iOS
		addOrUpdateScreenCap (new xARMScreenCap("iPhone", 		new Vector2(1136, 640), "WDVGA","16:9~", 	4.0f, 326, "@2x", 		xARMScreenCapGroup.iOS, 1, 29.5f, "iPhone SE, 5S, 5C, 5 & iPod touch 6., 5."));
		addOrUpdateScreenCap (new xARMScreenCap("iPhone", 		new Vector2(960, 640), "DVGA", 	"", 		3.5f, 326, "@2x", 		xARMScreenCapGroup.iOS, 2, 28.3f, "iPhone 4S, 4 & iPod touch 4."));
		addOrUpdateScreenCap (new xARMScreenCap("iPad", 		new Vector2(1024, 768), "XGA",	"", 		9.7f, 132, "@1x", 		xARMScreenCapGroup.iOS, 3, 23.6f, "iPad 2., 1."));
		addOrUpdateScreenCap (new xARMScreenCap("iPad mini", 	new Vector2(1024, 768), "XGA",	"", 		7.9f, 163, "@1x", 		xARMScreenCapGroup.iOS, 3, 23.6f, "iPad mini"));
		addOrUpdateScreenCap (new xARMScreenCap("iPad", 		new Vector2(2048, 1536), "QXGA","", 		9.7f, 264, "@2x", 		xARMScreenCapGroup.iOS, 4, 16.1f, "iPad 4., 3., Pro (9.7), Air 2, Air"));
		addOrUpdateScreenCap (new xARMScreenCap("iPad mini", 	new Vector2(2048, 1536), "QXGA","", 		7.9f, 326, "@2x", 		xARMScreenCapGroup.iOS, 4, 16.1f, "iPad mini 4, mini 3, mini 2"));
		addOrUpdateScreenCap (new xARMScreenCap("iPhone", 		new Vector2(480, 320), "HVGA",	"",			3.5f, 163, "@1x", 		xARMScreenCapGroup.iOS, 5, 1.3f	, "iPhone 3GS, 3G, 1 & iPod touch 3., 2., 1."));
		addOrUpdateScreenCap (new xARMScreenCap("iPhone", 		new Vector2(1334, 750), "custom", "16:9~", 	4.7f, 326, "@2x", 		xARMScreenCapGroup.iOS, 999, -1f, "iPhone 6S, 6"));
		addOrUpdateScreenCap (new xARMScreenCap("iPhone", 		new Vector2(1920, 1080), "FHD", "", 		5.5f, 401, "@2.6x",		xARMScreenCapGroup.iOS, 999, -1f, "iPhone 6S Plus, 6 Plus (Unity uses native resolution)"));
		addOrUpdateScreenCap (new xARMScreenCap("iPhone", 		new Vector2(2208, 1242), "custom", "", 		5.5f,   0, "@3x", 		xARMScreenCapGroup.iOS, 999, -1f, "Probable future iPhone resolution"));
		addOrUpdateScreenCap (new xARMScreenCap("iPad Pro", 	new Vector2(2732, 2048), "custom", "4:3~", 12.9f, 264, "@2x", 		xARMScreenCapGroup.iOS, 999, -1f, "iPad Pro (12.9)"), new xARMScreenCap(new Vector2(2732, 2048), 12.22f, xARMScreenCapGroup.iOS)); 

		// Android 
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(800, 480), "WVGA", "", 		3.7f, 0, "", 	xARMScreenCapGroup.Android,  1,24.8f, "Nexus S (4), Nexus One (3.7), Galaxy S2 (4.3), ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 720), "HD", "", 		4.7f, 0, "", 	xARMScreenCapGroup.Android,  2,17.0f, "Galaxy Nexus, Fire Phone, Galaxy Note 2 (5.5), ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1920, 1080), "FHD","", 		5.0f, 0, "", 	xARMScreenCapGroup.Android,  3,10.7f, "Nexus 5X (5.2), 5 (4.96), Galaxy S5 (5.1), S4 (5), ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 800), "WXGA", "", 	7.0f, 0, "", 	xARMScreenCapGroup.Android,  4, 9.4f, "Nexus 7, Kindle Fire HD, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 800), "WXGA","", 	   10.1f, 0, "", 	xARMScreenCapGroup.Android,  4, 9.4f, "Galaxy Tab 2 10.1, Galaxy Tab 10.1, Galaxy Note 10.1, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1024, 600), "WSVGA","5:3~", 7.0f, 0, "", 	xARMScreenCapGroup.Android,  5, 9.3f, "Galaxy Tab, Kindle Fire, Nook Tablet, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(480, 320), "HVGA","", 		3.2f, 0, "", 	xARMScreenCapGroup.Android,  6, 7.6f, "Galaxy, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(960, 540), "qHD","", 		4.3f, 0, "", 	xARMScreenCapGroup.Android,  7, 6.9f, "Sensation, Droid Razr, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(854, 480), "FWVGA","16:9~", 4.0f, 0, "", 	xARMScreenCapGroup.Android,  8, 6.1f, "Xperia Play, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(320, 240), "QVGA","", 		3.1f, 0, "", 	xARMScreenCapGroup.Android,  9, 2.8f, "Galaxy Mini, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1024, 768), "XGA","", 		8.0f, 0, "", 	xARMScreenCapGroup.Android, 10, 2.1f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1024, 768), "XGA","", 	   10.0f, 0, "", 	xARMScreenCapGroup.Android, 10, 2.1f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1920, 1200), "WUXGA","", 	8.9f, 0, "", 	xARMScreenCapGroup.Android, 11, 1.0f, "Kindle Fire HD 8.9, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1920, 1200), "WUXGA","", 	7.0f, 0, "", 	xARMScreenCapGroup.Android, 11, 1.0f, "Nexus 7 2., ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 768), "WXGA","", 		4.7f, 0, "", 	xARMScreenCapGroup.Android, 12, 0.5f, "Nexus 4, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1440, 900), "WXGA+","", 	7.0f, 0, "", 	xARMScreenCapGroup.Android, 13, 0.3f, "Nook HD, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(2560, 1600), "WQXGA","",   10.1f, 0, "", 	xARMScreenCapGroup.Android, 14, 0.2f, "Nexus 10, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(800, 600), "SVGA","", 		7.0f, 0, "", 	xARMScreenCapGroup.Android, 15, 0.2f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(640, 360), "nHD","", 		3.5f, 0, "", 	xARMScreenCapGroup.Android, 16, 0.1f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1800, 1080), "custom","", 	5.1f, 0, "", 	xARMScreenCapGroup.Android, 17, 0.1f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1066, 600), "custom","", 	7.6f, 0, "", 	xARMScreenCapGroup.Android, 18, 0.1f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1920, 1280), "custom","", 	9.0f, 0, "", 	xARMScreenCapGroup.Android, 19, 0.1f, "Nook HD+, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(2048, 1536), "QXGA","", 	8.9f, 0, "", 	xARMScreenCapGroup.Android, 20, 0.1f, "Nexus 9..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(2048, 1536), "QXGA","", 	9.7f, 0, "", 	xARMScreenCapGroup.Android, 20, 0.1f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(960, 640), "DVGA","", 		4.0f, 0, "", 	xARMScreenCapGroup.Android, 21, 0.1f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1600, 900), "HD+","", 	   13.0f, 0, "", 	xARMScreenCapGroup.Android, 22, 0.1f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1024, 576), "WSVGA","",    10.0f, 0, "", 	xARMScreenCapGroup.Android, 23, 0.0f, "...")); // pos 24, but pos 23 is 0x0
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(2560, 1440), "QHD","",	   6.0f, 0, "", 	xARMScreenCapGroup.Android, 999, -1f, "Nexus 6P (5.7), 6 (5.96), Galaxy Note 4 (5.7)..."));

		// Windows Phone 8
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(800, 480), "WVGA","", 		4.0f, 0, "",	xARMScreenCapGroup.WinPhone8, 1,80.9f, "Lumia 620 (3.8), 520, 525 (4.0), 720, 820 (4.3), 625 (4.7), ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 768), "WXGA","", 		4.5f, 0, "",	xARMScreenCapGroup.WinPhone8, 2,14.3f, "Lumia 920, 925, 928, 1020, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 720), "HD","", 		4.8f, 0, "",	xARMScreenCapGroup.WinPhone8, 3, 4.8f, "ATIV S, S Neo, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 720), "HD","", 		6.0f, 0, "",	xARMScreenCapGroup.WinPhone8, 3, 4.8f, "Lumia 1320, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1920, 1080), "FHD","", 		5.0f, 0, "",	xARMScreenCapGroup.WinPhone8, 4, 0.0f, "Lumia Icon, 930, ATIV SE, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1920, 1080), "FHD","", 		6.0f, 0, "",	xARMScreenCapGroup.WinPhone8, 4, 0.0f, "Lumia 1520, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(854, 480), "FWVGA","16:9~", 4.5f, 0, "",	xARMScreenCapGroup.WinPhone8, 5, 0.0f, "Lumia 630, 635, ..."));

		// WinRT
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1366, 768), "WXGA","16:9~", 10.6f, 0, "", 	xARMScreenCapGroup.WindowsRT, 1,83.3f, "Surface (10.6), ATIV Tab (10.1), ...")); // pos 1 & 3 are same res
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1366, 768), "WXGA","16:9~", 11.6f, 0, "", 	xARMScreenCapGroup.WindowsRT, 1,83.3f, "IdeaPad Yoga 11, ...")); // pos 1 & 3 are same res
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1920, 1080), "FHD","", 		10.6f, 0, "", 	xARMScreenCapGroup.WindowsRT, 2,14.5f, "Surface 2 (10.6), Lumia 2520 (10.1), ..."));

		// Blackberry
		
		// Standalone
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1920, 1080), "FHD","", 			40, 0, "", xARMScreenCapGroup.Standalone,  1,29.6f, "TVs, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1366, 768), "WXGA","16:9~", 	14, 0, "", xARMScreenCapGroup.Standalone,  2,20.9f, "Notebooks, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1440, 900), "WXGA+","", 		19, 0, "", xARMScreenCapGroup.Standalone,  3, 8.8f, "Desktop Monitors, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1600, 900), "HD+","", 			15, 0, "", xARMScreenCapGroup.Standalone,  4, 7.2f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 1024), "SXGA","", 		17, 0, "", xARMScreenCapGroup.Standalone,  5, 6.8f, "Desktop Monitors, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1680, 1050), "WSXGA+","", 		15, 0, "", xARMScreenCapGroup.Standalone,  6, 6.3f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1024, 768), "XGA","", 			14, 0, "", xARMScreenCapGroup.Standalone,  7, 5.3f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 800), "WXGA","", 			14, 0, "", xARMScreenCapGroup.Standalone,  8, 5.2f, "Notebooks, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1360, 768), "WXGA","16:9~", 	14, 0, "", xARMScreenCapGroup.Standalone,  9, 2.7f, "Notebooks, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1920, 1200), "WUXGA","", 		24, 0, "", xARMScreenCapGroup.Standalone, 10, 1.8f, "Desktop Monitors, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(2560, 1440), "QHD","", 			27, 0, "", xARMScreenCapGroup.Standalone, 11, 1.2f, "Desktop Monitors, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 768), "WXGA","", 			14, 0, "", xARMScreenCapGroup.Standalone, 12, 0.8f, "Notebooks, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 720), "HD","", 			32, 0, "", xARMScreenCapGroup.Standalone, 13, 0.7f, "TVs, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1280, 960), "SXGA-","", 		17, 0, "", xARMScreenCapGroup.Standalone, 14, 0.6f, "..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1152, 864), "XGA+","", 			17, 0, "", xARMScreenCapGroup.Standalone, 15, 0.5f, "Desktop Monitors, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1600, 1200), "UXGA","", 		20, 0, "", xARMScreenCapGroup.Standalone, 16, 0.3f, "Desktop Monitors, ..."));
		addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1024, 600), "WSVGA","16:9~", 	10, 0, "", xARMScreenCapGroup.Standalone, 17, 0.2f, "Netbooks, ..."));


        // Custom
        /* How to add custom ScreenCaps:
		 * Every line adds one (landscape) ScreenCap. Portaits are created automatically.
		 * DPI, etc. are set automatically if not set manually.
		 * Use "xARMScreenCapGroup.Custom" to put new SreenCaps into the Custom categorie in Options.
		 * Don't change existing lines and ensure to make a backup of your added lines. After updating xARM the additons need to be inserted again.
		 * 
		 * Example (1920x1080 Full-HD 42"):
		 */
        // addOrUpdateScreenCap (new xARMScreenCap("", new Vector2(1920, 1080), "","", 42, 0, "", xARMScreenCapGroup.Custom, 999, -1f, "Description"));
        addOrUpdateScreenCap(new xARMScreenCap("custom iPhone", new Vector2(800, 450), "", "", 2.8f, 0, "", xARMScreenCapGroup.Custom, 999, -1f, "custom iPhone 6S, 6"));
         
        // create portrait ScreenCaps
        List<xARMScreenCap> portraitScreenCaps = new List<xARMScreenCap>();
		foreach(xARMScreenCap currScreenCap in AvailScreenCaps){
			if(currScreenCap.Group != xARMScreenCapGroup.Standalone && currScreenCap.IsLandscape && currScreenCap.IsBaseRes) portraitScreenCaps.Add (currScreenCap.Clone (true));
		}
		// add
		foreach(xARMScreenCap currScreenCap in portraitScreenCaps){
			addOrUpdateScreenCap (currScreenCap);
		}
		portraitScreenCaps.Clear();
		

		// create Android ScreenCaps with navigation/system bar offset
		List<xARMScreenCap[]> navigationBarScreenCaps = new List<xARMScreenCap[]>();
		foreach(xARMScreenCap currScreenCap in AvailScreenCaps){
			if(currScreenCap.Group == xARMScreenCapGroup.Android && currScreenCap.IsBaseRes)
				
				navigationBarScreenCaps.Add (currScreenCap.CreateNavigationBarVersion ());
		}
		// add
		foreach(xARMScreenCap[] currScreenCap in navigationBarScreenCaps){
			addOrReplaceScreenCap (currScreenCap[0], currScreenCap[1]); //
		}
		navigationBarScreenCaps.Clear ();

		// mark loaded SC list as unchanged to prevent resave
		xARMScreenCap.ListChanged = false;
	}
	
	private static void addOrUpdateScreenCap (xARMScreenCap screenCapToAdd, xARMScreenCap screenCapToReplace = null){
		int screenCapIndex;

		// replace existing SC
		if(screenCapToReplace is xARMScreenCap){
			// find SC to replace
			screenCapIndex = getScreenCapIndex (screenCapToReplace);
			// no SC to replace > find SC to update
			if(screenCapIndex == -1) screenCapIndex = getScreenCapIndex (screenCapToAdd);

		} else {
			screenCapIndex = getScreenCapIndex (screenCapToAdd);
		}

		if(screenCapIndex >= 0){ // update (don't add duplicates)
			// values to keep
			bool origEnabledState = AvailScreenCaps[screenCapIndex].Enabled;
			
			AvailScreenCaps[screenCapIndex] = screenCapToAdd;
			AvailScreenCaps[screenCapIndex].Enabled = origEnabledState;
			
		} else { // add
			AvailScreenCaps.Add (screenCapToAdd);
		}
	}

	private static void addOrReplaceScreenCap (xARMScreenCap screenCapToAdd, xARMScreenCap screenCapToReplace){
		int screenCapIndexToReplace = getScreenCapIndex (screenCapToReplace);
		int screenCapIndexToAdd = getScreenCapIndex (screenCapToAdd);
		
		if(screenCapIndexToReplace >= 0){ // replace
			// values to keep
			bool origEnabledState = AvailScreenCaps[screenCapIndexToReplace].Enabled;
			
			AvailScreenCaps[screenCapIndexToReplace] = screenCapToAdd;
			AvailScreenCaps[screenCapIndexToReplace].Enabled = origEnabledState;
			
		} else if(screenCapIndexToAdd >= 0){ // update (don't add duplicates)
			// values to keep
			bool origEnabledState = AvailScreenCaps[screenCapIndexToAdd].Enabled;
			
			AvailScreenCaps[screenCapIndexToAdd] = screenCapToAdd;
			AvailScreenCaps[screenCapIndexToAdd].Enabled = origEnabledState;

		} else { // add
			AvailScreenCaps.Add (screenCapToAdd);
		}
	}
	
	private static int getScreenCapIndex(xARMScreenCap screenCapToCheck){
		for (int x= 0; x< AvailScreenCaps.Count; x++){
			if(AvailScreenCaps[x] == screenCapToCheck) return x;
		}
		
		return -1;
	}
	#endregion
	
	#region Proxy
	public static void CreateProxyGO (){
		// create Proxy only if needed and not while switching to play mode 
		if(!myxARMProxyGO && (EditorApplication.isPlaying || !EditorApplication.isPlayingOrWillChangePlaymode)){
						
			// create GO with components attached
			myxARMProxyGO = new GameObject("xARMProxy");
			myxARMProxy = myxARMProxyGO.AddComponent<xARMProxy> ();
			if(xARMManager.Config.IntegrationUGUIPhySize) myxARMProxyGO.AddComponent<xARMDelegateUGUI> ();

			
			if(!myxARMProxy){
				RemoveProxyGO ();
				xARMPreviewWindow.WarningBoxText = "Could not create xARMProxy. Do NOT put xARM in the Editor folder.";
				xARMGalleryWindow.WarningBoxText = "Could not create xARMProxy. Do NOT put xARM in the Editor folder.";
			}
		}
	}
	
	public static void RemoveProxyGO (){
		MonoBehaviour.DestroyImmediate (myxARMProxyGO);
	}

	public static void RecreateProxyGO (){
		RemoveProxyGO();
		CreateProxyGO();
	}

	// reset xARM values on scene switch
	public static void ResetOnSceneSwitch(){
		if (currentScene != GetCurrentScene()){
			currentScene = GetCurrentScene();
			// reset
			ScreenCapUpdateInProgress = false;
			SceneChanged ();
		}
	}
	#endregion
	
	#region ScreenCaps
	public static bool IsToUpdate(xARMScreenCap screenCap){
		if(screenCap.LastUpdateTime != lastChangeInEditorTime && screenCap.LastUpdateTryTime != lastChangeInEditorTime){
			return true;
		} else {
			return false;
		}
	}
	

	public static void UpdateScreenCap(xARMScreenCap screenCap){
		// make current ScreenCap available to delegates
		UpdatingScreenCap = screenCap;

		ResizeGameView (screenCap.Resolution);
		// run custom code
		if(OnPreScreenCapUpdate != null) OnPreScreenCapUpdate ();

		xARMManager.ScreenCapUpdateInProgress = true;

		// wait x frames to ensure correct results with other (lazy) plugins
		if(xARMManager.CurrEditorMode == EditorMode.Play){ // don't wait in Play mode
			Proxy.StartWaitXFramesCoroutine (screenCap, 0);
			
		} else {
			Proxy.StartWaitXFramesCoroutine (screenCap, xARMManager.Config.FramesToWait);
		}
	}

	public static void UpdateScreenCapAtEOF(xARMScreenCap screenCap){
		// capture Render at EndOfFrame
		Proxy.StartUpdateScreenCapCoroutine (screenCap);
		// force EndOfFrame - to execute yield
		GameView.Repaint ();
	}
	
	public static void ReadScreenCapFromGameView(xARMScreenCap screenCap){
		int width = (int)screenCap.Resolution.x;
		int height = (int)screenCap.Resolution.y;

		// check if the GameView has the correct size
		if(screenCap.Resolution.x == Screen.width && screenCap.Resolution.y == Screen.height){ 
			// read screen to Tex2D
			Texture2D screenTex = new Texture2D(width, height, TextureFormat.RGB24, false);
			screenTex.ReadPixels (new Rect(0, 0, width, height), 0, 0, false);
			screenTex.Apply (false, false); // readable to enable export as file

			// update ScreenCap
			screenCap.Texture = screenTex;
			screenCap.LastUpdateTime = lastChangeInEditorTime;
			screenCap.LastUpdateTryTime = lastChangeInEditorTime;
			screenCap.UpdatedSuccessful = true;
			
			// repaint editor windows
			if(myxARMPreviewWindow) myxARMPreviewWindow.Repaint ();
			if(myxARMGalleryWindow) myxARMGalleryWindow.Repaint ();
			
		} else {
			// mark this ScreenCap as not updated and display message
			screenCap.LastUpdateTryTime = lastChangeInEditorTime;
			screenCap.UpdatedSuccessful = false;

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3|| UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
			xARMGalleryWindow.WarningBoxText = "Could not update all ScreenCaps. Switch 'GameView' to 'Free Aspect'.";
			xARMPreviewWindow.WarningBoxText = "Could not update all ScreenCaps. Switch 'GameView' to 'Free Aspect'.";

#else	// 5.4+
			xARMGalleryWindow.WarningBoxText = "Could not update all ScreenCaps. Switch 'GameView' to 'Free Aspect' and (de)activate Retina/HiDPI mode in Options.";
			xARMPreviewWindow.WarningBoxText = "Could not update all ScreenCaps. Switch 'GameView' to 'Free Aspect' and (de)activate Retina/HiDPI mode in Options.";

#endif
		}
		
		// run custom code
		if(OnPostScreenCapUpdate != null) OnPostScreenCapUpdate ();

		UpdatingScreenCap = null;
	}
	
	public static void FinalizeScreenCapUpdate(){
		// unload unused ScreenCaps
		Resources.UnloadUnusedAssets ();

		// set default GameView size
		ResizeGameViewToDefault ();

		// LiveUpdate - remove help message if it's not longer relevant
		if(AllScreenCapsUpdatedSuccesfull()){
			xARMPreviewWindow.WarningBoxText = "";
			xARMGalleryWindow.WarningBoxText = "";
		}
		
		// run custom code
		if(OnFinalizeScreenCapUpdate != null) OnFinalizeScreenCapUpdate ();
	}

	private static void AbortUpdatePreview(){
		AbortUpdate(ref PreviewIsUpdating);
	}

	private static void AbortUpdateGallery(){
		AbortUpdate(ref GalleryIsUpdating);
	}

	private static void AbortUpdate(ref bool windowIsUpdating){
		// rest all states
		windowIsUpdating = false;
		ScreenCapUpdateInProgress = false;
		FinalizeScreenCapUpdate ();
		FinalizeScreenCapInProgress = false;
	}

	public static bool AllScreenCapsUpdatedRecently(){
		// no recent scene change?
		if(lastAllScreenCapsUpdatedTime == lastChangeInEditorTime) return false;
		
		// update still in progress?
		foreach(xARMScreenCap currScreenCap in ActiveScreenCaps){
			if(currScreenCap.LastUpdateTryTime != lastChangeInEditorTime) return false;
		}
		
		return true;
	}

	public static void SetAllScreenCapsUpdated(){
		lastAllScreenCapsUpdatedTime = lastChangeInEditorTime;
	}
	
	private static bool AllScreenCapsUpdatedSuccesfull(){
		foreach(xARMScreenCap currScreenCap in ActiveScreenCaps){
			if(!currScreenCap.UpdatedSuccessful) return false;
		}
		return true;
	}
	
	#endregion
	
	#region GameView
	// update Game View position
	public static void UpdateGameView(){
		ResizeGameView (new Vector2(Screen.width, Screen.height), true); // force update

		GameView.Focus ();
		skipNextUpdate = true;
	}

	public static void SwitchHideGameView(){
		HideGameViewToggle = !HideGameViewToggle;
		
		Config.HideGameView = HideGameViewToggle;
		UpdateGameView ();
		
		// repaint editor windows
		if(myxARMPreviewWindow) myxARMPreviewWindow.Repaint ();
		if(myxARMGalleryWindow) myxARMGalleryWindow.Repaint ();
	}


	private static void ResizeGameView(Vector2 newSize, bool force = false){

		if(xARMManager.Config.EditorUsesHIDPI){
			newSize.x /= 2;
			newSize.y /= 2;
		}

		if(!GameView) OpenGameView();

		// ensure current GV position value is correct
		SaveGameViewPosition ();

		// force is used to change the GV position without changing its resolution/size
		if(force){
			// --- FloatingGameView() ---
			Vector2 gameViewPos;
			
			// select target GV position
			if(Config.HideGameView){
				gameViewPos = Config.HiddenGameViewPosition;
			} else {
				gameViewPos = Config.GameViewPosition;
			}

			int x = Mathf.RoundToInt(gameViewPos.x);
			int y = Mathf.RoundToInt(gameViewPos.y);

			int width = Mathf.RoundToInt(currentGameViewRect.width);
			int height = Mathf.RoundToInt(currentGameViewRect.height);
			
			if(!GameView) OpenGameView();
			
			// undock GameView and set default size (doesn't work 100%)
			currentGameViewRect = new Rect(x, y, width, height);

			// skip update caused by this
			skipNextUpdate = true;
		}
		// if not forced resize only on resolution changes
		else if(newSize.x != Screen.width || newSize.y != Screen.height){ 
			// save original values
			Vector2 prevMinSize, prevMaxSize;
			prevMinSize = GameView.minSize;
			prevMaxSize = GameView.maxSize;

			// undock and resize
			FloatingGameView (newSize);
			
			// add toolbar offset
			Vector2 newSizeWithOffset = newSize + GameViewToolbarOffset;

			// ensure resize
			GameView.minSize = newSizeWithOffset;
			GameView.maxSize = newSizeWithOffset;
			
			// restore previous values (keeps GV resizable)
			GameView.minSize = prevMinSize;
			GameView.maxSize = prevMaxSize;

			// skip update caused by this
			skipNextUpdate = true;
		}
	}

	public static void ResizeGameViewToDefault(){
		ResizeGameView (xARMManager.DefaultGameViewResolution);
	}

	// ensures there is a free floating GameView window
	public static void FloatingGameView(Vector2 size){
		Vector2 gameViewPos;

		// select target GV position
		if(Config.HideGameView){
			gameViewPos = Config.HiddenGameViewPosition;
		} else {
			gameViewPos = Config.GameViewPosition;
		}

		// add offset
		size += GameViewToolbarOffset;
		
		int width = Mathf.RoundToInt(size.x);
		int height = Mathf.RoundToInt(size.y);
		int x = Mathf.RoundToInt(gameViewPos.x);
		int y = Mathf.RoundToInt(gameViewPos.y);

		if(!GameView) OpenGameView();

		// undock GameView and set default size (doesn't work 100%)
		currentGameViewRect = new Rect(x, y, width, height);
	} 

	// save GameView position when focused and not hidden
	public static void SaveGameViewPosition(){

		if(GameViewHasFocus && Config.AutoTraceGameViewPosition && !Config.HideGameView){ // trace GV position

			// workaround to not save some positions
			if(
				// workaround: 0,12 is the position while switching edit<->play mode
				CurrentGameViewPosition != new Vector2(0f, 12f) && 
				CurrentGameViewPosition != new Vector2(0f, 12f + YScrollOffset) && 
				// workaround: assume odd y-scrolling, if Game View has only moved y+yScrollOffset
				CurrentGameViewPosition != new Vector2(Config.GameViewPosition.x, Config.GameViewPosition.y + YScrollOffset) &&
				// workaround: hide GV position (fast repeated clicking on "Hide GV")
				CurrentGameViewPosition != new Vector2(Config.HiddenGameViewPosition.x, Config.HiddenGameViewPosition.y) &&
				CurrentGameViewPosition != new Vector2(Config.HiddenGameViewPosition.x, Config.HiddenGameViewPosition.y + YScrollOffset) 
			){
				// save position
				Config.GameViewPosition = CurrentGameViewPosition;
			}
		} 
		else if(!Config.AutoTraceGameViewPosition && !Config.HideGameView) { // use fixed GV position
			Config.GameViewPosition = Config.FixedGameViewPosition;
		}
	}

	public static void EnsureNextFrame (){
		// in Editor mode - fake scene change
		if(xARMManager.CurrEditorMode == EditorMode.Edit){
			if(Proxy){
				// add random rotation to fake scene change (Proxy-HideFlag has to be None)
				Proxy.gameObject.transform.rotation = Random.rotation;
			}
		}

		// in Play mode - frames are rolling by

		// in Pause mode - Step
		if(xARMManager.CurrEditorMode == EditorMode.Pause){
			EditorApplication.Step ();
		}
	}

	private static string GetCurrentScene(){
#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3|| UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
		return EditorApplication.currentScene; 
#else
		return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
#endif

	}


	public static void SceneChanged(){
		lastChangeInEditorTime = EditorApplication.timeSinceStartup;
	}

	private static void OpenGameView(){
		EditorApplication.ExecuteMenuItem ("Window/Game");
	}
	#endregion

	#region Preview and Gallery tools
	public static void RepaintAllWindows (){
		xARMPreviewWindow.RepaintNextUpdate = true;
		xARMGalleryWindow.RepaintNextUpdate = true;
	}


	public static void SendHeartbeatPreview(){
		// save heartbeat
		previewHeartbeat = EditorApplication.timeSinceStartup;
	}

	public static void SendHeartbeatGallery(){
		// save heartbeat
		galleryHeartbeat = EditorApplication.timeSinceStartup;

	}


	//check if a window was deactivated (e.g. tab is in background)
	public static void CheckHeartbeat(){
		// time of last heartbeat unchanged & was alive last time = just got missing
		if(previewHeartbeat == checkedPreviewHeartbeat && PreviewWindowIsAlive){
			PreviewWindowIsAlive = false;
			AbortUpdatePreview();

		} else if(previewHeartbeat != checkedPreviewHeartbeat) {
			PreviewWindowIsAlive = true;
			checkedPreviewHeartbeat = previewHeartbeat;
		}

		if(galleryHeartbeat == checkedGalleryHeartbeat && GalleryWindowIsAlive){
			GalleryWindowIsAlive = false;
			AbortUpdateGallery();

		} else if(galleryHeartbeat != checkedGalleryHeartbeat) {
			GalleryWindowIsAlive = true;
			checkedGalleryHeartbeat = galleryHeartbeat;
		}
	}


	// save one SC as file
	public static void SaveScreenCapFile (){
		xARMScreenCap screenCap = xARMPreviewWindow.SelectedScreenCap;

		if(screenCap.Texture.width != 4){ // not placeholder
			string defaultFileName = screenCap.Name + " " + screenCap.Diagonal + " " +screenCap.DPILabel + " " + screenCap.Resolution.x + "x" + screenCap.Resolution.y  + ".png";
			// open export to file panel
			string exportFilePath = EditorUtility.SaveFilePanel ("Export ScreenCap as PNG", xARMManager.Config.ExportPath, defaultFileName, "png");

			// export
			if(exportFilePath.Length > 0) ExportScreenCapToFile (screenCap, exportFilePath);

		} else {
			Debug.LogWarning ("xARM: ScreenCap not exported. Please update it before export.");
		}

	}

	// save all SCs as files
	public static void SaveAllScreenCapFiles (){
		// open export to folder panel
		string exportFolderPath = EditorUtility.SaveFolderPanel ("Export all ScreenCaps as PNGs (overwrites existing files)", xARMManager.Config.ExportPath, ".png");

		if(exportFolderPath.Length > 0){
			// export all SCs
			foreach(xARMScreenCap currScreenCap in ActiveScreenCaps){
				if(currScreenCap.Texture.width != 4){ // not placeholder
					string fileName = currScreenCap.Name + " " + currScreenCap.Diagonal + " " + currScreenCap.DPILabel + " " + currScreenCap.Resolution.x + "x" + currScreenCap.Resolution.y + ".png";

					// export
					ExportScreenCapToFile (currScreenCap, exportFolderPath + "/" + fileName);

				} else {
					Debug.LogWarning ("xARM: ScreenCap not exported. Please update it before export.");
				}
			}
		}
	}

	// export ScreenCap as PNG file
	private static void ExportScreenCapToFile (xARMScreenCap screenCap, string path){
		FileStream fs = new FileStream(path, FileMode.Create);
		BinaryWriter bw = new BinaryWriter(fs);
		bw.Write (screenCap.Texture.EncodeToPNG ());
		bw.Close ();
		fs.Close ();
	}

	public static void PreviewNextScreenCap (){
		if(myxARMPreviewWindow) xARMPreviewWindow.SelectNextScreencap();
	}

	public static void PreviewPreviousScreenCap (){
		if(myxARMPreviewWindow) xARMPreviewWindow.SelectPreviousScreencap();
	}

	#endregion

	#region Other assets
	// get state of other asset
	private static bool GetUpdateInProgress(string reflectType, string reflectProperty){

		System.Type type = System.Type.GetType(reflectType);
		if(type == null) return false;

		System.Reflection.PropertyInfo property = type.GetProperty(reflectProperty);
		if(property == null) return false;

		return (bool)property.GetValue(type, null);
	}

	// abort other assets update
	private static void AbortOtherUpdate(string reflectType, string reflectMethod){

		System.Type type = System.Type.GetType(reflectType);
		if(type != null){
			System.Reflection.MethodInfo method = type.GetMethod(reflectMethod);
			if(method != null){
				method.Invoke(new object[]{}, null);
			}
		}
	}
	#endregion

	#endregion
}
#endif