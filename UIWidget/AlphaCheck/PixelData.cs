using System.Collections.Generic;
using System;

[Serializable]
public class PixelInfo {
    public int x;
    public int s;
    public int e;
}

public class PixelData {
    public List<PixelInfo> pixcels;
}
