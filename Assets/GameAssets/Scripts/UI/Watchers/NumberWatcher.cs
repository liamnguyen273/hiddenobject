using System;
using com.brg.Common.Localization;
using Lean.Transition;
using TMPro;
using UnityEngine;

namespace com.tinycastle.StickerBooker
{
    public class NumberWatcher : ResourceWatcher
    {
        [Header("Number components")]
        [SerializeField] private TextLocalizer _number;
        [SerializeField] private bool _floatyOnChange;
        
        private void Awake()
        {
            _number.RawString = "";
        }

        protected override void OnResourceChange(int newValue, int change)
        {
            base.OnResourceChange(newValue, change);
            
            _number.RawString = FormatNumber(newValue);

            if (_floatyOnChange && change != 0)
            {
                GM.Instance.Effects.PlayFloatyText(change > 0 ? $"+{change}" : change.ToString(),
                    transform, Vector3.zero, -0.2f);
            }
        }

        protected virtual string FormatNumber(int value)
        {
            return value.ToString();
        }
    }
}