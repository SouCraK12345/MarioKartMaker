using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BlackOutMaker : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    public GameObject blackoutRenderer;
    public RenderTexture BlackOutRenderTexture;
    public VideoClip black_out;
    public VideoClip black_open;
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        blackoutRenderer.GetComponent<RawImage>().texture = BlackOutRenderTexture;
        videoPlayer.clip = black_open;
        videoPlayer.Play();
    }
    public void BlackOut()
    {
        videoPlayer.clip = black_out;
        videoPlayer.Play();
    }
    public void BlackOpen()
    {
        videoPlayer.clip = black_open;
        videoPlayer.Play();
    }
}
