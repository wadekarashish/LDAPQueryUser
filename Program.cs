// Decompiled with JetBrains decompiler
// Type: LDAPQueryUserTest.Program
// Assembly: LDAPQueryUserTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EB2059A6-71AC-4D0F-B8F5-3AD585E3BC32
// Assembly location: D:\Tests\Tests\AD User Test\LDAPQueryUserTest.exe

using System;
using System.Windows.Forms;

namespace LDAPQueryUserTest
{
  internal static class Program
  {
    [STAThread]
    private static void Main()
    {
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run((Form) new Form1());
    }
  }
}
