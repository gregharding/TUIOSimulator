//
// Invisible Button
// http://answers.unity3d.com/questions/801928/46-ui-making-a-button-transparent.html
//

using UnityEngine;

namespace UnityEngine.UI {

	public class Touchable : Graphic {
		
		public override bool Raycast(Vector2 sp, Camera eventCamera) {
			return true;
		}

		protected override void OnPopulateMesh(VertexHelper vh) {
			vh.Clear();
		}
	}
}
