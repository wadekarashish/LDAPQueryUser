// Decompiled with JetBrains decompiler
// Type: LDAPQueryUserTest.DomainManager
// Assembly: LDAPQueryUserTest, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: EB2059A6-71AC-4D0F-B8F5-3AD585E3BC32
// Assembly location: D:\Tests\Tests\AD User Test\LDAPQueryUserTest.exe

using System;
using System.DirectoryServices.ActiveDirectory;
using System.Text;

namespace LDAPQueryUserTest
{
  public static class DomainManager
  {
    static DomainManager()
    {
      Domain domain = (Domain) null;
      DomainController domainController = (DomainController) null;
      try
      {
        domain = Domain.GetCurrentDomain();
        DomainManager.DomainName = domain.Name;
        domainController = domain.PdcRoleOwner;
        DomainManager.DomainControllerName = domainController.Name.Split('.')[0];
        DomainManager.ComputerName = Environment.MachineName;
      }
      finally
      {
        if (domain != null)
          domain.Dispose();
        if (domainController != null)
          domainController.Dispose();
      }
    }

    public static string DomainControllerName { get; private set; }

    public static string ComputerName { get; private set; }

    public static string DomainName { get; private set; }

    public static string DomainPath
    {
      get
      {
        bool flag = true;
        StringBuilder stringBuilder = new StringBuilder(200);
        string domainName = DomainManager.DomainName;
        char[] chArray = new char[1]{ '.' };
        foreach (string str in domainName.Split(chArray))
        {
          if (flag)
          {
            stringBuilder.Append("DC=");
            flag = false;
          }
          else
            stringBuilder.Append(",DC=");
          stringBuilder.Append(str);
        }
        return stringBuilder.ToString();
      }
    }

    public static string RootPath
    {
      get
      {
        return string.Format("LDAP://{0}/{1}", (object) DomainManager.DomainName, (object) DomainManager.DomainPath);
      }
    }
  }
}
