using System.Reflection;
using System.Runtime.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using ConsoleApp3.COM;
using Microsoft.Windows.Widgets.Providers;
using WinRT;
using System.Text.Json.Nodes;
using System.Text.Json;

namespace ConsoleApp3
{
    namespace COM
    {
        static class Guids
        {
            public const string IClassFactory = "00000001-0000-0000-C000-000000000046";
            public const string IUnknown = "00000000-0000-0000-C000-000000000046";
        }

        /// 
        /// IClassFactory declaration
        /// 
        [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid(COM.Guids.IClassFactory)]
        internal interface IClassFactory
        {
            [PreserveSig]
            int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);
            [PreserveSig]
            int LockServer(bool fLock);
        }
    }

    class WidgetProviderFactory<T> : IClassFactory
    where T: IWidgetProvider, new()
    {
        public int CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
        {
            ppvObject = IntPtr.Zero;

            if (pUnkOuter != IntPtr.Zero)
            {
                Marshal.ThrowExceptionForHR(CLASS_E_NOAGGREGATION);
            }

            if (riid == typeof(T).GUID || riid == Guid.Parse(COM.Guids.IUnknown))
            {
                // Create the instance of the .NET object
                ppvObject = MarshalInspectable<IWidgetProvider>.FromManaged(new T());
                //ppvObject = Marshal.GetComInterfaceForObject(
                //    new T().As<IWidgetProvider>,
                //    typeof(IWidgetProvider));
            }
            else
            {
                // The object that ppvObject points to does not support the
                // interface identified by riid.
                Marshal.ThrowExceptionForHR(E_NOINTERFACE);
            }

            return 0;
        }

        int IClassFactory.LockServer(bool fLock)
        {
            return 0;
        }

        private const int CLASS_E_NOAGGREGATION = -2147221232;
        private const int E_NOINTERFACE = -2147467262;
    }

    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.None)]
    [Guid("75281d50-4c59-4d98-880b-b8f026082cbd")]
    public sealed class WidgetProvider : IWidgetProvider
    {
        private static readonly string _template = @"{
            ""type"": ""AdaptiveCard"",
            ""body"": [
                {
                    ""type"": ""TextBlock"",
                    ""text"": ""${frameworkName}""
                },
                {
                    ""type"": ""TextBlock"",
                    ""text"": ""Invokes: ${cInvokes}""
                },
                {
                    ""type"": ""TextBlock"",
                    ""text"": ""last invoked: ${lastInvoke}""
                }
            ]
        }";

        private class WidgetState
        {
            public WidgetState()
            {
                cInvokes = 0;
                lastInvoke = "";
                frameworkName = Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName;
            }
            public uint cInvokes { get; set; }
            public string lastInvoke { get; set; }
            public string? frameworkName { get; }
        }

        private WidgetState _widgetState = new WidgetState();

        private void _updateData(string id)
        {
            WidgetUpdateRequestOptions options = new WidgetUpdateRequestOptions(id);
            options.Data = JsonSerializer.Serialize(_widgetState);
            WidgetManager.GetDefault().UpdateWidget(options);
        }

        public WidgetProvider() { }

        public void Activate(WidgetContext widgetContext)
        {
            Console.WriteLine($"Activate id: {widgetContext.Id} definitionId: {widgetContext.DefinitionId}");
            _widgetState.cInvokes++;
            _widgetState.lastInvoke = "Activate";
        }

        public void CreateWidget(WidgetContext widgetContext)
        {
            Console.WriteLine($"CreateWidget id: {widgetContext.Id} definitionId: {widgetContext.DefinitionId}");
            _widgetState.lastInvoke = "CreateWidget";

            WidgetUpdateRequestOptions options = new WidgetUpdateRequestOptions(widgetContext.Id);
            options.Template = _template; //$"{{ \"type\": \"AdaptiveCard\", \"body\": [ {{ \"type\": \"TextBlock\", \"text\": \"{_frameworkName}\"  }} ] }}";
            options.Data = JsonSerializer.Serialize(_widgetState);
            WidgetManager.GetDefault().UpdateWidget(options);
            _widgetState.cInvokes++;
        }

        public void Deactivate(string widgetId)
        {
            Console.WriteLine($"Deactivate id: {widgetId}");
            _widgetState.lastInvoke = "Deactivate";

            _updateData(widgetId);

            _widgetState.cInvokes++;
        }

        public void DeleteWidget(string widgetId, string customState)
        {
            Console.WriteLine($"DeleteWidget id: {widgetId}");
            _widgetState.lastInvoke = "DeleteWidget";

            _updateData(widgetId);

            _widgetState.cInvokes++;
        }

        public void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
        {
            Console.WriteLine($"OnActionInvoked id: {actionInvokedArgs.WidgetContext.Id} definitionId: {actionInvokedArgs.WidgetContext.DefinitionId}");
            _widgetState.lastInvoke = "OnActionInvoked";

            _updateData(actionInvokedArgs.WidgetContext.Id);

            _widgetState.cInvokes++;
        }

        public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
        {
            Console.WriteLine($"OnWidgetContextChanged id: {contextChangedArgs.WidgetContext.Id} definitionId: {contextChangedArgs.WidgetContext.DefinitionId}");
            _widgetState.lastInvoke = "OnWidgetContextChanged";

            _updateData(contextChangedArgs.WidgetContext.Id);

            _widgetState.cInvokes++;
        }
    }
}
