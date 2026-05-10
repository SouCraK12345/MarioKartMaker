using UnityEngine;
using UnityEngine.Video;

public class BlackOutMaker : MonoBehaviour
{
    private VideoPlayer videoPlayer;
    public VideoClip black_out;
    public VideoClip black_open;
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
