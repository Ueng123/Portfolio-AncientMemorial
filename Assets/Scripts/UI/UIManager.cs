using System.Collections.Generic;
using AncientMemorial;
using TMPro;
using UnityEngine;

namespace AncientMemorial.UI {
	public class UIManager : Manager<UIManager> {
		private static readonly int Show = Animator.StringToHash("Show");

		[Header("TitleUI")] public Animator TitleUIAnimator;

		public Queue<TitleElement> TitleQueue = new();

		public TMP_Text TitleText;
		public TMP_Text MainContent;
		public TMP_Text SubContent;

		public DelayedAction ShowCooldown;
		public bool          Showable = true;
		
		public float ShowDuration = 7f;

		public void ScheduleTitle(string title, string mainContent, string subContent, float speed) {
			TitleQueue.Enqueue(new TitleElement(title, mainContent, subContent, speed));
		}

		private void ShowTitle(TitleElement item) {
			TitleText.text   = item.title;
			MainContent.text = item.mainText;
			SubContent.text  = item.subText;

			TitleUIAnimator.speed = item.speed;
			TitleUIAnimator.SetTrigger(Show);

			Showable           = false;
			ShowCooldown.delay = ShowDuration / item.speed;
			ShowCooldown.Execute();
		}

		public override void Initialize() {
			ShowCooldown = new DelayedAction(ShowDuration, () => Showable = true);
		}

		public override void ManagerUpdate() {
			if (!Showable || TitleQueue.Count == 0) return;
			ShowTitle(TitleQueue.Dequeue());
		}

		public override void ManagerFixedUpdate() { }
	}
}