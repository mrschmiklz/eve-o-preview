using System.Runtime.InteropServices;

namespace EveOPreview.Services.Interop
{
	internal static class UxThemeNativeMethods
	{
		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		public static extern int SetWindowTheme(nint hwnd, string pszSubAppName, string pszSubIdList);
	}
}
