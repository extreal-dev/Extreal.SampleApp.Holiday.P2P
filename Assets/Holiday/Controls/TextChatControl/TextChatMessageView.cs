﻿using System.Diagnostics.CodeAnalysis;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Extreal.SampleApp.Holiday.Controls.TextChatControl
{
    public class TextChatMessageView : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;

        private bool destroyed;

        [SuppressMessage("Style", "IDE0051")]
        private void OnDestroy()
            => destroyed = true;

        public void SetText(string message)
        {
            messageText.text = message;
            PassMessageAsync().Forget();
        }

        private async UniTaskVoid PassMessageAsync()
        {
            var ancestor = transform.parent;
            while (ancestor.GetComponent<Canvas>() == null)
            {
                ancestor = ancestor.transform.parent;
            }

            var canvasRectTransform = ancestor.GetComponent<RectTransform>();
            var rect = canvasRectTransform.rect;
            var canvasWidth = rect.width;
            var canvasHeight = rect.height;
            var velocity = Random.Range(0.2f, 0.5f) * canvasWidth;
            var lifetime = (canvasWidth + messageText.preferredWidth) / velocity;

            var rectTransform = GetComponent<RectTransform>();
            var sizeDelta = new Vector2(messageText.preferredWidth, messageText.preferredHeight);
            sizeDelta = new Vector2(sizeDelta.x, messageText.preferredHeight);
            rectTransform.sizeDelta = sizeDelta;

            if ((Random.Range(0, 10) & 1) == 0)
            {
                rectTransform.anchoredPosition
                    = new Vector2
                    (
                        -messageText.preferredWidth,
                        Random.Range(0f, canvasHeight - messageText.preferredHeight)
                    );
            }
            else
            {
                rectTransform.anchoredPosition
                    = new Vector2
                    (
                        canvasWidth,
                        Random.Range(0f, canvasHeight - messageText.preferredHeight)
                    );
                velocity = -velocity;
            }

            for (var t = 0f; t < lifetime; t += Time.deltaTime)
            {
                if (destroyed)
                {
                    return;
                }

                rectTransform.anchoredPosition += velocity * Time.deltaTime * Vector2.right;
                await UniTask.Yield();
            }

            Destroy(gameObject);
        }
    }
}
