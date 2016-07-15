/*

所有新建文件都会自动替换头文件描述 

步骤： 
1.修改UNITY自带的脚本模板文件 Unity.app/Contents/Resources/ScriptTemplates/81-C# Script-NewBehaviourScript.cs.txt 
2.头部添加描述 //  Created by #SMARTDEVELOPERS# on #CREATIONDATE#.

Created by hcq

*/

using UnityEngine;
using UnityEditor;
using System.Collections;

public class CodeTemplate : UnityEditor.AssetModificationProcessor
{
	public static void OnWillCreateAsset (string path)
	{
		path = path.Replace (".meta", "");
		int index = path.LastIndexOf (".");
		string file = path.Substring (index);
		if (file != ".cs" && file != ".js" && file != ".boo")
			return;

		index = Application.dataPath.LastIndexOf ("Assets");
		path = Application.dataPath.Substring (0, index) + path;
		file = System.IO.File.ReadAllText (path);

		file = file.Replace ("#CREATIONDATE#", System.DateTime.Now.ToString ("d"));
		file = file.Replace ("#SMARTDEVELOPERS#", "hcq");

		System.IO.File.WriteAllText (path, file);
		AssetDatabase.Refresh ();
	}
}