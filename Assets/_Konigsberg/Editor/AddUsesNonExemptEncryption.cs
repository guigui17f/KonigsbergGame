#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class AddUsesNonExemptEncryption
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(pathToBuiltProject + "/Info.plist");
            plist.root.SetBoolean("ITSAppUsesNonExemptEncryption", false);
            plist.WriteToFile(pathToBuiltProject + "/Info.plist");
        }
    }
}
#endif