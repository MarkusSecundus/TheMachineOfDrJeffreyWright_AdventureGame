using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LayerMasks
{
    public static int RoomStatic => LayerMask.GetMask(nameof(RoomStatic));
    public static int AnyRoom => LayerMask.GetMask(nameof(RoomStatic), "FloorFake");
}
