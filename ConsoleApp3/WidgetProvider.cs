using System.Reflection;
using System.Runtime.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp3.COM;
using Microsoft.Windows.Widgets.Providers;
using WinRT;

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
                ppvObject = Marshal.GetComInterfaceForObject(
                    new T(),
                    typeof(IWidgetProvider));
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
        public WidgetProvider() { }

        public void Activate(WidgetContext widgetContext)
        {
            Console.WriteLine("Activate");
        }

        public void CreateWidget(WidgetContext widgetContext)
        {
            Console.WriteLine($"CreateWidget id: {widgetContext.Id} definitionId: {widgetContext.DefinitionId}");

            WidgetUpdateRequestOptions options = new WidgetUpdateRequestOptions(widgetContext.Id);
            options.Template = @"{ ""type"": ""AdaptiveCard"", ""body"": [ { ""type"": ""TextBlock"", ""text"": ""${version}"" ] }";
            options.Data = $"{{ \"version\": {Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName} }}";
            WidgetManager.GetDefault().UpdateWidget(options);
        }

        public void Deactivate(string widgetId)
        {
            Console.WriteLine("Deactivated");
        }

        public void DeleteWidget(string widgetId, string customState)
        {
            Console.WriteLine("DeleteWidget");
        }

        public void OnActionInvoked(WidgetActionInvokedArgs actionInvokedArgs)
        {
            Console.WriteLine("OnActionInvoked");
        }

        public void OnWidgetContextChanged(WidgetContextChangedArgs contextChangedArgs)
        {
            Console.WriteLine("OnWidgetContextChanged");
        }
    }
}
