using System;
using com.brg.Common.Logging;
using UnityEngine;

namespace com.brg.Common.UI
{
    public class UIPopup : MonoBehaviour, IInitializable
    {
	    [SerializeField] private string _explicitName = "";
	    
	    private PopupManager _manager;
		private UIPopupAnimationBase _animation;
		private UIPopupBehaviour _behaviour;
		private RectTransform _rect;
		private int _popupZ;
		
		private LogObj Log { get; set; }
		
		public InitializationState State { get; protected set; } = InitializationState.NOT_INITIALIZED;
		public bool Usable => State == InitializationState.SUCCESSFUL;
		public ReinitializationPolicy ReInitPolicy => ReinitializationPolicy.NOT_ALLOWED;
		public IProgressItem GetInitializeProgressItem()
		{
			return new SingleProgressItem((out bool success) =>
			{
				success = State == InitializationState.SUCCESSFUL;
				return State > InitializationState.INITIALIZING;
			}, null, null, 100);
		}

		public int PopupZ
		{
			get => _popupZ;
			set
			{
				_popupZ = value;
				var pos = _rect.anchoredPosition3D;
				pos.z = -_popupZ;
				_rect.anchoredPosition3D = pos;
			}
		}

		public string ExplicitName => _explicitName;
		public UIPopupBehaviour Behaviour => _behaviour;
		public UIPopupAnimationBase Animation => _animation;

		public bool Active
		{
			get => _functionallyActive;
			set
			{
				if (value)
				{
					Show();
				}
				else
				{
					Hide();
				}
			}
		}

		private void OnDestroy()
		{
			if (_animation)
			{
				_animation.OnStateChangeAction -= OnAnimStateChange;
			}
		}

		internal void SetManager(PopupManager manager, LogObj log)
		{
			_manager = manager;
			Log = log;
		}

		public void Initialize()
		{
			if (_explicitName == string.Empty || _explicitName == "")
			{
				_explicitName = name;
				Log.Warn(_explicitName, $"UIPopup is missing explicit name, will use name of game object ({name})");
			}
			
			Log.Info($"Popup \"{_explicitName}\" is initializing.");
			
			// Get components
			_rect = GetComponent<RectTransform>();
			_animation = GetComponent<UIPopupAnimationBase>() ?? gameObject.AddComponent<UIPopupAnimationDefault>();
			_behaviour = GetComponent<UIPopupBehaviour>() ?? gameObject.AddComponent<UIPopupBehaviour>();
			
			_rect.anchoredPosition = new Vector2(0f, 0f);

			_animation.Popup = this;
			_behaviour.Popup = this;

			_behaviour.Initialize();
			_animation.Initialize();

			_animation.OnStateChangeAction += OnAnimStateChange;
			
			State = InitializationState.SUCCESSFUL;
			
			Log.Info($"Popup \"{_explicitName}\" has completed initialization.");
		}

		private bool _functionallyActive;
		private bool _transiting;

		public bool IsShowing => _functionallyActive && _transiting;
		public bool IsHiding => !_functionallyActive && _transiting;
		public bool FunctionallyActive => _functionallyActive;
		public bool Transiting => _transiting;
		public bool HasManager => _manager != null;

		public void Show(bool immediately = false)
		{
			Log.Info(_explicitName, $"Show request is called (immediately: {immediately})");
			_manager.ShowPopup(this, immediately, "");
		}

		public void ShowAndChain(Type chainTo, bool immediately = false)
		{
			Log.Info(_explicitName, $"Show request is called (immediately: {immediately}, chaining to {chainTo})");
			_manager.ShowPopup(this, immediately, chainTo);
		}
		
		public void ShowAndChain(string chainTo, bool immediately = false)
		{
			Log.Info(_explicitName, $"Show request is called (immediately: {immediately}, chaining to {chainTo})");
			_manager.ShowPopup(this, immediately, chainTo);
		}

		public void Hide(bool immediately = false)
		{
			Log.Info(_explicitName, $"Hide request is called (immediately: {immediately})");
			_manager.HidePopup(this, immediately);
		}
		
		internal void ManagerShow(bool immediately)
		{
			_functionallyActive = true;
			_animation.PlayShow();
		}

		internal void ManagerHide(bool immediately)
		{			
			_functionallyActive = false;
			_animation.PlayHide();
		}

		private void OnAnimStateChange(UIAnimState state)
		{
			_transiting = state == UIAnimState.SHOW_START || state == UIAnimState.HIDE_START;
			switch (state)
			{
				case UIAnimState.SHOW_START:
					_behaviour.ShowStartEvent?.Invoke();
					break;
				case UIAnimState.SHOW_END:
					_behaviour.ShowEndEvent?.Invoke();
					break;
				case UIAnimState.HIDE_START:
					_behaviour.HideStartEvent?.Invoke();
					break;
				case UIAnimState.HIDE_END:
					_behaviour.HideEndEvent?.Invoke();
					break;
				case UIAnimState.NONE:
					_behaviour.CleanUpOnShowSessionCompleted();
					break;
			}
		}
    }
}
