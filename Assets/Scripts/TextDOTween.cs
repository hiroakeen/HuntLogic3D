using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextDOTween : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    private Sequence seq;

    private void Start()
    {
        AnimateLoadingText();
    }

    private void AnimateLoadingText()
    {
        seq = DOTween.Sequence();

        for (int i = 0; i < 2; i++)
        {
            seq.Append(loadingText.DOFade(0f, 1f)); // 1秒かけて透明に
            seq.Join(loadingText.rectTransform.DOScale(1.2f, 1f)); // 同時に拡大
            seq.Append(loadingText.DOFade(1f, 1f)); // 1秒かけて元に戻す
            seq.Join(loadingText.rectTransform.DOScale(1f, 1f)); // サイズも戻す
        }

        Invoke(nameof(StopAnimation), 3f);
    }

    private void StopAnimation()
    {
        if (seq != null && seq.IsActive())
        {
            seq.Kill(); // ちゃんと停止
        }
    }

    private void OnDestroy()
    {
        if (seq != null && seq.IsActive())
        {
            seq.Kill(); // シーン破棄時も停止
        }
    }
}
