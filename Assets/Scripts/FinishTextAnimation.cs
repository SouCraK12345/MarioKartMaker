using System.Collections;
using UnityEngine;
using TMPro;

public class FinishTextAnimation : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float delay = 0.1f;
    public float duration = 0.5f;

    void OnEnable()
    {
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {
        textMesh.ForceMeshUpdate();

        TMP_TextInfo textInfo = textMesh.textInfo;


        // 🔽 ここ追加：全体を非表示にする
        for (int m = 0; m < textInfo.meshInfo.Length; m++)
        {
            Color32[] colors = textInfo.meshInfo[m].colors32;
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i].a = 0;
            }
        }

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            StartCoroutine(AnimateCharacter(i));
            yield return new WaitForSeconds(delay);
        }
    }

    IEnumerator AnimateCharacter(int index)
    {
        TMP_TextInfo textInfo = textMesh.textInfo;

        if (!textInfo.characterInfo[index].isVisible)
            yield break;

        int materialIndex = textInfo.characterInfo[index].materialReferenceIndex;
        int vertexIndex = textInfo.characterInfo[index].vertexIndex;

        Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
        Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

        float time = 0;

        // 初期値
        Vector3 center = (vertices[vertexIndex] + vertices[vertexIndex + 2]) / 2;
        Vector3[] originalVertices = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            originalVertices[i] = vertices[vertexIndex + i];
        }

        // 最初は非表示・スケール0
        for (int i = 0; i < 4; i++)
        {
            Vector3 offset = originalVertices[i] - center;
            vertices[vertexIndex + i] = center + offset * 0f;
            colors[vertexIndex + i].a = 0;
        }
        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

        while (time < duration)
        {
            float t = time / duration;

            // スケール（拡大）
            float scale = Mathf.Lerp(2f, 1f, t);

            for (int i = 0; i < 4; i++)
            {
                Vector3 offset = originalVertices[i] - center;
                vertices[vertexIndex + i] = center + offset * scale;
            }

            // 透明度（フェードイン）
            byte alpha = (byte)Mathf.Lerp(0, 255, t);
            for (int i = 0; i < 4; i++)
            {
                colors[vertexIndex + i].a = alpha;
            }

            textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All);

            time += Time.deltaTime;
            yield return null;
        }

        // 最終状態を明示的にセット
        for (int i = 0; i < 4; i++)
        {
            vertices[vertexIndex + i] = originalVertices[i];
            colors[vertexIndex + i].a = 255;
        }
        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }
}
