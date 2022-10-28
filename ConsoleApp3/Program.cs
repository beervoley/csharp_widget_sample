using ConsoleApp3;
using ConsoleApp3.COM;
using System.Runtime.InteropServices;

[DllImport("ole32.dll")]
static extern int CoRegisterClassObject(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rclsid,
            [MarshalAs(UnmanagedType.IUnknown)] object pUnk,
            uint dwClsContext,
            uint flags,
            out uint lpdwRegister);

[DllImport("ole32.dll")]
static extern int CoRevokeClassObject(uint dwRegister);


if (args.Length == 1 && args[0] == "-RegisterProcessAsComServer")
{
    Console.WriteLine("Do the COM server thing!");
    uint cookie;
    Guid CLSID_Factory = Guid.Parse("75281d50-4c59-4d98-880b-b8f026082cbd");
    CoRegisterClassObject(CLSID_Factory, new WidgetProviderFactory<WidgetProvider>(), 0x4, 0x1, out cookie);

    Console.WriteLine("Registered. Press ENTER to exit.");
    Console.ReadLine();

    CoRevokeClassObject(cookie);
}
else
{
    Console.WriteLine("Nothing to do.");
}