using UnityEngine;
using System.Collections;

public class RefreshGalleryWrapper : MonoBehaviour {

#if UNITY_ANDROID

	
	void SetGalleryPath()
	{
		AndroidJavaClass javaClass = new AndroidJavaClass("com.astricstore.androidutil.AndroidGallery");
		javaClass.CallStatic("SetGalleryPath");
	}

	void RefreshGallery(string path)
	{
		AndroidJavaClass javaClass = new AndroidJavaClass("com.astricstore.androidutil.AndroidGallery");
		javaClass.CallStatic("RefreshGallery",path);
	}
#endif
}
