using System;
using CoreAnimation;

namespace TZStackView
{
	public class AnimationDelegate : CAAnimationDelegate
	{
		public Action AnimationStoppedCallback { get; set;}

		public override void AnimationStopped (CAAnimation anim, bool finished)
		{
			AnimationStoppedCallback?.Invoke ();
		}
	}
}

