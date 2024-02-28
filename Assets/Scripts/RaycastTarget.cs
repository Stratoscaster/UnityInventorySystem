using UnityEngine.UI;
public class RaycastTarget : Graphic
{
    // Used for basic raycasting ONLY
    public override void SetMaterialDirty()
    {
        return;
    }

    public override void SetVerticesDirty()
    {
        return;
    }
}
