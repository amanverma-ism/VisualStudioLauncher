﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VisualStudioLauncher
{
    public static class NativeMethods
    {
        [DllImport("user32.dll", EntryPoint = "GetWindowRect")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);
        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);


        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        public static bool GetWindowRectangle(HandleRef hWnd, out System.Windows.Rect oRect)
        {
            RECT lpRect;
            bool res = GetWindowRect(hWnd, out lpRect);
            System.Windows.Rect rect = new System.Windows.Rect(lpRect.Left, lpRect.Top, Math.Abs(lpRect.Right - lpRect.Left), Math.Abs(lpRect.Top - lpRect.Bottom));
            oRect = rect;
            return res;
        }
    }
}
