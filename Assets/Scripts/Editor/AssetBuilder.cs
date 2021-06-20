using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class AssetBuilder : MonoBehaviour
    {
        [MenuItem("Assets/Build AssetBundle")]
        static void CreateAssetBundlesMain()
        {
            //BuildAssetBundleOptions.None表示默认的压缩格式LZMA，优点：压缩的非常小。缺点：解压时可能卡顿
            //BuildAssetBundleOptions.UncompressedAssetBundle：不压缩
            //BuildAssetBundleOptions.ChunkBasedCompression：这种方案。推荐使用
            //第三个参数是平台         
            BuildPipeline.BuildAssetBundles("Assets/Artworks/Bundles/",
                BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.StandaloneWindows);
        }
    
        [MenuItem("Assets/Get AssetBundle names")]
        static void GetAssetBundleNames()
        {
            string[] names = AssetDatabase.GetAllAssetBundleNames();
            foreach (string name in names)
            {
                Debug.Log("AssetBundle:" + name); 
            }
        }
    }
}