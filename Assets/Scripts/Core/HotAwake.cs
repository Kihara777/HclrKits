using UnityEngine;

public class HotAwake : MonoBehaviour
{
    private void OnHotAwake()
    {
        Loader.Invoke("Kits", "Hello", "NowYouSeeMe");

        Loader.Instantiate("kits", "Hello.prefab");
    }
}
