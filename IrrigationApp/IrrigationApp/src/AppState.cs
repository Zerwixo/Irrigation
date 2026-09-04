using System.Collections.Generic;

namespace IrrigationApp;

public static class AppState
{
    public static List<Data>? DataList { get; set; }
    public const string ServerBaseUrl = "http://192.168.1.88:5000/";
    
}

