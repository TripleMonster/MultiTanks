using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

/*
 *  by = byte 
    i = int
    ui = uint
    bo = bool
    f = float
    i64 = Int64
*/

namespace UnityEventUtils {
    public class UEvent_by : UnityEvent<byte> {}  
	public class UEvent_bo : UnityEvent<bool> {}
    public class UEvent_f : UnityEvent<float> {}
    public class UEvent_s : UnityEvent<string> {}

    public class UEvent_by_by : UnityEvent<byte, byte> { }
    public class UEvent_by_f : UnityEvent<byte, float> { }
    public class UEvent_f_f : UnityEvent<float, float> { }

    public class UEvent_by_f_f : UnityEvent<byte, float, float> { }
    public class UEvent_by_i64_i64 : UnityEvent<byte, Int64, Int64> { }

    public class UEvent_by_f_f_f : UnityEvent<byte, float, float, float> { }
}

