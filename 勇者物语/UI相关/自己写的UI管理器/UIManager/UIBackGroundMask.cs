using UnityEngine;
using System.Collections;

public class UIBackGroundMask : UIWindowBase 
{
    public override void Open()
    {
        IsMask = false;
        IsMovable = true;
        CanMarginClick = false;
    }

}
