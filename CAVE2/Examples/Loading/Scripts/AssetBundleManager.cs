using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetBundleManager : MonoBehaviour {

    struct AssetBundleInfo
    {
        public AssetBundle assetBundle;
        public bool loaded;
        public float loadProgress;
        public AssetBundleCreateRequest request;
        public float startTime;
    }

    [SerializeField]
    bool loadAssetBundles;

    [SerializeField]
    string[] assetBundlePath;

    Hashtable assetBundles = new Hashtable();

    // Use this for initialization
    void Start() {
        StartCoroutine("LoadAssetBundle", assetBundlePath[0]);
    }

    // Update is called once per frame
    void Update() {
        if (assetBundlePath.Length > 0)
        {
            AssetBundleInfo testBundle = (AssetBundleInfo)assetBundles[assetBundlePath[0]];
            if (assetBundlePath != null && !testBundle.request.isDone)
            {
                Debug.Log(Time.time - testBundle.startTime + ": " + testBundle.request.progress);
            }
        }
    }

    IEnumerator LoadAssetBundle(string assetBundlePath)
    {
        AssetBundleInfo assetBundleInfo = new AssetBundleInfo();
        assetBundleInfo.loaded = false;
        assetBundleInfo.startTime = Time.time;

        if (!System.IO.Path.IsPathRooted(assetBundlePath))
        {
            assetBundlePath = Application.dataPath + "/" + assetBundlePath;
        }

        Debug.Log("Loading AssetBundle '" + assetBundlePath + "'");
        AssetBundleCreateRequest request = AssetBundle.LoadFromFileAsync(assetBundlePath);
        assetBundleInfo.request = request;
        assetBundles[assetBundlePath] = assetBundleInfo;
        yield return request;

        if (request.assetBundle == null)
        {
            Debug.Log("Failed to load AssetBundle '" + assetBundlePath + "'!");
            yield break;
        }
        else
        {
            assetBundleInfo.assetBundle = request.assetBundle;
            assetBundleInfo.loaded = true;
            Debug.Log("Loaded AssetBundle '" + assetBundlePath + "'");
        }
        assetBundles[assetBundlePath] = assetBundleInfo;
        yield return null;
    }
}